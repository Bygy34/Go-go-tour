const githubToken = process.env.GITHUB_TOKEN;
const openAiKey = process.env.OPENAI_API_KEY;
const model = process.env.OPENAI_MODEL || "gpt-4o-mini";
const repo = process.env.GITHUB_REPOSITORY;
const prNumber = process.env.PR_NUMBER;

if (!githubToken) {
  throw new Error("GITHUB_TOKEN is required");
}

if (!openAiKey) {
  throw new Error("OPENAI_API_KEY is required. Add it to repository secrets.");
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
    "You are a strict senior reviewer for a .NET 6 + MVC/React repository.",
    "Review only changed code from the pull request.",
    "Focus on correctness, security, breaking behavior, performance hotspots, and maintainability.",
    "Be concise and practical.",
    "Return markdown in Russian with sections:",
    "1) Итог",
    "2) Блокирующие замечания",
    "3) Неблокирующие замечания",
    "4) Что хорошо",
    "If there are no issues in a section, write 'Нет'.",
  ].join(" ");

  const user = [
    `PR: ${pr.title}`,
    `Описание: ${pr.body || "(без описания)"}`,
    "Измененные файлы и патчи:",
    diff || "(нет текстовых изменений)",
  ].join("\n\n");

  const response = await fetch("https://api.openai.com/v1/chat/completions", {
    method: "POST",
    headers: {
      Authorization: `Bearer ${openAiKey}`,
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
    throw new Error(`OpenAI API ${response.status}: ${text}`);
  }

  const data = await response.json();
  return data.choices?.[0]?.message?.content?.trim() || "AI review is empty.";
}

async function upsertPrComment(body) {
  const marker = "<!-- ai-pr-review -->";
  const fullBody = `${marker}\n## ?? AI PR Review\n\n${body}`;

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
  const pr = await getPrMeta();
  const files = await getPrFiles();
  const aiText = await runAiReview(pr, files);
  await upsertPrComment(aiText);
  console.log("AI PR review comment posted.");
})();
