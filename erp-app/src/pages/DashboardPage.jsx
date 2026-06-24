import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Card, PageHeader } from '../components/ui';

const quickLinks = [
    { to: '/products', title: 'Products', desc: 'Manage inventory and pricing' },
    { to: '/sales', title: 'Sales Orders', desc: 'Create and fulfill customer orders' },
    { to: '/production', title: 'Production', desc: 'Run manufacturing workflows' },
    { to: '/purchase-orders', title: 'Purchase Orders', desc: 'Receive supplier deliveries' },
    { to: '/suppliers', title: 'Suppliers', desc: 'Manage vendor contacts' },
    { to: '/bom', title: 'Bill of Materials', desc: 'Define product components' },
];

export default function DashboardPage() {
    const { user } = useAuth();
    const name = user?.fullName ?? user?.FullName ?? 'there';

    return (
        <div>
            <PageHeader
                title={`Welcome, ${name}`}
                description="Your manufacturing ERP dashboard"
            />

            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                {quickLinks.map((link) => (
                    <Link key={link.to} to={link.to}>
                        <Card title={link.title}>
                            <p className="text-sm text-slate-500">{link.desc}</p>
                        </Card>
                    </Link>
                ))}
            </div>
        </div>
    );
}
