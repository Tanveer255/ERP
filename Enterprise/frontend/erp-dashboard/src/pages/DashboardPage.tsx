export default function DashboardPage() {
  const kpis = [
    { label: 'Open Production Orders', value: '128' },
    { label: 'Stock Accuracy', value: '98.4%' },
    { label: 'OTIF', value: '94.1%' },
    { label: 'Plant Utilization', value: '82.7%' },
  ];

  return (
    <div>
      <h2 className="text-2xl font-semibold">Manufacturing Dashboard</h2>
      <p className="mt-1 text-slate-400">Real-time KPIs across plants and warehouses</p>
      <div className="mt-8 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {kpis.map((kpi) => (
          <div key={kpi.label} className="rounded-xl border border-slate-800 bg-slate-900 p-5">
            <p className="text-sm text-slate-400">{kpi.label}</p>
            <p className="mt-2 text-3xl font-bold text-indigo-300">{kpi.value}</p>
          </div>
        ))}
      </div>
    </div>
  );
}
