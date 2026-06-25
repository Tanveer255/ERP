import { createSlice } from '@reduxjs/toolkit';

const uiSlice = createSlice({
    name: 'ui',
    initialState: { sidebarOpen: true, lastError: null, lastSuccess: null },
    reducers: {
        setSidebarOpen(state, action) {
            state.sidebarOpen = action.payload;
        },
        setLastError(state, action) {
            state.lastError = action.payload;
            state.lastSuccess = null;
        },
        setLastSuccess(state, action) {
            state.lastSuccess = action.payload;
            state.lastError = null;
        },
        clearMessages(state) {
            state.lastError = null;
            state.lastSuccess = null;
        },
    },
});

export const { setSidebarOpen, setLastError, setLastSuccess, clearMessages } = uiSlice.actions;
export default uiSlice.reducer;
