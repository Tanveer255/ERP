import { apiRequest } from './client';

export const getBOMs = () => apiRequest('/api/BOM');
export const createBOM = (data) => apiRequest('/api/BOM/create-bom', { method: 'POST', body: data });
