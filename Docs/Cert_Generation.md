# Oprettelse af Self-Signed Certifikat til Live Server på Windows 11

Denne guide beskriver, hvordan du genererer et selvsigneret SSL-certifikat med OpenSSL, så Live Server kan køre HTTPS på `localhost`.

---

## 1. Installer OpenSSL
Hvis du ikke allerede har OpenSSL installeret, kan du hente det fra [Win32/Win64 OpenSSL](https://slproweb.com/products/Win32OpenSSL.html) og installere det.

Efter installationen skal du sørge for, at OpenSSL er tilgængelig i **CMD** ved at køre:

```cmd
openssl version
```

Hvis kommandoen returnerer en version, er OpenSSL korrekt installeret.

## 2. Opret mappen til certifikater
Åbn **CMD som administrator** og kør:

```cmd
mkdir C:\certificates
cd C:\certificates
```

---

## 3. Generer en privat nøgle

```cmd
openssl genpkey -algorithm RSA -out localhost.key
```

---

## 4. Opret en konfigurationsfil (`localhost.cnf`)
Opret en fil med navnet `localhost.cnf` i `C:\certificates`, og indsæt følgende:

```ini
[req]
default_bits = 2048
prompt = no
default_md = sha256
distinguished_name = dn
req_extensions = v3_req

[dn]
C = DK
ST = Hovedstaden
L = København
O = MyCompany
OU = Development
CN = localhost

[v3_req]
keyUsage = critical, digitalSignature, keyEncipherment
extendedKeyUsage = serverAuth
subjectAltName = @alt_names

[alt_names]
DNS.1 = localhost
DNS.2 = 127.0.0.1
```

---

## 5. Generer en certifikat-signeringsforespørgsel (CSR)

```cmd
openssl req -new -key localhost.key -out localhost.csr -config localhost.cnf
```

---

## 6. Generer det selvsignerede certifikat

```cmd
openssl x509 -req -days 365 -in localhost.csr -signkey localhost.key -out localhost.pem -extfile localhost.cnf -extensions v3_req
```

---

## 7. Installer certifikatet i Windows’ Trusted Root CA

1. **Åbn MMC**:
   - Tryk `Win + R`, skriv `mmc`, og tryk `Enter`.
2. **Tilføj certifikatkonsol**:
   - Gå til **File** → **Add/Remove Snap-in...**.
   - Vælg **Certificates** → **Computer Account** → **Next** → **Local Computer** → **Finish**.
3. **Importer certifikatet**:
   - Gå til **Trusted Root Certification Authorities** → **Certificates**.
   - Højreklik → **All Tasks** → **Import**.
   - Vælg `C:\certificates\localhost.pem`.
   - Følg guiden og bekræft import.

---

## 8. Konfigurer Live Server til at bruge HTTPS
Hvis Live Server understøtter direkte angivelse af certifikat, tilføj disse linjer i **`.vscode/settings.json`**:

```json
{
  "liveServer.settings.https": {
    "cert": "C:/certificates/localhost.pem",
    "key": "C:/certificates/localhost.key"
  }
}
```

---

## 9. Genstart Live Server og Chrome


## 10. Bekræft at HTTPS virker

- Åbn `https://localhost:5500` i Chrome.
- Hvis du stadig ser en advarsel, klik på **"Advanced"** → **"Proceed to localhost"**.
- Tjek Chrome DevTools (`F12`) → **Security** for at sikre, at certifikatet er accepteret.

Efter dette er HTTPS aktiveret for Live Server.
