import { useCallback, useEffect, useState } from 'react';
import * as inventoryApi from '../api/inventory';
import { API_MODE } from '../api/client';
import { Alert, Badge, Card, PageHeader, Spinner } from '../components/ui';

export default function InventoryPage() {
    const [stock, setStock] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    const load = useCallback(async () => {
        setLoading(true);
        setError('');
        try {
            const data = await inventoryApi.getStock();
            setStock(Array.isArray(data) ? data : []);
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        load();
    }, [load]);

    return (
        <div>
            <PageHeader
                title="Inventory"
                description={
                    API_MODE === 'gateway'
                        ? 'Stock balances from the Enterprise Inventory service'
                        : 'Stock levels derived from product records (monolith mode)'
                }
            />

            {error && <Alert type="error">{error}</Alert>}

            {loading ? (
                <Spinner />
            ) : (
                <Card title={`${stock.length} item(s)`}>
                    <div className="overflow-x-auto">
                        <table className="min-w-full text-sm">
                            <thead>
                                <tr className="border-b border-slate-200 text-left text-slate-500">
                                    <th className="px-3 py-2">Product</th>
                                    <th className="px-3 py-2">SKU / Name</th>
                                    <th className="px-3 py-2">On hand</th>
                                    <th className="px-3 py-2">Unit</th>
                                </tr>
                            </thead>
                            <tbody>
                                {stock.map((row) => (
                                    <tr key={row.productId ?? row.id ?? row.sku} className="border-b border-slate-100">
                                        <td className="px-3 py-2 font-mono text-xs text-slate-500">
                                            {(row.productId ?? row.id ?? '').toString().slice(0, 8)}
                                        </td>
                                        <td className="px-3 py-2">{row.sku ?? row.name ?? '—'}</td>
                                        <td className="px-3 py-2">
                                            <Badge color={(row.quantityOnHand ?? 0) > 0 ? 'green' : 'yellow'}>
                                                {row.quantityOnHand ?? row.quantity ?? 0}
                                            </Badge>
                                        </td>
                                        <td className="px-3 py-2">{row.unit ?? 'pcs'}</td>
                                    </tr>
                                ))}
                                {stock.length === 0 && (
                                    <tr>
                                        <td colSpan={4} className="px-3 py-6 text-center text-slate-500">
                                            No stock records found.
                                        </td>
                                    </tr>
                                )}
                            </tbody>
                        </table>
                    </div>
                </Card>
            )}
        </div>
    );
}
