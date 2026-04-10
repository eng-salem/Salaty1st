# Quick Test Data Send - Turso Dashboard

## EASIEST METHOD - Just Copy & Paste SQL

### Step 1: Open Turso Dashboard
Go to: **https://app.turso.tech/manhag/databases/counter/data**

Click on the **SQL** tab at the top.

---

### Step 2: Create Table (First Time Only)

Copy and paste this entire block, then click **Run**:

```sql
CREATE TABLE IF NOT EXISTS device_counters (
    device_id TEXT PRIMARY KEY NOT NULL,
    machine_name TEXT,
    os_version TEXT,
    dotnet_version TEXT,
    app_version TEXT,
    first_seen TEXT NOT NULL,
    last_seen TEXT NOT NULL,
    total_launches INTEGER DEFAULT 0,
    country TEXT,
    city TEXT
);

CREATE INDEX IF NOT EXISTS idx_device_last_seen ON device_counters(last_seen);
CREATE INDEX IF NOT EXISTS idx_device_first_seen ON device_counters(first_seen);
CREATE INDEX IF NOT EXISTS idx_device_launches ON device_counters(total_launches);
```

---

### Step 3: Send Your Test Data

Copy and paste this (it has YOUR PC's info already filled in):

```sql
INSERT INTO device_counters 
(device_id, machine_name, os_version, dotnet_version, app_version, first_seen, last_seen, total_launches, country)
VALUES 
('4DAF26FB53D49397', 'SALEM-AHMED', 'Windows NT 10.0.26200.0', '4.0.30319.42000', '1.0.10', datetime('now'), datetime('now'), 1, 'EG')
ON CONFLICT(device_id) DO UPDATE SET 
    last_seen = datetime('now'), 
    total_launches = total_launches + 1, 
    app_version = '1.0.10';
```

Click **Run**.

---

### Step 4: Verify It Worked

```sql
SELECT * FROM device_counters;
```

You should see your device listed!

---

## Your PC's Test Data

| Field | Value |
|-------|-------|
| Device ID | 4DAF26FB53D49397 |
| Machine Name | SALEM-AHMED |
| OS Version | Windows NT 10.0.26200.0 |
| .NET Version | 4.0.30319.42000 |
| App Version | 1.0.10 |
| Country | EG (Egypt) |

---

## Why Scripts Don't Work

Turso's AWS-hosted databases (`*.aws-eu-west-1.turso.io`) don't support the standard libSQL HTTP API (`/v2` endpoint). 

The only ways to interact are:
1. **Turso Dashboard** (web UI) - ✅ WORKS
2. **Turso CLI** - ✅ WORKS (if installed)
3. **libsql-client** (Python/Node) - ✅ WORKS (if package installed)
4. **HTTP API scripts** - ❌ DOESN'T WORK with AWS endpoints

---

## Alternative: Install Turso CLI

If you want to use command line:

```bash
# Windows (using scoop)
scoop bucket add turso https://github.com/turso-dev/scoop-bucket.git
scoop install turso

# Login
turso auth login

# Send data
turso db execute counter-manhag "INSERT INTO device_counters (device_id, machine_name, os_version, dotnet_version, app_version, first_seen, last_seen, total_launches, country) VALUES ('4DAF26FB53D49397', 'SALEM-AHMED', 'Windows NT 10.0.26200.0', '4.0.30319.42000', '1.0.10', datetime('now'), datetime('now'), 1, 'EG') ON CONFLICT(device_id) DO UPDATE SET last_seen=datetime('now'), total_launches=total_launches+1, app_version='1.0.10'"
```

---

**That's it! Just use the dashboard - it's the easiest way.**
