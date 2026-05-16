const API = 'http://localhost:5000/api';

const Auth = {
  token:       () => localStorage.getItem('sk_token'),
  user:        () => JSON.parse(localStorage.getItem('sk_user') || 'null'),
  loggedIn:    () => !!localStorage.getItem('sk_token'),
  save(data) {
    localStorage.setItem('sk_token', data.token);
    localStorage.setItem('sk_user', JSON.stringify({
      id: data.userId, name: data.fullName,
      email: data.email, role: data.role, avatar: data.avatar || ''
    }));
  },
  clear() {
    localStorage.removeItem('sk_token');
    localStorage.removeItem('sk_user');
  },
  isAdmin()      { const r = Auth.user()?.role; return r==='Admin'||r==='SuperAdmin'; },
  isSuperAdmin() { return Auth.user()?.role === 'SuperAdmin'; },
  isFreelancer() { return Auth.user()?.role === 'Freelancer'; },
  isClient()     { return Auth.user()?.role === 'Client'; },
};

async function apiFetch(path, opts = {}) {
  const headers = { 'Content-Type': 'application/json' };
  if (Auth.token()) headers['Authorization'] = `Bearer ${Auth.token()}`;
  if (opts.body instanceof FormData) delete headers['Content-Type'];
  let res;
  try {
    res = await fetch(`${API}${path}`, { ...opts, headers: { ...headers, ...opts.headers } });
  } catch {
    throw new Error('Не може да се свърже със сървъра. Увери се че dotnet run върви.');
  }
  if (res.status === 204) return null;
  const data = await res.json().catch(() => ({}));
  if (!res.ok) throw new Error(data.message || `Грешка ${res.status}`);
  return data;
}

function buildNav() {
  const u = Auth.user();
  const navLinks = document.querySelector('.nav-links');
  const loginBtn    = document.getElementById('nav-login-btn');
  const registerBtn = document.getElementById('nav-register-btn');
  const userBox     = document.getElementById('nav-user');
  const userNameEl  = document.getElementById('nav-user-name');
  const userAvatarEl= document.getElementById('nav-user-avatar');
  const adminLink   = document.getElementById('nav-admin-link');

  const currentPage = window.location.pathname.split('/').pop() || 'index.html';
  const a = (href, label) => {
    const page = href.split('/').pop();
    const active = page === currentPage ? ' class="active"' : '';
    return `<li><a href="${href}"${active}>${label}</a></li>`;
  };

  if (navLinks) {
    if (!u) {
      // Гост — без Табло, Профил, Услуги
      navLinks.innerHTML =
        a('/index.html','Начало') +
        a('/pages/freelancers.html','Фрийлансъри') +
        a('/pages/projects.html','Проекти') +
        a('/pages/how-it-works.html','Как работи');
    } else if (u.role === 'Freelancer') {
      navLinks.innerHTML =
        a('/index.html','Начало') +
        a('/pages/projects.html','Проекти') +
        a('/pages/my-bids.html','Мои оферти') +
        a('/pages/dashboard.html','Табло') +
        a('/pages/services.html','Услуги') +
        a('/pages/profile.html','Профил');
    } else if (u.role === 'Client') {
      navLinks.innerHTML =
        a('/index.html','Начало') +
        a('/pages/freelancers.html','Фрийлансъри') +
        a('/pages/browse-services.html','Оферти') +
        a('/pages/projects.html','Проекти') +
        a('/pages/dashboard.html','Табло');
    } else if (Auth.isAdmin()) {
      navLinks.innerHTML =
        a('/index.html','Начало') +
        a('/pages/freelancers.html','Фрийлансъри') +
        a('/pages/projects.html','Проекти') +
        a('/pages/admin.html','⚙ Администрация');
    }
  }

  if (u) {
    if (loginBtn)    loginBtn.style.display    = 'none';
    if (registerBtn) registerBtn.style.display = 'none';
    if (userBox)     userBox.style.display     = 'flex';
    if (userNameEl)  userNameEl.textContent    = u.name.split(' ')[0];
    if (userAvatarEl) {
      if (u.avatar) {
        userAvatarEl.innerHTML = `<img src="http://localhost:5000${u.avatar}" alt="" style="width:100%;height:100%;object-fit:cover;border-radius:50%">`;
      } else {
        userAvatarEl.textContent = u.name[0].toUpperCase();
      }
    }
    if (adminLink) adminLink.style.display = Auth.isAdmin() ? '' : 'none';
    const chatBtn = document.getElementById('nav-chat-btn');
    if (chatBtn) {
      chatBtn.style.display = u ? 'flex' : 'none';
      if (u) startChatBadgePolling();
    }
  } else {
    if (loginBtn)    loginBtn.style.display    = '';
    if (registerBtn) registerBtn.style.display = '';
    if (userBox)     userBox.style.display     = 'none';
    if (adminLink)   adminLink.style.display   = 'none';
  }
}

function logout() {
  Auth.clear();
  window.location.href = '/index.html';
}

function openModal(id)  { document.getElementById(id)?.classList.add('open'); }
function closeModal(id) { document.getElementById(id)?.classList.remove('open'); }

document.addEventListener('click', e => {
  if (e.target.classList.contains('modal-overlay')) e.target.classList.remove('open');
});
document.addEventListener('keydown', e => {
  if (e.key === 'Escape')
    document.querySelectorAll('.modal-overlay.open').forEach(m => m.classList.remove('open'));
});

function showAlert(id, msg, type = 'error') {
  const el = document.getElementById(id);
  if (!el) return;
  el.textContent = msg;
  el.className = `alert alert-${type} show`;
}
function hideAlert(id) {
  const el = document.getElementById(id);
  if (el) el.className = 'alert';
}

function initSkillsInput(wrapperId, inputId, hiddenId) {
  const wrapper = document.getElementById(wrapperId);
  const input   = document.getElementById(inputId);
  const hidden  = document.getElementById(hiddenId);
  if (!wrapper || !input) return;
  let skills = hidden?.value ? hidden.value.split(',').filter(Boolean) : [];

  function render() {
    wrapper.querySelectorAll('.skill-tag').forEach(t => t.remove());
    skills.forEach(s => {
      const tag = document.createElement('span');
      tag.className = 'skill-tag';
      tag.innerHTML = `${s}<button type="button" onclick="removeSkillTag('${wrapperId}','${hiddenId}','${s.replace(/'/g,"\\'")}')">×</button>`;
      wrapper.insertBefore(tag, input);
    });
    if (hidden) hidden.value = skills.join(',');
  }

  input.addEventListener('keydown', e => {
    if ((e.key==='Enter'||e.key===',') && input.value.trim()) {
      e.preventDefault();
      const s = input.value.trim().replace(',','');
      if (s && !skills.includes(s)) { skills.push(s); render(); }
      input.value = '';
    }
    if (e.key==='Backspace' && !input.value && skills.length) { skills.pop(); render(); }
  });

  render();
  return { getSkills: ()=>skills, setSkills: (arr)=>{ skills=[...arr]; render(); } };
}

window.removeSkillTag = (wrapperId, hiddenId, skill) => {
  const hidden = document.getElementById(hiddenId);
  if (!hidden) return;
  const skills = hidden.value.split(',').filter(s => s && s !== skill);
  hidden.value = skills.join(',');
  document.getElementById(wrapperId)?.querySelectorAll('.skill-tag').forEach(t => {
    if (t.childNodes[0]?.textContent?.trim() === skill) t.remove();
  });
};
window.removeSkill = window.removeSkillTag;

window.addEventListener('scroll', () => {
  document.getElementById('main-nav')?.classList.toggle('scrolled', scrollY > 10);
});

document.addEventListener('DOMContentLoaded', buildNav);

// ── Chat unread badge ──────────────────────────────────────────
// Simple approach: track last seen updatedAt per conversation
// Only show badge if last message was sent by the OTHER person

let _badgeTimer = null;

async function startChatBadgePolling() {
  if (_badgeTimer) return; // already running
  await updateChatBadge();
  _badgeTimer = setInterval(updateChatBadge, 10000);
}

async function updateChatBadge() {
  if (!Auth.loggedIn()) return;
  const badge = document.getElementById('chat-badge');
  if (!badge) return;

  try {
    const me = Auth.user();
    const myId = me?.id;
    const convs = await apiFetch('/chat/conversations');
    const arr = Array.isArray(convs) ? convs : [];

    const seenData = JSON.parse(localStorage.getItem('sk_seen_v2') || '{}');
    let unread = 0;

    for (const conv of arr) {
      if (!conv.lastMessage) continue;

      const convKey = 'c' + conv.id;
      const lastMsg = conv.lastMessage;
      
      // Only count if last message is from OTHER person
      if (!lastMsg || lastMsg.senderId === myId) continue;

      const lastTime = new Date(lastMsg.createdAt || conv.updatedAt || 0).getTime();
      const seenTime = seenData[convKey] || 0;

      if (lastTime > seenTime) {
        unread++;
      }
    }

    if (unread > 0) {
      badge.textContent = unread > 9 ? '9+' : String(unread);
      badge.style.display = 'flex';
    } else {
      badge.style.display = 'none';
    }
  } catch(e) {
    // Silently fail
  }
}

function markChatSeen() {
  try {
    const seenData = JSON.parse(localStorage.getItem('sk_seen_v2') || '{}');
    apiFetch('/chat/conversations').then(convs => {
      const arr = Array.isArray(convs) ? convs : [];
      const now = Date.now();
      arr.forEach(conv => { seenData['c' + conv.id] = now; });
      localStorage.setItem('sk_seen_v2', JSON.stringify(seenData));
    }).catch(() => {});
  } catch {}
  const badge = document.getElementById('chat-badge');
  if (badge) badge.style.display = 'none';
}

function markConvSeen(convId) {
  try {
    const seenData = JSON.parse(localStorage.getItem('sk_seen_v2') || '{}');
    seenData['c' + convId] = Date.now();
    localStorage.setItem('sk_seen_v2', JSON.stringify(seenData));
  } catch {}
  updateChatBadge();
}
