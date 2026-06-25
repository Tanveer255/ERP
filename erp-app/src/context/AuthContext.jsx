import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { ApiError, API_MODE, setAccessTokenGetter } from '../api/client';
import * as authApi from '../api/auth';
import { useAppDispatch } from '../store/hooks';
import { clearAuth, setAccessToken, setUser } from '../store/features/auth/authSlice';

const AuthContext = createContext(null);
const TOKEN_KEY = 'erp_access_token';

export function AuthProvider({ children }) {
    const dispatch = useAppDispatch();
    const [user, setUserState] = useState(null);
    const [accessToken, setAccessTokenState] = useState(
        () => localStorage.getItem(TOKEN_KEY) ?? null
    );
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        setAccessTokenGetter(() => accessToken);
    }, [accessToken]);

    const syncUser = useCallback(
        (profile) => {
            setUserState(profile);
            dispatch(setUser(profile));
        },
        [dispatch]
    );

    const refreshProfile = useCallback(async () => {
        try {
            const result = await authApi.getProfile();
            if (result?.succeeded ?? result?.Succeeded) {
                const profile = result.data ?? result.Data;
                syncUser(profile);
                return true;
            }
            syncUser(null);
            return false;
        } catch (err) {
            if (err instanceof ApiError && err.status === 401) {
                syncUser(null);
                return false;
            }
            syncUser(null);
            return false;
        }
    }, [syncUser]);

    useEffect(() => {
        refreshProfile().finally(() => setLoading(false));
    }, [refreshProfile]);

    const login = async (email, password) => {
        const result = await authApi.login(email, password);
        const ok = result?.succeeded ?? result?.Succeeded;
        if (!ok) {
            throw new Error(result?.message ?? result?.Message ?? 'Login failed');
        }

        if (API_MODE === 'gateway') {
            const token = result?.data?.accessToken ?? result?.Data?.accessToken ?? result?.accessToken;
            if (token) {
                setAccessTokenState(token);
                localStorage.setItem(TOKEN_KEY, token);
                dispatch(setAccessToken(token));
            }
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
        setAccessTokenState(null);
        localStorage.removeItem(TOKEN_KEY);
        dispatch(clearAuth());
        syncUser(null);
    };

    const value = useMemo(
        () => ({ user, loading, login, signup, logout, refreshProfile, isAuthenticated: !!user, accessToken }),
        [user, loading, refreshProfile, accessToken]
    );

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
    const ctx = useContext(AuthContext);
    if (!ctx) throw new Error('useAuth must be used within AuthProvider');
    return ctx;
}
