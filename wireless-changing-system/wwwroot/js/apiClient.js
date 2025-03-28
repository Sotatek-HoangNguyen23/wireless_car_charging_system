let isRefreshing = false;
let refreshPromise = null;

// Hàm chung xử lý refresh token
async function performTokenRefresh() {
    try {
        const refreshResponse = await fetch('https://localhost:7191/api/auth/refresh-token', {
            method: 'POST',
            credentials: 'include'
        });
        console.log('Refresh Token Response:', {
            status: refreshResponse.status,
            headers: [...refreshResponse.headers.entries()]
        });
        if (!refreshResponse.ok) {
            const errorText = await refreshResponse.text();
            throw new Error(`Refresh failed: ${errorText}`);
        }
        const data = await refreshResponse.json();

        //sessionStorage.setItem('accessToken', data.accessToken);
        //if (data.fullName) sessionStorage.setItem('fullname', data.fullname);
        //if (data.role) sessionStorage.setItem('role', data.role);
        //if (data.avatarUrl) sessionStorage.setItem('avatar_url', data.avatarUrl);

        return data.accessToken;
    } catch (error) {
        console.error('Refresh token error:', error);
        logout();
        throw error; // Re-throw để xử lý ở nơi gọi
    }
}

export async function requestAccessToken() {
    if (isRefreshing) {
        return refreshPromise; 
    }

    isRefreshing = true;
    refreshPromise = performTokenRefresh()
        .finally(() => {
            isRefreshing = false;
        });

    return refreshPromise;
}

export async function fetchWithAuth(url, options = {}) {
    // Clone options để tránh mutation
    const authOptions = {
        ...options,
        headers: { ...options.headers }
    };

    const currentToken = sessionStorage.getItem('accessToken');
    if (currentToken) {
        authOptions.headers.Authorization = `Bearer ${currentToken}`;
    }

    let response = await fetch(url, authOptions);

    if (response.status === 401) {
        try {
            const newToken = await requestAccessToken(); 

            const retryOptions = {
                ...authOptions,
                headers: {
                    ...authOptions.headers,
                    Authorization: `Bearer ${newToken}`
                }
            };

            return fetch(url, retryOptions);
        } catch (error) {
            return Promise.reject(error);
        }
    }

    return response;
}

export async function logout() {
    try {
        await fetch('https://localhost:7191/api/auth/logout', {
            method: 'POST',
            credentials: 'include'
        });
    } catch (error) {
        console.error('Logout error:', error);
    } finally {
        sessionStorage.clear();
        window.location.href = '/wireless-charging/auth';
    }
}