import { apiRequest } from './client';

export const getProductionOrders = () => apiRequest('/api/Production');
export const createProductionOrder = (data) =>
    apiRequest('/api/Production/create-production-order', { method: 'POST', body: data });
export const prepareProduction = (orderId) =>
    apiRequest(`/api/Production/prepare-production?orderId=${orderId}`, { method: 'POST' });
export const issueMaterial = (orderId) =>
    apiRequest(`/api/Production/issue-material?orderId=${orderId}`, { method: 'POST' });
export const startProduction = (orderId) =>
    apiRequest(`/api/Production/start-production?orderId=${orderId}`, { method: 'POST' });
export const advanceProduction = (orderId) =>
    apiRequest(`/api/Production/advance-production?orderId=${orderId}`, { method: 'POST' });
export const completeProduction = (orderId) =>
    apiRequest(`/api/Production/complete-production?orderId=${orderId}`, { method: 'POST' });
