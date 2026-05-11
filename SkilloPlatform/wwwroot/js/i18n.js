// ═══════════════════════════════════════════════════
//  Skillo i18n — Bulgarian / English
//  Works by replacing text content on the page
// ═══════════════════════════════════════════════════

const I18n = {
  lang: localStorage.getItem('sk_lang') || 'bg',

  // All translatable strings
  strings: {
    bg: {
      // Navigation
      'Начало': 'Начало',
      'Фрийлансъри': 'Фрийлансъри',
      'Проекти': 'Проекти',
      'Как работи': 'Как работи',
      'Мои оферти': 'Мои оферти',
      'Табло': 'Табло',
      'Профил': 'Профил',
      'Услуги': 'Услуги',
      'Оферти': 'Оферти',
      'Моите проекти': 'Моите проекти',
      'Съобщения': 'Съобщения',
      'Изход': 'Изход',
      'Вход': 'Вход',
      'Регистрация': 'Регистрация',
      // Common
      'Търси': 'Търси',
      'Зарежда се…': 'Зарежда се…',
      'Изчисти': 'Изчисти',
      'Запази': 'Запази',
      'Откажи': 'Откажи',
      'Изтрий': 'Изтрий',
      'Редактирай': 'Редактирай',
      'Добави': 'Добави',
      'Виж профил': 'Виж профил',
      'Виж детайли': 'Виж детайли',
      '✓ Верифициран': '✓ Верифициран',
      // Freelancers page
      'Намери фрийлансър': 'Намери фрийлансър',
      'Проверени специалисти от цяла България, готови за твоя проект': 'Проверени специалисти от цяла България, готови за твоя проект',
      'Категория': 'Категория',
      'Ниво': 'Ниво',
      'Всички': 'Всички',
      'Макс. цена (€/час)': 'Макс. цена (€/час)',
      // Projects page
      'Активни проекти': 'Активни проекти',
      'Намери проект за твоите умения и кандидатствай сега': 'Намери проект за твоите умения и кандидатствай сега',
      '+ Публикувай проект': '+ Публикувай проект',
      'Бюджет': 'Бюджет',
      'Кандидатствай': 'Кандидатствай',
      // Chat
      '💬 Съобщения': '💬 Съобщения',
      'Нов разговор': 'Нов разговор',
      'Напиши съобщение…': 'Напиши съобщение…',
      'Изпрати': 'Изпрати',
      // Footer
      'За нас': 'За нас',
      'Контакти': 'Контакти',
      'Поверителност': 'Поверителност',
      'Условия за ползване': 'Условия за ползване',
      // Buttons
      'Виж профил →': 'Виж профил →',
      '💬 Изпрати съобщение': '💬 Изпрати съобщение',
    },
    en: {
      // Navigation
      'Начало': 'Home',
      'Фрийлансъри': 'Freelancers',
      'Проекти': 'Projects',
      'Как работи': 'How it works',
      'Мои оферти': 'My Bids',
      'Табло': 'Dashboard',
      'Профил': 'Profile',
      'Услуги': 'Services',
      'Оферти': 'Offers',
      'Моите проекти': 'My Projects',
      'Съобщения': 'Messages',
      'Изход': 'Logout',
      'Вход': 'Login',
      'Регистрация': 'Register',
      // Common
      'Търси': 'Search',
      'Зарежда се…': 'Loading…',
      'Изчисти': 'Clear',
      'Запази': 'Save',
      'Откажи': 'Cancel',
      'Изтрий': 'Delete',
      'Редактирай': 'Edit',
      'Добави': 'Add',
      'Виж профил': 'View profile',
      'Виж детайли': 'View details',
      '✓ Верифициран': '✓ Verified',
      // Freelancers page
      'Намери фрийлансър': 'Find a freelancer',
      'Проверени специалисти от цяла България, готови за твоя проект': 'Verified specialists from Bulgaria, ready for your project',
      'Категория': 'Category',
      'Ниво': 'Level',
      'Всички': 'All',
      'Макс. цена (€/час)': 'Max price (€/hr)',
      // Projects page
      'Активни проекти': 'Active projects',
      'Намери проект за твоите умения и кандидатствай сега': 'Find a project that matches your skills',
      '+ Публикувай проект': '+ Post project',
      'Бюджет': 'Budget',
      'Кандидатствай': 'Apply',
      // Chat
      '💬 Съобщения': '💬 Messages',
      'Нов разговор': 'New conversation',
      'Напиши съобщение…': 'Type a message…',
      'Изпрати': 'Send',
      // Footer
      'За нас': 'About us',
      'Контакти': 'Contact',
      'Поверителност': 'Privacy',
      'Условия за ползване': 'Terms of use',
      // Buttons
      'Виж профил →': 'View profile →',
      '💬 Изпрати съобщение': '💬 Send message',
    }
  },

  t(key) {
    return this.strings[this.lang]?.[key] || key;
  },

  // Apply translations by walking all text nodes
  applyToPage() {
    if (this.lang === 'bg') return; // BG is default, no changes needed
    const dict = this.strings[this.lang];
    if (!dict) return;

    // Translate elements with data-i18n
    document.querySelectorAll('[data-i18n]').forEach(el => {
      const key = el.getAttribute('data-i18n');
      if (dict[key]) el.textContent = dict[key];
    });

    // Translate nav links
    document.querySelectorAll('.nav-links a').forEach(el => {
      const txt = el.textContent.trim();
      if (dict[txt]) el.textContent = dict[txt];
    });

    // Translate buttons and labels by text content
    const selectors = ['button', 'a.btn', 'label', 'h1', 'h2', 'h3', '.page-hero h1', '.page-hero p'];
    selectors.forEach(sel => {
      document.querySelectorAll(sel).forEach(el => {
        // Only translate leaf nodes (no child elements except spans)
        const childElements = [...el.children].filter(c => c.tagName !== 'SPAN' && c.tagName !== 'DIV');
        if (childElements.length === 0) {
          const txt = el.textContent.trim();
          if (dict[txt]) el.textContent = dict[txt];
        }
      });
    });

    // Translate placeholders
    document.querySelectorAll('input[placeholder], textarea[placeholder]').forEach(el => {
      const ph = el.placeholder;
      if (dict[ph]) el.placeholder = dict[ph];
    });

    // Update page title
    document.documentElement.lang = this.lang;
  },

  setLang(newLang) {
    this.lang = newLang;
    localStorage.setItem('sk_lang', newLang);
    this.applyToPage();
    this.updateAllSwitchers();
  },

  toggle() {
    this.setLang(this.lang === 'bg' ? 'en' : 'bg');
  },

  updateAllSwitchers() {
    document.querySelectorAll('.lang-switcher-btn').forEach(btn => {
      btn.textContent = this.lang === 'bg' ? '🇬🇧 EN' : '🇧🇬 БГ';
    });
  },

  buildSwitcher(containerId) {
    const el = document.getElementById(containerId);
    if (!el) return;
    const btn = document.createElement('button');
    btn.className = 'lang-switcher-btn';
    btn.textContent = this.lang === 'bg' ? '🇬🇧 EN' : '🇧🇬 БГ';
    btn.style.cssText = [
      'background:rgba(27,95,168,.08)',
      'border:2px solid var(--g200)',
      'border-radius:20px',
      'padding:5px 14px',
      'cursor:pointer',
      'font-family:DM Sans,sans-serif',
      'font-weight:700',
      'font-size:.82rem',
      'color:var(--g700)',
      'transition:.18s',
      'white-space:nowrap',
    ].join(';');
    btn.addEventListener('mouseover', () => btn.style.borderColor = 'var(--blue)');
    btn.addEventListener('mouseout',  () => btn.style.borderColor = 'var(--g200)');
    btn.addEventListener('click',     () => I18n.toggle());
    el.innerHTML = '';
    el.appendChild(btn);
  }
};

document.addEventListener('DOMContentLoaded', () => {
  // Build switcher in nav
  I18n.buildSwitcher('lang-switcher');
  // Apply translations (only if EN is selected)
  if (I18n.lang === 'en') {
    // Small delay to let page render first
    setTimeout(() => I18n.applyToPage(), 100);
  }
});
