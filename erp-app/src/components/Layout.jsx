import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Button } from './ui';

const navItems = [
    { to: '/', label: 'Dashboard', end: true },
    { to: '/products', label: 'Products' },
    { to: '/sales', label: 'Sales Orders' },
    { to: '/production', label: 'Production' },
    { to: '/purchase-orders', label: 'Purchase Orders' },
    { to: '/suppliers', label: 'Suppliers' },
    { to: '/bom', label: 'Bill of Materials' },
    { to: '/profile', label: 'Profile' },
];

export default function Layout() {
    const { user, logout } = useAuth();
    const navigate = useNavigate();

    const handleLogout = async () => {
        await logout();
        navigate('/login');
    };

    const displayName = user?.fullName ?? user?.FullName ?? user?.email ?? user?.Email ?? 'User';

    return (
        <div className="flex min-h-screen">
            <aside className="hidden w-64 flex-shrink-0 border-r border-slate-200 bg-white lg:block">
                <div className="border-b border-slate-100 px-6 py-5">
                    <p className="text-lg font-bold text-indigo-600">ERP</p>
                    <p className="text-xs text-slate-500">Manufacturing Suite</p>
                </div>
                <nav className="space-y-1 p-4">
                    {navItems.map((item) => (
                        <NavLink
                            key={item.to}
                            to={item.to}
                            end={item.end}
                            className={({ isActive }) =>
                                `block rounded-lg px-3 py-2 text-sm font-medium transition ${
                                    isActive
                                        ? 'bg-indigo-50 text-indigo-700'
                                        : 'text-slate-600 hover:bg-slate-50 hover:text-slate-900'
                                }`
                            }
                        >
                            {item.label}
                        </NavLink>
                    ))}
                </nav>
            </aside>

            <div className="flex flex-1 flex-col">
                <header className="flex items-center justify-between border-b border-slate-200 bg-white px-6 py-4">
                    <div className="lg:hidden">
                        <p className="text-lg font-bold text-indigo-600">ERP</p>
                    </div>
                    <div className="ml-auto flex items-center gap-4">
                        <span className="hidden text-sm text-slate-600 sm:block">{displayName}</span>
                        <Button variant="secondary" onClick={handleLogout}>
                            Log out
                        </Button>
                    </div>
                </header>

                <main className="flex-1 overflow-auto p-6">
                    <Outlet />
                </main>
            </div>
        </div>
    );
}
