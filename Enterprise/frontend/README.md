# Frontend

The canonical ERP frontend is **`erp-app`** at the repository root:

```
D:/ERP/erp-app
```

Run it with:

```bash
cd erp-app
npm install
npm run dev
```

For Enterprise microservices, set gateway mode in `.env`:

```env
VITE_API_PROXY_TARGET=http://localhost:5000
VITE_API_MODE=gateway
```
