import { useState } from 'react';
import { Link, Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Alert, Button, Input } from '../components/ui';

export default function SignupPage() {
    const { signup, isAuthenticated, loading } = useAuth();
    const [form, setForm] = useState({
        email: '',
        password: '',
        firstName: '',
        lastName: '',
        businessName: '',
        phoneNumber: '',
        countryCode: '',
        isTermsAgreed: true,
    });
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');
    const [submitting, setSubmitting] = useState(false);

    if (!loading && isAuthenticated) return <Navigate to="/" replace />;

    const update = (field) => (e) => setForm({ ...form, [field]: e.target.value });

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setSuccess('');
        setSubmitting(true);
        try {
            const result = await signup(form);
            setSuccess(result?.message ?? result?.Message ?? 'Account created. Check your email to confirm.');
        } catch (err) {
            setError(err.message || 'Signup failed');
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-slate-900 via-indigo-950 to-slate-900 px-4 py-12">
            <div className="w-full max-w-lg rounded-2xl border border-white/10 bg-white p-8 shadow-2xl">
                <div className="mb-8 text-center">
                    <h1 className="text-2xl font-bold text-slate-900">Create account</h1>
                    <p className="mt-2 text-sm text-slate-500">Start managing your manufacturing ERP</p>
                </div>

                {error && <div className="mb-4"><Alert type="error">{error}</Alert></div>}
                {success && <div className="mb-4"><Alert type="success">{success}</Alert></div>}

                <form onSubmit={handleSubmit} className="grid gap-4 sm:grid-cols-2">
                    <Input label="First name" value={form.firstName} onChange={update('firstName')} required />
                    <Input label="Last name" value={form.lastName} onChange={update('lastName')} required />
                    <div className="sm:col-span-2">
                        <Input label="Business name" value={form.businessName} onChange={update('businessName')} required />
                    </div>
                    <div className="sm:col-span-2">
                        <Input label="Email" type="email" value={form.email} onChange={update('email')} required />
                    </div>
                    <Input label="Phone" value={form.phoneNumber} onChange={update('phoneNumber')} />
                    <Input label="Country code" value={form.countryCode} onChange={update('countryCode')} placeholder="US" />
                    <div className="sm:col-span-2">
                        <Input label="Password" type="password" value={form.password} onChange={update('password')} required />
                    </div>
                    <div className="sm:col-span-2">
                        <Button type="submit" className="w-full" disabled={submitting}>
                            {submitting ? 'Creating account…' : 'Create account'}
                        </Button>
                    </div>
                </form>

                <p className="mt-6 text-center text-sm text-slate-500">
                    Already have an account?{' '}
                    <Link to="/login" className="font-medium text-indigo-600 hover:text-indigo-500">
                        Sign in
                    </Link>
                </p>
            </div>
        </div>
    );
}
