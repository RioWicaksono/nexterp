# NEXTERP FRONTEND GUIDELINES

Project-specific frontend standards for NEXTERP ERP system.

---

## TECH STACK

| Technology | Version | Purpose |
|------------|---------|---------|
| Next.js | 14.2 | React framework |
| React | 18 | UI library |
| TypeScript | 5.x | Type safety |
| TailwindCSS | 3.x | Styling |
| Lucide React | latest | Icons |

---

## PROJECT STRUCTURE

```
ERP.WebUI/
├── src/
│   ├── app/                    # App Router (Next.js 14)
│   │   ├── (auth)/            # Auth pages (login, register)
│   │   ├── (dashboard)/       # Protected pages
│   │   │   ├── inventory/     # INVENTORY module
│   │   │   ├── sales/         # SALES module
│   │   │   ├── hr/            # HRM module
│   │   │   └── admin/         # SuperAdmin pages
│   │   ├── api/               # API routes (if needed)
│   │   ├── layout.tsx         # Root layout
│   │   └── page.tsx          # Landing page
│   ├── components/
│   │   ├── ui/                # Base UI components
│   │   ├── forms/             # Form components
│   │   ├── tables/           # Data table components
│   │   └── layout/            # Layout components (sidebar, header)
│   ├── lib/
│   │   ├── api.ts             # API client
│   │   ├── auth.ts            # Auth utilities
│   │   └── utils.ts           # Helper functions
│   └── types/
│       ├── api.ts             # API types
│       └── domain.ts          # Domain types
└── public/                    # Static assets
```

---

## API CLIENT PATTERN

### Centralized API Client
```typescript
// src/lib/api.ts
const API_BASE = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

interface ApiOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';
  body?: object;
  headers?: Record<string, string>;
}

export async function api<T>(endpoint: string, options: ApiOptions = {}) {
  const token = getAuthToken();
  
  const response = await fetch(`${API_BASE}${endpoint}`, {
    method: options.method || 'GET',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': token ? `Bearer ${token}` : '',
      ...options.headers,
    },
    body: options.body ? JSON.stringify(options.body) : undefined,
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || 'API Error');
  }

  return response.json() as Promise<T>;
}
```

### Usage
```typescript
// GET request
const employees = await api<Employee[]>('/api/v1/employees');

// POST request
const newEmployee = await api<Employee>('/api/v1/employees', {
  method: 'POST',
  body: employeeData,
});
```

---

## COMPONENT PATTERNS

### Server Components (Default)
```typescript
// src/app/inventory/page.tsx
export default async function InventoryPage() {
  const data = await fetchInventoryData();
  
  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold">Inventory</h1>
      <InventoryTable data={data} />
    </div>
  );
}
```

### Client Components (Interactive)
```typescript
'use client';

import { useState } from 'react';

export function InventoryForm() {
  const [loading, setLoading] = useState(false);
  
  // Client-side interactivity
}
```

### Data Tables
```typescript
import { DataTable } from '@/components/tables/DataTable';
import { columns } from './columns';

export function InventoryTable({ data }: { data: Inventory[] }) {
  return <DataTable columns={columns} data={data} />;
}
```

---

## FORM HANDLING

### React Hook Form + Zod
```typescript
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';

const schema = z.object({
  name: z.string().min(1, 'Name is required'),
  code: z.string().min(1, 'Code is required'),
});

export function EmployeeForm() {
  const form = useForm<z.infer<typeof schema>>({
    resolver: zodResolver(schema),
    defaultValues: initialData,
  });

  const onSubmit = async (data: z.infer<typeof schema>) => {
    await api('/api/v1/employees', { method: 'POST', body: data });
  };

  return <Form {...form} />;
}
```

---

## AUTHENTICATION FLOW

### Protected Routes
```typescript
// middleware.ts
import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

export function middleware(request: NextRequest) {
  const token = request.cookies.get('auth_token');
  
  if (!token && !isPublicRoute(request.nextUrl.pathname)) {
    return NextResponse.redirect(new URL('/login', request.url));
  }
  
  return NextResponse.next();
}

export const config = {
  matcher: ['/((?!login|register|api).)*'],
};
```

### Login Page
```typescript
'use client';

export function LoginForm() {
  const [error, setError] = useState('');
  
  const onSubmit = async (data: LoginFormData) => {
    try {
      const response = await api<AuthResponse>('/api/v1/auth/login', {
        method: 'POST',
        body: data,
      });
      
      // Store token and redirect
      cookies().set('auth_token', response.token);
      router.push('/dashboard');
    } catch (err) {
      setError('Invalid credentials');
    }
  };
}
```

---

## STYLING CONVENTIONS

### TailwindCSS Classes
- Use utility classes for rapid development
- Extract repeated patterns to components
- Maintain consistent spacing scale

### Dark Mode
```typescript
// Use system preference
<div className="dark:bg-gray-900 dark:text-white">
```

---

## ERROR HANDLING

### API Error Display
```typescript
try {
  await api('/api/v1/resource', { method: 'POST', body: data });
} catch (error) {
  toast.error(error.message || 'Failed to save');
}
```

### Form Validation Errors
```typescript
const { formState: { errors } } = useForm();
return (
  <>
    {errors.name && (
      <span className="text-red-500 text-sm">{errors.name.message}</span>
    )}
  </>
);
```

---

## PERFORMANCE OPTIMIZATION

### Image Optimization
```typescript
import Image from 'next/image';

<Image 
  src="/logo.png" 
  alt="NEXTERP Logo"
  width={120}
  height={40}
/>
```

### Code Splitting
```typescript
const HeavyComponent = dynamic(() => import('@/components/HeavyComponent'), {
  loading: () => <Skeleton />,
});
```

---

**Auto-loaded for:** Frontend tasks in NEXTERP
