import { useCallback, useEffect, useState } from 'react';
import * as bomApi from '../api/bom';
import * as productsApi from '../api/products';
import { Alert, Button, Card, Input, PageHeader, Select, Spinner } from '../components/ui';

export default function BOMPage() {
    const [boms, setBoms] = useState([]);
    const [products, setProducts] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');
    const [form, setForm] = useState({
        productId: '',
        componentId: '',
        quantityRequested: 1,
        unit: 'pcs',
    });

    const load = useCallback(async () => {
        setLoading(true);
        try {
            const [bomsData, productsData] = await Promise.all([
                bomApi.getBOMs(),
                productsApi.getProducts(),
            ]);
            setBoms(Array.isArray(bomsData) ? bomsData : []);
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
            await bomApi.createBOM({
                productId: form.productId,
                components: [{
                    componentId: form.componentId,
                    quantityRequested: Number(form.quantityRequested),
                    unit: form.unit,
                }],
            });
            setSuccess('BOM created');
            load();
        } catch (err) {
            setError(err.message);
        }
    };

    return (
        <div>
            <PageHeader title="Bill of Materials" description="Define product component structures" />

            {error && <div className="mb-4"><Alert type="error">{error}</Alert></div>}
            {success && <div className="mb-4"><Alert type="success">{success}</Alert></div>}

            <Card title="Create BOM" className="mb-6">
                <form onSubmit={handleCreate} className="grid gap-4 sm:grid-cols-2">
                    <Select label="Finished product" value={form.productId} onChange={(e) => setForm({ ...form, productId: e.target.value })} required>
                        <option value="">Select product</option>
                        {products.map((p) => (
                            <option key={p.id} value={p.id}>{p.name}</option>
                        ))}
                    </Select>
                    <Select label="Component" value={form.componentId} onChange={(e) => setForm({ ...form, componentId: e.target.value })} required>
                        <option value="">Select component</option>
                        {products.filter((p) => p.id !== form.productId).map((p) => (
                            <option key={p.id} value={p.id}>{p.name}</option>
                        ))}
                    </Select>
                    <Input label="Quantity" type="number" step="0.01" min="0.01" value={form.quantityRequested} onChange={(e) => setForm({ ...form, quantityRequested: e.target.value })} required />
                    <Input label="Unit" value={form.unit} onChange={(e) => setForm({ ...form, unit: e.target.value })} required />
                    <div className="sm:col-span-2">
                        <Button type="submit">Create BOM</Button>
                    </div>
                </form>
            </Card>

            <Card title={`BOMs (${boms.length})`}>
                {loading ? <Spinner /> : boms.length === 0 ? (
                    <p className="text-sm text-slate-500">No BOMs defined yet.</p>
                ) : (
                    <div className="space-y-4">
                        {boms.map((bom) => (
                            <div key={bom.id} className="rounded-lg border border-slate-100 p-4">
                                <p className="font-medium">{bom.productName ?? `Product ${bom.productId}`}</p>
                                <ul className="mt-2 space-y-1 text-sm text-slate-600">
                                    {(bom.items ?? []).map((item) => (
                                        <li key={item.id}>
                                            Component {item.componentId} — {item.quantity} {item.unit}
                                        </li>
                                    ))}
                                </ul>
                            </div>
                        ))}
                    </div>
                )}
            </Card>
        </div>
    );
}
