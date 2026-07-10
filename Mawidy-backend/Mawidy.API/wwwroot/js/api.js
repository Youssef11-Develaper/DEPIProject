const API_BASE = 'http://localhost:5154/api';   
// Token Management
const Auth = {
    getToken: () => localStorage.getItem('token'),
    getUser: () => JSON.parse(localStorage.getItem('user') || '{}'),
    setSession: (token, user) => {
        localStorage.setItem('token', token);
        localStorage.setItem('user', JSON.stringify(user));
    },
    clearSession: () => {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
    },
    isLoggedIn: () => !!localStorage.getItem('token'),
    isAdmin: () => JSON.parse(localStorage.getItem('user') || '{}').role === 'Admin',
    isBranchAdmin: () => JSON.parse(localStorage.getItem('user') || '{}').role === 'BranchAdmin',
};

// Base Request
async function request(endpoint, options = {}) {
    const token = Auth.getToken();

    const headers = {
        'Content-Type': 'application/json',
        ...(token && { 'Authorization': `Bearer ${token}` }),
        ...options.headers
    };

    const response = await fetch(`${API_BASE}${endpoint}`, {
        ...options,
        headers
    });

    const data = await response.json();

    if (!response.ok) {
        throw new Error(data.message || 'حدث خطأ، حاول مرة أخرى');
    }

    return data;
}

// API Calls
const API = {
    // Auth
    auth: {
        login: (dto) => request('/auth/login', {
            method: 'POST',
            body: JSON.stringify(dto)
        }),
        register: (dto) => request('/auth/register', {
            method: 'POST',
            body: JSON.stringify(dto)
        }),
        getProfile: () => request('/auth/profile'),
        updateProfile: (dto) => request('/auth/profile', {
            method: 'PUT',
            body: JSON.stringify(dto)
        }),
        forgotPassword: (email) => request(`/auth/forgot-password?email=${email}`, {
            method: 'POST'
        }),
        resetPassword: (userId, token, newPassword) => request(
            `/auth/reset-password?userId=${userId}&token=${token}&newPassword=${newPassword}`, {
            method: 'POST'
        }),
        changePassword: (dto) => request('/auth/change-password', {
            method: 'POST',
            body: JSON.stringify(dto)
        }),
    },

    // Appointments
    appointments: {
        getAll: () => request('/appointments'),
        getById: (id) => request(`/appointments/${id}`),
        getSlots: (branchId, serviceTypeId, date) =>
            request(`/appointments/slots?branchId=${branchId}&serviceTypeId=${serviceTypeId}&date=${date}`),
        create: (dto) => request('/appointments', {
            method: 'POST',
            body: JSON.stringify(dto)
        }),
        cancel: (id) => request(`/appointments/${id}/cancel`, { method: 'PUT' }),
        reschedule: (id, dto) => request(`/appointments/${id}/reschedule`, {
            method: 'PUT',
            body: JSON.stringify(dto)
        }),
        getQR: (id) => request(`/appointments/${id}/qr`),
    },

    // Branches
    branches: {
        getAll: () => request('/branches'),
        getById: (id) => request(`/branches/${id}`),
        getByGovernorate: (id) => request(`/branches/by-governorate/${id}`),
        getGovernorates: () => request('/branches/governorates'),
        getServices: () => request('/branches/services'),
        getOperators: () => request('/branches/operators'),
        getOperatorServices: (operatorId) => request('/branches/operator-services' + (operatorId ? `?operatorId=${operatorId}` : '')),
        getServiceDocuments: (serviceKey) => request(`/branches/service-documents/${serviceKey}`),
        getRatings: (id) => request(`/branches/${id}/ratings`),
    },

    // Complaints
    complaints: {
        getAll: () => request('/complaints'),
        getById: (id) => request(`/complaints/${id}`),
        create: (dto) => request('/complaints', {
            method: 'POST',
            body: JSON.stringify(dto)
        }),
    },

    // Ratings
    ratings: {
        getRecent: () => request('/ratings/recent'),
        create: (dto) => request('/ratings', {
            method: 'POST',
            body: JSON.stringify(dto)
        }),
    },

    // Admin
    admin: {
        getDashboard: () => request('/admin/dashboard'),
        getAppointments: (date, branchId) => {
            let url = `/admin/appointments?date=${date}`;
            if (branchId) url += `&branchId=${branchId}`;
            return request(url);
        },
        updateAppointmentStatus: (id, status) => request(`/admin/appointments/${id}/status`, {
            method: 'PUT',
            body: JSON.stringify({ status })
        }),
        getComplaints: (status) => {
            let url = '/admin/complaints';
            if (status !== undefined) url += `?status=${status}`;
            return request(url);
        },
        respondToComplaint: (id, dto) => request(`/admin/complaints/${id}/respond`, {
            method: 'PUT',
            body: JSON.stringify(dto)
        }),
        getBranches: () => request('/admin/branches'),
        createBranch: (dto) => request('/admin/branches', {
            method: 'POST',
            body: JSON.stringify(dto)
        }),
        updateBranch: (id, dto) => request(`/admin/branches/${id}`, {
            method: 'PUT',
            body: JSON.stringify(dto)
        }),
        deleteBranch: (id) => request(`/admin/branches/${id}`, { method: 'DELETE' }),
        addSchedule: (branchId, dto) => request(`/admin/branches/${branchId}/schedules`, {
            method: 'POST',
            body: JSON.stringify(dto)
        }),
        deleteSchedule: (id) => request(`/admin/branches/schedules/${id}`, { method: 'DELETE' }),
        addHoliday: (branchId, dto) => request(`/admin/branches/${branchId}/holidays`, {
            method: 'POST',
            body: JSON.stringify(dto)
        }),
        deleteHoliday: (id) => request(`/admin/branches/holidays/${id}`, { method: 'DELETE' }),
        addServiceUnavailability: (branchId, dto) =>
            request(`/admin/branches/${branchId}/service-unavailability`, {
                method: 'POST',
                body: JSON.stringify(dto)
            }),
        getServices: () => request('/admin/services'),
        createService: (dto) => request('/admin/services', {
            method: 'POST',
            body: JSON.stringify(dto)
        }),
        updateService: (id, dto) => request(`/admin/services/${id}`, {
            method: 'PUT',
            body: JSON.stringify(dto)
        }),
        deleteService: (id) => request(`/admin/services/${id}`, { method: 'DELETE' }),
        getDailyReport: (date, branchId) => {
            let url = `${API_BASE}/admin/reports/daily?date=${date}`;
            if (branchId) url += `&branchId=${branchId}`;
            return url;
        },
    },

    // Branch Admin
    branchAdmin: {
        getDashboard: () => request('/branchadmin/dashboard'),
        completeAppointment: (id) => request(`/branchadmin/appointments/${id}/complete`, {
            method: 'PUT'
        }),
        addHoliday: (dto) => request('/branchadmin/holidays', {
            method: 'POST',
            body: JSON.stringify(dto)
        }),
        addServiceUnavailability: (dto) =>
            request('/branchadmin/service-unavailability', {
                method: 'POST',
                body: JSON.stringify(dto)
            }),
    }
};

// UI Helpers
const UI = {
    showToast: (message, type = 'success') => {
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.innerHTML = `
            <i class="bi bi-${type === 'success' ? 'check-circle' : 'exclamation-circle'}"></i>
            <span>${message}</span>
        `;
        document.body.appendChild(toast);
        setTimeout(() => toast.classList.add('show'), 10);
        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    },

    showLoading: (el) => {
        el.disabled = true;
        el.dataset.originalText = el.innerHTML;
        el.innerHTML = '<span class="spinner"></span> جاري التحميل...';
    },

    hideLoading: (el) => {
        el.disabled = false;
        el.innerHTML = el.dataset.originalText;
    },

    formatDate: (date) => new Date(date).toLocaleDateString('ar-EG', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    }),

    formatTime: (timeSpan) => {
        const parts = timeSpan.split(':');
        const hours = parseInt(parts[0]);
        const minutes = parts[1];
        const period = hours >= 12 ? 'م' : 'ص';
        const displayHours = hours > 12 ? hours - 12 : hours === 0 ? 12 : hours;
        return `${displayHours}:${minutes} ${period}`;
    },

    getStatusBadge: (status) => {
        const badges = {
            'Pending': '<span class="badge badge-warning">معلق</span>',
            'Confirmed': '<span class="badge badge-success">مؤكد</span>',
            'Completed': '<span class="badge badge-primary">مكتمل</span>',
            'Cancelled': '<span class="badge badge-danger">ملغي</span>',
        };
        return badges[status] || status;
    },

    getComplaintStatusBadge: (status) => {
        const badges = {
            'Submitted': '<span class="badge badge-warning">تم الإرسال</span>',
            'UnderReview': '<span class="badge badge-info">قيد المراجعة</span>',
            'Resolved': '<span class="badge badge-success">تم الحل</span>',
            'Closed': '<span class="badge badge-secondary">مغلقة</span>',
        };
        return badges[status] || status;
    },

    redirectIfNotLoggedIn: () => {
        if (!Auth.isLoggedIn()) {
            window.location.href = '/index.html';
        }
    },

    redirectIfNotAdmin: () => {
        if (!Auth.isAdmin()) {
            window.location.href = '/index.html';
        }
    }
};

function bridgePortalLinks() {
    const token = Auth.getToken();
    if (!token) return;
    
    document.querySelectorAll('a').forEach(link => {
        const href = link.getAttribute('href');
        if (href && (href.includes('/banks/index.html') || href.includes('/hospitals/Index.html'))) {
            try {
                // If it's a relative URL or absolute, resolve it correctly
                const url = new URL(href, window.location.origin);
                url.searchParams.set('token', token);
                link.setAttribute('href', url.toString());
            } catch (e) {
                console.error("Error bridging portal link", e);
            }
        }
    });
}
document.addEventListener('DOMContentLoaded', bridgePortalLinks);
