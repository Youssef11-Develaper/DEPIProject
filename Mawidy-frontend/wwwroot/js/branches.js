// ══ OPERATOR LOGO HELPER ══════════════════════════════════════════════════════
function opLogoClass(key) {
    const map = { vodafone:"op-logo-vodafone", orange:"op-logo-orange", etisalat:"op-logo-etisalat", we:"op-logo-we" };
    return map[key] || "";
}

let curOp     = 'all';
let actFT     = [];
let vqEntryId = null;

// ══ FILTER ════════════════════════════════════════════════════════════════════
function applyFilter() {
    const q   = (document.getElementById('srch')?.value || '').trim().toLowerCase();
    const geo = (window.getGeoFilter && window.getGeoFilter()) || {};
    const cards = document.querySelectorAll('.bcard');
    let visible = 0;

    cards.forEach(card => {
        const op     = card.dataset.op;
        const status = card.dataset.status;
        const svcs   = (card.dataset.services || '').split(',');
        const govId  = parseInt(card.dataset.gov);
        const distId = parseInt(card.dataset.dist);
        const name   = (card.querySelector('.bcname')?.textContent || '').toLowerCase();
        const addr   = (card.querySelector('.bcaddr')?.textContent || '').toLowerCase();

        let show = true;
        if (curOp !== 'all' && op !== curOp)             show = false;
        if (actFT.includes('open')   && status !== 'open')   show = false;
        if (actFT.includes('busy')   && status !== 'busy')   show = false;
        if (actFT.includes('closed') && status !== 'closed') show = false;
        if (actFT.includes('new')  && !svcs.includes('new'))  show = false;
        if (actFT.includes('sim')  && !svcs.includes('sim'))  show = false;
        if (actFT.includes('pkg')  && !svcs.includes('pkg'))  show = false;
        if (actFT.includes('bill') && !svcs.includes('bill')) show = false;
        if (geo.govId  && govId  !== geo.govId)          show = false;
        if (geo.distId && distId !== geo.distId)         show = false;
        if (q && !name.includes(q) && !addr.includes(q)) show = false;

        card.style.display = show ? '' : 'none';
        if (show) visible++;
    });

    document.getElementById('resc').textContent = `يُعرض ${visible} من ${typeof TOTAL !== 'undefined' ? TOTAL : ALL_BRANCHES.length} فرع`;
    document.getElementById('empty')?.classList.toggle('show', visible === 0);
}

function filterOp(k, btn) {
    curOp = k;
    document.querySelectorAll('.optab').forEach(t => t.classList.remove('on'));
    btn.classList.add('on');
    applyFilter();
}

function toggleFtags() {
    document.getElementById('ftags').classList.toggle('open');
    document.getElementById('ftbtn').classList.toggle('on');
}

function togF(btn, k) {
    btn.classList.toggle('on');
    actFT.includes(k) ? (actFT = actFT.filter(f => f !== k)) : actFT.push(k);
    applyFilter();
}

function clearAll() {
    curOp = 'all'; actFT = [];
    document.querySelectorAll('.optab').forEach((t, i) => t.classList.toggle('on', i === 0));
    document.querySelectorAll('.ftag').forEach(t => t.classList.remove('on'));
    if (document.getElementById('srch')) document.getElementById('srch').value = '';
    if (window.clearGov) clearGov();
    applyFilter();
}

function setView(m, btn) {
    document.querySelectorAll('.vb').forEach(b => b.classList.remove('on'));
    btn.classList.add('on');
    document.getElementById('grid').classList.toggle('lv', m === 'list');
}

// ══ MODAL ═════════════════════════════════════════════════════════════════════
async function openBranchDetail(id) {
    const b = ALL_BRANCHES.find(x => x.id === id);
    if (!b) return;
    buildModalHeader(b);
    buildModalTabs(b);
    buildModalFooter(b);
    document.getElementById('modal').classList.add('show');
    document.body.style.overflow = 'hidden';
    loadServicesTab(id);
}

function buildModalHeader(b) {
    const sl = { open: 'متاح', busy: 'مزدحم', closed: 'مغلق' }[b.status] || '';
    document.getElementById('mhdr').innerHTML = `
        <div class="mopb" style="background:${b.opBg};color:${b.opColor};display:flex;align-items:center;gap:8px"><div class="oplogo ${opLogoClass(b.op)}" style="width:22px;height:22px;border-radius:5px;flex-shrink:0"></div>${b.opName}</div>
        <div style="display:flex;align-items:center;gap:8px;flex-wrap:wrap;margin-bottom:6px">
            <div class="mbn">${b.shortName}</div>
            <div class="gov-badge">${b.govEmoji} ${b.govName}</div>
        </div>
        <div class="madr">📍 ${b.distName} — ${b.addr}</div>
        <div style="display:flex;align-items:center;gap:9px;margin-top:9px">
            <div class="sp ${b.status}">${sl}</div>
            <span style="font-size:11px;color:var(--muted)">⭐ ${b.rating}</span>
        </div>
        <div class="minrow">
            <div class="mch">📍 ${b.dist} كم</div>
            <div class="mch">🔢 ${b.queue} طابور</div>
            <div class="mch">⏱ ${b.wait}</div>
        </div>`;
}

function buildModalTabs(b) {
    document.getElementById('mtabs').innerHTML = `
        <button class="mtab on" onclick="switchTab('svc',this,${b.id})">🛠️ الخدمات والأوراق</button>
        <button class="mtab"    onclick="switchTab('queue',this,${b.id})">🔢 الطابور</button>
        <button class="mtab"    onclick="switchTab('hours',this,${b.id})">🕐 المواعيد</button>
        <button class="mtab"    onclick="switchTab('rating',this,${b.id})">⭐ التقييم</button>`;
}

function buildModalFooter(b) {
    document.getElementById('mfoot').innerHTML = `
        <div style="display:flex;gap:9px">
            <button class="bkfull" style="flex:1" onclick="joinVirtualQueue(${b.id})">🎫 طابور افتراضي</button>
            <a href="${BOOK_URL}/${b.id}" class="bkfull" style="flex:1;text-decoration:none;text-align:center">📅 احجز موعد</a>
        </div>`;
}

function switchTab(tab, btn, branchId) {
    document.querySelectorAll('.mtab').forEach(t => t.classList.remove('on'));
    btn.classList.add('on');
    if (tab === 'svc')    loadServicesTab(branchId);
    if (tab === 'queue')  loadQueueTab(branchId);
    if (tab === 'hours')  loadHoursTab();
    if (tab === 'rating') loadRatingTab(branchId);
}

async function loadServicesTab(branchId) {
    const b = ALL_BRANCHES.find(x => x.id === branchId);
    document.getElementById('mbody').innerHTML = `
        <div style="font-size:12px;color:var(--muted);margin-bottom:12px;background:var(--pl);padding:9px 12px;border-radius:9px">
            👇 اختر الخدمة لعرض المستندات المطلوبة
        </div>
        <div class="svgrid" id="svcGrid">
            ${(b?.services || []).map(s => `
                <div class="svitem" onclick="loadDocs('${s.key}',this)">
                    <div class="svi">${s.icon}</div>
                    <div><div class="svn">${s.name}</div></div>
                </div>`).join('')}
        </div>
        <div id="docsArea"></div>`;
}

async function loadDocs(serviceKey, el) {
    document.querySelectorAll('.svitem').forEach(i => i.classList.remove('sel'));
    el.classList.add('sel');
    const res  = await fetch(`${DOCS_URL}?serviceKey=${serviceKey}`);
    const docs = await res.json();
    document.getElementById('docsArea').innerHTML = `
        <div class="docsec">
            <div class="doctitle">📋 المستندات المطلوبة</div>
            ${docs.map(d => `
                <div class="docitem">
                    <div class="${d.docType === 0 ? 'dcheck' : 'dopt'}">${d.docType === 0 ? '✓' : '★'}</div>
                    <div>
                        <div class="dtxt">${d.docType === 1 ? '<span class="doptag">اختياري</span>' : ''} ${d.textAr}</div>
                        ${d.noteAr ? `<div class="dnote">${d.noteAr}</div>` : ''}
                    </div>
                </div>`).join('')}
            <div style="margin-top:10px;padding:8px 12px;background:#f0f4ff;border-radius:8px;font-size:11px;color:var(--muted)">
                ✅ = إلزامي &nbsp;·&nbsp; ★ = اختياري
            </div>
        </div>`;
}

function loadQueueTab(branchId) {
    const b = ALL_BRANCHES.find(x => x.id === branchId);
    document.getElementById('mbody').innerHTML = `
        <div class="qbox">
            <div class="qnum">${b?.queue ?? 0}</div>
            <div class="qlbl">شخص في الطابور الآن</div>
            <div class="qwt">⏱ وقت الانتظار: ${b?.wait ?? '—'}</div>
        </div>`;
}

function loadHoursTab() {
    const today = new Date().getDay();
    const hours = [
        { day: 'الأحد',    time: '9:00 ص — 9:00 م' },
        { day: 'الاثنين',  time: '9:00 ص — 9:00 م' },
        { day: 'الثلاثاء', time: '9:00 ص — 9:00 م' },
        { day: 'الأربعاء', time: '9:00 ص — 9:00 م' },
        { day: 'الخميس',   time: '9:00 ص — 5:00 م' },
        { day: 'الجمعة',   time: 'مغلق', closed: true },
        { day: 'السبت',    time: '9:00 ص — 9:00 م' },
    ];
    document.getElementById('mbody').innerHTML = hours.map((h, i) => `
        <div class="hrow${today === i ? ' td' : ''}">
            <span class="hday${today === i ? ' t' : ''}">${h.day}${today === i ? ' (اليوم)' : ''}</span>
            <span class="${h.closed ? 'hclosed' : 'htime'}">${h.time}</span>
        </div>`).join('');
}

function loadRatingTab(branchId) {
    const b = ALL_BRANCHES.find(x => x.id === branchId);
    const r = b?.rating ?? 4.0;
    const aspects = ['سرعة الخدمة', 'كفاءة الموظفين', 'نظافة الفرع', 'وقت الانتظار'];
    const offsets  = [0, 0.1, 0.2, -0.3];
    document.getElementById('mbody').innerHTML = `
        <div style="text-align:center;margin-bottom:16px">
            <div style="font-family:'Cairo',sans-serif;font-size:52px;font-weight:900;color:var(--purple)">${r}</div>
            <div style="font-size:12px;color:var(--muted)">من 5</div>
        </div>
        ${aspects.map((a, i) => {
            const v     = Math.max(1, Math.min(5, +(r + offsets[i]).toFixed(1)));
            const stars = Array.from({ length: 5 }, (_, j) =>
                `<span class="${j < Math.round(v) ? 'sf' : 'se'}">★</span>`).join('');
            return `<div class="rrow">
                <div class="rasp">${a}</div>
                <div class="stars">${stars}</div>
                <div class="rv">${v}</div>
            </div>`;
        }).join('')}`;
}

function closeMod() {
    document.getElementById('modal').classList.remove('show');
    document.body.style.overflow = '';
}
document.getElementById('modal')?.addEventListener('click', e => {
    if (e.target.id === 'modal') closeMod();
});

// ══ VIRTUAL QUEUE ═════════════════════════════════════════════════════════════
function joinVirtualQueue(branchId) {
    const name  = prompt('اسمك الكريم؟'); if (!name)  return;
    const phone = prompt('رقم هاتفك؟');   if (!phone) return;
    fetch(JOIN_URL, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ branchId, customerName: name, customerPhone: phone, serviceKey: 'general' })
    }).then(r => r.json()).then(data => { closeMod(); showVirtualQueue(data); });
}

function showVirtualQueue(data) {
    document.getElementById('vqBranchName').textContent = data.branchName;
    document.getElementById('vqNum').textContent        = data.position;
    document.getElementById('vqStatusTxt').textContent  = `أنت رقم ${data.position} في الطابور`;
    document.getElementById('vqAheadTxt').textContent   = `${data.totalAhead} شخص قبلك`;
    document.getElementById('vqEtaNum').textContent     = data.estimatedWaitMinutes;
    const pct = Math.max(0, 1 - data.totalAhead / Math.max(1, data.position));
    document.getElementById('vqArc').style.strokeDashoffset = 289 - (289 * pct);
    vqEntryId = data.entryId;
    document.getElementById('vqOverlay').classList.add('show');
}

function leaveQueue() {
    if (!vqEntryId) return;
    fetch(`${LEAVE_URL}/${vqEntryId}`, { method: 'POST' });
    document.getElementById('vqOverlay').classList.remove('show');
    vqEntryId = null;
    showToast('👋', 'غادرت الطابور', 'يمكنك الانضمام مجدداً في أي وقت');
}
