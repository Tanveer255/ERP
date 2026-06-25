import { API_MODE, apiRequest } from './client';
import { getProducts } from './products';

export async function getStock() {
    if (API_MODE === 'gateway') {
        return apiRequest('/api/v1/inventory/stock');
    }

    const products = await getProducts();
    const list = Array.isArray(products) ? products : [];
    return list.map((p) => ({
        productId: p.id ?? p.Id,
        sku: p.sku ?? p.Sku ?? p.name ?? p.Name,
        name: p.name ?? p.Name,
        quantityOnHand: p.quantityRequested ?? p.QuantityRequested ?? 0,
        unit: p.unit ?? p.Unit ?? 'pcs',
    }));
}

export async function adjustStock(payload) {
    if (API_MODE === 'gateway') {
        return apiRequest('/api/v1/inventory/adjust', { method: 'POST', body: payload });
    }
    throw new Error('Stock adjustments require gateway mode (Enterprise Inventory service).');
}
