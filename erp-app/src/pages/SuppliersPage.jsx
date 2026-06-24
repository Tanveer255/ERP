import { useCallback, useEffect, useState } from 'react';
import * as contactsApi from '../api/contacts';
import * as productsApi from '../api/products';
import { Alert, Button, Card, Input, PageHeader, Select, Spinner } from '../components/ui';

export default function SuppliersPage() {
    const [suppliers, setSuppliers] = useState([]);
    const [products, setProducts] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');
    const [form, setForm] = useState({
        name: '',
        email: '',
        phone: '',
        city: '',
        country: '',
        productId: '',
        price: 0,
    });

    const load = useCallback(async () => {
        setLoading(true);
        try {
            const [suppliersData, productsData] = await Promise.all([
                contactsApi.getSuppliers(),
                productsApi.getProducts(),
            ]);
            setSuppliers(Array.isArray(suppliersData) ? suppliersData : []);
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
            await contactsApi.createSupplier({
                name: form.name,
                email: form.email,
                phone: form.phone,
                city: form.city,
                country: form.country,
                products: form.productId
                    ? [{ productId: form.productId, price: Number(form.price), leadTimeInDays: 3, isPreferred: true }]
                    : [],
            });
            setSuccess('Supplier created');
            setForm({ name: '', email: '', phone: '', city: '', country: '', productId: '', price: 0 });
            load();
        } catch (err) {
            setError(err.message);
        }
    };

    return (
        <div>
            <PageHeader title="Suppliers" description="Manage vendor contacts and product links" />

            {error && <div className="mb-4"><Alert type="error">{error}</Alert></div>}
            {success && <div className="mb-4"><Alert type="success">{success}</Alert></div>}

            <Card title="Add supplier" className="mb-6">
                <form onSubmit={handleCreate} className="grid gap-4 sm:grid-cols-2">
                    <Input label="Name" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
                    <Input label="Email" type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
                    <Input label="Phone" value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
                    <Input label="City" value={form.city} onChange={(e) => setForm({ ...form, city: e.target.value })} />
                    <Input label="Country" value={form.country} onChange={(e) => setForm({ ...form, country: e.target.value })} />
                    <Select label="Linked product (optional)" value={form.productId} onChange={(e) => setForm({ ...form, productId: e.target.value })}>
                        <option value="">None</option>
                        {products.map((p) => (
                            <option key={p.id} value={p.id}>{p.name}</option>
                        ))}
                    </Select>
                    <Input label="Supply price" type="number" step="0.01" value={form.price} onChange={(e) => setForm({ ...form, price: e.target.value })} />
                    <div className="sm:col-span-2">
                        <Button type="submit">Create supplier</Button>
                    </div>
                </form>
            </Card>

            <Card title={`Suppliers (${suppliers.length})`}>
                {loading ? <Spinner /> : suppliers.length === 0 ? (
                    <p className="text-sm text-slate-500">No suppliers yet.</p>
                ) : (
                    <div className="overflow-x-auto">
                        <table className="w-full text-left text-sm">
                            <thead>
                                <tr className="border-b border-slate-100 text-slate-500">
                                    <th className="pb-3 pr-4 font-medium">Name</th>
                                    <th className="pb-3 pr-4 font-medium">Email</th>
                                    <th className="pb-3 pr-4 font-medium">Phone</th>
                                    <th className="pb-3 font-medium">Location</th>
                                </tr>
                            </thead>
                            <tbody>
                                {suppliers.map((s) => (
                                    <tr key={s.id} className="border-b border-slate-50">
                                        <td className="py-3 pr-4 font-medium">{s.name}</td>
                                        <td className="py-3 pr-4">{s.email || '—'}</td>
                                        <td className="py-3 pr-4">{s.phone || '—'}</td>
                                        <td className="py-3">{[s.city, s.country].filter(Boolean).join(', ') || '—'}</td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}
            </Card>
        </div>
    );
}
