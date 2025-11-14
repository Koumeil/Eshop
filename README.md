# 🛍️ EShop - Documentation Technique

<p align="center">
  <img src="https://img.shields.io/github/actions/workflow/status/koumeil/eshop/ci-cd.yml?branch=main&label=CI%2FCD&logo=github&style=flat-square" alt="GitHub Actions"/>
  <img src="https://img.shields.io/badge/Container-GHCR.io-blue?logo=docker&style=flat-square" alt="GHCR"/>
  <img src="https://img.shields.io/badge/.NET-8.0-blueviolet?logo=dotnet&style=flat-square" alt=".NET 8"/>
  <img src="https://img.shields.io/github/license/koumeil/eshop?style=flat-square" alt="License"/>
</p>

---

## 📌 Table des Matières

- [Vue d'ensemble](#-vue-densemble)
- [Architecture & Stack Technique](#-architecture--stack-technique)
- [Structure du Projet](#-structure-du-projet)
- [Démarrage Rapide](#-démarrage-rapide)
  - [Prérequis](#-prérequis)
  - [Installation Docker](#-installation-docker)
  - [Démarrage sans Docker](#-démarrage-sans-docker)
  - [Configuration SSL Windows](#-configuration-ssl-windows)
- [Tester l’API avec Swagger](#-tester-lapi-avec-swagger)
- [Fonctionnalités Clés](#-fonctionnalités-clés)
- [Sécurité](#-sécurité)
- [CI/CD & Pipeline](#-cicd--pipeline)
- [Docker & Monitoring](#-docker--monitoring)
- [Licence & Crédit](#-licence--crédit)

---

## 📋 Vue d'ensemble

**EShop** est une plateforme e-commerce moderne, construite avec **.NET 8**, basée sur :

- **Clean Architecture**
- **Domain-Driven Design (DDD)**
- **CI/CD automatisé**
- **Containerisation Docker**
- **HTTPS et JWT Security intégrés**

Objectif : fournir un environnement prêt pour le développement et la production avec un minimum de configuration.

---

## 🏗️ Architecture & Stack Technique

| Couche        | Technologie / Outils                |
|---------------|-----------------------------------|
| Backend       | .NET 8, ASP.NET Core, EF Core      |
| Base de données | PostgreSQL 16                     |
| Frontend      | Séparé (`frontend/`)               |
| Authentification | JWT, HTTPS obligatoire           |
| Containerisation | Docker, Docker Compose           |
| CI/CD         | GitHub Actions, GHCR               |

---

## 🗂️ Structure du Projet

```plaintext
Eshop/
├── .github/workflows/       # Pipelines CI/CD
├── src/                     # Code source
│   ├── API/                 # Couche Présentation
│   ├── Application/         # Logique métier
│   ├── Domain/              # Entités et Value Objects
│   ├── Infrastructure/      # Persistance, migrations
│   └── Tests/               # Tests unitaires
├── frontend/                # Application frontend
├── docs/                    # Documentation
├── ssl-certs/               # Certificats auto-signés
├── docker-compose.yml
├── Dockerfile
├── init-ssl.sh
└── first-run.bat
```

---

## 🚀 Démarrage Rapide

### 🧩 Prérequis

- Docker Desktop + Docker Compose
- Droits administrateur (Windows) pour SSL
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) si lancement sans Docker

---

### ⚡ Installation Docker

```bash
git clone <repository>
cd Eshop
docker-compose up --build
```

---

### ⚡ Démarrage sans Docker

Si vous ne souhaitez pas utiliser Docker, il est possible de lancer l’application et d’initialiser la base de données localement. Deux méthodes sont proposées : **via script PowerShell** ou **manuelle**.

---

#### 🧩 Prérequis

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)  
- PostgreSQL 16 ou compatible  
- PowerShell (Windows) ou terminal compatible  
- Droits suffisants pour créer la base de données  

---

#### 1️⃣ Méthode recommandée : script PowerShell `setup-db.ps1`

Ce script automatise :

- La restauration des packages NuGet
- La création de la migration initiale (si elle n’existe pas)
- L’application des migrations sur la base PostgreSQL

**Étapes :**

1. Ouvrir PowerShell en mode Administrateur
2. Autoriser l’exécution des scripts (si nécessaire, une seule fois) :

```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

3. Exécuter le script d’initialisation :

```powershell
.\setup-db.ps1
```

> Le script affichera les étapes et confirmera que la base est initialisée.  
> Ensuite, démarrez l’API :

```bash
dotnet run --project src/API
```

---

#### 2️⃣ Méthode manuelle : commandes EF Core

Si vous préférez gérer les migrations manuellement :

1. Se placer dans le dossier du projet :

```bash
cd Eshop
```

2. Créer la migration initiale (si elle n’existe pas) :

```bash
dotnet ef migrations add Initial -p src/Infrastructure -s src/API -o Migrations
```

3. Appliquer la migration sur la base de données :

```bash
dotnet ef database update -p src/Infrastructure -s src/API
```

4. Démarrer l’API :

```bash
dotnet run --project src/API
```

---

✅ Les deux méthodes permettent d’avoir une base PostgreSQL prête, avec toutes les migrations appliquées et des seeds automatiques configurés.


### 🔐 Configuration SSL Windows

Exécuter en administrateur :

```powershell
first-run.bat
```

- Génère un certificat auto-signé
- L’ajoute au store Windows
- Assure HTTPS pour localhost et Docker

---

## 🛠️ Tester l’API avec Swagger

Swagger UI : [https://localhost:7002/swagger](https://localhost:7002/swagger)

1. POST `/api/auth/login` pour récupérer un JWT  
2. Copier le token et cliquer sur **Authorize**  
3. Tester les endpoints protégés  

---

## ⚙️ Fonctionnalités Clés

- Auto-initialisation DB + Seed
- Health checks intégrés
- HTTPS obligatoire
- JWT Auth + rôles Admin/User
- Logging structuré
- CI/CD + Docker automatisé

---

## 🔒 Sécurité

- HTTPS obligatoire (certificat SAN)
- JWT Authentication
- Validation via Value Objects
- User roles : Admin / User

---

## 🔄 CI/CD & Pipeline

- Workflow : `.github/workflows/ci-cd.yml`
- Étapes :
  - Build & Test
  - Publish
  - Docker Build & Push → GHCR
- Déclencheur : push sur `main`

---

## 🐳 Docker & Monitoring

- Services : `api` (.NET 8), `db` (PostgreSQL)
- Health check :

```yaml
healthcheck:
  test: ["CMD", "stat", "/https/aspnetapp.crt"]
  interval: 3s
  timeout: 2s
  retries: 15
```

- Logs : `docker-compose logs -f api`
- Arrêt propre : `docker-compose down`

---

## 📜 Licence & Crédit

© 2025 – EShop Made with ❤️ by Koumeil  
License : MIT

