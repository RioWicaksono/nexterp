# NEXTERP Module System

Modul-modul dalam sistem NEXTERP dapat diaktifkan atau dinonaktifkan dengan mudah melalui konfigurasi.

## Struktur Folder

```
modules/
├── module-manifest.json    # Konfigurasi utama semua modul
├── sales/                 # Modul Penjualan
│   ├── module.config.json # Konfigurasi modul
│   └── README.md         # Dokumentasi modul
├── inventory/            # Modul Inventory
├── purchasing/           # Modul Pengadaan
├── hrm/                  # Modul HRM (Payroll, Attendance)
├── accounting/           # Modul Akuntansi
├── projects/             # Modul Proyek
├── quality/              # Modul Quality
├── assets/               # Modul Aset
└── analytics/            # Modul Analytics
```

## Tiers (Paket Lisensi)

| Tier | Modul | Deskripsi |
|------|-------|-----------|
| **STARTER** | SALES, INVENTORY, PURCHASING | Untuk usaha kecil |
| **PROFESSIONAL** | + HRM, ACCOUNTING | Untuk usaha menengah |
| **ENTERPRISE** | + PROJECTS, QUALITY, ASSETS, ANALYTICS | Suite lengkap |

## Mengaktifkan/Menonaktifkan Modul

### 1. Melalui `module-manifest.json`

```json
{
  "modules": {
    "sales": {
      "enabled": true,  // ubah ke false untuk menonaktifkan
      "tier": "starter"
    },
    "hrm": {
      "enabled": false, // ubah ke true untuk mengaktifkan
      "tier": "professional"
    }
  }
}
```

### 2. Melalui `module.config.json` (per modul)

```json
{
  "module": "hrm",
  "enabled": true,
  "features": {
    "payroll": {
      "enabled": false  // Nonaktifkan fitur payroll saja
    }
  }
}
```

## Modul yang Tersedia

### Starter Modules (Selalu Tersedia)
- **SALES** - Manajemen pelanggan, pesanan, faktur
- **INVENTORY** - Manajemen stok, gudang, batch/serial
- **PURCHASING** - Manajemen supplier, PO, receipt

### Professional Modules
- **HRM** - Karyawan, absensi, cuti, payroll
- **ACCOUNTING** - Chart of accounts, jurnal, laporan keuangan

### Enterprise Modules
- **PROJECTS** - Manajemen proyek, task, Gantt
- **QUALITY** - Inspeksi, NCR, CAPA
- **ASSETS** - Aset tetap, depresiasi, maintenance
- **ANALYTICS** - Dashboard, KPI, reporting

## API Endpoints

### Get All Modules
```
GET /api/v1/modules
```

### Get Module Status
```
GET /api/v1/modules/{code}/status
```

### Get Tiers
```
GET /api/v1/modules/tiers
```

### Check Module Access
```
GET /api/v1/modules/{code}/access
```

## Contoh Penggunaan

### C# - Memeriksa Modul
```csharp
public class PayrollController : Controller
{
    private readonly IModuleManager _moduleManager;

    public IActionResult Calculate()
    {
        if (!_moduleManager.IsModuleEnabled("HRM"))
            return Forbid("HRM module is not enabled");

        // ... logic
    }
}
```

### C# - Memeriksa Fitur
```csharp
if (_moduleManager.IsFeatureEnabled("HRM", "payroll"))
{
    // Payroll feature is enabled
}
```

### API Filter - Otomatis Block
```csharp
[RequireModule("HRM")]
public class PayrollController : Controller
{
    // Semua action di controller ini require HRM module
}

[RequireFeature("HRM", "payroll")]
public IActionResult CalculatePayroll()
{
    // Hanya berjalan jika fitur payroll enabled
}
```

## Konfigurasi HRM

Modul HRM memiliki konfigurasi khusus untuk payroll Indonesia:

```json
{
  "module": "hrm",
  "settings": {
    "pph21Calculation": true,
    "bpjsKerjaEnabled": true,
    "bpjsKesehatanEnabled": true,
    "thrEnabled": true,
    "overtimeRateMultiplier": 1.5
  },
  "payroll": {
    "pph21": {
      "method": "gross",
      "ptkpRates": {
        "tk0": 54000000,
        "k0": 58500000
      }
    },
    "bpjs": {
      "jht": { "employeeRate": 0.02, "employerRate": 0.037 },
      "jp": { "employeeRate": 0.01, "employerRate": 0.02 },
      "kesehatan": { "employeeRate": 0.01, "employerRate": 0.04 }
    }
  }
}
```

## Deployment Notes

1. Module manifest dibaca saat startup
2. Untuk perubahan runtime, restart aplikasi diperlukan
3. Module access di-cache per request
4. SuperAdmin bypass semua module checks
