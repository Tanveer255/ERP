const API_BASE = import.meta.env.VITE_API_URL || '';
export const API_MODE = import.meta.env.VITE_API_MODE || 'monolith';

let accessTokenGetter = () => null;

export function setAccessTokenGetter(getter) {
    accessTokenGetter = getter;
}

export class ApiError extends Error {
    constructor(message, status, body) {
        super(message);
        this.status = status;
        this.body = body;
    }
}

export async function apiRequest(path, { method = 'GET', body, headers = {} } = {}) {
    const token = accessTokenGetter();
    const authHeaders =
        API_MODE === 'gateway' && token ? { Authorization: `Bearer ${token}` } : {};

    const response = await fetch(`${API_BASE}${path}`, {
        method,
        credentials: API_MODE === 'gateway' ? 'omit' : 'include',
        headers: {
            ...(body !== undefined ? { 'Content-Type': 'application/json' } : {}),
            ...authHeaders,
            ...headers,
        },
        body: body !== undefined ? JSON.stringify(body) : undefined,
    });

    const text = await response.text();
    let data = null;
    if (text) {
        try {
            data = JSON.parse(text);
        } catch {
            data = text;
        }
    }

    if (!response.ok) {
        const message =
            typeof data === 'string'
                ? data
                : data?.message || data?.Message || response.statusText;
        throw new ApiError(message || 'Request failed', response.status, data);
    }

    return data;
}
