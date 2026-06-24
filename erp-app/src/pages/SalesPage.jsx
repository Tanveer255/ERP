import { useCallback, useEffect, useState } from 'react';
import * as productsApi from '../api/products';
import * as salesApi from '../api/sales';
import { Alert, Badge, Button, Card, Input, PageHeader, Select, Spinner } from '../components/ui';

function statusColor(status) {
    const s = String(status ?? '').toLowerCase();
    if (s.includes('complete')) return 'green';
    if (s.includes('partial')) return 'yellow';
    if (s.includes('pending')) return 'slate';
    return 'blue';
}

export default function SalesPage() {
    const [orders, setOrders] = useState([]);
    const [products, setProducts] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');
    const [form, setForm] = useState({
        customerName: '',
        customerEmail: '',
        productId: '',
        quantityRequested: 1,
    });

    const load = useCallback(async () => {
        setLoading(true);
        try {
            const [ordersData, productsData] = await Promise.all([
                salesApi.getSalesOrders(),
                productsApi.getProducts(),
            ]);
            setOrders(Array.isArray(ordersData) ? ordersData : []);
            setProducts(Array.isArray(productsData) ? productsData : []);
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => { load(); }, [load]);

    const handleCreate = async (e) => {
        e.preventDefault();
        setError('');
        setSuccess('');
        try {
            const result = await salesApi.createSalesOrder({
                customerName: form.customerName,
                customerEmail: form.customerEmail || null,
                items: [{ productId: form.productId, quantityRequested: Number(form.quantityRequested) }],
            });
            setSuccess(result?.message ?? 'Sales order created');
            setForm({ customerName: '', customerEmail: '', productId: '', quantityRequested: 1 });
            load();
        } catch (err) {
            setError(err.message);
        }
    };

    const runAction = (action, id) => async () => {
        setError('');
        setSuccess('');
        try {
            const result = await action(id);
            setSuccess(typeof result === 'string' ? result : result?.message ?? 'Action completed');
            load();
        } catch (err) {
            setError(err.message);
        }
    };

    return (
        <div>
            <PageHeader title="Sales Orders" description="Create and manage customer orders" />

            {error && <div className="mb-4"><Alert type="error">{error}</Alert></div>}
            {success && <div className="mb-4"><Alert type="success">{success}</Alert></div>}

            <Card title="Create sales order" className="mb-6">
                <form onSubmit={handleCreate} className="grid gap-4 sm:grid-cols-2">
                    <Input label="Customer name" value={form.customerName} onChange={(e) => setForm({ ...form, customerName: e.target.value })} required />
                    <Input label="Customer email" type="email" value={form.customerEmail} onChange={(e) => setForm({ ...form, customerEmail: e.target.value })} />
                    <Select label="Product" value={form.productId} onChange={(e) => setForm({ ...form, productId: e.target.value })} required>
                        <option value="">Select product</option>
                        {products.map((p) => (
                            <option key={p.id} value={p.id}>{p.name} ({p.code})</option>
                        ))}
                    </Select>
                    <Input label="Quantity" type="number" step="0.01" min="0.01" value={form.quantityRequested} onChange={(e) => setForm({ ...form, quantityRequested: e.target.value })} required />
                    <div className="sm:col-span-2">
                        <Button type="submit">Create order</Button>
                    </div>
                </form>
            </Card>

            <Card title={`Orders (${orders.length})`}>
                {loading ? <Spinner /> : orders.length === 0 ? (
                    <p className="text-sm text-slate-500">No sales orders yet.</p>
                ) : (
                    <div className="space-y-4">
                        {orders.map((order) => (
                            <div key={order.id} className="rounded-lg border border-slate-100 p-4">
                                <div className="flex flex-wrap items-center justify-between gap-2">
                                    <div>
                                        <p className="font-medium">{order.orderNumber}</p>
                                        <p className="text-sm text-slate-500">{order.customerName} · ${order.totalAmount?.toFixed?.(2) ?? order.totalAmount}</p>
                                    </div>
                                    <Badge color={statusColor(order.status)}>{String(order.status)}</Badge>
                                </div>
                                <div className="mt-3 flex flex-wrap gap-2">
                                    <Button variant="secondary" className="text-xs" onClick={runAction(salesApi.updateSalesOrderStock, order.id)}>Update stock</Button>
                                    <Button variant="secondary" className="text-xs" onClick={runAction(salesApi.shipSalesOrder, order.id)}>Ship</Button>
                                    <Button variant="secondary" className="text-xs" onClick={runAction(salesApi.runMrp, order.id)}>Run MRP</Button>
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </Card>
        </div>
    );
}
