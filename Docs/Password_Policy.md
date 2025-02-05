# Password Policy

## Formål
Denne password policy fastsætter krav til oprettelse, anvendelse og opbevaring af adgangskoder i webshoppen for at minimere risikoen for uautoriseret adgang.

## Krav til Adgangskoder

### Længde
- Minimumslængde: **8**
- Maximumslængde: **64**

>- Minimum length of the passwords should be enforced by the application. Passwords shorter than 8 characters are considered to be weak (NIST SP800-63B).
>- Maximum password length should be at least 64 characters to allow passphrases (NIST SP800-63B).

[Link](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html#implement-proper-password-strength-controls)

### Kompleksitet
- Ingen krav om specifikke tegn. Alle tegn inkl. Unicode og whitespace tilladt.
- Opfordringer til brug af lange passphrases.
- Almindelige og kompromitterede adgangskoder afvises ved oprettelse.
- [zxcvbn password strength indicator](https://github.com/zxcvbn-ts/zxcvbn)

>- Allow usage of all characters including unicode and whitespace. There should be no password composition rules limiting the type of characters permitted. There should be no requirement for upper or lower case or numbers or special characters.
>- Include a password strength meter to help users create a more complex password and block common and previously breached passwords

[Link](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html#implement-proper-password-strength-controls)

## Adgangskodehåndtering

### Skift af adgangskode
- Udløber ikke automatisk.
- Bruger skal skifte adgangskode ved mistanke om kompromittering.
    - [Have I Been Pwned Passwords API](https://haveibeenpwned.com/Passwords)

>Ensure credential rotation when a password leak occurs, at the time of compromise identification or when authenticator technology changes. Avoid requiring periodic password changes; instead, encourage users to pick strong passwords and enable [Multifactor Authentication Cheat Sheet (MFA)](https://cheatsheetseries.owasp.org/cheatsheets/Multifactor_Authentication_Cheat_Sheet.html).

[Link](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html#implement-proper-password-strength-controls)

## Opbevaring og Beskyttelse
- Koder hashes og saltes med moderne algoritme.
- Ingen lagring af adgangskoder i klartekst.

>It is critical for an application to store a password using the right cryptographic technique. Please see [Password Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html) for details on this feature.

[Link](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html#store-passwords-in-a-secure-fashion)

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
