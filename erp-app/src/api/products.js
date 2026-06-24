import { apiRequest } from './client';

export const getProducts = () => apiRequest('/api/Product');
export const getProduct = (id) => apiRequest(`/api/Product/${id}`);
export const createProduct = (data) => apiRequest('/api/Product', { method: 'POST', body: data });
export const updateProduct = (id, data) => apiRequest(`/api/Product/${id}`, { method: 'PUT', body: data });
export const deleteProduct = (id) => apiRequest(`/api/Product/${id}`, { method: 'DELETE' });
