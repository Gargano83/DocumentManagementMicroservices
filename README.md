# 📄 Microservizi per la Gestione Documentale

Questo progetto implementa un sistema per la gestione del ciclo di vita dei documenti commerciali (Preventivi, Fatture Proforma, Ordini di vendita). Sviluppato in .NET, utilizza un'architettura a microservizi basata sul pattern Vertical Slice, separando nettamente le logiche di lettura e scrittura tramite CQRS.

---

## 🏗 Architettura e Tecnologie

L'infrastruttura è orchestrata tramite **.NET Aspire**, che centralizza la configurazione e permette l'avvio simultaneo di tutti i servizi e dei relativi database senza necessità di installazioni manuali.

* **API Gateway (YARP):** Agisce come unico punto di ingresso per le chiamate esterne. Si occupa del routing verso i microservizi e della validazione crittografica dei token JWT, garantendo la sicurezza perimetrale.
* **Identity Service:** Microservizio dedicato in via esclusiva alla verifica delle credenziali utente e all'emissione dei token di accesso.
* **Document Service:** Il nucleo dell'applicazione. Gestisce la creazione, l'aggiornamento e le transizioni di stato dei documenti. Utilizza **MediatR** per l'instradamento dei comandi e **FluentValidation** per il controllo dei dati in ingresso.
* **Database (MongoDB):** Archiviazione NoSQL che sfrutta il polimorfismo per salvare diverse tipologie di documenti in un'unica collezione logica. Implementa controlli di concorrenza ottimistica e inizializza automaticamente i dati di base all'avvio (Data Seeding).
* **Code e Messaggistica (RabbitMQ & MassTransit):** Le transizioni di stato dei documenti generano eventi asincroni. Questi eventi vengono letti da consumatori interni per operazioni secondarie, come il salvataggio dei log di controllo in un database separato, senza rallentare le risposte delle API.
* **Caching (Redis & HybridCache):** Ottimizza le operazioni di lettura restituendo copie leggere dei dati (DTO), alleggerendo il carico sul database principale.

---

## 🚀 Avvio Rapido

Il sistema è progettato per essere eseguito localmente in modo immediato, delegando la creazione dell'infrastruttura a contenitori temporanei.

### 1. Prerequisiti
* **Docker Desktop** (installato e in esecuzione).
* **.NET SDK** (versione 10).
* Un ambiente di sviluppo (Visual Studio 2026).

### 2. Avvio
1. Clona il repository e apri il file della soluzione (`.slnx`).
2. Imposta il progetto **`.AppHost`** come progetto di avvio principale.
3. Premi **`F5`** (o avvia in modalità Debug).

L'orchestrazione scaricherà le immagini Docker necessarie, avvierà MongoDB, Redis e RabbitMQ, ed eseguirà l'API Gateway e i microservizi in parallelo.

---

## 📊 Monitoraggio e Telemetria

All'avvio, il sistema aprirà nel browser la dashboard di **.NET Aspire**. Da questa interfaccia centralizzata è possibile:
* Verificare lo stato di salute di tutti i contenitori.
* Consultare i log strutturati.
* Seguire le tracce distribuite (Distributed Tracing) per capire come ogni singola richiesta HTTP attraversa il Gateway, i servizi e le code di messaggistica.

---

## ✅ Qualità del Codice e Test

Il progetto include una strategia di validazione rigorosa per garantire l'affidabilità del dominio:
* **Test End-to-End (E2E):** Tramite **xUnit** e **Testcontainers**, la suite avvia istanze reali di MongoDB, Redis e RabbitMQ su Docker per collaudare l'intero ciclo vitale dell'applicazione, dalla chiamata API fino al salvataggio nel database, per poi distruggere le risorse a test concluso.
* **Analisi Statica:** Predisposizione all'uso di **SonarQube** per il monitoraggio continuo della qualità del codice e della copertura dei test.

---

## ☸️ Infrastruttura e Kubernetes

L'architettura è predisposta per l'installazione su cluster Kubernetes. Invece di mantenere manualmente i file manifest YAML (come `Deployment`, `Service`, `ConfigMap` e `Secret`), il progetto sfrutta **Aspirate (Aspir8)**. Questo strumento permette di generare automaticamente tutte le risorse necessarie per Kubernetes partendo direttamente dalla definizione dell'infrastruttura presente nel file `AppHost.cs`, garantendo un allineamento perfetto e privo di errori tra l'ambiente di sviluppo locale e il rilascio in produzione.
