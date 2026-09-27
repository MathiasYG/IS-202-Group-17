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

# 2. Systemarkitektur

Applikasjonen følger MVC-arkitekturen.

```text
                    +-------------------+
                    |     Nettleser      |
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

## Model

`ResourceViewModel` ligger i `WebApplicationInAspire/Models/ResourceViewModel.cs`.

Modellen inneholder:

- `Name`
- `Type`
- `Description`
- `ContactName`
- `PhoneNumber`
- `Latitude`
- `Longitude`

Modellen bruker Data Annotations for validering av obligatoriske felt.

## Controller

`ResourceController` ligger i `WebApplicationInAspire/Controllers/ResourceController.cs`.

Controlleren har to actions for registrering:

### GET `/Resource/Register`

GET-metoden oppretter en `ResourceViewModel`, setter et standard kartpunkt og sender modellen til `Register.cshtml`.

### POST `/Resource/Register`

POST-metoden mottar `ResourceViewModel` fra skjemaet.

- Hvis `ModelState` ikke er gyldig, returneres registreringssiden med modellen.
- Hvis modellen er gyldig, logges registreringen og modellen sendes til `Overview.cshtml`.

## Registrerings-view

`WebApplicationInAspire/Views/Resource/Register.cshtml` inneholder skjemaet.

Skjemaet bruker ASP.NET tag helpers (`asp-for` og `asp-validation-for`) for modellfeltene. Latitude og Longitude sendes som skjulte input-felter.

## Leaflet og dataflyt

Leaflet brukes til å la brukeren velge en posisjon.

Dataflyten for kartpunktet er:

1. Brukeren klikker på kartet.
2. Leaflet henter `lat` og `lng` fra klikket.
3. En markør opprettes eller flyttes til den valgte posisjonen.
4. Latitude skrives til det skjulte `Latitude`-feltet.
5. Longitude skrives til det skjulte `Longitude`-feltet.
6. Når skjemaet sendes inn, blir koordinatene sendt til `ResourceController` sammen med resten av modellen.

På `Overview.cshtml` leses Latitude og Longitude fra modellen. Kartet sentreres på koordinatene, og en markør plasseres på samme sted. Markøren får en popup med ressursens navn.

---

# 3. Drift og kjøring

## 3.1 Kjøre webapplikasjonen lokalt

Fra roten av repositoryet:

```bash
dotnet run --project WebApplicationInAspire/WebApplicationInAspire.csproj
```

Prosjektets `launchSettings.json` angir følgende lokale adresser:

```text
http://localhost:5011
https://localhost:7018
```

## 3.2 Kjøre med .NET Aspire

AppHost ligger i `WebApplicationInAspire.AppHost/` og registrerer webprosjektet slik:

```csharp
builder.AddProject<Projects.WebApplicationInAspire>("webapplicationinaspire");
```

Start AppHost fra roten av repositoryet:

```bash
dotnet run --project WebApplicationInAspire.AppHost/WebApplicationInAspire.AppHost.csproj
```

Aspire starter webprosjektet som er registrert i AppHost og viser prosjektet i Aspire-dashboardet.

## 3.3 Docker

Prosjektet bruker en multi-stage `Dockerfile` i roten av repositoryet.

Dockerfilen består av to steg:

1. `mcr.microsoft.com/dotnet/sdk:10.0` brukes til restore, build og publish.
2. `mcr.microsoft.com/dotnet/aspnet:10.0` brukes som mindre runtime-image.

Applikasjonen eksponeres på port `8080` i containeren.

### Bygge Docker-imaget

Kjør fra roten av repositoryet:

```bash
docker build -t resource-registration .
```

### Kjøre containeren

```bash
docker run --rm -p 8080:8080 resource-registration
```

Åpne deretter:

```text
http://localhost:8080
```

### Kontrollere containeren

Kjør:

```bash
docker ps
```

Containeren skal stå som kjørende. Hvis den stopper, kan loggene undersøkes med:

```bash
docker logs <container-id>
```

### Docker-verifisering

Docker-verifiseringen skal kontrollere følgende:

- [ ] `docker build` fullføres uten feil.
- [ ] Containeren starter uten feil.
- [ ] Webapplikasjonen åpnes på `http://localhost:8080`.
- [ ] Registreringssiden kan åpnes.
- [ ] Leaflet-kartet vises.
- [ ] Et kartpunkt kan velges.
- [ ] Registreringen kan sendes til Overview.
- [ ] Overview viser registrerte data og kartmarkør.

**Faktisk resultat:** Fyll inn resultatet etter at kommandoene over er kjørt på en maskin med Docker.

---

# 4. Testscenarier og resultater

## Test 1 – Fyll ut skjema og velg kartpunkt

**Fremgangsmåte**

1. Åpne `/Resource/Register`.
2. Fyll inn navn, type, beskrivelse, kontaktperson og telefonnummer.
3. Klikk på et punkt i Leaflet-kartet.
4. Kontroller at markøren flyttes til valgt punkt.
5. Send inn skjemaet.

**Forventet resultat**

Kartpunktet registreres som Latitude og Longitude, og hele modellen sendes med POST til `ResourceController`.

**Faktisk resultat**

`[Fyll inn etter gjennomført test]`

**Status**

`[Bestått / Ikke bestått]`

---

## Test 2 – POST sender data til Overview

**Fremgangsmåte**

1. Registrer en ressurs med alle obligatoriske felter.
2. Velg et punkt på kartet.
3. Trykk på `Registrer`.
4. Kontroller Overview-siden.

**Forventet resultat**

Controlleren mottar modellen og returnerer `Overview` med samme ressursdata. Overview skal vise navn, type, beskrivelse, kontaktperson, telefonnummer, Latitude og Longitude. Kartet skal vise en markør på de registrerte koordinatene.

**Faktisk resultat**

`[Fyll inn etter gjennomført test]`

**Status**

`[Bestått / Ikke bestått]`

---

## Test 3 – Validering ved manglende felt

**Fremgangsmåte**

1. Åpne registreringsskjemaet.
2. La ett eller flere obligatoriske felt stå tomme.
3. Send inn skjemaet.

**Forventet resultat**

`ModelState.IsValid` skal være `false`, og Controlleren skal returnere registrerings-viewet i stedet for å sende brukeren til Overview.

**Faktisk resultat**

`[Fyll inn etter gjennomført test]`

**Status**

`[Bestått / Ikke bestått]`

---

## Test 4 – Docker-verifisering

**Fremgangsmåte**

```bash
docker build -t resource-registration .
docker run --rm -p 8080:8080 resource-registration
```

Åpne deretter `http://localhost:8080`.

**Forventet resultat**

Docker-imaget bygges, containeren starter, og webapplikasjonen kan åpnes og brukes gjennom nettleseren.

**Faktisk resultat**

`[Fyll inn etter faktisk Docker-kjøring]`

**Status**

`[Bestått / Ikke bestått]`

---

# 5. Testoversikt

| Test | Forventet resultat | Faktisk resultat | Status |
|---|---|---|---|
| Skjema + kartpunkt | Koordinater lagres og sendes med modellen | [Fyll inn] | [ ] |
| POST → Overview | Registrerte data og riktig kartpunkt vises | [Fyll inn] | [ ] |
| Manglende felt | Validering stopper innsending og viser skjemaet på nytt | [Fyll inn] | [ ] |
| Docker | Image bygges og applikasjonen kjører i container | [Fyll inn] | [ ] |

---

# 6. Konklusjon

Applikasjonen implementerer en MVC-basert ressursregistrering der Leaflet brukes til å velge geografisk posisjon. ResourceController mottar registreringsdataene og sender en gyldig modell videre til Overview. .NET Aspire AppHost brukes til å starte webprosjektet, og Docker kan brukes til å bygge og kjøre webapplikasjonen som en container.

Dokumentasjonen beskriver arkitekturen, dataflyten, kjøring med .NET Aspire og Docker samt testene som skal gjennomføres for å verifisere funksjonaliteten.
