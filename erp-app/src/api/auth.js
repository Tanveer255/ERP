import { apiRequest } from './client';

const DEV_CAPTCHA = 'dev-bypass';

export async function login(email, password) {
    return apiRequest('/api/Users/Login', {
        method: 'POST',
        body: { email, password, recaptchaToken: DEV_CAPTCHA },
    });
}

export async function signup(payload) {
    return apiRequest('/api/Users/Signup', {
        method: 'POST',
        body: { ...payload, recaptchaToken: DEV_CAPTCHA },
    });
}

export async function logout(email) {
    return apiRequest('/api/Users/Logout', {
        method: 'POST',
        body: { email },
    });
}

export async function getProfile() {
    return apiRequest('/api/Users/GetProfile');
}
