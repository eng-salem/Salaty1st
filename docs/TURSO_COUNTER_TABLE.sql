-- ============================================
-- Device Counter Database Schema
-- Database: counter-manhag (Turso/libSQL)
-- URL: libsql://counter-manhag.aws-eu-west-1.turso.io
-- Purpose: Track PrayerWidget usage per device
-- ============================================

-- Create device_counters table
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

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS idx_device_last_seen ON device_counters(last_seen);
CREATE INDEX IF NOT EXISTS idx_device_first_seen ON device_counters(first_seen);
CREATE INDEX IF NOT EXISTS idx_device_launches ON device_counters(total_launches);
CREATE INDEX IF NOT EXISTS idx_device_country ON device_counters(country);

-- ============================================
-- Example Queries
-- ============================================

-- Get total number of devices
-- SELECT COUNT(*) as total_devices FROM device_counters;

-- Get total launches across all devices
-- SELECT SUM(total_launches) as total_launches FROM device_counters;

-- Get most active devices
-- SELECT device_id, machine_name, total_launches, last_seen 
-- FROM device_counters 
-- ORDER BY total_launches DESC 
-- LIMIT 10;

-- Get new devices today
-- SELECT * FROM device_counters 
-- WHERE first_seen >= datetime('now', 'start of day');

-- Get devices by app version
-- SELECT app_version, COUNT(*) as device_count, SUM(total_launches) as launches 
-- FROM device_counters 
-- GROUP BY app_version 
-- ORDER BY app_version;

-- Get recent activity (last 24 hours)
-- SELECT * FROM device_counters 
-- WHERE last_seen >= datetime('now', '-1 day') 
-- ORDER BY last_seen DESC;

-- Insert new device
-- INSERT INTO device_counters 
-- (device_id, machine_name, os_version, dotnet_version, app_version, first_seen, last_seen, total_launches)
-- VALUES 
-- ('ABC123', 'DESKTOP-PC', 'Windows 10', '5.0.17', '1.0.10', datetime('now'), datetime('now'), 1);

-- Update existing device (increment launches)
-- UPDATE device_counters 
-- SET last_seen = datetime('now'),
--     total_launches = total_launches + 1,
--     app_version = '1.0.10'
-- WHERE device_id = 'ABC123';

-- ============================================
-- Usage Statistics Views
-- ============================================

-- Daily Active Users (DAU) - last 24 hours
-- CREATE VIEW IF NOT EXISTS dau AS
-- SELECT COUNT(DISTINCT device_id) as daily_active_users
-- FROM device_counters
-- WHERE last_seen >= datetime('now', '-1 day');

-- Monthly Active Users (MAU) - last 30 days
-- CREATE VIEW IF NOT EXISTS mau AS
-- SELECT COUNT(DISTINCT device_id) as monthly_active_users
-- FROM device_counters
-- WHERE last_seen >= datetime('now', '-30 days');

-- Device activity by country
-- CREATE VIEW IF NOT EXISTS activity_by_country AS
-- SELECT country, 
--        COUNT(*) as device_count, 
--        SUM(total_launches) as total_launches
-- FROM device_counters
-- WHERE country IS NOT NULL
-- GROUP BY country
-- ORDER BY device_count DESC;

-- ============================================
-- Cleanup Queries (Optional)
-- ============================================

-- Delete devices inactive for 90+ days
-- DELETE FROM device_counters 
-- WHERE last_seen < datetime('now', '-90 days');

-- Reset test devices
-- DELETE FROM device_counters WHERE device_id LIKE 'TEST%';

-- ============================================
-- Sample Data (For Testing)
-- ============================================

-- INSERT INTO device_counters 
-- (device_id, machine_name, os_version, dotnet_version, app_version, first_seen, last_seen, total_launches, country)
-- VALUES 
-- ('TEST001', 'Test-PC-1', 'Windows 10 Pro', '5.0.17', '1.0.10', datetime('now'), datetime('now'), 5, 'EG'),
-- ('TEST002', 'Test-PC-2', 'Windows 11', '5.0.17', '1.0.9', datetime('now'), datetime('now'), 12, 'SA'),
-- ('TEST003', 'Test-PC-3', 'Windows 10', '5.0.17', '1.0.10', datetime('now'), datetime('now'), 3, 'US');
