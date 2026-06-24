export function Button({ children, variant = 'primary', className = '', ...props }) {
    const variants = {
        primary: 'bg-indigo-600 text-white hover:bg-indigo-700 focus:ring-indigo-500',
        secondary: 'bg-white text-slate-700 border border-slate-300 hover:bg-slate-50',
        danger: 'bg-red-600 text-white hover:bg-red-700 focus:ring-red-500',
        ghost: 'text-slate-600 hover:bg-slate-100',
    };

    return (
        <button
            type="button"
            className={`inline-flex items-center justify-center rounded-lg px-4 py-2 text-sm font-medium transition focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 ${variants[variant]} ${className}`}
            {...props}
        >
            {children}
        </button>
    );
}

export function Input({ label, error, className = '', ...props }) {
    return (
        <label className="block space-y-1">
            {label && <span className="text-sm font-medium text-slate-700">{label}</span>}
            <input
                className={`w-full rounded-lg border border-slate-300 px-3 py-2 text-sm shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500 ${error ? 'border-red-400' : ''} ${className}`}
                {...props}
            />
            {error && <span className="text-xs text-red-600">{error}</span>}
        </label>
    );
}

export function Select({ label, children, className = '', ...props }) {
    return (
        <label className="block space-y-1">
            {label && <span className="text-sm font-medium text-slate-700">{label}</span>}
            <select
                className={`w-full rounded-lg border border-slate-300 px-3 py-2 text-sm shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500 ${className}`}
                {...props}
            >
                {children}
            </select>
        </label>
    );
}

export function Card({ title, children, actions }) {
    return (
        <div className="rounded-xl border border-slate-200 bg-white shadow-sm">
            {(title || actions) && (
                <div className="flex items-center justify-between border-b border-slate-100 px-6 py-4">
                    {title && <h2 className="text-lg font-semibold text-slate-900">{title}</h2>}
                    {actions}
                </div>
            )}
            <div className="p-6">{children}</div>
        </div>
    );
}

export function Alert({ type = 'error', children }) {
    const styles = {
        error: 'border-red-200 bg-red-50 text-red-800',
        success: 'border-green-200 bg-green-50 text-green-800',
        info: 'border-blue-200 bg-blue-50 text-blue-800',
    };
    return (
        <div className={`rounded-lg border px-4 py-3 text-sm ${styles[type]}`}>{children}</div>
    );
}

export function Spinner() {
    return (
        <div className="flex items-center justify-center py-12">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-indigo-200 border-t-indigo-600" />
        </div>
    );
}

export function Badge({ children, color = 'slate' }) {
    const colors = {
        slate: 'bg-slate-100 text-slate-700',
        green: 'bg-green-100 text-green-800',
        yellow: 'bg-yellow-100 text-yellow-800',
        blue: 'bg-blue-100 text-blue-800',
        red: 'bg-red-100 text-red-800',
    };
    return (
        <span className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${colors[color]}`}>
            {children}
        </span>
    );
}

export function PageHeader({ title, description, actions }) {
    return (
        <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div>
                <h1 className="text-2xl font-bold text-slate-900">{title}</h1>
                {description && <p className="mt-1 text-sm text-slate-500">{description}</p>}
            </div>
            {actions}
        </div>
    );
}
