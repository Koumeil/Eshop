# 🛍️ EShop - Documentation Technique

<p align="start"> 
  <img src="https://img.shields.io/github/actions/workflow/status/koumeil/eshop/ci-cd.yml?branch=main&label=CI%2FCD&logo=github&style=flat-square" alt="GitHub Actions"> 
  <img src="https://img.shields.io/badge/Container-GHCR.io-blue?logo=docker&style=flat-square" alt="GHCR"> 
  <img src="https://img.shields.io/badge/.NET-8.0-blueviolet?logo=dotnet&style=flat-square" alt=".NET 8"> 
</p>

---

## 🧭 Table des Matières

1. [📋 Vue d'ensemble](#-vue-densemble)
2. [🏗️ Architecture](#️-architecture)
   - [⚙️ Stack Technique](#️-stack-technique)
   - [🗂️ Structure du Projet](#️-structure-du-projet)
3. [🚀 Démarrage Rapide](#-démarrage-rapide)
   - [🧩 Prérequis](#-prérequis)
   - [⚡ Installation avec Docker](#-installation-avec-docker)
   - [🔐 Configuration SSL Automatique (Windows)](#-configuration-ssl-automatique-windows)
   - [⚡ Démarrage Rapide sans Docker](#-démarrage-rapide-sans-docker)
4. [🛠️ Tester l’API via Swagger](#-tester-lapi-via-swagger)
5. [⚙️ Fonctionnalités Techniques](#️-fonctionnalités-techniques)
6. [🔒 Sécurité](#-sécurité)
7. [🔄 CI/CD Pipeline](#-cicd-pipeline)
8. [🧰 Configuration Docker](#-configuration-docker)
9. [🔐 Gestion SSL](#-gestion-ssl)
10. [📊 Monitoring & Maintenance](#-monitoring--maintenance)
11. [🎯 Points Clés](#-points-clés)
12. [📜 Licence et Crédit](#-licence-et-crédit)

---

## 📋 Vue d'ensemble

**EShop** est une plateforme e-commerce moderne construite avec **.NET 8**, suivant les principes **Clean Architecture** et **Domain-Driven Design**.  
Le projet intègre un pipeline **CI/CD complet**, avec déploiement containerisé et automatisé.

---

## 🏗️ Architecture

### ⚙️ Stack Technique

| Composant / Domaine         | Technologie / Description |
|-----------------------------|---------------------------|
| **Backend**                 | .NET 8, ASP.NET Core, **Clean Architecture**, **DDD**, **MediatR**, Domain Events, Value Objects |
| **Base de données**         | PostgreSQL 16, EF Core, migrations et seeds automatisés |
| **Frontend**                | React (frontend/), TypeScript, consommation API REST, JWT + HTTPS |
| **Sécurité**                | JWT Authentication, HTTPS obligatoire, rôles et claims, endpoints protégés |
| **Containerisation**        | Docker + Docker Compose, volumes persistants, health checks, réseau sécurisé |
| **CI/CD**                   | GitHub Actions, tests unitaires & intégration, Docker Build & Push GHCR |
| **Tests & Qualité**         | xUnit, Moq, couverture code, tests migrations & seeds |
| **Logging & Monitoring**    | Serilog, health checks, logs structurés |
| **Architecture globale**    | Couches API / Application / Domain / Infrastructure, séparation claire des responsabilités |

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

---

## 🚀 Démarrage Rapide

### 🧩 Prérequis

- 🐳 Docker Desktop (avec Docker Compose) pour la méthode Docker  
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) pour la méthode sans Docker  
- PostgreSQL 16 ou version compatible  
- PowerShell (Windows) ou terminal compatible  
- Droits administrateur pour la configuration SSL  

---

### ⚡ Installation avec Docker

```bash
# 1️⃣ Cloner le dépôt
git clone <repository>

# 2️⃣ Se placer dans le dossier du projet
cd Eshop

# 3️⃣ Construire et démarrer les conteneurs
docker-compose up --build
```

---

### 🔐 Configuration SSL Automatique (Windows)

⚠️ Exécuter en tant qu’administrateur

```bash
first-run.bat
```

---

### ⚡ Démarrage Rapide sans Docker

Si vous ne souhaitez pas utiliser Docker, vous pouvez lancer l’application et initialiser la base de données localement. Deux méthodes sont disponibles.

---

#### Méthode 1️⃣ : Script PowerShell `setup-db.ps1` (recommandée)

Ce script automatise :

- La restauration des packages NuGet
- La création de la migration initiale (si elle n’existe pas)
- L’application des migrations sur PostgreSQL

**Étapes :**

1. Ouvrir PowerShell en Administrateur
2. Autoriser l’exécution des scripts (si nécessaire, une seule fois) :

```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

3. Exécuter le script :

```powershell
.\setup-db.ps1
```

> Le script affichera les étapes et confirmera que la base est initialisée.  
> Ensuite, démarrez l’API :

```bash
dotnet run --project src/API
```

---

#### Méthode 2️⃣ : Commandes EF Core manuelles

1. Se placer dans le dossier du projet :

```bash
cd Eshop
```

2. Créer la migration initiale (si inexistante) :

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

### 🛠️ Tester l’API via Swagger

1️⃣ Ouvrir Swagger UI :  

```bash
https://localhost:7002/swagger
```

2️⃣ Se connecter pour obtenir un JWT  

- Dans Swagger, trouver le controller `Auth`
- Ouvrir la méthode POST `/api/auth/login`
- Cliquer sur `Try it out`
- Remplir le corps JSON :

```json
{
  "email": "alice.martin@example.com",
  "password": "Password123!"
}
```

- Cliquer sur `Execute`  
- Copier le token JWT renvoyé :

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

3️⃣ Utiliser le token pour tester les endpoints protégés  

- Cliquer sur le bouton **Authorize** en haut à droite de Swagger
- Coller le token précédé de `Bearer ` (sans guillemets) :

```text
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

- Cliquer sur **Authorize** puis **Close**  
- Tous les endpoints protégés par `[Authorize]` sont maintenant accessibles  

4️⃣ Tester les endpoints selon les rôles :

- Utilisateurs classiques → consulter/éditer leurs données  
- Admin → voir tous les utilisateurs, supprimer des comptes, etc.

| Service          | URL                                                              |
| ---------------- | ---------------------------------------------------------------- |
| **Application**  | [https://localhost:7002](https://localhost:7002)                 |
| **Swagger UI**   | [https://localhost:7002/swagger](https://localhost:7002/swagger) |
| **Health Check** | [https://localhost:7002/health](https://localhost:7002/health)   |
| **PostgreSQL**   | localhost:5432                                                   |

---

### ⚙️ Fonctionnalités Techniques

- ✅ Migrations Base de Données (avec retry logic)  
- ✅ Peuplement via userSeeds.json  
- ✅ Certificat SSL auto-signé avec SAN  
- ✅ Health Checks intégrés  

---

### 🔒 Sécurité

- HTTPS obligatoire  
- Certificats SAN (localhost + host.docker.internal)  
- JWT Authentication  
- Value Objects avec validation métier intégrée  

---

### 🔄 CI/CD Pipeline

- Fichier : `.github/workflows/ci-cd.yml`  
- Déclencheur : push sur la branche `main`  

**Étapes principales :**

- Setup Certificate – Décode le certificat depuis les secrets GitHub  
- Build & Test – Restauration, compilation et tests  
- Publish – Publication de l’application  
- Docker Build & Push – Envoi vers GHCR  

---

### 🐳 Container Registry

| Élément     | Détail                         |
| ----------- | ------------------------------ |
| **Images**  | `ghcr.io/koumeil/eshop:latest` |
| **Tags**    | `latest`, `commit SHA`         |
| **Secrets** | Certificat SSL + mot de passe  |

---

### 🧰 Configuration Docker

- **Services Déployés** :  
  - api → Application .NET 8 (HTTPS)  
  - db → PostgreSQL 16 (volume persistant)  

- **Health Checks** :

```yaml
healthcheck:
  test: ["CMD", "stat", "/https/aspnetapp.crt"]
  interval: 3s
  timeout: 2s
  retries: 15
  start_period: 5s
```

- **Variables d’Environnement** :

```yaml
ASPNETCORE_URLS: https://+:7002
ASPNETCORE_Kestrel__Certificates__Default__Path: /https/aspnetapp.pfx
ConnectionStrings__DefaultConnection: Host=db;Port=5432;Database=eshop
```

---

### 🔐 Gestion SSL

- Script `init-ssl.sh` → génération certificat SAN, keystore PKCS12, ajout au store de confiance du conteneur  
- Windows `first-run.bat` → extraction certificat, installation dans store Root Windows, redémarrage services cryptographiques  

---

### 📊 Monitoring & Maintenance

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

- Health endpoints : `GET /health` → état de l’application et base  
- Logs structurés avec niveaux de sévérité  

---

### 🎯 Points Clés

| 💡 Objectif              | 🧩 Description                  |
| ------------------------ | -------------------------------- |
| **Zero Configuration**   | Démarrage immédiat après clone   |
| **HTTPS First**          | Sécurité par défaut              |
| **Automation Complete**  | DB, SSL, Seeds automatiques      |
| **Production Ready**     | CI/CD, Health Checks, Monitoring |
| **Developer Experience** | Environnement cohérent dev/prod  |

---

### 🔔 Note

Le certificat SSL est auto-signé → les navigateurs afficheront un avertissement.  
Exécutez `first-run.bat` pour l’ajouter au store de confiance Windows.

---

### © 2025 – EShop Made with ❤️ by Koumeil
