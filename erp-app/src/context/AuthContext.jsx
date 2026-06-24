import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { ApiError } from '../api/client';
import * as authApi from '../api/auth';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    const refreshProfile = useCallback(async () => {
        try {
            const result = await authApi.getProfile();
            if (result?.succeeded ?? result?.Succeeded) {
                setUser(result.data ?? result.Data);
                return true;
            }
            setUser(null);
            return false;
        } catch (err) {
            if (err instanceof ApiError && err.status === 401) {
                setUser(null);
                return false;
            }
            setUser(null);
            return false;
        }
    }, []);

    useEffect(() => {
        refreshProfile().finally(() => setLoading(false));
    }, [refreshProfile]);

    const login = async (email, password) => {
        const result = await authApi.login(email, password);
        const ok = result?.succeeded ?? result?.Succeeded;
        if (!ok) {
            throw new Error(result?.message ?? result?.Message ?? 'Login failed');
        }
        await refreshProfile();
        return result;
    };

    const signup = async (payload) => {
        const result = await authApi.signup(payload);
        const ok = result?.succeeded ?? result?.Succeeded;
        if (!ok) {
            throw new Error(result?.message ?? result?.Message ?? 'Signup failed');
        }
        return result;
    };

    const logout = async () => {
        if (user?.email ?? user?.Email) {
            try {
                await authApi.logout(user.email ?? user.Email);
            } catch {
                // ignore logout errors
            }
        }
        setUser(null);
    };

    const value = useMemo(
        () => ({ user, loading, login, signup, logout, refreshProfile, isAuthenticated: !!user }),
        [user, loading, refreshProfile]
    );

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
    const ctx = useContext(AuthContext);
    if (!ctx) throw new Error('useAuth must be used within AuthProvider');
    return ctx;
}
