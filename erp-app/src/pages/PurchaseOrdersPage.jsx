import { useCallback, useEffect, useState } from 'react';
import * as purchaseOrdersApi from '../api/purchaseOrders';
import { Alert, Badge, Button, Card, PageHeader, Spinner } from '../components/ui';

export default function PurchaseOrdersPage() {
    const [orders, setOrders] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');

    const load = useCallback(async () => {
        setLoading(true);
        try {
            const data = await purchaseOrdersApi.getPurchaseOrders();
            setOrders(Array.isArray(data) ? data : []);
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => { load(); }, [load]);

    const handleReceive = async (id) => {
        setError('');
        setSuccess('');
        try {
            await purchaseOrdersApi.receivePurchaseOrder(id);
            setSuccess('Purchase order received');
            load();
        } catch (err) {
            setError(err.message);
        }
    };

    return (
        <div>
            <PageHeader title="Purchase Orders" description="Receive goods from suppliers" />

            {error && <div className="mb-4"><Alert type="error">{error}</Alert></div>}
            {success && <div className="mb-4"><Alert type="success">{success}</Alert></div>}

            <Card title={`Purchase orders (${orders.length})`}>
                {loading ? <Spinner /> : orders.length === 0 ? (
                    <p className="text-sm text-slate-500">No purchase orders. They are created automatically via MRP.</p>
                ) : (
                    <div className="space-y-4">
                        {orders.map((order) => (
                            <div key={order.id} className="rounded-lg border border-slate-100 p-4">
                                <div className="flex flex-wrap items-center justify-between gap-2">
                                    <div>
                                        <p className="font-medium">{order.orderNumber}</p>
                                        <p className="text-sm text-slate-500">{order.items?.length ?? 0} line items</p>
                                    </div>
                                    <Badge color={String(order.status).includes('Received') ? 'green' : 'yellow'}>
                                        {String(order.status)}
                                    </Badge>
                                </div>
                                {String(order.status) !== 'Received' && (
                                    <div className="mt-3">
                                        <Button variant="secondary" className="text-xs" onClick={() => handleReceive(order.id)}>
                                            Receive order
                                        </Button>
                                    </div>
                                )}
                            </div>
                        ))}
                    </div>
                )}
            </Card>
        </div>
    );
}
