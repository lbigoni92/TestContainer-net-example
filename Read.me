#  Testcontainers .NET – Esempio con REST API + PostgreSQL

Questo progetto dimostrativo ha lo scopo di testare l'utilizzo di **Testcontainers** all'interno di un'applicazione .NET. 
La soluzione è composta da **due progetti** principali:

---

## 📁 Struttura del progetto

- **`Testcontainers_Template`**: progetto REST API .NET minimal di esempio (`WeatherForecast`)
- **`Testcontainers_Template.Test`**: progetto di test xUnit che testa l’API utilizzando container Docker

---

##  Docker Compose

All’interno della root del progetto si trova un file `docker-compose.yml` che:

- Avvia **2 container**:
  - Un container PostgreSQL
  - Un container contenente il sito ASP.NET con REST API
- Configura la rete per farli comunicare

Questo setup consente di testare l'integrazione tra backend e database in un ambiente isolato.

---

##  Progetto di Test – `Testcontainers_Template.Test`

Contiene 3 file principali di test, che servono a scopi diversi:

### 🔹 `PostgresIntegrationTests.cs`
Esegue test di connessione e integrazione contro un container PostgreSQL, utilizzando la libreria `Testcontainers`.

### 🔹 `SqlServerIntegrationTests.cs`
Esegue test simili a quelli sopra, ma usando SQL Server come DB (utile per confronti o test alternativi).

### 🔹 `WeatherForecastApiTests.cs` 👉 **(principale)**

Questo è il test più rappresentativo. Fa quanto segue:

1. **Avvia il container** tramite Docker Compose (eseguito separatamente).
2. Effettua una chiamata HTTP al controller `/weatherforecast` del sito ASP.NET avviato nel container.
3. Verifica che la risposta abbia:
   - Codice HTTP 200
   - Un corpo contenente almeno un oggetto `WeatherForecast`

Serve per **testare end-to-end** il funzionamento reale dell'API esposta all'interno del container Docker, simulando un contesto di produzione isolato.

---

##  Dipendenze principali

Nel file `.csproj` del progetto di test vengono usate queste librerie:

- [`DotNet.Testcontainers`](https://github.com/testcontainers/testcontainers-dotnet) – per avviare container nei test
- `xUnit` – framework di testing
- `Microsoft.NET.Test.Sdk`, `coverlet`, `FluentAssertions` – per asserzioni e test coverage
- `HttpClient` – per effettuare richieste alle API

---

##  Limitazioni conosciute

- **`Testcontainers` non supporta direttamente `docker-compose.yml`**, quindi:
  - Il file Compose deve essere avviato manualmente (es. `docker compose up`) oppure gestito da script esterni, in qesti test la soluzione è stata creare 2 container metterli nella solita rete e farli comunicare.
- Il test `WeatherForecastApiTests.cs` presuppone che il container dell’API sia **già in esecuzione e accessibile**

---
