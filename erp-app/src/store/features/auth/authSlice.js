import { createSlice } from '@reduxjs/toolkit';

const initialState = {
    accessToken: null,
    user: null,
    isAuthenticated: false,
};

const authSlice = createSlice({
    name: 'auth',
    initialState,
    reducers: {
        setUser(state, action) {
            state.user = action.payload;
            state.isAuthenticated = !!action.payload;
        },
        setAccessToken(state, action) {
            state.accessToken = action.payload;
        },
        clearAuth(state) {
            state.user = null;
            state.accessToken = null;
            state.isAuthenticated = false;
        },
    },
});

export const { setUser, setAccessToken, clearAuth } = authSlice.actions;
export default authSlice.reducer;
