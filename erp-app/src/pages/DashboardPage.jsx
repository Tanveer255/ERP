import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { API_MODE } from '../api/client';
import { Card, PageHeader } from '../components/ui';

const quickLinks = [
    { to: '/products', title: 'Products', desc: 'Manage inventory and pricing' },
    { to: '/inventory', title: 'Inventory', desc: 'View stock levels and balances' },
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
                description={`Your manufacturing ERP dashboard (${API_MODE} API mode)`}
            />

            <div className="mb-6 grid gap-4 sm:grid-cols-3">
                <Card title="API mode">
                    <p className="text-2xl font-semibold text-indigo-600">{API_MODE}</p>
                    <p className="text-sm text-slate-500">
                        {API_MODE === 'gateway'
                            ? 'Connected via Enterprise API Gateway (JWT Bearer)'
                            : 'Connected to ASP.NET monolith (cookie auth)'}
                    </p>
                </Card>
                <Card title="Modules">
                    <p className="text-2xl font-semibold text-indigo-600">{quickLinks.length}</p>
                    <p className="text-sm text-slate-500">Active business modules</p>
                </Card>
                <Card title="Frontend">
                    <p className="text-2xl font-semibold text-indigo-600">erp-app</p>
                    <p className="text-sm text-slate-500">React 19 + Redux Toolkit + Tailwind v4</p>
                </Card>
            </div>

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
