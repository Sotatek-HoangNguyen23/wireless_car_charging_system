let isRefreshing = false;
let refreshPromise = null;

async function performTokenRefresh() {
    try {
        const refreshResponse = await fetch('https://localhost:7191/api/auth/refresh-token', {
            method: 'POST',
            credentials: 'include'
        });

        if (!refreshResponse.ok) throw new Error('Refresh failed');
        const data = await refreshResponse.json();
        console.log('New token:', data);
        // Lưu thông tin mới vào sessionStorage
        sessionStorage.setItem('accessToken', data.accessToken);
        sessionStorage.setItem('fullName', data.fullname); // Đảm bảo key đúng
        sessionStorage.setItem("avatar_url", "https://th.bing.com/th/id/R.b3a1e1a27386019b0be253c09cb3701b?rik=rCmgbtux1pK1rA&pid=ImgRaw&r=0");
        sessionStorage.setItem('role', data.role);

        const event = new Event('userDataUpdated');
        window.dispatchEvent(event);

        return data.accessToken;
    } catch (error) {
        logout();
        throw error;
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