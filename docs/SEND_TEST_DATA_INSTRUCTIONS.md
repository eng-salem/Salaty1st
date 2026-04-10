# How to Send Test Data to Turso Database

## Method 1: Turso Dashboard (Recommended)

### Step 1: Create the Table

1. Go to: https://app.turso.tech/manhag/databases/counter/data
2. Click on **SQL** or **Console** tab
3. Copy and paste this SQL:

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

4. Click **Run**

### Step 2: Insert Test Data

Copy and paste this SQL (update the values if needed):

```sql
-- Insert test device data
INSERT INTO device_counters 
(device_id, machine_name, os_version, dotnet_version, app_version, first_seen, last_seen, total_launches, country)
VALUES 
('4DAF26FB53D49397', 'SALEM-AHMED', 'Windows NT 10.0.26200.0', '4.0.30319.42000', '1.0.10', datetime('now'), datetime('now'), 1, 'EG')
ON CONFLICT(device_id) DO UPDATE SET 
    last_seen = datetime('now'), 
    total_launches = total_launches + 1, 
    app_version = '1.0.10';
```

### Step 3: Verify Data

```sql
-- View all devices
SELECT * FROM device_counters;

-- View test device
SELECT * FROM device_counters WHERE device_id = '4DAF26FB53D49397';
```

---

## Method 2: Turso CLI

If you have the Turso CLI installed:

```bash
# Install Turso CLI (if not installed)
# Windows:
scoop bucket add turso https://github.com/turso-dev/scoop-bucket.git
scoop install turso

# Or download from: https://github.com/tursodatabase/turso/releases

# Login to Turso
turso auth login

# Execute SQL
turso db execute counter-manhag "CREATE TABLE IF NOT EXISTS device_counters (...)"

# Insert test data
turso db execute counter-manhag "INSERT INTO device_counters (device_id, machine_name, ...) VALUES (...)"
```

---

## Method 3: libsql-client (Python)

```python
# Install libsql client
pip install libsql-client

# Python script to send data
import libsql
import datetime
import hashlib

# Connect to database
conn = libsql.connect(
    'counter-manhag',
    url='libsql://counter-manhag.aws-eu-west-1.turso.io',
    auth_token='YOUR_TURSO_AUTH_TOKEN_HERE'  # Replace with actual token
)

# Generate device ID
device_id = hashlib.sha256(f"{username}-{machine_name}".encode()).hexdigest()[:16].upper()

# Insert data
conn.execute("""
    INSERT INTO device_counters 
    (device_id, machine_name, os_version, dotnet_version, app_version, first_seen, last_seen, total_launches, country)
    VALUES (?, ?, ?, ?, ?, datetime('now'), datetime('now'), 1, 'EG')
    ON CONFLICT(device_id) DO UPDATE SET 
        last_seen = datetime('now'), 
        total_launches = total_launches + 1
""", (device_id, machine_name, os_version, dotnet_version, app_version))

conn.commit()
conn.close()
```

---

## Test Data from Your PC

Here's the exact data that would be sent from your PC:

| Field | Value |
|-------|-------|
| **device_id** | 4DAF26FB53D49397 |
| **machine_name** | SALEM-AHMED |
| **os_version** | Windows NT 10.0.26200.0 |
| **dotnet_version** | 4.0.30319.42000 |
| **app_version** | 1.0.10 |
| **first_seen** | (current UTC datetime) |
| **last_seen** | (current UTC datetime) |
| **total_launches** | 1 (increments on each launch) |
| **country** | EG (Egypt) |

---

## Quick SQL Snippets

### Insert First Test Record
```sql
INSERT INTO device_counters 
(device_id, machine_name, os_version, dotnet_version, app_version, first_seen, last_seen, total_launches, country)
VALUES 
('4DAF26FB53D49397', 'SALEM-AHMED', 'Windows NT 10.0.26200.0', '4.0.30319.42000', '1.0.10', datetime('now'), datetime('now'), 1, 'EG');
```

### Increment Launch Count
```sql
UPDATE device_counters 
SET last_seen = datetime('now'), 
    total_launches = total_launches + 1, 
    app_version = '1.0.10'
WHERE device_id = '4DAF26FB53D49397';
```

### View All Data
```sql
SELECT device_id, machine_name, total_launches, last_seen, country
FROM device_counters
ORDER BY last_seen DESC;
```

### Delete Test Data
```sql
DELETE FROM device_counters WHERE device_id = '4DAF26FB53D49397';
```

---

## Troubleshooting

### Error: "table device_counters does not exist"
**Solution:** Run the CREATE TABLE SQL first (see Step 1 above)

### Error: "UNIQUE constraint failed"
**Solution:** This is normal for subsequent launches. Use INSERT ... ON CONFLICT or just UPDATE.

### Error: "authentication failed"
**Solution:** Make sure you're using the correct auth token from Turso dashboard.

---

**Note:** The PowerShell script (`send_test_data.ps1`) doesn't work with Turso's AWS-hosted databases because they use a different API endpoint. Use the Turso dashboard instead.
