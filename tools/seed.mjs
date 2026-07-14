// Idempotent demo seeder. Drives the running API (local or deployed).
//   Usage:  node tools/seed.mjs            (defaults to http://localhost:5261)
//           API=https://your-api node tools/seed.mjs
//
// Creates two users, a mix of public/private posts, comments, replies and likes
// so the feed has realistic content to review.

const BASE = process.env.API ?? "http://localhost:5261";

async function call(path, { method = "GET", body, cookie, form } = {}) {
  const headers = {};
  if (cookie) headers.Cookie = cookie;
  if (body && !form) headers["Content-Type"] = "application/json";
  const res = await fetch(`${BASE}${path}`, {
    method,
    headers,
    body: form ? body : body ? JSON.stringify(body) : undefined,
  });
  const setCookie = res.headers.get("set-cookie");
  const text = await res.text();
  const data = text ? JSON.parse(text) : null;
  return { status: res.status, data, cookie: setCookie?.split(";")[0] };
}

async function ensureUser(user) {
  const reg = await call("/api/auth/register", { method: "POST", body: user });
  if (reg.status === 201) return reg.cookie;
  // Already exists → log in.
  const login = await call("/api/auth/login", {
    method: "POST",
    body: { email: user.email, password: user.password },
  });
  return login.cookie;
}

async function createPost(cookie, content, visibility) {
  const form = new FormData();
  form.set("content", content);
  form.set("visibility", visibility);
  const res = await call("/api/posts", { method: "POST", cookie, body: form, form: true });
  return res.data;
}

async function comment(cookie, postId, content, parentCommentId) {
  const res = await call(`/api/posts/${postId}/comments`, {
    method: "POST",
    cookie,
    body: { content, parentCommentId: parentCommentId ?? null },
  });
  return res.data;
}

const likePost = (cookie, id) => call(`/api/posts/${id}/like`, { method: "POST", cookie });
const likeComment = (cookie, id) => call(`/api/comments/${id}/like`, { method: "POST", cookie });

async function main() {
  console.log(`Seeding ${BASE} ...`);

  const alice = await ensureUser({ firstName: "Alice", lastName: "Anderson", email: "alice@demo.com", password: "Passw0rd!" });
  const bob = await ensureUser({ firstName: "Bob", lastName: "Brown", email: "bob@demo.com", password: "Passw0rd!" });
  if (!alice || !bob) throw new Error("Could not obtain auth cookies");

  const p1 = await createPost(alice, "Just shipped a new feature 🚀 (public)", "Public");
  const p2 = await createPost(bob, "Anyone up for code review? (public)", "Public");
  await createPost(alice, "My private notes — only I can see this.", "Private");

  const c1 = await comment(bob, p1.id, "Congrats! What did you ship?");
  await comment(alice, p1.id, "Thanks!", c1.id); // reply
  await comment(alice, p2.id, "Sure, send the PR.");

  await likePost(bob, p1.id);
  await likePost(alice, p2.id);
  await likeComment(alice, c1.id);

  console.log("Done. Demo logins: alice@demo.com / bob@demo.com  (password: Passw0rd!)");
}

main().catch((e) => {
  console.error("Seed failed:", e.message);
  process.exit(1);
});
