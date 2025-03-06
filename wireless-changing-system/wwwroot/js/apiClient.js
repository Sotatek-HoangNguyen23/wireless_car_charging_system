let isRefreshing = false;
let refreshPromise = null; 
export async function fetchWithAuth(url, options = {}) {
    let accessToken = sessionStorage.getItem('accessToken');

    if (accessToken) {
        options.headers = {
            ...options.headers,
            'Authorization': `Bearer ${accessToken}`
        };
    }

    let response = await fetch(url, options);

    if (response.status === 401 || !accessToken) {
        if (!isRefreshing) {
            isRefreshing = true;
            refreshPromise = refreshAccessToken(); 
        }

        const newToken = await refreshPromise;
        isRefreshing = false;

        if (newToken) {
            options.headers['Authorization'] = `Bearer ${newToken}`;
            return fetch(url, options); // Thử lại request với token mới
        } else {
            window.location.href = '/wireless-charging/auth';
            throw new Error('Refresh token hết hạn hoặc không hợp lệ. Vui lòng đăng nhập lại.');
        }
    }

    return response;
}

async function refreshAccessToken() {
    try {
        const refreshResponse = await fetch('https://localhost:7191/api/auth/refresh-token', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include' 
        });

        if (!refreshResponse.ok) {
            throw new Error('Refresh token không hợp lệ');
        }
        const data = await refreshResponse.json();
        sessionStorage.setItem('accessToken', data.accessToken);
        return data.accessToken;
    } catch (error) {
        console.error('Lỗi khi refresh token:', error);
        sessionStorage.removeItem('accessToken');
        return null;
    }
}
