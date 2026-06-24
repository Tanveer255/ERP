import { useCallback, useEffect, useState } from 'react';
import * as productsApi from '../api/products';
import { Alert, Badge, Button, Card, Input, PageHeader, Spinner } from '../components/ui';

const emptyForm = {
    name: '',
    unit: 'pcs',
    quantityRequested: 0,
    salePrice: 0,
    unitCost: 0,
    discountAmount: 0,
    discountPercentage: 0,
    taxPercentage: 0,
    isManufactured: false,
};

export default function ProductsPage() {
    const [products, setProducts] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');
    const [form, setForm] = useState(emptyForm);
    const [showForm, setShowForm] = useState(false);

    const load = useCallback(async () => {
        setLoading(true);
        setError('');
        try {
            const data = await productsApi.getProducts();
            setProducts(Array.isArray(data) ? data : []);
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => { load(); }, [load]);

    const update = (field) => (e) => {
        const val = e.target.type === 'checkbox' ? e.target.checked : e.target.value;
        setForm({ ...form, [field]: val });
    };

    const handleCreate = async (e) => {
        e.preventDefault();
        setError('');
        setSuccess('');
        try {
            await productsApi.createProduct({
                ...form,
                quantityRequested: Number(form.quantityRequested),
                salePrice: Number(form.salePrice),
                unitCost: Number(form.unitCost),
                discountAmount: Number(form.discountAmount),
                discountPercentage: Number(form.discountPercentage),
                taxPercentage: Number(form.taxPercentage),
            });
            setSuccess('Product created');
            setForm(emptyForm);
            setShowForm(false);
            load();
        } catch (err) {
            setError(err.message);
        }
    };

    const handleDelete = async (id) => {
        if (!confirm('Delete this product?')) return;
        try {
            await productsApi.deleteProduct(id);
            load();
        } catch (err) {
            setError(err.message);
        }
    };

    return (
        <div>
            <PageHeader
                title="Products"
                description="Manage product catalog and inventory"
                actions={
                    <Button onClick={() => setShowForm(!showForm)}>
                        {showForm ? 'Cancel' : 'Add product'}
                    </Button>
                }
            />

            {error && <div className="mb-4"><Alert type="error">{error}</Alert></div>}
            {success && <div className="mb-4"><Alert type="success">{success}</Alert></div>}

            {showForm && (
                <Card title="New product" className="mb-6">
                    <form onSubmit={handleCreate} className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                        <Input label="Name" value={form.name} onChange={update('name')} required />
                        <Input label="Unit" value={form.unit} onChange={update('unit')} required />
                        <Input label="Initial stock" type="number" step="0.01" value={form.quantityRequested} onChange={update('quantityRequested')} />
                        <Input label="Sale price" type="number" step="0.01" value={form.salePrice} onChange={update('salePrice')} />
                        <Input label="Unit cost" type="number" step="0.01" value={form.unitCost} onChange={update('unitCost')} />
                        <Input label="Tax %" type="number" step="0.01" value={form.taxPercentage} onChange={update('taxPercentage')} />
                        <label className="flex items-center gap-2 text-sm">
                            <input type="checkbox" checked={form.isManufactured} onChange={update('isManufactured')} />
                            Manufactured in-house
                        </label>
                        <div className="sm:col-span-2 lg:col-span-3">
                            <Button type="submit">Create product</Button>
                        </div>
                    </form>
                </Card>
            )}

            <Card title={`Products (${products.length})`}>
                {loading ? (
                    <Spinner />
                ) : products.length === 0 ? (
                    <p className="text-sm text-slate-500">No products yet. Add your first product above.</p>
                ) : (
                    <div className="overflow-x-auto">
                        <table className="w-full text-left text-sm">
                            <thead>
                                <tr className="border-b border-slate-100 text-slate-500">
                                    <th className="pb-3 pr-4 font-medium">Code</th>
                                    <th className="pb-3 pr-4 font-medium">Name</th>
                                    <th className="pb-3 pr-4 font-medium">Unit</th>
                                    <th className="pb-3 pr-4 font-medium">Type</th>
                                    <th className="pb-3 font-medium">Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                {products.map((p) => (
                                    <tr key={p.id} className="border-b border-slate-50">
                                        <td className="py-3 pr-4 font-mono text-xs">{p.code}</td>
                                        <td className="py-3 pr-4">{p.name}</td>
                                        <td className="py-3 pr-4">{p.unit}</td>
                                        <td className="py-3 pr-4">
                                            <Badge color={p.isManufactured ? 'blue' : 'slate'}>
                                                {p.isManufactured ? 'Manufactured' : 'Purchased'}
                                            </Badge>
                                        </td>
                                        <td className="py-3">
                                            <Button variant="danger" className="px-2 py-1 text-xs" onClick={() => handleDelete(p.id)}>
                                                Delete
                                            </Button>
                                        </td>
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
