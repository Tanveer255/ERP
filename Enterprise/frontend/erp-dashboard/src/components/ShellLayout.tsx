import { Outlet, NavLink } from 'react-router-dom';

const modules = [
  'Manufacturing', 'Inventory', 'Sales', 'Procurement', 'Quality', 'Finance', 'HR', 'Reporting',
];

export default function ShellLayout() {
  return (
    <div className="flex min-h-screen">
      <aside className="w-64 border-r border-slate-800 bg-slate-900 p-4">
        <h1 className="text-lg font-bold text-indigo-400">Enterprise ERP</h1>
        <nav className="mt-6 space-y-1">
          {modules.map((m) => (
            <NavLink key={m} to="/" className="block rounded px-3 py-2 text-sm text-slate-300 hover:bg-slate-800">
              {m}
            </NavLink>
          ))}
        </nav>
      </aside>
      <main className="flex-1 p-8">
        <Outlet />
      </main>
    </div>
  );
}
