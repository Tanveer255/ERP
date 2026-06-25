import { FormEvent, useState } from 'react';
import { useDispatch } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { setTokens } from '../features/auth/authSlice';
import type { AppDispatch } from '../app/store';

export default function LoginPage() {
  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    try {
      const res = await fetch('/api/v1/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
      });
      if (!res.ok) throw new Error('Login failed');
      const data = await res.json();
      dispatch(setTokens({ accessToken: data.accessToken, refreshToken: data.refreshToken }));
      navigate('/');
    } catch {
      setError('Invalid credentials');
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-950">
      <form onSubmit={onSubmit} className="w-full max-w-md rounded-xl border border-slate-800 bg-slate-900 p-8">
        <h1 className="text-xl font-bold">Enterprise ERP</h1>
        {error && <p className="mt-4 text-sm text-red-400">{error}</p>}
        <input className="mt-4 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2" placeholder="Email" value={email} onChange={(e) => setEmail(e.target.value)} />
        <input className="mt-3 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2" type="password" placeholder="Password" value={password} onChange={(e) => setPassword(e.target.value)} />
        <button className="mt-6 w-full rounded bg-indigo-600 py-2 font-medium hover:bg-indigo-500" type="submit">Sign in</button>
      </form>
    </div>
  );
}
