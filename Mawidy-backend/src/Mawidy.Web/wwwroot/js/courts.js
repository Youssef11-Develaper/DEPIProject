// ══ SCROLL ══
function scrollSec(id) { document.querySelector(id)?.scrollIntoView({behavior:'smooth'}); }

// ══ FADE IN ══
const obs = new IntersectionObserver(e=>{ e.forEach(x=>{ if(x.isIntersecting) x.target.classList.add('visible'); }); },{threshold:.1});
document.querySelectorAll('.fade-in').forEach(el=>obs.observe(el));

// ══ DATE MIN ══
document.getElementById('bk-date').min = new Date().toISOString().split('T')[0];

// ══ COURTS DATA ══
// courtsData is injected from C# via @Html.Raw in Index.cshtml

let currentFilter = 'الكل';
let searchQuery = '';

async function renderCourts() {
  const response = await fetch(`/Courts/Filter?type=${encodeURIComponent(currentFilter)}&search=${encodeURIComponent(searchQuery)}`);
  if (response.ok) {
    const html = await response.text();
    document.getElementById('courtsGrid').innerHTML = html;
  }
}

function filterCourts(type, btn) {
  currentFilter = type;
  if(btn){ document.querySelectorAll('.filter-btn').forEach(b=>b.classList.remove('active')); btn.classList.add('active'); }
  renderCourts();
  scrollSec('#courts-list');
}
function filterBySearch(val) { searchQuery = val; renderCourts(); }

// renderCourts is pre-rendered on load via Razor

// ══ TIME SLOT ══
let selectedTime = '';
function selectTime(btn, time) {
  selectedTime = time;
  document.querySelectorAll('.time-slot:not(.taken)').forEach(b=>b.classList.remove('selected'));
  btn.classList.add('selected');
}

// ══ BOOKING ══
function openBookModal(service, court) {
  if(service) document.getElementById('bk-service').value = service;
  if(court) document.getElementById('bk-court').value = court;
  scrollSec('#booking');
}

function submitBooking() {
  const name = document.getElementById('bk-name').value.trim();
  const phone = document.getElementById('bk-phone').value.trim();
  const nid = document.getElementById('bk-nid').value.trim();
  const court = document.getElementById('bk-court').value;
  const service = document.getElementById('bk-service').value;
  const date = document.getElementById('bk-date').value;
  if(!name || !phone) { alert('من فضلك ادخل اسمك ورقم هاتفك'); return; }
  if(!selectedTime && !date) { alert('اختار التاريخ والوقت المناسب'); return; }
  const code = '⚖️-' + Math.random().toString(36).substr(2,6).toUpperCase();
  const qNum = Math.floor(Math.random()*30) + 50;
  const timeStr = selectedTime || '10:30';
  const dateStr = date ? new Date(date).toLocaleDateString('ar-EG') : '15 يناير';
  document.getElementById('modalInfo').innerHTML = `
    <div class="modal-info-row"><span class="modal-info-label">المحكمة</span><span class="modal-info-val">${court}</span></div>
    <div class="modal-info-row"><span class="modal-info-label">الخدمة</span><span class="modal-info-val">${service}</span></div>
    <div class="modal-info-row"><span class="modal-info-label">التاريخ</span><span class="modal-info-val">${dateStr}</span></div>
    <div class="modal-info-row"><span class="modal-info-label">الوقت</span><span class="modal-info-val">${timeStr} ص</span></div>
    <div class="modal-info-row"><span class="modal-info-label">رقم الطابور</span><span class="modal-info-val" style="color:var(--court);font-size:18px;font-weight:900">${qNum}</span></div>`;
  document.getElementById('modalCode').textContent = code;
  document.getElementById('successModal').classList.add('show');
  // Update queue counter
  document.getElementById('queueCurrent').textContent = parseInt(document.getElementById('queueCurrent').textContent) + 1;
}

function closeModal() { document.getElementById('successModal').classList.remove('show'); }
document.getElementById('successModal').addEventListener('click', e=>{ if(e.target.id==='successModal') closeModal(); });

// ══ CASE TRACKING ══
// caseData is injected from C# via @Html.Raw in Index.cshtml

async function trackCase(caseNum) {
  const input = document.getElementById('caseInput');
  const num = caseNum || input.value.trim();
  if(!num) { alert('ادخل رقم القضية'); return; }
  input.value = num;
  
  const response = await fetch(`/Courts/Track?caseNum=${encodeURIComponent(num)}`);
  if (response.ok) {
    const html = await response.text();
    document.getElementById('caseResultContainer').innerHTML = html;
  }
}

// ══ STATS ANIMATION ══
function animNum(id, target) {
  let cur=0; const step=Math.ceil(target/40);
  const t = setInterval(()=>{ cur=Math.min(cur+step,target); document.getElementById(id).textContent=cur; if(cur>=target)clearInterval(t); },30);
}
const statsObs = new IntersectionObserver(e=>{
  e.forEach(x=>{ if(x.isIntersecting){ animNum('sn1',128); animNum('sn2',34); animNum('sn3',17); statsObs.disconnect(); } });
},{threshold:.3});
const trackEl = document.getElementById('tracking');
if(trackEl) statsObs.observe(trackEl);

// ══ FAQ ══
// faqs is injected from C# via @Html.Raw in Index.cshtml

// FAQs are pre-rendered on load via Razor

function toggleFaq(i) {
  const item = document.getElementById('faq'+i);
  const isOpen = item.classList.contains('open');
  document.querySelectorAll('.faq-item').forEach(el=>el.classList.remove('open'));
  if(!isOpen) item.classList.add('open');
}

// ══ QUEUE LIVE ANIMATION ══
setInterval(()=>{
  const el = document.getElementById('queueCurrent');
  if(el && Math.random() > .7) {
    const cur = parseInt(el.textContent);
    const delta = Math.random() > .5 ? 1 : -1;
    el.textContent = Math.max(1, cur + delta);
  }
}, 5000);