# Webshop Projekt

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Vue.js](https://img.shields.io/badge/Vue.js-3-4FC08D?logo=vue.js)](https://vuejs.org/)
[![SQLite](https://img.shields.io/badge/SQLite-3-003B57?logo=sqlite)](https://www.sqlite.org/)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?logo=bootstrap)](https://getbootstrap.com/)
[![Licens](https://img.shields.io/badge/Licens-MIT-blue)](LICENSE)

4. semester obligatorisk opgave i valgfaget **Sikker Software** (forår 2025).

Dette projekt er en simpel webshop, der demonstrerer sikkerhedsrelaterede best practices i overensstemmelse med [OWASP Application Security Verification Standard (ASVS)](https://owasp.org/www-project-application-security-verification-standard/). Applikationen er opdelt i en Vue 3-frontend og en ASP.NET Core 8 Web API-backend med SQLite som database.

---

## Indholdsfortegnelse

- [Webshop Projekt](#webshop-projekt)
  - [Indholdsfortegnelse](#indholdsfortegnelse)
  - [Funktioner](#funktioner)
    - [Brugerhåndtering](#brugerhåndtering)
    - [Produkter og kurv](#produkter-og-kurv)
    - [Bestilling](#bestilling)
    - [Administration](#administration)
  - [Teknologier](#teknologier)
  - [Sikkerhedsforanstaltninger](#sikkerhedsforanstaltninger)
  - [Kom i gang](#kom-i-gang)
    - [Forudsætninger](#forudsætninger)
    - [Installation](#installation)
    - [Databaseopsætning](#databaseopsætning)
  - [Projektstruktur](#projektstruktur)
  - [Licens](#licens)

---

## Funktioner

### Brugerhåndtering
- Registrering med adgangskodestyrkevalidering (zxcvbn)
- Login og logout med rate limiting
- Glemt adgangskode via e-mail med tidsbegrænset token
- Skift adgangskode for indloggede brugere
- Rollebaseret adgangsstyring: `Guest`, `Customer`, `Admin`

### Produkter og kurv
- Gennemse produktkatalog
- Tilføj og fjern produkter fra kurv (sessionsbaseret)
- CRUD-operationer for produkter (kun Admin)

### Bestilling
- Afgiv ordre ved checkout
- Se ordrehistorik pr. bruger

### Administration
- Administrationspanel til brugeroversigt og -håndtering

---

## Teknologier

| Lag                 | Teknologi                           | Version  |
| ------------------- | ----------------------------------- | -------- |
| Frontend            | Vue.js                              | 3        |
| Frontend UI         | Bootstrap                           | 5.3.3    |
| Frontend HTTP       | Axios                               | CDN      |
| Backend framework   | ASP.NET Core Web API (C#)           | .NET 8.0 |
| Database            | SQLite via System.Data.SQLite       | 1.0.119  |
| Adgangskode-hashing | Argon2 (Isopoh.Cryptography.Argon2) | 2.0.0    |
| Adgangskodestyrke   | zxcvbn-core                         | 7.0.92   |
| E-mail              | MailKit / MimeKit                   | 4.10.0   |
| API-dokumentation   | Swagger (Swashbuckle.AspNetCore)    | 6.6.2    |
| JSON-serialisering  | Newtonsoft.Json                     | 13.0.3   |

---

## Sikkerhedsforanstaltninger

Applikationen implementerer udvalgte krav fra OWASP ASVS:

- **Sessionhåndtering:** Sikre cookies (`HttpOnly`, `Secure`, `SameSite=None`, `__Host-`-præfiks), timeout ved inaktivitet (15 min.) og absolut maksimaltid (30 min.)
- **Autentifikation:** Rate limiting på login og adgangskodegendannelse, Argon2-hashing af adgangskoder, validering mod kompromitterede adgangskoder
- **Adgangskontrol:** Rollebaseret middleware og custom `SessionAuthorize`-attribut
- **Inputvalidering:** Validering på API-niveau, begrænsning af Content-Type headers
- **Sikkerhedsheadere:** `Content-Security-Policy`, `Strict-Transport-Security` (HSTS, 2 år), `X-Content-Type-Options`
- **API-sikkerhed:** CSRF-beskyttelse via `Origin`-headervalidering, CORS-whitelist

---

## Kom i gang

### Forudsætninger

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- En browser og en statisk filserver til frontend (f.eks. VS Code Live Server med HTTPS)
- SMTP-adgang til e-mailafsendelse (konfigureres via User Secrets)

### Installation

1. Klon repositoriet:
   ```bash
   git clone https://github.com/Hillgrove/WebshopSolution.git
   cd WebshopSolution
   ```

2. Konfigurer User Secrets for API-projektet:
   ```bash
   cd Webshop.API
   dotnet user-secrets set "Email:Host" "<smtp-host>"
   dotnet user-secrets set "Email:Port" "<smtp-port>"
   dotnet user-secrets set "Email:Username" "<smtp-bruger>"
   dotnet user-secrets set "Email:Password" "<smtp-adgangskode>"
   dotnet user-secrets set "Email:From" "<afsender@eksempel.dk>"
   dotnet user-secrets set "PasswordReset:BaseUrl" "https://localhost:5500"
   ```

3. Byg og kør backend:
   ```bash
   dotnet run --project Webshop.API
   ```
   API'en starter på `https://localhost:7016`. Swagger UI er tilgængeligt på `/swagger`.

4. Åbn frontend:
   Serv indholdet af `Webshop.Frontend/` via en HTTPS-aktiveret statisk server (f.eks. VS Code Live Server på port 5500). Se [Docs/Cert_Generation.md](Docs/Cert_Generation.md) for opsætning af lokalt SSL-certifikat.

### Databaseopsætning

Databasen initialiseres automatisk ved opstart af API'et. `DatabaseInitializer` opretter følgende tabeller, hvis de ikke allerede eksisterer:

| Tabel        | Beskrivelse                                     |
| ------------ | ----------------------------------------------- |
| `Users`      | Brugere med rolle og hashed adgangskode         |
| `Products`   | Produkter med navn, beskrivelse og pris (i øre) |
| `Orders`     | Ordrer knyttet til brugere                      |
| `OrderItems` | Ordrelinjer med produktreference og antal       |

En standardbruger med rollen `Admin` seedtes automatisk ved første opstart.

---

## Projektstruktur

```
WebshopSolution/
├── Docs/                        # Supplerende dokumentation
│   ├── Cert_Generation.md       # Vejledning til lokalt SSL-certifikat
│   └── Password_Policy.md       # Adgangskodepolitik
│
├── Webshop.API/                 # ASP.NET Core Web API (startprojekt)
│   ├── Attributes/              # Custom autorisationsattribut (SessionAuthorize)
│   ├── Controllers/             # REST-controllere (Users, Products, Cart, Orders)
│   ├── Database/                # SQLite-databasefil (webshop.db)
│   ├── Middleware/              # Sikkerhedsmiddleware (CSRF, headers, roller)
│   ├── appsettings.json         # Basiskonfiguration (port, database)
│   └── Program.cs               # Dependency injection, middleware-pipeline
│
├── Webshop.Data/                # Dataadgangslag
│   ├── Models/                  # Domænemodeller (User, Product, Order, OrderItem, CartItem)
│   ├── Repositories/            # SQLite-implementationer og interfaces
│   └── DatabaseInitializer.cs  # Skemaoprettelse og seed-data
│
├── Webshop.Services/            # Forretningslogik
│   ├── UserService.cs           # Brugerregistrering, login, adgangskodehåndtering
│   ├── HashingService.cs        # Argon2-hashing
│   ├── PasswordService.cs       # Eksternt kald til kompromitteret-adgangskode-API
│   ├── RateLimitingService.cs   # Rate limiting for login og gendannelse
│   ├── EmailService.cs          # SMTP e-mailafsendelse
│   └── ValidationService.cs     # E-mailformatvalidering
│
├── Webshop.Shared/              # Delte typer (DTOs og enums)
│   ├── DTOs/                    # Data Transfer Objects (9 stk.)
│   └── Enums/                   # Fejlkoder
│
├── Webshop.Frontend/            # Vue 3 SPA (serveres som statiske filer)
│   ├── pages/                   # Sidekomponenter (11 stk.)
│   ├── assets/                  # CSS- og JS-biblioteker (Bootstrap, Axios m.fl.)
│   ├── index.html               # HTML-indgangspunkt
│   ├── index.js                 # Vue Router og app-initialisering
│   └── layoutComponent.js       # Navigationslayout og sessionsstyring
│
└── WebshopSolution.sln          # Visual Studio-solutionsfil
```

---

## Licens

Dette projekt er udgivet under [MIT-licensen](LICENSE).
