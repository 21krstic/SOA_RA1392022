const API = "http://localhost:5000";

// ---------- session ----------
const session = {
  get token() { return localStorage.getItem("token"); },
  get userId() { return localStorage.getItem("userId"); },
  get username() { return localStorage.getItem("username"); },
  get role() { return localStorage.getItem("role"); },
  set(data) {
    localStorage.setItem("token", data.token);
    localStorage.setItem("userId", data.userId);
    localStorage.setItem("username", data.username);
    localStorage.setItem("role", data.role);
    renderSession();
  },
  clear() {
    localStorage.clear();
    renderSession();
  },
};

function renderSession() {
  const el = document.getElementById("session-info");
  el.textContent = session.token
    ? `${session.username} (${session.role}) — id: ${session.userId}`
    : "Not logged in";
}

// ---------- API + logging ----------
function log(method, path, status, body) {
  const el = document.getElementById("log");
  const entry = document.createElement("div");
  entry.className = "entry" + (status >= 400 || status === 0 ? " err" : "");
  entry.textContent = `${method} ${path} -> ${status}\n${JSON.stringify(body, null, 2)}`;
  el.prepend(entry);
}

async function api(method, path, body, { auth = true, isForm = false } = {}) {
  const headers = {};
  if (auth && session.token) headers["Authorization"] = `Bearer ${session.token}`;
  if (!isForm && body !== undefined) headers["Content-Type"] = "application/json";

  let status = 0;
  let data = null;
  try {
    const res = await fetch(`${API}${path}`, {
      method,
      headers,
      body: body === undefined ? undefined : isForm ? body : JSON.stringify(body),
    });
    status = res.status;
    const text = await res.text();
    data = text ? tryParseJson(text) : null;
  } catch (err) {
    data = { error: String(err) };
  }
  log(method, path, status, data);
  return { status, data, ok: status >= 200 && status < 300 };
}

function tryParseJson(text) {
  try { return JSON.parse(text); } catch { return text; }
}

async function uploadImage(kind, file) {
  const form = new FormData();
  form.append("file", file);
  const { data, ok } = await api("POST", `/api/uploads/${kind}-image`, form, { isForm: true });
  return ok ? data.path : null;
}

// ---------- tabs ----------
document.getElementById("tabs").addEventListener("click", (e) => {
  const btn = e.target.closest("button[data-tab]");
  if (!btn) return;
  document.querySelectorAll("#tabs button").forEach((b) => b.classList.toggle("active", b === btn));
  document.querySelectorAll(".tab").forEach((t) => t.classList.toggle("active", t.id === btn.dataset.tab));
  if (btn.dataset.tab === "tours") setTimeout(() => keypointMap.invalidateSize(), 0);
  if (btn.dataset.tab === "position") setTimeout(() => positionMap.invalidateSize(), 0);
});

// ---------- auth ----------
document.getElementById("register-form").addEventListener("submit", async (e) => {
  e.preventDefault();
  const f = new FormData(e.target);
  const { data, ok } = await api("POST", "/api/auth/register", Object.fromEntries(f), { auth: false });
  if (ok) session.set(data);
});

document.getElementById("login-form").addEventListener("submit", async (e) => {
  e.preventDefault();
  const f = new FormData(e.target);
  const { data, ok } = await api("POST", "/api/auth/login", Object.fromEntries(f), { auth: false });
  if (ok) session.set(data);
});

document.getElementById("logout-btn").addEventListener("click", () => session.clear());

// ---------- profile ----------
document.getElementById("load-profile-btn").addEventListener("click", async () => {
  const { data, ok } = await api("GET", `/api/users/${session.userId}`, undefined, { auth: false });
  if (!ok) return;
  document.getElementById("profile-view").innerHTML = `
    <div class="card">
      <strong>${data.firstName} ${data.lastName}</strong> (${data.role})<br/>
      ${data.biography ?? ""}<br/>
      <em>${data.motto ?? ""}</em>
      ${data.profileImagePath ? `<img src="${API}${data.profileImagePath}" />` : ""}
    </div>`;
  const form = document.getElementById("profile-form");
  form.firstName.value = data.firstName ?? "";
  form.lastName.value = data.lastName ?? "";
  form.biography.value = data.biography ?? "";
  form.motto.value = data.motto ?? "";
  form.profileImagePath.value = data.profileImagePath ?? "";
});

document.getElementById("profile-form").addEventListener("submit", async (e) => {
  e.preventDefault();
  const form = e.target;
  const fileInput = document.getElementById("profile-image-input");
  if (fileInput.files[0]) {
    const path = await uploadImage("profile", fileInput.files[0]);
    if (path) form.profileImagePath.value = path;
  }
  const body = Object.fromEntries(new FormData(form));
  await api("PUT", `/api/users/${session.userId}/profile`, body);
});

// ---------- blog ----------
document.getElementById("blog-form").addEventListener("submit", async (e) => {
  e.preventDefault();
  const form = e.target;
  const files = document.getElementById("blog-image-input").files;
  const imagePaths = [];
  for (const file of files) {
    const path = await uploadImage("blog", file);
    if (path) imagePaths.push(path);
  }
  const body = { ...Object.fromEntries(new FormData(form)), imagePaths };
  const { ok } = await api("POST", "/api/blogs", body);
  if (ok) form.reset();
});

document.getElementById("load-feed-btn").addEventListener("click", async () => {
  const { data, ok } = await api("GET", "/api/blogs/feed");
  if (!ok) return;
  document.getElementById("feed-view").innerHTML = data.map((b) => `
    <div class="card">
      <strong>${b.title}</strong> by ${b.authorId}<br/>
      ${b.description}<br/>
      ${(b.imagePaths || []).map((p) => `<img src="${API}${p}" />`).join("")}
      <div class="comments" data-blog-id="${b.id}"></div>
      <form class="comment-form" data-blog-id="${b.id}">
        <input name="text" placeholder="comment" required />
        <button type="submit">Comment</button>
      </form>
    </div>`).join("") || "<p>No blogs (follow someone first).</p>";
  data.forEach((b) => loadComments(b.id));
});

async function loadComments(blogId) {
  const { data, ok } = await api("GET", `/api/blogs/${blogId}/comments`, undefined, { auth: false });
  if (!ok) return;
  const container = document.querySelector(`.comments[data-blog-id="${blogId}"]`);
  if (container) {
    container.innerHTML = data.map((c) => `<div>💬 <em>${c.authorId}</em>: ${c.text}</div>`).join("") || "<div><em>No comments yet.</em></div>";
  }
}

document.getElementById("feed-view").addEventListener("submit", async (e) => {
  if (!e.target.classList.contains("comment-form")) return;
  e.preventDefault();
  const blogId = e.target.dataset.blogId;
  const body = Object.fromEntries(new FormData(e.target));
  const { ok } = await api("POST", `/api/blogs/${blogId}/comments`, body);
  if (ok) {
    e.target.reset();
    await loadComments(blogId);
  }
});

document.getElementById("follow-form").addEventListener("submit", async (e) => {
  e.preventDefault();
  const body = Object.fromEntries(new FormData(e.target));
  await api("POST", "/api/follows", body);
});

document.getElementById("unfollow-btn").addEventListener("click", async () => {
  const followeeId = document.querySelector('#follow-form [name=followeeId]').value;
  await api("DELETE", `/api/follows?followeeId=${encodeURIComponent(followeeId)}`);
});

document.getElementById("load-recs-btn").addEventListener("click", async () => {
  const { data, ok } = await api("GET", `/api/follows/${session.userId}/recommendations`, undefined, { auth: false });
  if (ok) document.getElementById("recs-view").innerHTML = `<pre>${JSON.stringify(data, null, 2)}</pre>`;
});

// ---------- tours (guide) ----------
document.getElementById("tour-form").addEventListener("submit", async (e) => {
  e.preventDefault();
  const form = e.target;
  const raw = Object.fromEntries(new FormData(form));
  const body = { ...raw, tags: raw.tags ? raw.tags.split(",").map((t) => t.trim()) : [] };
  const { data, ok } = await api("POST", "/api/tours", body);
  if (ok) {
    form.reset();
    // Avoid having to copy-paste the id by hand for the very next step.
    document.getElementById("keypoint-tour-id").value = data.id;
  }
});

document.getElementById("load-my-tours-btn").addEventListener("click", async () => {
  const { data, ok } = await api("GET", "/api/tours/mine");
  if (!ok) return;
  document.getElementById("my-tours-view").innerHTML = data.map((t) => `
    <div class="card">
      <strong>${t.name}</strong> — ${t.status} — id: <code>${t.id}</code><br/>
      ${t.description}<br/>
      <button data-use="${t.id}">Use this tour id</button>
      <button data-publish="${t.id}">Publish</button>
      <button data-archive="${t.id}">Archive</button>
    </div>`).join("") || "<p>No tours yet.</p>";
});

document.getElementById("my-tours-view").addEventListener("click", async (e) => {
  const useId = e.target.dataset.use;
  const publishId = e.target.dataset.publish;
  const archiveId = e.target.dataset.archive;
  if (useId) {
    document.getElementById("keypoint-tour-id").value = useId;
    document.getElementById("browse-tour-id").value = useId;
    document.getElementById("exec-tour-id").value = useId;
  }
  if (publishId) await api("PUT", `/api/tours/${publishId}/status`, { status: "Published" });
  if (archiveId) await api("PUT", `/api/tours/${archiveId}/status`, { status: "Archived" });
});

let pendingLatLng = null;
const keypointMap = L.map("keypoint-map").setView([45.2671, 19.8335], 13);
L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", { attribution: "OSM" }).addTo(keypointMap);
let keypointMarkers = [];
keypointMap.on("click", (e) => {
  pendingLatLng = e.latlng;
  document.getElementById("kp-lat").textContent = e.latlng.lat.toFixed(6);
  document.getElementById("kp-lng").textContent = e.latlng.lng.toFixed(6);
  L.marker(e.latlng).addTo(keypointMap);
});

document.getElementById("keypoint-form").addEventListener("submit", async (e) => {
  e.preventDefault();
  if (!pendingLatLng) { alert("Click the map first to set a location."); return; }
  const tourId = document.getElementById("keypoint-tour-id").value;
  const form = e.target;
  const fileInput = document.getElementById("keypoint-image-input");
  if (fileInput.files[0]) {
    const path = await uploadImage("keypoint", fileInput.files[0]);
    if (path) form.imagePath.value = path;
  }
  const raw = Object.fromEntries(new FormData(form));
  const body = { ...raw, latitude: pendingLatLng.lat, longitude: pendingLatLng.lng };
  const { ok } = await api("POST", `/api/tours/${tourId}/keypoints`, body);
  if (ok) form.reset();
});

document.getElementById("load-keypoints-btn").addEventListener("click", async () => {
  const tourId = document.getElementById("keypoint-tour-id").value;
  const { data, ok } = await api("GET", `/api/tours/${tourId}/keypoints`, undefined, { auth: !!session.token });
  if (!ok) return;
  keypointMarkers.forEach((m) => keypointMap.removeLayer(m));
  keypointMarkers = data.map((k) => L.marker([k.latitude, k.longitude]).addTo(keypointMap).bindPopup(`${k.type}: ${k.name}`));
  document.getElementById("keypoints-view").innerHTML = data.map((k) => `
    <div class="card">${k.type}: <strong>${k.name}</strong> — ${k.description}
    ${k.imagePath ? `<img src="${API}${k.imagePath}" />` : ""}</div>`).join("");
});

// ---------- browse & cart ----------
let lastBrowsedTour = null;
document.getElementById("browse-tour-btn").addEventListener("click", async () => {
  const tourId = document.getElementById("browse-tour-id").value;
  const { data, ok } = await api("GET", `/api/tours/${tourId}`, undefined, { auth: false });
  const kp = await api("GET", `/api/tours/${tourId}/keypoints`, undefined, { auth: !!session.token });
  if (!ok) return;
  lastBrowsedTour = data;
  document.getElementById("browse-tour-view").innerHTML = `
    <div class="card">
      <strong>${data.name}</strong> — ${data.status} — difficulty: ${data.difficulty}<br/>
      ${data.description}<br/>
      Visible key points: ${kp.ok ? kp.data.map((k) => k.type).join(", ") : "n/a"}
    </div>`;
  const cartForm = document.getElementById("cart-add-form");
  cartForm.tourId.value = data.id;
  cartForm.tourName.value = data.name;
});

document.getElementById("cart-add-form").addEventListener("submit", async (e) => {
  e.preventDefault();
  const body = Object.fromEntries(new FormData(e.target));
  body.price = Number(body.price);
  await api("POST", `/api/cart/${session.userId}/items`, body);
});

document.getElementById("load-cart-btn").addEventListener("click", async () => {
  const { data, ok } = await api("GET", `/api/cart/${session.userId}`);
  if (!ok) return;
  document.getElementById("cart-view").innerHTML = `
    <p>Total: ${data.totalPrice}</p>
    ${data.items.map((i) => `
      <div class="card">${i.tourName} — ${i.price}
        <button data-remove="${i.tourId}">Remove</button>
      </div>`).join("") || "<p>Cart is empty.</p>"}`;
});

document.getElementById("cart-view").addEventListener("click", async (e) => {
  const tourId = e.target.dataset.remove;
  if (!tourId) return;
  await api("DELETE", `/api/cart/${session.userId}/items/${tourId}`);
});

document.getElementById("checkout-btn").addEventListener("click", async () => {
  const { data, ok } = await api("POST", `/api/cart/${session.userId}/checkout`);
  if (ok) document.getElementById("checkout-view").innerHTML = `<pre>${JSON.stringify(data, null, 2)}</pre>`;
});

document.getElementById("load-purchases-btn").addEventListener("click", async () => {
  const { data, ok } = await api("GET", `/api/purchases/${session.userId}`);
  if (ok) document.getElementById("purchases-view").innerHTML = `<pre>${JSON.stringify(data, null, 2)}</pre>`;
});

// ---------- position simulator ----------
let lastPosition = null;
const positionMap = L.map("position-map").setView([45.2671, 19.8335], 13);
L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", { attribution: "OSM" }).addTo(positionMap);
let positionMarker = null;
positionMap.on("click", async (e) => {
  lastPosition = e.latlng;
  if (positionMarker) positionMap.removeLayer(positionMarker);
  positionMarker = L.marker(e.latlng).addTo(positionMap);
  await api("PUT", `/api/positions/${session.userId}`, { latitude: e.latlng.lat, longitude: e.latlng.lng });
});

document.getElementById("load-position-btn").addEventListener("click", async () => {
  const { data, ok } = await api("GET", `/api/positions/${session.userId}`);
  if (ok) document.getElementById("position-view").innerHTML = `<pre>${JSON.stringify(data, null, 2)}</pre>`;
});

// ---------- tour execution ----------
let lastExecutionId = null;
document.getElementById("start-exec-btn").addEventListener("click", async () => {
  const tourId = document.getElementById("exec-tour-id").value;
  const { data, ok } = await api("POST", "/api/executions", { tourId });
  if (ok) {
    lastExecutionId = data.id;
    document.getElementById("exec-id").value = data.id;
  }
  document.getElementById("exec-view").innerHTML = `<pre>${JSON.stringify(data, null, 2)}</pre>`;
});

document.getElementById("check-progress-btn").addEventListener("click", async () => {
  const id = document.getElementById("exec-id").value;
  if (!lastPosition) { alert("Click the Position Simulator map first."); return; }
  const { data } = await api("POST", `/api/executions/${id}/check-progress`, {
    latitude: lastPosition.lat, longitude: lastPosition.lng,
  });
  document.getElementById("exec-action-view").innerHTML = `<pre>${JSON.stringify(data, null, 2)}</pre>`;
});

document.getElementById("complete-exec-btn").addEventListener("click", async () => {
  const id = document.getElementById("exec-id").value;
  const { data } = await api("POST", `/api/executions/${id}/complete`);
  document.getElementById("exec-action-view").innerHTML = `<pre>${JSON.stringify(data, null, 2)}</pre>`;
});

document.getElementById("abandon-exec-btn").addEventListener("click", async () => {
  const id = document.getElementById("exec-id").value;
  const { data } = await api("POST", `/api/executions/${id}/abandon`);
  document.getElementById("exec-action-view").innerHTML = `<pre>${JSON.stringify(data, null, 2)}</pre>`;
});

renderSession();
