import { apiRequest } from './client';

export const getSuppliers = () => apiRequest('/api/Contact');
export const createSupplier = (data) =>
    apiRequest('/api/Contact/create-supplier', { method: 'POST', body: data });
