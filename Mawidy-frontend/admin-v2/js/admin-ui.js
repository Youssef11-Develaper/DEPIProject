// Mawidy Admin V2 UI helper and layout manager
const AdminUI = {
    // Inject sidebar and header into the page
    init: (activeItem) => {
        // Enforce Admin access first
        if (typeof UI !== 'undefined' && typeof UI.redirectIfNotAdmin === 'function') {
            UI.redirectIfNotAdmin();
        }

        // Apply saved theme
        const savedTheme = localStorage.getItem('admin-theme') || 'light';
        document.documentElement.setAttribute('data-theme', savedTheme);

        // Injects standard toast container
        if (!document.getElementById('admin-toast-container')) {
            const toastContainer = document.createElement('div');
            toastContainer.id = 'admin-toast-container';
            toastContainer.className = 'admin-toast-container';
            document.body.appendChild(toastContainer);
        }

        // Injects shared confirmation modal
        if (!document.getElementById('confirm-modal-backdrop')) {
            const confirmModal = document.createElement('div');
            confirmModal.id = 'confirm-modal-backdrop';
            confirmModal.className = 'modal-backdrop';
            confirmModal.innerHTML = `
                <div class="admin-modal" style="max-width: 400px;">
                    <div class="admin-modal-header">
                        <span class="admin-modal-title" style="color:var(--danger)">تأكيد الإجراء</span>
                        <button class="admin-modal-close" onclick="AdminUI.closeConfirm()">&times;</button>
                    </div>
                    <div class="admin-modal-body">
                        <p id="confirm-modal-text"></p>
                    </div>
                    <div class="admin-modal-footer">
                        <button class="admin-btn admin-btn-outline" onclick="AdminUI.closeConfirm()">إلغاء</button>
                        <button class="admin-btn admin-btn-danger" id="confirm-modal-yes">تأكيد</button>
                    </div>
                </div>
            `;
            document.body.appendChild(confirmModal);
        }
        // Render Sidebar
        const sidebarContainer = document.getElementById('sidebar-container');
        if (sidebarContainer) {
            sidebarContainer.innerHTML = AdminUI.getSidebarHtml(activeItem);
            // Restore collapse state on desktop only
            if (window.innerWidth > 768 && localStorage.getItem('admin-sidebar-collapsed') === 'true') {
                document.body.classList.add('sidebar-collapsed');
            }
        }

        // Injects sidebar overlay for mobile
        if (!document.getElementById('admin-sidebar-overlay')) {
            const overlay = document.createElement('div');
            overlay.id = 'admin-sidebar-overlay';
            overlay.className = 'sidebar-overlay';
            overlay.onclick = () => {
                document.body.classList.remove('sidebar-open');
            };
            document.body.appendChild(overlay);
        }

        // Render Header
        const headerContainer = document.getElementById('header-container');
        if (headerContainer) {
            const user = typeof Auth !== 'undefined' ? Auth.getUser() : {};
            const initials = (user.firstName || 'م').substring(0, 1) + (user.lastName || 'د').substring(0, 1);
            const isDark = (localStorage.getItem('admin-theme') === 'dark');
            headerContainer.innerHTML = `
                <div style="display:flex;align-items:center;gap:15px">
                    <button class="toggle-sidebar-btn" onclick="AdminUI.toggleSidebar()" style="color:var(--text-color)">
                        <i class="bi bi-justify"></i>
                    </button>
                    <span style="font-weight:600;font-size:0.95rem;color:var(--text-muted)">
                        نظام إدارة السجل المدني الرقمي - لوحة الإدارة
                    </span>
                </div>
                <div class="header-user-menu">
                    <button class="admin-btn" id="theme-toggle-btn" onclick="AdminUI.toggleTheme()"
                        title="تبديل المظهر"
                        style="background:none;border:1px solid var(--border-color);border-radius:8px;padding:6px 10px;color:var(--text-color);cursor:pointer;">
                        <i class="bi ${isDark ? 'bi-sun-fill' : 'bi-moon-fill'}"></i>
                    </button>
                    <div class="notification-bell" onclick="AdminUI.showNotifications()">
                        <i class="bi bi-bell"></i>
                        <span class="notification-badge"></span>
                    </div>
                    <div class="user-profile-badge">
                        <div class="user-avatar">${initials}</div>
                        <div>
                            <div class="user-name">${user.firstName || 'المدير'} ${user.lastName || 'العام'}</div>
                            <div style="font-size:0.75rem;color:var(--text-muted)">مدير النظام</div>
                        </div>
                    </div>
                </div>
            `;
        }
    },

    toggleTheme: () => {
        const current = document.documentElement.getAttribute('data-theme') || 'light';
        const next = current === 'light' ? 'dark' : 'light';
        document.documentElement.setAttribute('data-theme', next);
        localStorage.setItem('admin-theme', next);

        const btn = document.getElementById('theme-toggle-btn');
        if (btn) {
            btn.innerHTML = `<i class="bi ${next === 'dark' ? 'bi-sun-fill' : 'bi-moon-fill'}"></i>`;
        }
    },
    toggleSidebar: () => {
        if (window.innerWidth <= 768) {
            document.body.classList.toggle('sidebar-open');
        } else {
            document.body.classList.toggle('sidebar-collapsed');
            const isCollapsed = document.body.classList.contains('sidebar-collapsed');
            localStorage.setItem('admin-sidebar-collapsed', isCollapsed);
        }
        setTimeout(() => {
            window.dispatchEvent(new Event('resize'));
        }, 300);
    },
    getSidebarHtml: (active) => {
        const isDark = (localStorage.getItem('admin-theme') === 'dark');

        const sections = [
            {
                label: null,
                items: [
                    { id: 'dashboard', label: 'الرئيسية', icon: 'bi-speedometer2', link: '/admin-v2/index.html' },
                ]
            },
            {
                label: 'المستخدمون',
                items: [
                    { id: 'users', label: 'المستخدمين', icon: 'bi-people', link: '/admin-v2/users.html' },
                    { id: 'roles', label: 'الصلاحيات والأدوار', icon: 'bi-person-badge', link: '/admin-v2/roles.html' },
                ]
            },
            {
                label: 'إدارة الفروع',
                items: [
                    { id: 'civil-branches', label: 'السجل المدني', icon: 'bi-building', link: '/admin-v2/civil-branches.html', emoji: '🏛' },
                    { id: 'hospital-branches', label: 'المستشفيات', icon: 'bi-hospital', link: '/admin-v2/hospital-branches.html', emoji: '🏥' },
                    { id: 'bank-branches', label: 'البنوك', icon: 'bi-bank', link: '/admin-v2/bank-branches.html', emoji: '🏦' },
                    { id: 'branches', label: 'شركات الاتصالات', icon: 'bi-phone', link: '/admin-v2/branches.html', emoji: '📱' },
                    { id: 'courts', label: 'المحاكم والقضايا', icon: 'bi-journal-text', link: '/admin-v2/courts.html', emoji: '⚖' },
                ]
            },
            {
                label: 'العمليات',
                items: [
                    { id: 'appointments', label: 'المواعيد', icon: 'bi-calendar-check', link: '/admin-v2/appointments.html' },
                    { id: 'services', label: 'الخدمات', icon: 'bi-list-check', link: '/admin-v2/services.html' },
                    { id: 'complaints', label: 'الشكاوى', icon: 'bi-exclamation-circle', link: '/admin-v2/complaints.html' },
                ]
            },
            {
                label: 'الأدوات',
                items: [
                    { id: 'reports', label: 'التقارير', icon: 'bi-file-pdf', link: '/admin-v2/reports.html' },
                    { id: 'settings', label: 'الإعدادات', icon: 'bi-gear', link: '/admin-v2/settings.html' },
                ]
            }
        ];

        let html = `
            <aside class="admin-sidebar">
                <div class="sidebar-header">
                    <div class="brand-info">
                        <i class="bi bi-building-fill" style="font-size:1.5rem;color:var(--primary-light)"></i>
                        <span>لوحة التحكم v2</span>
                    </div>
                </div>
                <ul class="sidebar-menu">
        `;

        sections.forEach(section => {
            if (section.label) {
                html += `<li class="sidebar-section-label">${section.label}</li>`;
            }
            section.items.forEach(item => {
                const isActive = item.id === active ? 'active' : '';
                const emojiHtml = item.emoji ? `<span style="font-size:1rem;margin-left:2px">${item.emoji}</span>` : `<i class="bi ${item.icon}"></i>`;
                html += `
                    <li class="menu-item ${isActive}">
                        <a href="${item.link}">
                            ${emojiHtml}
                            <span>${item.label}</span>
                        </a>
                    </li>
                `;
            });
        });

        html += `
                </ul>
                <div class="sidebar-footer">
                    <button class="logout-btn" onclick="AdminUI.logout()">
                        <i class="bi bi-box-arrow-right"></i>
                        <span>تسجيل الخروج</span>
                    </button>
                </div>
            </aside>
        `;
        return html;
    },

    logout: () => {
        if (typeof Auth !== 'undefined') {
            Auth.clearSession();
        }
        window.location.href = '../index.html';
    },

    showToast: (message, type = 'success') => {
        const container = document.getElementById('admin-toast-container');
        if (!container) return;

        const toast = document.createElement('div');
        toast.className = `admin-toast admin-toast-${type}`;
        
        let icon = 'check-circle-fill';
        if (type === 'error') icon = 'exclamation-triangle-fill';
        if (type === 'info') icon = 'info-circle-fill';

        toast.innerHTML = `
            <i class="bi bi-${icon}"></i>
            <span>${message}</span>
        `;
        container.appendChild(toast);

        setTimeout(() => {
            toast.style.opacity = '0';
            toast.style.transform = 'translateY(10px)';
            toast.style.transition = 'all 0.3s ease';
            setTimeout(() => toast.remove(), 300);
        }, 3500);
    },

    confirm: (message, onYes) => {
        const backdrop = document.getElementById('confirm-modal-backdrop');
        const text = document.getElementById('confirm-modal-text');
        const yesBtn = document.getElementById('confirm-modal-yes');

        if (!backdrop || !text || !yesBtn) return;

        text.textContent = message;
        backdrop.classList.add('show');

        yesBtn.onclick = () => {
            onYes();
            AdminUI.closeConfirm();
        };
    },

    closeConfirm: () => {
        const backdrop = document.getElementById('confirm-modal-backdrop');
        if (backdrop) backdrop.classList.remove('show');
    },

    showNotifications: () => {
        AdminUI.showToast("لا توجد إشعارات جديدة حالياً", "info");
    },

    createSkeletonRows: (colsCount, rowsCount = 5) => {
        let html = '';
        for (let r = 0; r < rowsCount; r++) {
            html += '<tr>';
            for (let c = 0; c < colsCount; c++) {
                html += '<td><div class="skeleton-row"></div></td>';
            }
            html += '</tr>';
        }
        return html;
    },

    renderPagination: (containerId, page, totalPages, onPageChangeName) => {
        const container = document.getElementById(containerId);
        if (!container) return;

        if (totalPages <= 1) {
            container.innerHTML = '';
            return;
        }

        let html = `
            <button class="page-btn" ${page === 1 ? 'disabled' : ''} onclick="${onPageChangeName}(${page - 1})">
                <i class="bi bi-chevron-right"></i>
            </button>
        `;

        for (let i = 1; i <= totalPages; i++) {
            if (i === 1 || i === totalPages || (i >= page - 2 && i <= page + 2)) {
                html += `
                    <button class="page-btn ${i === page ? 'active' : ''}" onclick="${onPageChangeName}(${i})">
                        ${i}
                    </button>
                `;
            } else if (i === page - 3 || i === page + 3) {
                html += `<span style="color:var(--text-muted)">...</span>`;
            }
        }

        html += `
            <button class="page-btn" ${page === totalPages ? 'disabled' : ''} onclick="${onPageChangeName}(${page + 1})">
                <i class="bi bi-chevron-left"></i>
            </button>
        `;

        container.innerHTML = html;
    },

    getDayName: (day) => {
        const daysMap = ['الأحد', 'الإثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت'];
        if (day === null || day === undefined) return '';
        if (typeof day === 'number') return daysMap[day] || '';
        const dayStr = String(day).trim();
        if (/^\d+$/.test(dayStr)) {
            return daysMap[parseInt(dayStr)] || '';
        }
        const lower = dayStr.toLowerCase();
        switch (lower) {
            case 'sunday': return 'الأحد';
            case 'monday': return 'الإثنين';
            case 'tuesday': return 'الثلاثاء';
            case 'wednesday': return 'الأربعاء';
            case 'thursday': return 'الخميس';
            case 'friday': return 'الجمعة';
            case 'saturday': return 'السبت';
            default: return dayStr;
        }
    }
};
