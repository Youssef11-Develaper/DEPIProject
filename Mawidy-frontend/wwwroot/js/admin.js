function toggleSidebar() {
    document.getElementById('sidebar').classList.toggle('collapsed');
    document.getElementById('adminMain').classList.toggle('full');
}

document.addEventListener('DOMContentLoaded', () => {
    const el = document.getElementById('topbarDate');
    if (el) {
        el.textContent = new Date().toLocaleDateString('ar-EG', {
            weekday: 'long', year: 'numeric', month: 'long', day: 'numeric'
        });
    }
    const toast = document.getElementById('adminToast');
    if (toast) setTimeout(() => toast.classList.remove('show'), 4000);

    document.querySelectorAll('.bar-fill,.trend-fill,.svc-bar,.gov-stat-bar,.op-bar')
        .forEach(el => {
            const w = el.style.width;
            const h = el.style.height;
            el.style.width = '0'; el.style.height = '0';
            setTimeout(() => { el.style.width = w; el.style.height = h; }, 200);
        });
});

function showAdminToast(msg) {
    let t = document.getElementById('adminToast');
    if (!t) {
        t = document.createElement('div');
        t.id = 'adminToast';
        t.className = 'admin-toast';
        t.innerHTML = `<span id="adminToastMsg"></span><button onclick="this.parentElement.classList.remove('show')">✕</button>`;
        document.body.appendChild(t);
    }
    const msgEl = document.getElementById('adminToastMsg');
    if (msgEl) msgEl.textContent = msg;
    t.classList.add('show');
    clearTimeout(t._timer);
    t._timer = setTimeout(() => t.classList.remove('show'), 4000);
}
