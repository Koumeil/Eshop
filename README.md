# 🛍️ EShop - Documentation Technique

<p align="start">
  <img src="https://img.shields.io/github/actions/workflow/status/koumeil/eshop/ci-cd.yml?branch=main&label=CI%2FCD&logo=github&style=flat-square" alt="GitHub Actions">
  <img src="https://img.shields.io/badge/Container-GHCR.io-blue?logo=docker&style=flat-square" alt="GHCR">
  <img src="https://img.shields.io/badge/.NET-8.0-blueviolet?logo=dotnet&style=flat-square" alt=".NET 8">
  <img src="https://img.shields.io/github/license/koumeil/eshop?style=flat-square" alt="License">
</p>

---

## 🧭 Table des Matières

1. [📋 Vue d'ensemble](#-vue-densemble)
2. [🏗️ Architecture](#️-architecture)
   - [⚙️ Stack Technique](#️-stack-technique)
   - [🗂️ Structure du Projet](#️-structure-du-projet)
3. [🚀 Démarrage Rapide](#-démarrage-rapide-avec-docker)
   - [🧩 Prérequis](#-prérequis)
   - [⚡ Installation](#-installation)
   - [🔐 Configuration SSL Automatique (Windows)](#-configuration-ssl-automatique-windows)
   - [⚡ Démarrage Rapide sans Docker Compose](#-démarrage-rapide-sans-docker-compose)
4. [⚙️ Fonctionnalités Techniques](#️-fonctionnalités-techniques)
5. [🔒 Sécurité](#-sécurité)
6. [🔄 CI/CD Pipeline](#-cicd-pipeline)
7. [🧰 Configuration Docker](#-configuration-docker)
8. [🔐 Gestion SSL](#-gestion-ssl)
9. [📊 Monitoring & Maintenance](#-monitoring--maintenance)
10. [🎯 Points Clés](#-points-clés)
11. [📜 Licence et Crédit](#-licence-et-crédit)

---

## 📋 Vue d'ensemble

**EShop** est une plateforme e-commerce moderne construite avec **.NET 8**, suivant les principes **Clean Architecture** et **Domain-Driven Design**.  
Le projet intègre un pipeline **CI/CD complet**, avec déploiement containerisé et automatisé.

---

## 🏗️ Architecture

### ⚙️ Stack Technique

| Composant | Technologie |
|------------|-------------|
| **Backend** | .NET 8, ASP.NET Core, EF Core |
| **Base de données** | PostgreSQL 16 |
| **Frontend** | Application séparée (`frontend/`) |
| **Sécurité** | JWT, HTTPS obligatoire |
| **Containerisation** | Docker, Docker Compose |
| **CI/CD** | GitHub Actions, GitHub Container Registry |

---

### 🗂️ Structure du Projet

```plaintext
Eshop/
├── .github/workflows/     # Pipeline CI/CD
├── src/                   # Code source .NET
│   ├── API/              # Couche Présentation
│   ├── Application/      # Logique métier
│   ├── Domain/           # Entités, Value Objects
│   ├── Infrastructure/   # Persistence, Migrations
│   └── Tests/            # Tests unitaires
├── frontend/             # Application frontend
├── docs/                 # Documentation
├── ssl-certs/            # Certificats auto-générés
├── docker-compose.yml
├── Dockerfile
├── init-ssl.sh
└── first-run.bat
```

### 🚀 Démarrage Rapide avec Docker
## 🧩 Prérequis

- 🐳 Docker Desktop (avec Docker Compose)
- 🔑 Droits administrateur (pour la configuration SSL)

### ⚡ Installation

```bash
# 1️⃣ Cloner le dépôt
git clone <repository>

# 2️⃣ Se placer dans le dossier du projet
cd Eshop

# 3️⃣ Construire et démarrer les conteneurs
docker-compose up --build
```

### 🔐 Configuration SSL Automatique (Windows)

### ⚠️ Exécuter en tant qu’administrateur

```bash
first-run.bat
```

### ⚡ Démarrage Rapide sans Docker Compose

---

#### 🧩 Prérequis

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- PostgreSQL 16 ou version compatible
- PowerShell (Windows) ou terminal compatible
- Droits suffisants pour créer la base de données

---

#### 🛠️ Étapes de configuration

1️⃣ **Cloner le dépôt :**
```bash
git clone <repository>
cd Eshop
```

2️⃣ **Initialiser la base de données localement (sans Docker)**  

Si vous ne souhaitez pas utiliser Docker ou Docker Compose, vous pouvez lancer le script PowerShell fourni pour préparer votre base de données et appliquer les migrations :

```powershell
# Autoriser l'exécution des scripts si nécessaire (une seule fois) Administrateur
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Exécuter le script d'initialisation (Administrateur)
.\setup-db.ps1
```
Ce script fait automatiquement :

- La restauration des packages NuGet

- La création de la migration initiale (si elle n'existe pas)

- L'application des migrations sur votre base PostgreSQL

3️⃣ Démarrer l’API localement :

```bash 
dotnet run --project src/API
```


| Service          | URL                                                              |
| ---------------- | ---------------------------------------------------------------- |
| **Application**  | [https://localhost:7002](https://localhost:7002)                 |
| **Swagger UI**   | [https://localhost:7002/swagger](https://localhost:7002/swagger) |
| **Health Check** | [https://localhost:7002/health](https://localhost:7002/health)   |
| **PostgreSQL**   | localhost:5432                                                   |


### ⚙️ Fonctionnalités Techniques
### 🧠 Initialisation Automatique

- ✅ Migrations Base de Données (avec retry logic)
- ✅ Peuplement via userSeeds.json
- ✅ Certificat SSL auto-signé avec SAN
- ✅ Health Checks intégrés


### 🔒 Sécurité

- HTTPS obligatoire (aucun endpoint HTTP)

- Certificats SAN (localhost + host.docker.internal)

- JWT Authentication

- Value Objects avec validation métier intégrée

### 🔄 CI/CD Pipeline
### 📁 Workflow

- Fichier : .github/workflows/ci-cd.yml

- Déclencheur : push sur la branche main

### 🧱 Étapes principales

- Setup Certificate – Décode le certificat depuis les secrets GitHub

- Build & Test – Restauration, compilation et exécution des tests

- Publish – Publication de l’application

- Docker Build & Push – Envoi vers GHCR


### 🐳 Container Registry

| Élément     | Détail                         |
| ----------- | ------------------------------ |
| **Images**  | `ghcr.io/koumeil/eshop:latest` |
| **Tags**    | `latest`, `commit SHA`         |
| **Secrets** | Certificat SSL + mot de passe  |

### 🧰 Configuration Docker

### 🔧 Services Déployés

- api → Application .NET 8 (HTTPS)

- db → PostgreSQL 16 (volume persistant)

### 💓 Health Checks

```yaml 
healthcheck:
  test: ["CMD", "stat", "/https/aspnetapp.crt"]
  interval: 3s
  timeout: 2s
  retries: 15
  start_period: 5s
```

### 🌍 Variables d’Environnement

```yaml
ASPNETCORE_URLS: https://+:7002
ASPNETCORE_Kestrel__Certificates__Default__Path: /https/aspnetapp.pfx
ConnectionStrings__DefaultConnection: Host=db;Port=5432;Database=eshop
```

### 🔐 Gestion SSL
### 🧾 Script d’Initialisation (init-ssl.sh)

- Génération du certificat avec SAN

- Création du keystore PKCS12

- Ajout au store de confiance du conteneur

### 🪟 Configuration Windows (first-run.bat)

- Extraction du certificat depuis le conteneur

- Installation dans le store Root Windows

- Redémarrage des services cryptographiques


### 📊 Monitoring & Maintenance
### 🔍 Commandes Utiles

```bash
# Surveillance des logs
docker-compose logs -f api

# Arrêt propre
docker-compose down

# Nettoyage complet
docker-compose down -v

# Statut des services
docker-compose ps
```

###  🩺 Health Endpoints

- GET /health → État de l’application et de la base de données

- Logs structurés avec niveaux de sévérité

### 🎯 Points Clés

| 💡 Objectif              | 🧩 Description                   |
| ------------------------ | -------------------------------- |
| **Zero Configuration**   | Démarrage immédiat après clone   |
| **HTTPS First**          | Sécurité par défaut              |
| **Automation Complete**  | DB, SSL, Seeds automatiques      |
| **Production Ready**     | CI/CD, Health Checks, Monitoring |
| **Developer Experience** | Environnement cohérent dev/prod  |


### 🔔 Note : Le certificat SSL étant auto-signé, les navigateurs afficheront un avertissement de sécurité.
- Exécute first-run.bat pour l’ajouter au store de confiance Windows.


### © 2025 – EShop Made with ❤️ by Koumeil

