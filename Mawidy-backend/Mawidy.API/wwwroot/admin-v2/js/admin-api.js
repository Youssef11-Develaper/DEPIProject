// Mawidy Admin V2 API Extender
// Requires ../js/api.js to be loaded first in the HTML.

if (typeof API !== 'undefined') {
    API.adminv2 = {
        // Get paginated users with filters
        getUsers: (search, role, page = 1, pageSize = 10) => {
            let url = `/adminv2/users?page=${page}&pageSize=${pageSize}`;
            if (search) url += `&search=${encodeURIComponent(search)}`;
            if (role) url += `&role=${encodeURIComponent(role)}`;
            return request(url);
        },

        // Get single user details + appointments
        getUserDetail: (id) => {
            return request(`/adminv2/users/${id}`);
        },

        // Admin password reset
        resetPassword: (id, newPassword) => {
            return request(`/adminv2/users/${id}/reset-password`, {
                method: 'POST',
                body: JSON.stringify({ newPassword })
            });
        },

        // Get all operators
        getOperators: () => request('/adminv2/operators'),

        // Get operators filtered by category
        getOperatorsByCategory: async (category) => {
            const res = await request('/adminv2/operators');
            const all = res.data || [];
            const categoryMap = {
                'civil': ['civil_registry'],
                'telecom': ['vodafone', 'orange', 'etisalat', 'we'],
                'banks': ['nbe', 'banquemisr', 'cib', 'qnb', 'alexbank'],
                'hospitals': ['hospital_generic']
            };
            const keys = categoryMap[category] || [];
            return { data: all.filter(o => keys.includes(o.key)) };
        },

        getOperatorServices: (operatorId) => {
            return request(`/adminv2/operator-services?operatorId=${operatorId}`);
        },
        createOperatorService: (dto) => request('/adminv2/operator-services', {
            method: 'POST',
            body: JSON.stringify(dto)
        }),
        updateOperatorService: (id, dto) => request(`/adminv2/operator-services/${id}`, {
            method: 'PUT',
            body: JSON.stringify(dto)
        }),
        deleteOperatorService: (id) => request(`/adminv2/operator-services/${id}`, {
            method: 'DELETE'
        }),

        getDistricts: (governorateId) => {
            let url = '/adminv2/districts';
            if (governorateId) url += `?governorateId=${governorateId}`;
            return request(url);
        },

        // Courts Management API Calls
        getCourtsList: () => request('/admin/courts-list'),
        createCourt: (dto) => request('/admin/courts-list', {
            method: 'POST',
            body: JSON.stringify(dto)
        }),
        updateCourt: (id, dto) => request(`/admin/courts-list/${id}`, {
            method: 'PUT',
            body: JSON.stringify(dto)
        }),
        deleteCourt: (id) => request(`/admin/courts-list/${id}`, {
            method: 'DELETE'
        }),

        // Court Departments
        getCourtDepartments: (courtId) => {
            let url = '/admin/court-departments';
            if (courtId) url += `?courtId=${courtId}`;
            return request(url);
        },
        createCourtDepartment: (dto) => request('/admin/court-departments', {
            method: 'POST',
            body: JSON.stringify(dto)
        }),

        // Court Services
        getCourtServices: () => request('/admin/court-services'),
        createCourtService: (dto) => request('/admin/court-services', {
            method: 'POST',
            body: JSON.stringify(dto)
        }),

        // Legal Cases
        getLegalCases: () => request('/admin/legal-cases'),
        createLegalCase: (dto) => request('/admin/legal-cases', {
            method: 'POST',
            body: JSON.stringify(dto)
        }),

        // Court Bookings
        getCourtBookings: (date, courtId) => {
            let url = '/admin/court-bookings';
            let params = [];
            if (date) params.push(`date=${date}`);
            if (courtId) params.push(`courtId=${courtId}`);
            if (params.length > 0) url += `?${params.join('&')}`;
            return request(url);
        },
        updateCourtBookingStatus: (id, status) => request(`/admin/court-bookings/${id}/status`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(status)
        })
    };
} else {
    console.error('Base API not loaded! Make sure to import ../js/api.js before admin-api.js.');
}
