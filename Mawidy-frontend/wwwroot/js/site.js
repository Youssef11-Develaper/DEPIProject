function showToast(icon, title, sub) {
    const t = document.getElementById('toast');
    if (!t) return;
    document.getElementById('ticon').textContent  = icon;
    document.getElementById('ttitle').textContent = title;
    document.getElementById('tsub').textContent   = sub;
    t.classList.add('show');
    setTimeout(() => t.classList.remove('show'), 4000);
}
function closeToast() {
    document.getElementById('toast')?.classList.remove('show');
}
