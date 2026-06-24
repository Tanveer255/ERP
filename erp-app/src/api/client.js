const API_BASE = import.meta.env.VITE_API_URL || '';

export class ApiError extends Error {
    constructor(message, status, body) {
        super(message);
        this.status = status;
        this.body = body;
    }
}

export async function apiRequest(path, { method = 'GET', body, headers = {} } = {}) {
    const response = await fetch(`${API_BASE}${path}`, {
        method,
        credentials: 'include',
        headers: {
            ...(body !== undefined ? { 'Content-Type': 'application/json' } : {}),
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
