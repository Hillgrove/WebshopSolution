# Password Policy

## Formål
Denne password policy fastsætter krav til oprettelse, anvendelse og opbevaring af adgangskoder i webshoppen for at minimere risikoen for uautoriseret adgang.

## Krav til Adgangskoder

### Længde
- Minimumslængde: **8** (gerne **15**)
- Maximumslængde: **64 til 128 tegn**

### Kompleksitet
- Ingen krav om specifikke tegn. Alle tegn inkl. Unicode og whitespace tilladt.
- Opfordringer til brug af lange passphrases.
- Almindelige og kompromitterede adgangskoder afvises ved oprettelse.
- [zxcvbn password strength indicator](https://github.com/zxcvbn-ts/zxcvbn)

## Adgangskodehåndtering

### Skift af adgangskode
- Udløber ikke automatisk.
- Bruger skal skifte adgangskode ved mistanke om kompromittering.
- [Have I Been Pwned Passwords API](https://haveibeenpwned.com/Passwords)

## Opbevaring og Beskyttelse
- Koder hashes og saltes med moderne algoritme.
- Ingen lagring af adgangskoder i klartekst.

## Multifaktorgodkendelse
- **MFA er påkrævet for admin-konti.**
- **MFA anbefales for almindelige brugere og skal kunne aktiveres frivilligt.**
- Understøttede MFA-metoder: **TOTP og WebAuthn** (måske?).

## Yderligere Sikkerhedsforanstaltninger

### Konto- og loginbeskyttelse
- **Rate limiting på loginforsøg** – 3 forsøg.
- **Brute-force beskyttelse** – Låsning af konto i 10 min. ved gentagne mislykkede forsøg.

### Password Managers
- Brugere opfordres til at benytte en **password manager**.
