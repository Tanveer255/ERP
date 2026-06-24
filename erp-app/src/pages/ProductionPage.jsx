import { useCallback, useEffect, useState } from 'react';
import * as productsApi from '../api/products';
import * as productionApi from '../api/production';
import { Alert, Badge, Button, Card, Input, PageHeader, Select, Spinner } from '../components/ui';

export default function ProductionPage() {
    const [orders, setOrders] = useState([]);
    const [products, setProducts] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');
    const [form, setForm] = useState({ productId: '', quantityRequested: 1 });

    const load = useCallback(async () => {
        setLoading(true);
        try {
            const [ordersData, productsData] = await Promise.all([
                productionApi.getProductionOrders(),
                productsApi.getProducts(),
            ]);
            setOrders(Array.isArray(ordersData) ? ordersData : []);
            setProducts(Array.isArray(productsData) ? productsData.filter((p) => p.isManufactured) : []);
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
        const now = new Date();
        const finish = new Date(now);
        finish.setDate(finish.getDate() + 7);
        try {
            await productionApi.createProductionOrder({
                productId: form.productId,
                quantityRequested: Number(form.quantityRequested),
                startDate: now.toISOString(),
                finishDate: finish.toISOString(),
            });
            setSuccess('Production order created');
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
            setSuccess(typeof result === 'string' ? result : result?.message ?? 'Step completed');
            load();
        } catch (err) {
            setError(err.message);
        }
    };

    const steps = [
        { label: 'Prepare', fn: productionApi.prepareProduction },
        { label: 'Issue material', fn: productionApi.issueMaterial },
        { label: 'Start', fn: productionApi.startProduction },
        { label: 'Advance', fn: productionApi.advanceProduction },
        { label: 'Complete', fn: productionApi.completeProduction },
    ];

    return (
        <div>
            <PageHeader title="Production" description="Manufacturing orders and workflow" />

            {error && <div className="mb-4"><Alert type="error">{error}</Alert></div>}
            {success && <div className="mb-4"><Alert type="success">{success}</Alert></div>}

            <Card title="Create production order" className="mb-6">
                <form onSubmit={handleCreate} className="grid gap-4 sm:grid-cols-2">
                    <Select label="Product (manufactured)" value={form.productId} onChange={(e) => setForm({ ...form, productId: e.target.value })} required>
                        <option value="">Select product</option>
                        {products.map((p) => (
                            <option key={p.id} value={p.id}>{p.name}</option>
                        ))}
                    </Select>
                    <Input label="Quantity" type="number" step="0.01" min="0.01" value={form.quantityRequested} onChange={(e) => setForm({ ...form, quantityRequested: e.target.value })} required />
                    <div className="sm:col-span-2">
                        <Button type="submit">Create order</Button>
                    </div>
                </form>
            </Card>

            <Card title={`Production orders (${orders.length})`}>
                {loading ? <Spinner /> : orders.length === 0 ? (
                    <p className="text-sm text-slate-500">No production orders yet.</p>
                ) : (
                    <div className="space-y-4">
                        {orders.map((order) => (
                            <div key={order.id} className="rounded-lg border border-slate-100 p-4">
                                <div className="flex flex-wrap items-center justify-between gap-2">
                                    <div>
                                        <p className="font-medium">{order.orderNumber}</p>
                                        <p className="text-sm text-slate-500">Qty: {order.plannedQuantity}</p>
                                    </div>
                                    <Badge color="blue">{order.status}</Badge>
                                </div>
                                <div className="mt-3 flex flex-wrap gap-2">
                                    {steps.map(({ label, fn }) => (
                                        <Button key={label} variant="secondary" className="text-xs" onClick={runAction(fn, order.id)}>
                                            {label}
                                        </Button>
                                    ))}
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </Card>
        </div>
    );
}
