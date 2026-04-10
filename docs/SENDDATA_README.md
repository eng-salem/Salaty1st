# Send Test Data to Turso - Using .NET Client

## Method 1: Turso Dashboard (EASIEST - RECOMMENDED)

Just copy-paste SQL into the Turso dashboard:

1. Go to: https://app.turso.tech/manhag/databases/counter/data
2. Click **SQL** tab
3. Paste and run:

```sql
CREATE TABLE IF NOT EXISTS device_counters (
    device_id TEXT PRIMARY KEY,
    machine_name TEXT,
    os_version TEXT,
    dotnet_version TEXT,
    app_version TEXT,
    first_seen TEXT,
    last_seen TEXT,
    total_launches INTEGER DEFAULT 0,
    country TEXT
);

INSERT INTO device_counters 
(device_id, machine_name, os_version, dotnet_version, app_version, first_seen, last_seen, total_launches, country)
VALUES 
('4DAF26FB53D49397', 'SALEM-AHMED', 'Microsoft Windows NT 10.0.26200.0', '4.0.30319.42000', '1.0.10', datetime('now'), datetime('now'), 1, 'EG')
ON CONFLICT(device_id) DO UPDATE SET 
    last_seen = datetime('now'), 
    total_launches = total_launches + 1, 
    app_version = '1.0.10';
```

---

## Method 2: .NET Client (Libsql.Client)

**NOTE:** The Libsql.Client package API is different from what the Turso docs show. The package uses a different namespace.

### Setup

```bash
cd TursoTestApp
dotnet add package Libsql.Client
```

### Working Code Example

```csharp
using Libsql.Client;

// Connect and execute
var db = await Libsql.Client.Database.Open(url, authToken);
await db.ExecuteAsync("SQL HERE");
```

**However**, the current Libsql.Client 0.5.1 package has API differences. 

---

## Method 3: Use SQLite-net-pcl (Alternative)

```bash
dotnet add package sqlite-net-pcl
```

This works with libSQL databases but requires additional setup.

---

## RECOMMENDATION

**Use the Turso Dashboard** (Method 1). It's:
- ✅ Instant - no setup required
- ✅ Reliable - direct database access
- ✅ Visual - you can see the data immediately
- ✅ Simple - just copy-paste SQL

The .NET client libraries are still evolving and may have API inconsistencies.

---

## Your Test Data (Copy This)

```sql
-- Your PC's exact test data
INSERT INTO device_counters 
(device_id, machine_name, os_version, dotnet_version, app_version, first_seen, last_seen, total_launches, country)
VALUES 
('4DAF26FB53D49397', 'SALEM-AHMED', 'Microsoft Windows NT 10.0.26200.0', '4.0.30319.42000', '1.0.10', datetime('now'), datetime('now'), 1, 'EG')
ON CONFLICT(device_id) DO UPDATE SET 
    last_seen = datetime('now'), 
    total_launches = total_launches + 1;
```

**Run it here:** https://app.turso.tech/manhag/databases/counter/data
