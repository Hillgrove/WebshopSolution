# Webshop Projekt – Obligatorisk Studieaktivitet

## 📚 Projektbeskrivelse

Dette projekt er udarbejdet som en obligatorisk studieaktivitet i valgfaget *Sikker Software*. Formålet er at designe og implementere en simpel webshop, hvor brugere kan oprette sig, gennemse produkter, lægge varer i kurv og foretage køb – alt sammen med fokus på sikkerhedsrelaterede best practices.

## 🧩 Funktionalitet

- 🔐 Brugerhåndtering (registrering, login, logout, glemt/adgangskodeændring)
- 🛒 Produkter og kurv (CRUD via API og visning i frontend)
- 📦 Bestilling og ordreoversigt
- 👤 Roller: `Guest`, `Customer`, `Admin`
- 🌐 Frontend i Vue 3 + Vue Router
- 🔙 Backend i C# (.NET 8 Web API)

## 🛠️ Teknologi-stack

| Lag        | Teknologi                      |
|------------|--------------------------------|
| Frontend   | HTML, Vue 3, Bootstrap 5       |
| Backend    | ASP.NET Core Web API (C#)      |
| Database   | SQLite (direkte via `System.Data.SQLite`) |
| Kommunikation | RESTful API med `axios` og `withCredentials` |
| Session    | `ISession` (HttpOnly, Secure, SameSite=None) |

## 🗄️ Databasemodel

Relationel database uden brug af ORM. Tabellenavnene er:

- `Users`: Gemmer brugere og deres roller
- `Products`: Produkter med navn, beskrivelse og pris i øre
- `Orders`: Brugerens ordrer
- `OrderItems`: Varer relateret til en ordre

## 🧾 Brugerroller

| Rolle     | Beskrivelse               |
|-----------|---------------------------|
| Guest     | Uidentificeret bruger     |
| Customer  | Registreret kunde         |
| Admin     | bruger med ekstra rettigheder

## ⚙️ Sikkerhedsforanstaltninger

Applikationen følger relevante principper fra [OWASP Application Security Verification Standard (ASVS)](https://owasp.org/www-project-application-security-verification-standard/), herunder:

- **Session Management:**
  - Beskyttelse mod session fixation og sikre session cookies
  - Automatisk session timeout ved inaktivitet

- **Authentication & Password Handling:**
  - Sikring af login-flow med rate limiting
  - Anvendelse af stærke hash-algoritmer til adgangskoder
  - Gendannelse og ændring af adgangskoder med passende kontrol

- **Access Control:**
  - Rollebaseret adgangsstyring med isolerede brugerroller

- **Input Validation:**
  - Kontrol af input på API-niveau
  - Begrænsning af uautoriserede Content-Type headers

- **Security Headers:**
  - Brug af HTTP-headers som `Content-Security-Policy`, `Strict-Transport-Security` og `X-Content-Type-Options`

- **API Security:**
  - CSRF-beskyttelse via validering af `Origin` header
  - Begrænsning af tilladte HTTP-metoder og oprindelser (CORS)

Implementeringen af disse områder har til formål at efterleve godkendte sikkerhedsstandarder og mindske risikoen for kendte angrebsvektorer.
