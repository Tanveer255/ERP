import { useAuth } from '../context/AuthContext';
import { Card, PageHeader } from '../components/ui';

export default function ProfilePage() {
    const { user } = useAuth();

    const fields = [
        { label: 'Email', value: user?.email ?? user?.Email },
        { label: 'Full name', value: user?.fullName ?? user?.FullName },
        { label: 'First name', value: user?.firstName ?? user?.FirstName },
        { label: 'Last name', value: user?.lastName ?? user?.LastName },
        { label: 'Phone', value: user?.phoneNumber ?? user?.PhoneNumber },
        { label: 'Tenant ID', value: user?.tenantId ?? user?.TenantId },
    ];

    return (
        <div>
            <PageHeader title="Profile" description="Your account information" />
            <Card title="Account details">
                <dl className="divide-y divide-slate-100">
                    {fields.map(({ label, value }) => (
                        <div key={label} className="flex justify-between py-3 text-sm">
                            <dt className="font-medium text-slate-500">{label}</dt>
                            <dd className="text-slate-900">{value || '—'}</dd>
                        </div>
                    ))}
                </dl>
            </Card>
        </div>
    );
}
