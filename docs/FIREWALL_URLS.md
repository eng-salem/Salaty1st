# PrayerWidget (Salaty.First) - Firewall URL Allowlist

## 🔥 Required URLs for Full Functionality

Allow these URLs in your firewall/proxy for PrayerWidget to work correctly:

---

## ✅ Core Services (Required)

| Service | URL | Port | Protocol | Purpose |
|---------|-----|------|----------|---------|
| **IP Geolocation** | `https://ipwhois.app` | 443 | HTTPS | Auto-detect location from IP address |
| **Address Geocoding** | `https://nominatim.openstreetmap.org` | 443 | HTTPS | Convert address to coordinates |
| **Islamic Quotes** | `https://quotes.islamicquotes.deno.net` | 443 | HTTPS | Fetch daily Islamic quotes/hadith |
| **Update Check** | `https://raw.githubusercontent.com` | 443 | HTTPS | Check for new app versions |

---

## 🔗 External Links (Optional - Browser)

These URLs are opened in the user's default browser (not by the app directly):

| Service | URL | Port | Protocol | Purpose |
|---------|-----|------|----------|---------|
| **GitHub Releases** | `https://github.com/eng-salem/Salaty1st` | 443 | HTTPS | Download updates |
| **Facebook Page** | `https://www.facebook.com/Salaty.1st` | 443 | HTTPS | Community support |

---

## 📊 Analytics (Optional)

| Service | URL | Port | Protocol | Purpose |
|---------|-----|------|----------|---------|
| **Counter API** | `https://api.counterapi.dev` | 443 | HTTPS | Anonymous usage statistics |

---

## 🔧 Firewall Rules

### **Windows Firewall (PowerShell)**

Run as Administrator to create firewall rules:

```powershell
# Allow PrayerWidget executable
New-NetFirewallRule -DisplayName "Salaty.First" `
  -Program "C:\Program Files\Salaty.First\Salaty.First.exe" `
  -Direction Outbound -Action Allow -Enabled True

# Allow HTTPS outbound (if not already allowed)
New-NetFirewallRule -DisplayName "Salaty.First HTTPS" `
  -Program "C:\Program Files\Salaty.First\Salaty.First.exe" `
  -Direction Outbound -Action Allow -RemotePort 443 -Protocol TCP -Enabled True
```

---

### **Setup MSI Filename**

The installer is now named: **`salaty-setup.msi`**

Install with:
```cmd
msiexec /i salaty-setup.msi /passive
```

---

### **Corporate Proxy Configuration**

If using a corporate proxy, add these domains to the allowlist:

```
ipwhois.app
nominatim.openstreetmap.org
quotes.islamicquotes.deno.net
raw.githubusercontent.com
api.counterapi.dev (optional)
```

---

## 📋 Minimum Required (Basic Functionality)

If you need to minimize allowed URLs, these are **absolutely required**:

| Priority | URL | What Breaks If Blocked |
|----------|-----|------------------------|
| 🔴 **Required** | `ipwhois.app` | IP location detection fails |
| 🟡 **Optional** | `nominatim.openstreetmap.org` | Address search fails (manual city selection still works) |
| 🟡 **Optional** | `quotes.islamicquotes.deno.net` | Islamic quotes feature disabled |
| 🟡 **Optional** | `raw.githubusercontent.com` | Auto-update check fails |
| ⚪ **Analytics** | `api.counterapi.dev` | Usage statistics not sent (app works fine) |

---

## 🧪 Testing Connectivity

After configuring firewall, test each URL:

```powershell
# Test IP Geolocation
Invoke-WebRequest -Uri "https://ipwhois.app/json/" -UseBasicParsing

# Test Geocoding
Invoke-WebRequest -Uri "https://nominatim.openstreetmap.org/search?q=London&format=json" -UseBasicParsing

# Test Quotes API
Invoke-WebRequest -Uri "https://quotes.islamicquotes.deno.net/quote" -UseBasicParsing

# Test Update Check
Invoke-WebRequest -Uri "https://raw.githubusercontent.com/eng-salem/Salaty1st/main/version.json" -UseBasicParsing

# Test Analytics (optional)
Invoke-WebRequest -Uri "https://api.counterapi.dev/v2/salaty/launches" -UseBasicParsing
```

All should return HTTP 200 OK.

---

## 🔒 Security Notes

| Aspect | Details |
|--------|---------|
| **All connections use HTTPS** | ✅ Encrypted TLS 1.2+ |
| **No authentication required** | ✅ All APIs are public/free |
| **No sensitive data sent** | ✅ Only IP address for geolocation |
| **Rate limits** | Nominatim: 1 req/sec, Others: unlimited |
| **Data privacy** | No personal data transmitted |

---

## 📞 IT Administrator Notes

For enterprise deployments:

1. **Whitelist domains** in proxy/firewall
2. **Allow outbound HTTPS (443)** for `Salaty.First.exe`
3. **No inbound rules required**
4. **No local server** - all communication is outbound
5. **Offline mode available** - prayer times calculated locally (database included)

---

## 🛠️ Troubleshooting

### Feature Not Working?

| Issue | Check This URL |
|-------|----------------|
| Can't auto-detect location | `ipwhois.app` |
| Address search fails | `nominatim.openstreetmap.org` |
| No quotes displayed | `quotes.islamicquotes.deno.net` |
| Update check fails | `raw.githubusercontent.com` |

### Check Application Logs

```cmd
# View recent errors
Get-Content "$env:LOCALAPPDATA\PrayerWidget\debug.log" -Tail 50
```

---

## 📧 Contact

For firewall/IT support questions:
- **GitHub Issues:** https://github.com/eng-salem/Salaty1st/issues
- **Facebook:** https://www.facebook.com/Salaty.1st

---

**Last Updated:** March 14, 2026
**Version:** 1.0.9
