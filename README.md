# Resource Registration

## 1. Om prosjektet

Dette prosjektet er en ASP.NET Core MVC-applikasjon for registrering av ressurser. Brukeren kan fylle ut informasjon om en ressurs, velge ressursens geografiske posisjon i et Leaflet-kart og sende registreringen til en oversiktsside.

Applikasjonen består av en webapplikasjon og et .NET Aspire AppHost-prosjekt.

### Teknologier

- ASP.NET Core MVC / .NET 10
- Razor Views
- Bootstrap
- Leaflet 1.9.4
- OpenStreetMap
- .NET Aspire
- Docker

---

## 2. Systemarkitektur

Applikasjonen følger MVC-arkitekturen.

```text
+-------------------+
|     Nettleser     |
+---------+---------+
          |
          v
+-------------------+
| Register.cshtml   |
| Skjema + Leaflet  |
+---------+---------+
          |
          | POST
          v
+-------------------+
| ResourceController|
+---------+---------+
          |
          v
+-------------------+
| ResourceViewModel |
+---------+---------+
          |
          v
+-------------------+
| Overview.cshtml   |
| Data + Leaflet    |
+-------------------+
```

### Modell

`ResourceViewModel` ligger i:

`WebApplicationInAspire/Models/ResourceViewModel.cs`

Modellen inneholder følgende felter:

- `Name`
- `Type`
- `Description`
- `ContactName`
- `PhoneNumber`
- `Latitude`
- `Longitude`

Modellen bruker Data Annotations for validering av obligatoriske felt. Koordinatene er nullable verdier (`double?`), slik at brukeren må velge en lokasjon i kartet før skjemaet kan sendes inn.

### Controller

`ResourceController` ligger i:

`WebApplicationInAspire/Controllers/ResourceController.cs`

Controlleren har to actions for registrering:

#### GET `/Resource/Register`

GET-metoden oppretter en tom `ResourceViewModel` og sender modellen til `Register.cshtml`.

#### POST `/Resource/Register`

POST-metoden mottar `ResourceViewModel` fra skjemaet.

Hvis `ModelState` ikke er gyldig, returneres registreringssiden med modellen og tilhørende valideringsmeldinger.

Hvis modellen er gyldig, logges registreringen og modellen sendes videre til `Overview.cshtml`.

### Registrerings-view

`WebApplicationInAspire/Views/Resource/Register.cshtml` inneholder registreringsskjemaet.

Skjemaet bruker ASP.NET Tag Helpers som `asp-for` og `asp-validation-for` for modellfeltene. `Latitude` og `Longitude` sendes som skjulte input-felter.

### Leaflet og dataflyt

Leaflet brukes til å la brukeren velge en geografisk posisjon.

Dataflyten for kartpunktet er:

1. Brukeren klikker på kartet.
2. Leaflet henter `lat` og `lng` fra klikket.
3. En markør opprettes eller flyttes til den valgte posisjonen.
4. `Latitude` skrives til det skjulte `Latitude`-feltet.
5. `Longitude` skrives til det skjulte `Longitude`-feltet.
6. Når skjemaet sendes inn, blir koordinatene sendt til `ResourceController` sammen med resten av modellen.
7. På `Overview.cshtml` leses `Latitude` og `Longitude` fra modellen.
8. Kartet sentreres på koordinatene, og en markør plasseres på samme sted.
9. Markøren får en popup med ressursens navn.

---

## 3. Drift og kjøring

### 3.1 Kjøre webapplikasjonen lokalt

Kjør fra roten av repositoryet:

```
dotnet run --project WebApplicationInAspire/WebApplicationInAspire.csproj
```

Prosjektets `launchSettings.json` angir følgende lokale adresser:

```
http://localhost:5011
https://localhost:7018
```

### 3.2 Kjøre med .NET Aspire

AppHost ligger i `WebApplicationInAspire.AppHost/` og registrerer webprosjektet slik:

```
builder.AddProject<Projects.WebApplicationInAspire>("webapplicationinaspire");
```

Start AppHost fra roten av repositoryet:

```
dotnet run --project WebApplicationInAspire.AppHost/WebApplicationInAspire.AppHost.csproj
```

Aspire starter webprosjektet som er registrert i AppHost og viser prosjektet i Aspire-dashboardet.

### 3.3 Docker

Prosjektet bruker en multi-stage Dockerfile i roten av repositoryet.

Dockerfilen består av to steg:

- `mcr.microsoft.com/dotnet/sdk:10.0` brukes til restore, build og publish.
- `mcr.microsoft.com/dotnet/aspnet:10.0` brukes som et mindre runtime-image.

Applikasjonen eksponeres på port `8080` i containeren.

#### Bygge Docker-imaget

Kjør fra roten av repositoryet:

```
docker build -t resource-registration .
```

#### Kjøre containeren

```
docker run --rm -p 8080:8080 resource-registration
```

Åpne deretter:

```
http://localhost:8080/Resource/Register
```

#### Kontrollere containeren

Kjør:

```
docker ps
```

Containeren skal stå som kjørende. Hvis den stopper, kan loggene undersøkes med:

```
docker logs <container-id>
```

### Docker-verifisering

Docker-verifiseringen kontrollerer følgende:

- [x] `docker build` fullføres uten feil.
- [x] Containeren starter uten feil.
- [x] Webapplikasjonen åpnes på `http://localhost:8080`.
- [x] Registreringssiden kan åpnes.
- [x] Leaflet-kartet vises.
- [x] Et kartpunkt kan velges.
- [x] Registreringen kan sendes til Overview.
- [x] Overview viser registrerte data og kartmarkør.

**Faktisk resultat:** Docker-imaget ble bygget lokalt, og containeren startet på port `8080`. Registreringsskjemaet med Leaflet-kart og innsending til oversiktssiden fungerte som forventet.

---

## 4. Testscenarier og resultater

### Test 1 – Fyll ut skjema og velg kartpunkt

#### Fremgangsmåte

1. Åpne `/Resource/Register`.
2. Fyll inn navn, type, beskrivelse, kontaktperson og telefonnummer.
3. Klikk på et punkt i Leaflet-kartet.
4. Kontroller at markøren flyttes til valgt punkt.
5. Send inn skjemaet.

#### Forventet resultat

Kartpunktet registreres som `Latitude` og `Longitude`, og hele modellen sendes med POST til `ResourceController`.

#### Faktisk resultat

Skjemaet ble sendt inn uten feil. Valgt kartpunkt ble registrert som `Latitude` og `Longitude`, og alle utfylte verdier ble vist på Overview-siden.

**Status:** Bestått

### Test 2 – POST sender data til Overview

#### Fremgangsmåte

1. Registrer en ressurs med alle obligatoriske felter.
2. Velg et punkt på kartet.
3. Trykk på **Registrer**.
4. Kontroller Overview-siden.

#### Forventet resultat

Controlleren mottar modellen og returnerer Overview med samme ressursdata. Overview skal vise navn, type, beskrivelse, kontaktperson, telefonnummer, `Latitude` og `Longitude`. Kartet skal vise en markør på de registrerte koordinatene.

#### Faktisk resultat

Skjemaet ble sendt inn, og Overview-siden viste samme ressursdata som ble registrert. Navn, type, beskrivelse, kontaktperson, telefonnummer, `Latitude` og `Longitude` ble vist, sammen med et oppdatert kart med markør og popup.

**Status:** Bestått

### Test 3 – Validering ved manglende felt

#### Fremgangsmåte

1. Åpne registreringsskjemaet.
2. La ett eller flere obligatoriske felt stå tomme, eller la være å velge et punkt i kartet.
3. Send inn skjemaet.

#### Forventet resultat

`ModelState.IsValid` skal være `false`, og controlleren skal returnere registrerings-viewet i stedet for å sende brukeren til Overview. Tilhørende feilmeldinger skal vises.

#### Faktisk resultat

Ved manglende felter eller manglende lokasjon i kartet stoppet innsendingen, og tilhørende valideringsmeldinger ble vist med rød tekst under de respektive feltene.

**Status:** Bestått

### Test 4 – Docker-verifisering

#### Fremgangsmåte

```
docker build -t resource-registration .
docker run --rm -p 8080:8080 resource-registration
```

Åpne deretter:

```
http://localhost:8080/Resource/Register
```

#### Forventet resultat

Docker-imaget bygges, containeren starter, og webapplikasjonen kan åpnes og brukes gjennom nettleseren.

#### Faktisk resultat

Docker-imaget ble bygget uten feil. Containeren startet som forventet, og webapplikasjonen var tilgjengelig på `localhost:8080`. Registreringssiden med Leaflet-kart kunne åpnes og brukes.

**Status:** Bestått

---

## 5. Testoversikt

| Test | Forventet resultat | Faktisk resultat | Status |
|---|---|---|---|
| Skjema + kartpunkt | Koordinater lagres og sendes med modellen | Koordinater ble registrert og sendt med modellen | Bestått |
| POST → Overview | Registrerte data og riktig kartpunkt vises | Overview viser registrerte data og korrekt markør i kartet | Bestått |
| Manglende felt | Validering stopper innsending og viser skjemaet på nytt | Skjemaet stoppet innsending og viste valideringsfeil | Bestått |
| Docker | Image bygges og applikasjonen kjører i container | Image ble bygget uten feil og kjørte på port 8080 | Bestått |

---

## 6. Bruk av kunstig intelligens (KI)

I henhold til oppgavekravene er kunstig intelligens benyttet som støtte- og læringsverktøy gjennom prosjektet, fra idé og planlegging til koding, feilsøking og dokumentasjon.

### Verktøy benyttet

- **GitHub Copilot:** Kodeassistent direkte i IDE/VS Code for kodeforslag og maler. Vi brukte de også som sparringspartner for systemarkitektur, feilsøking av Docker- og kulturrelaterte problemer og strukturering av dokumentasjon.

### Bruksområder

#### Idé og arkitektur

- Diskusjon rundt MVC-mønsteret.
- Vurdering av hvordan klikkhendelser i Leaflet-kartet kan bindes mot ASP.NET Model Binding via skjulte felter.

#### Koding og validering

- Oppsett av `ResourceViewModel` med Data Annotations for validering.
- Håndtering av formateringsproblemer, blant annet desimalkomma kontra desimalpunktum ved bruk av `CultureInfo.InvariantCulture` mellom Leaflet og C#.

#### Drift og Docker

- Kvalitetssikring av multi-stage Dockerfile for .NET 10.

#### Feilsøking

- Feilsøking av valideringsfeilen `The value is not valid for Latitude`.
- Feilsøking av `CS1501`-kompileringsfeil ved `ToString`-formatering av nullable datatyper.

### Eksempler på faktiske prompts

> Hvordan setter jeg opp Leaflet.js slik at et klikk i kartet oppdaterer to skjulte inputfelter med Latitude og Longitude i et ASP.NET Core MVC-skjema?

> Hvorfor får jeg "The value is not valid for Latitude" i ASP.NET Core når Leaflet sender inn koordinater med desimalpunktum?

> Hvordan håndterer jeg at ResourceViewModel har nullable doubles for kartkoordinater slik at [Required] faktisk feiler hvis kartet ikke er klikket på?

---

## 7. Konklusjon

Applikasjonen implementerer en MVC-basert ressursregistrering der Leaflet brukes til å velge geografisk posisjon. `ResourceController` mottar registreringsdataene og sender en gyldig modell videre til Overview.

.NET Aspire AppHost brukes til å starte webprosjektet, og Docker kan brukes til å bygge og kjøre webapplikasjonen som en container.

Dokumentasjonen beskriver arkitekturen, dataflyten, kjøring med .NET Aspire og Docker samt testene som er gjennomført for å verifisere funksjonaliteten.
