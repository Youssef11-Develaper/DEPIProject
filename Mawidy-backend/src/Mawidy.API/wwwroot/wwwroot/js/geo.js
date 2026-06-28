let curGovId  = null;
let curDistId = null;
let geoOpen   = false;

function toggleGeo() {
    geoOpen = !geoOpen;
    document.getElementById('geoBody').classList.toggle('open', geoOpen);
    document.getElementById('geoToggle').textContent = geoOpen ? '▲ إخفاء المحافظات' : '▼ عرض المحافظات';
}

function showRegion(region, btn) {
    document.querySelectorAll('.rtab').forEach(t => t.classList.remove('on'));
    btn.classList.add('on');
    document.querySelectorAll('.gov-card').forEach(c => {
        c.style.display = (region === 'all' || c.dataset.region === region) ? '' : 'none';
    });
}

function selectGov(govId, govName, el) {
    if (curGovId === govId) { clearGov(); return; }
    curGovId  = govId;
    curDistId = null;

    document.querySelectorAll('.gov-card').forEach(c => c.classList.remove('sel'));
    if (el) el.classList.add('sel');

    const gov = ALL_GOVS.find(g => g.id === govId);
    if (!gov) return;

    buildDistrictGrid(gov);
    document.getElementById('distPanel').style.display = '';
    document.getElementById('govGrid').style.display   = 'none';

    document.getElementById('breadcrumb').style.display = '';
    document.getElementById('bcGov').textContent        = govName;
    document.getElementById('bcDistSep').style.display  = 'none';
    document.getElementById('bcDist').style.display     = 'none';

    applyFilter();
}

function buildDistrictGrid(gov) {
    const name = gov.name || '';
    document.getElementById('distGovName').textContent = gov.emoji + ' ' + name + ' — اختر المركز';
    const all = `<div class="dist-card ${curDistId === null ? 'sel' : ''}"
                      onclick="selectDist(null,'كل مراكز ${name}',this)">
                    <div class="dist-type">الكل</div>
                    <div class="dist-name">كل المراكز</div>
                    <div class="dist-count">${gov.count} فرع</div>
                 </div>`;
    const items = (gov.districts || []).map(d =>
        `<div class="dist-card ${curDistId === d.id ? 'sel' : ''}"
              onclick="selectDist(${d.id},'${d.name}',this)">
            <div class="dist-type">${d.type}</div>
            <div class="dist-name">${d.name}</div>
            <div class="dist-count">${d.count} فرع</div>
         </div>`
    ).join('');
    document.getElementById('distGrid').innerHTML = all + items;
}

function selectDist(distId, distName, el) {
    curDistId = distId;
    document.querySelectorAll('.dist-card').forEach(c => c.classList.remove('sel'));
    el.classList.add('sel');
    if (distId) {
        document.getElementById('bcDistSep').style.display = '';
        document.getElementById('bcDist').style.display    = '';
        document.getElementById('bcDist').textContent      = distName;
    } else {
        document.getElementById('bcDistSep').style.display = 'none';
        document.getElementById('bcDist').style.display    = 'none';
    }
    applyFilter();
}

function clearGov() {
    curGovId  = null;
    curDistId = null;
    document.querySelectorAll('.gov-card').forEach(c => c.classList.remove('sel'));
    document.getElementById('distPanel').style.display = 'none';
    document.getElementById('govGrid').style.display   = '';
    document.getElementById('breadcrumb').style.display = 'none';
    applyFilter();
}

window.getGeoFilter = () => ({ govId: curGovId, distId: curDistId });
