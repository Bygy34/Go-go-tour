const githubToken = process.env.GITHUB_TOKEN;
const model = process.env.AI_MODEL || "gpt-4o-mini";
const repo = process.env.GITHUB_REPOSITORY;
const prNumber = process.env.PR_NUMBER;
const jiraBaseUrl = process.env.JIRA_BASE_URL || "";
const jiraEmail = process.env.JIRA_USER_EMAIL || "";
const jiraToken = process.env.JIRA_API_TOKEN || "";

if (!githubToken) {
  throw new Error("GITHUB_TOKEN is required");
}

if (!repo || !prNumber) {
  throw new Error("GITHUB_REPOSITORY and PR_NUMBER are required");
}

const [owner, name] = repo.split("/");

async function gh(path, options = {}) {
  const response = await fetch(`https://api.github.com${path}`, {
    ...options,
    headers: {
      Authorization: `Bearer ${githubToken}`,
      Accept: "application/vnd.github+json",
      "X-GitHub-Api-Version": "2022-11-28",
      ...(options.headers || {}),
    },
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`GitHub API ${response.status}: ${text}`);
  }

  return response.status === 204 ? null : response.json();
}

async function getPrMeta() {
  return gh(`/repos/${owner}/${name}/pulls/${prNumber}`);
}

async function getPrFiles() {
  const files = [];
  let page = 1;

  while (true) {
    const batch = await gh(`/repos/${owner}/${name}/pulls/${prNumber}/files?per_page=100&page=${page}`);
    files.push(...batch);

    if (batch.length < 100) {
      break;
    }

    page += 1;
  }

  return files;
}

function buildDiffPayload(files) {
  const maxChars = 120_000;
  let total = 0;
  const blocks = [];

  for (const f of files) {
    const patch = f.patch || "(no patch available for this file type)";
    const block = `FILE: ${f.filename}\nSTATUS: ${f.status}\nPATCH:\n${patch}\n`;

    if (total + block.length > maxChars) {
      break;
    }

    blocks.push(block);
    total += block.length;
  }

  return blocks.join("\n---\n");
}

function extractJiraKey(text) {
  const match = text?.match(/[A-Z][A-Z0-9]+-\d+/);
  return match ? match[0] : null;
}

async function getJiraTicket(pr) {
  if (!jiraBaseUrl || !jiraEmail || !jiraToken) {
    return null;
  }

  const key =
    extractJiraKey(pr.head?.ref) ||
    extractJiraKey(pr.title) ||
    extractJiraKey(pr.body);

  if (!key) {
    return null;
  }

  // customfield_10016 — standard Acceptance Criteria field in Jira (may vary per instance)
  const url = `${jiraBaseUrl}/rest/api/3/issue/${key}?fields=summary,description,customfield_10016`;
  const response = await fetch(url, {
    headers: {
      Authorization: `Basic ${Buffer.from(`${jiraEmail}:${jiraToken}`).toString("base64")}`,
      Accept: "application/json",
    },
  });

  if (!response.ok) {
    console.warn(`Jira API ${response.status} for key ${key} — skipping Jira context.`);
    return null;
  }

  const data = await response.json();
  const fields = data.fields || {};

  const summary = fields.summary || "";
  const description = extractAdfText(fields.description);
  const acField =
    extractAdfText(fields["customfield_10016"]) ||
    extractAcFromText(description);

  return { key, summary, description, acceptanceCriteria: acField };
}

function extractAdfText(node) {
  if (!node) return "";
  if (typeof node === "string") return node;
  if (node.type === "text") return node.text || "";
  if (Array.isArray(node.content)) {
    return node.content.map(extractAdfText).join(" ").trim();
  }
  if (node.content) return extractAdfText(node.content);
  return "";
}

function extractAcFromText(text) {
  if (!text) return "";
  const match = text.match(/acceptance criteria[:\s]+([\s\S]*?)(?:\n#{1,3}\s|$)/i);
  return match ? match[1].trim() : "";
}

async function runAiReview(pr, files, jiraTicket) {
  const diff = buildDiffPayload(files);

  const jiraInstructions = jiraTicket
    ? [
        "",
        "### 7. Jira: Acceptance Criteria",
        `Ticket: ${jiraTicket.key} — ${jiraTicket.summary}`,
        "",
        "Description:",
        jiraTicket.description || "(not provided)",
        "",
        "Acceptance Criteria:",
        jiraTicket.acceptanceCriteria || "(not provided)",
        "",
        "For EACH acceptance criterion check:",
        "- Is it addressed by the diff? Mark: covered / partial / not covered",
        "- Is there a test or validation for it?",
        "- If NOT covered — flag as critical finding.",
      ].join("\n")
    : null;

  const system = [
    "You are a strict, pedantic senior code reviewer for a .NET 6 + Clean Architecture repository (EF Core, REST API).",
    "Your job is to produce a detailed structured review report for every PR. You must catch every defect.",
    "",
    "Analyze the diff and fill out EVERY section below. For each finding include:",
    "- \uD83D\uDCC4 File name",
    "- \uD83D\uDCCD Line reference or code snippet from the diff",
    "- \u2757 What is wrong and why",
    "- \uD83D\uDD27 How to fix (brief suggestion)",
    "",
    "Rate each section with a score: \u2705 (no issues), \u26A0\uFE0F (minor issues), \u274C (critical issues).",
    "",
    "## Sections to fill:",
    "",
    "### 1. \u0421\u0438\u043D\u0442\u0430\u043A\u0441\u0438\u0441 \u0438 \u0441\u0442\u0438\u043B\u044C",
    "Check: compilation errors, naming conventions (.NET/C# standards), formatting, unused usings/variables, inconsistent code style.",
    "",
    "### 2. \u0421\u0435\u043C\u0430\u043D\u0442\u0438\u043A\u0430 \u0438 \u043B\u043E\u0433\u0438\u043A\u0430",
    "Check: potential bugs, removed null checks, removed error handling, logic inversions, wrong conditions, unreachable code, changed HTTP status codes, broken API contracts.",
    "",
    "### 3. \u041A\u0430\u0447\u0435\u0441\u0442\u043E \u043A\u043E\u0434\u0430",
    "Check: readability, cyclomatic complexity, code duplication, long methods (>30 lines), magic numbers/strings, deep nesting, unclear variable names.",
    "",
    "### 4. \u0411\u0435\u0437\u043E\u043F\u0430\u0441\u043D\u043E\u0441\u0442\u044C",
    "Check: removed authorization/authentication checks, SQL injection, XSS, CSRF, hardcoded secrets/tokens/connection strings, insecure deserialization, missing input validation.",
    "",
    "### 5. \u0422\u0435\u0441\u0442\u043E\u0432\u043E\u0435 \u043F\u043E\u043A\u0440\u044B\u0442\u0438\u0435",
    "Check: are there tests for new/changed functionality? If new public methods/endpoints were added without tests — flag it. If tests were removed — flag it.",
    "",
    "### 6. \u0410\u0440\u0445\u0438\u0442\u0435\u043A\u0442\u0443\u0440\u0430 \u0438 SOLID",
    "Check: layer violations (e.g. Domain referencing Infrastructure, Controller containing business logic), tight coupling, missing dependency injection, God classes, single responsibility violations, interface segregation issues.",
    ...(jiraInstructions ? [jiraInstructions] : []),
    "",
    "## How to read the diff:",
    "- Lines starting with '-' were REMOVED.",
    "- Lines starting with '+' were ADDED.",
    "- If a '-' line contains important logic and the '+' replacement does NOT preserve it — flag it.",
    "",
    "## Output format:",
    "Respond in Russian. Use markdown with these exact sections:",
    "",
    "## \u0412\u0435\u0440\u0434\u0438\u043A\u0442",
    "'APPROVE \u2705' or 'REQUEST CHANGES \u274C' with one-sentence reason.",
    "",
    "## \u0421\u0432\u043E\u0434\u043A\u0430",
    "Table with columns: \u041A\u0430\u0442\u0435\u0433\u043E\u0440\u0438\u044F | \u041E\u0446\u0435\u043D\u043A\u0430 | \u041A\u043E\u043B-\u0432\u043E \u0437\u0430\u043C\u0435\u0447\u0430\u043D\u0438\u0439",
    "Categories: \u0421\u0438\u043D\u0442\u0430\u043A\u0441\u0438\u0441 \u0438 \u0441\u0442\u0438\u043B\u044C, \u0421\u0435\u043C\u0430\u043D\u0442\u0438\u043A\u0430 \u0438 \u043B\u043E\u0433\u0438\u043A\u0430, \u041A\u0430\u0447\u0435\u0441\u0442\u0432\u043E \u043A\u043E\u0434\u0430, \u0411\u0435\u0437\u043E\u043F\u0430\u0441\u043D\u043E\u0441\u0442\u044C, \u0422\u0435\u0441\u0442\u043E\u0432\u043E\u0435 \u043F\u043E\u043A\u0440\u044B\u0442\u0438\u0435, \u0410\u0440\u0445\u0438\u0442\u0435\u043A\u0442\u0443\u0440\u0430 \u0438 SOLID.",
    "Score per category: \u2705 / \u26A0\uFE0F / \u274C.",
    "",
    "## 1. \u0421\u0438\u043D\u0442\u0430\u043A\u0441\u0438\u0441 \u0438 \u0441\u0442\u0438\u043B\u044C",
    "Findings or '\u041D\u0435\u0442 \u0437\u0430\u043C\u0435\u0447\u0430\u043D\u0438\u0439'.",
    "",
    "## 2. \u0421\u0435\u043C\u0430\u043D\u0442\u0438\u043A\u0430 \u0438 \u043B\u043E\u0433\u0438\u043A\u0430",
    "Findings or '\u041D\u0435\u0442 \u0437\u0430\u043C\u0435\u0447\u0430\u043D\u0438\u0439'.",
    "",
    "## 3. \u041A\u0430\u0447\u0435\u144t\u043E \u043A\u043E\u0434\u0430",
    "Findings or '\u041D\u0435\u0442 \u0437\u0430\u043C\u0435\u144t\u0430\u043D\u0438\u0439'.",
    "",
    "## 4. \u0411\u0435\u0437\u043E\u043F\u0430\u0441\u043D\u043E\u144t\u044C",
    "Findings or '\u041D\u0435\u144t \u0437\u0430\u043C\u0435\u144t\u0435\u043D\u0438\u0439'.",
    "",
    "## 5. \u0422\u0435\u0441\u0442\u043E\u0432\u043E\u0435 \u043F\u043E\u043A\u0440\u144t\u0438\u0435",
    "Findings or '\u041D\u0435\u144t \u0437\u0430\u043C\u0435\u144t\u0435\u043D\u0438\u0439'.",
    "",
    "## 6. \u0410\u0440\u0445\u0438\u0442\u0435\u043A\u0442\u144e\u0440\u0430 \u0438 SOLID",
    "Findings or '\u041D\u0435\u144t \u0437\u0430\u043C\u0435\u144t\u0435\u043D\u0438\u0439'.",
    "",
    ...(jiraTicket
      ? [
          `## 7. Jira ${jiraTicket.key}: Acceptance Criteria`,
          "For each criterion: \u2705 covered / \u26A0\uFE0F partial / \u274C not covered.",
          "End with overall verdict: all criteria met or not.",
          "",
        ]
      : []),
    "## What is good",
    "Positive observations or '\u041D\u0435\u144t'.",
    "",
    "Be harsh. If in doubt, flag it. Better a false positive than a missed bug in production.",
  ].join("\n");

  const user = [
    `PR #${prNumber}: ${pr.title}`,
    `\u0410\u0432\u0442\u043E\u0440: ${pr.user?.login || "unknown"}`,
    `\u0412\u0435\u0442\u043A\u0430: ${pr.head?.ref || "unknown"} \u2192 ${pr.base?.ref || "unknown"}`,
    `\u041E\u043F\u0438\u0441\u0430\u043D\u0438\u0435: ${pr.body || "(\u0431\u0435\u0437 \u043E\u043F\u0438\u0441\u0430\u043D\u0438\u044F)"}`,
    jiraTicket ? `Jira: ${jiraTicket.key} \u2014 ${jiraTicket.summary}` : "",
    `\u041A\u043E\u043B-\u0432\u043E \u0444\u0430\u0439\u043B\u043E\u0432: ${files.length}`,
    "",
    "Diff (- = removed, + = added):",
    diff || "(\u043D\u0435\u0442 \u0442\u0435\u043A\u0441\u0442\u043E\u0432\u044B\u0445 \u0438\u0437\u043C\u0435\u043D\u0435\u043D\u0438\u0439)",
  ].filter(Boolean).join("\n");

  const response = await fetch("https://models.inference.ai.azure.com/chat/completions", {
    method: "POST",
    headers: {
      Authorization: `Bearer ${githubToken}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      model,
      temperature: 0.2,
      messages: [
        { role: "system", content: system },
        { role: "user", content: user },
      ],
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`AI API ${response.status}: ${text}`);
  }

  const data = await response.json();
  return data.choices?.[0]?.message?.content?.trim() || "AI review is empty.";
}

async function upsertPrComment(body) {
  const marker = "<!-- ai-pr-review -->";
  const fullBody = `${marker}\n## \uD83E\uDD16 AI Code Review Report\n\n${body}`;

  const comments = await gh(`/repos/${owner}/${name}/issues/${prNumber}/comments?per_page=100`);
  const existing = comments.find((c) => c.body?.includes(marker));

  if (existing) {
    await gh(`/repos/${owner}/${name}/issues/comments/${existing.id}`, {
      method: "PATCH",
      body: JSON.stringify({ body: fullBody }),
    });
    return;
  }

  await gh(`/repos/${owner}/${name}/issues/${prNumber}/comments`, {
    method: "POST",
    body: JSON.stringify({ body: fullBody }),
  });
}

(async () => {
  console.log(`Reviewing PR #${prNumber}...`);
  const pr = await getPrMeta();
  const files = await getPrFiles();
  console.log(`Found ${files.length} changed files.`);
  const jiraTicket = await getJiraTicket(pr);
  if (jiraTicket) {
    console.log(`Jira ticket found: ${jiraTicket.key} — ${jiraTicket.summary}`);
  } else {
    console.log("No Jira ticket linked — skipping Jira context.");
  }
  const aiText = await runAiReview(pr, files, jiraTicket);
  await upsertPrComment(aiText);
  console.log("AI review comment posted.");
})();
