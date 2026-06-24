import { useState } from 'react';
import { Link, Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Alert, Button, Input } from '../components/ui';

export default function LoginPage() {
    const { login, isAuthenticated, loading } = useAuth();
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [submitting, setSubmitting] = useState(false);

    if (!loading && isAuthenticated) return <Navigate to="/" replace />;

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setSubmitting(true);
        try {
            await login(email, password);
        } catch (err) {
            setError(err.message || 'Login failed');
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-slate-900 via-indigo-950 to-slate-900 px-4">
            <div className="w-full max-w-md rounded-2xl border border-white/10 bg-white p-8 shadow-2xl">
                <div className="mb-8 text-center">
                    <h1 className="text-2xl font-bold text-slate-900">Sign in to ERP</h1>
                    <p className="mt-2 text-sm text-slate-500">Manufacturing resource planning</p>
                </div>

                {error && (
                    <div className="mb-4">
                        <Alert type="error">{error}</Alert>
                    </div>
                )}

                <form onSubmit={handleSubmit} className="space-y-4">
                    <Input
                        label="Email"
                        type="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        required
                        autoComplete="email"
                    />
                    <Input
                        label="Password"
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                        autoComplete="current-password"
                    />
                    <Button type="submit" className="w-full" disabled={submitting}>
                        {submitting ? 'Signing in…' : 'Sign in'}
                    </Button>
                </form>

                <p className="mt-6 text-center text-sm text-slate-500">
                    No account?{' '}
                    <Link to="/signup" className="font-medium text-indigo-600 hover:text-indigo-500">
                        Create one
                    </Link>
                </p>
            </div>
        </div>
    );
}
