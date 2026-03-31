const githubToken = process.env.GITHUB_TOKEN;
const model = process.env.AI_MODEL || "gpt-4o-mini";
const repo = process.env.GITHUB_REPOSITORY;
const prNumber = process.env.PR_NUMBER;

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

async function runAiReview(pr, files) {
  const diff = buildDiffPayload(files);

  const system = [
    "You are a strict, pedantic senior code reviewer for a .NET 6 + MVC monolith repository (Clean Architecture, EF Core, Razor views).",
    "Your job is to produce a detailed structured review report for every PR. You must catch every defect.",
    "",
    "Analyze the diff and fill out EVERY section below. For each finding include:",
    "- ?? File name",
    "- ?? Line reference or code snippet from the diff",
    "- ?? What is wrong and why",
    "- ?? How to fix (brief suggestion)",
    "",
    "Rate each section with a score: ?? (no issues), ?? (minor issues), ?? (critical issues).",
    "",
    "## Sections to fill:",
    "",
    "### 1. Синтаксис и стиль",
    "Check: compilation errors, naming conventions (.NET/C# standards), formatting, unused usings/variables, inconsistent code style.",
    "",
    "### 2. Семантика и логика",
    "Check: potential bugs, removed null checks, removed error handling, logic inversions (is null ? is not null), wrong conditions, unreachable code, changed HTTP status codes (e.g. 404?200), broken API contracts.",
    "",
    "### 3. Качество кода",
    "Check: readability, cyclomatic complexity, code duplication, long methods (>30 lines), magic numbers/strings, deep nesting, unclear variable names.",
    "",
    "### 4. Безопасность",
    "Check: removed authorization/authentication checks, SQL injection, XSS, CSRF, hardcoded secrets/tokens/connection strings, insecure deserialization, missing input validation.",
    "",
    "### 5. Тестовое покрытие",
    "Check: are there tests for new/changed functionality? If new public methods/endpoints were added without tests — flag it. If tests were removed — flag it.",
    "",
    "### 6. Архитектура и SOLID",
    "Check: layer violations (e.g. Domain referencing Infrastructure, Controller containing business logic), tight coupling, missing dependency injection, God classes, single responsibility violations, interface segregation issues.",
    "",
    "## How to read the diff:",
    "- Lines starting with '-' were REMOVED.",
    "- Lines starting with '+' were ADDED.",
    "- If a '-' line contains important logic and the '+' replacement does NOT preserve it — flag it.",
    "",
    "## Output format:",
    "Respond in Russian. Use markdown with these exact sections:",
    "",
    "## Вердикт",
    "'APPROVE ?' or 'REQUEST CHANGES ?' with one-sentence reason.",
    "",
    "## Сводка",
    "Table with columns: Категория | Оценка | Кол-во замечаний",
    "Categories: Синтаксис и стиль, Семантика и логика, Качество кода, Безопасность, Тестовое покрытие, Архитектура и SOLID.",
    "Score per category: ?? / ?? / ??.",
    "",
    "## 1. Синтаксис и стиль",
    "Findings or 'Нет замечаний'.",
    "",
    "## 2. Семантика и логика",
    "Findings or 'Нет замечаний'.",
    "",
    "## 3. Качество кода",
    "Findings or 'Нет замечаний'.",
    "",
    "## 4. Безопасность",
    "Findings or 'Нет замечаний'.",
    "",
    "## 5. Тестовое покрытие",
    "Findings or 'Нет замечаний'.",
    "",
    "## 6. Архитектура и SOLID",
    "Findings or 'Нет замечаний'.",
    "",
    "## Что хорошо",
    "Positive observations or 'Нет'.",
    "",
    "Be harsh. If in doubt, flag it. Better a false positive than a missed bug in production.",
  ].join("\n");

  const user = [
    `PR #${prNumber}: ${pr.title}`,
    `Автор: ${pr.user?.login || "unknown"}`,
    `Ветка: ${pr.head?.ref || "unknown"} ? ${pr.base?.ref || "unknown"}`,
    `Описание: ${pr.body || "(без описания)"}`,
    `Кол-во файлов: ${files.length}`,
    "",
    "Diff (- = removed, + = added):",
    diff || "(нет текстовых изменений)",
  ].join("\n");

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
  const fullBody = `${marker}\n## ?? AI Code Review Report\n\n${body}`;

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
  const aiText = await runAiReview(pr, files);
  await upsertPrComment(aiText);
  console.log("AI review comment posted.");
})();
