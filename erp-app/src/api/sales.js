import { apiRequest } from './client';

export const getSalesOrders = () => apiRequest('/api/Sales');
export const createSalesOrder = (data) =>
    apiRequest('/api/Sales/create-sales-order', { method: 'POST', body: data });
export const updateSalesOrderStock = (salesOrderId) =>
    apiRequest(`/api/Sales/update-sales-order-stock?salesOrderId=${salesOrderId}`, { method: 'POST' });
export const shipSalesOrder = (salesOrderId) =>
    apiRequest(`/api/Sales/ship-sales-order?salesOrderId=${salesOrderId}`, { method: 'POST' });
export const runMrp = (salesOrderId) =>
    apiRequest(`/api/Sales/run-mrp?salesOrderId=${salesOrderId}`, { method: 'POST' });
