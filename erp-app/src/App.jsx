import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import Layout from './components/Layout';
import ProtectedRoute from './components/ProtectedRoute';
import LoginPage from './pages/LoginPage';
import SignupPage from './pages/SignupPage';
import DashboardPage from './pages/DashboardPage';
import ProfilePage from './pages/ProfilePage';
import ProductsPage from './pages/ProductsPage';
import SalesPage from './pages/SalesPage';
import ProductionPage from './pages/ProductionPage';
import PurchaseOrdersPage from './pages/PurchaseOrdersPage';
import SuppliersPage from './pages/SuppliersPage';
import InventoryPage from './pages/InventoryPage';
import BOMPage from './pages/BOMPage';

export default function App() {
    return (
        <AuthProvider>
            <BrowserRouter>
                <Routes>
                    <Route path="/login" element={<LoginPage />} />
                    <Route path="/signup" element={<SignupPage />} />
                    <Route element={<ProtectedRoute />}>
                        <Route element={<Layout />}>
                            <Route index element={<DashboardPage />} />
                            <Route path="products" element={<ProductsPage />} />
                            <Route path="inventory" element={<InventoryPage />} />
                            <Route path="sales" element={<SalesPage />} />
                            <Route path="production" element={<ProductionPage />} />
                            <Route path="purchase-orders" element={<PurchaseOrdersPage />} />
                            <Route path="suppliers" element={<SuppliersPage />} />
                            <Route path="bom" element={<BOMPage />} />
                            <Route path="profile" element={<ProfilePage />} />
                        </Route>
                    </Route>
                    <Route path="*" element={<Navigate to="/" replace />} />
                </Routes>
            </BrowserRouter>
        </AuthProvider>
    );
}
