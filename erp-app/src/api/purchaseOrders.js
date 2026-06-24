import { apiRequest } from './client';

export const getPurchaseOrders = () => apiRequest('/api/PurchaseOrder');
export const receivePurchaseOrder = (purchaseOrderId) =>
    apiRequest(`/api/PurchaseOrder/receive-purchase-order?purchaseOrderId=${purchaseOrderId}`, {
        method: 'POST',
    });
