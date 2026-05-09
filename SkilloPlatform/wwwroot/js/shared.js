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
    throw new Error('ÐÐµ Ð¼Ð¾Ð¶Ðµ Ð´Ð° ÑÐµ ÑÐ²ÑŠÑ€Ð¶Ðµ ÑÑŠÑ ÑÑŠÑ€Ð²ÑŠÑ€Ð°. Ð£Ð²ÐµÑ€Ð¸ ÑÐµ Ñ‡Ðµ dotnet run Ð²ÑŠÑ€Ð²Ð¸.');
  }
  if (res.status === 204) return null;
  const data = await res.json().catch(() => ({}));
  if (!res.ok) throw new Error(data.message || `Ð“Ñ€ÐµÑˆÐºÐ° ${res.status}`);
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
      // Ð“Ð¾ÑÑ‚ â€” Ð±ÐµÐ· Ð¢Ð°Ð±Ð»Ð¾, ÐŸÑ€Ð¾Ñ„Ð¸Ð», Ð£ÑÐ»ÑƒÐ³Ð¸
      navLinks.innerHTML =
        a('/index.html','ÐÐ°Ñ‡Ð°Ð»Ð¾') +
        a('/pages/freelancers.html','Ð¤Ñ€Ð¸Ð¹Ð»Ð°Ð½ÑÑŠÑ€Ð¸') +
        a('/pages/projects.html','ÐŸÑ€Ð¾ÐµÐºÑ‚Ð¸') +
        a('/pages/how-it-works.html','ÐšÐ°Ðº Ñ€Ð°Ð±Ð¾Ñ‚Ð¸');
    } else if (u.role === 'Freelancer') {
      navLinks.innerHTML =
        a('/index.html','ÐÐ°Ñ‡Ð°Ð»Ð¾') +
        a('/pages/projects.html','ÐŸÑ€Ð¾ÐµÐºÑ‚Ð¸') +
        a('/pages/my-bids.html','ÐœÐ¾Ð¸ Ð¾Ñ„ÐµÑ€Ñ‚Ð¸') +
        a('/pages/dashboard.html','Ð¢Ð°Ð±Ð»Ð¾') +
        a('/pages/profile.html','ÐŸÑ€Ð¾Ñ„Ð¸Ð»') +
        a('/pages/services.html','Ð£ÑÐ»ÑƒÐ³Ð¸');
    } else if (u.role === 'Client') {
      navLinks.innerHTML =
        a('/index.html','ÐÐ°Ñ‡Ð°Ð»Ð¾') +
        a('/pages/freelancers.html','Ð¤Ñ€Ð¸Ð¹Ð»Ð°Ð½ÑÑŠÑ€Ð¸') +
        a('/pages/browse-services.html','ÐžÑ„ÐµÑ€Ñ‚Ð¸') +
        a('/pages/projects.html','ÐŸÑ€Ð¾ÐµÐºÑ‚Ð¸') +
        a('/pages/dashboard.html','Ð¢Ð°Ð±Ð»Ð¾');
    } else if (Auth.isAdmin()) {
      navLinks.innerHTML =
        a('/index.html','ÐÐ°Ñ‡Ð°Ð»Ð¾') +
        a('/pages/freelancers.html','Ð¤Ñ€Ð¸Ð¹Ð»Ð°Ð½ÑÑŠÑ€Ð¸') +
        a('/pages/projects.html','ÐŸÑ€Ð¾ÐµÐºÑ‚Ð¸') +
        a('/pages/admin.html','âš™ ÐÐ´Ð¼Ð¸Ð½Ð¸ÑÑ‚Ñ€Ð°Ñ†Ð¸Ñ');
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
    if (chatBtn) chatBtn.style.display = u ? '' : 'none';
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
      tag.innerHTML = `${s}<button type="button" onclick="removeSkillTag('${wrapperId}','${hiddenId}','${s.replace(/'/g,"\\'")}')">Ã—</button>`;
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


