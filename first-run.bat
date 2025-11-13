@echo off
chcp 65001 >nul
cls
title 🔐 EShop - Configuration SSL Automatique

:: ================================
::      Title
:: ================================
echo.
echo ================================================
echo           EShop - Configuration SSL Automatique
echo ================================================
echo.

:: ================================
:: Vérification des privilèges Admin
:: ================================
echo 🔍 Vérification des privilèges administrateur...
net session
if %errorlevel% neq 0 (
    echo ❌ ERREUR : Exécutez ce script en tant qu'Administrateur !
    echo 1. Clic-droit sur first-run.bat
    echo 2. 'Exécuter en tant qu'administrateur'
    pause
    exit /b 1
)
echo ✅ Mode Administrateur confirmé
echo.

:: ================================
:: Vérification de Docker
:: ================================
echo 🔍 Vérification de Docker Desktop...
docker ps
if %errorlevel% neq 0 (
    echo ❌ Docker Desktop n'est pas démarré
    echo 1. Lancez Docker Desktop
    echo 2. Relancez ce script
    pause
    exit /b 1
)
echo ✅ Docker est en cours d'exécution
echo.

:: ================================
:: Vérification du container EShop
:: ================================
echo 🔍 Recherche du container "eshop-api-1"...
docker ps --filter "name=eshop-api-1" --format "{{.Names}}" | findstr "eshop-api-1"
if %errorlevel% neq 0 (
    echo ❌ Le container EShop n'est pas démarré
    echo 1. Executez : docker-compose up -d
    echo 2. Relancez ce script
    pause
    exit /b 1
)
echo ✅ Container EShop trouvé
echo.

:: ================================
:: Attente de l'application prête
:: ================================
echo ⏳ Attente que l'application soit prête...
set wait_count=0
:wait_health
docker inspect --format "{{.State.Health.Status}}" eshop-api-1 | findstr "healthy"
if %errorlevel% equ 0 (
    echo ✅ Application saine et prête !
    goto health_ready
)

docker exec eshop-api-1 test -f /https/aspnetapp.crt
if %errorlevel% equ 0 (
    echo ✅ Certificat détecté dans le container !
    goto health_ready
)

set /a wait_count+=1
if %wait_count% gtr 45 (
    echo ℹ️  Continuation sans attendre plus longtemps...
    goto health_ready
)

timeout /t 2 /nobreak >nul
goto wait_health

:health_ready
echo.

:: ================================
:: Nettoyage des anciens certificats
:: ================================
echo 🗑️  Suppression des anciens certificats Root...
certutil -delstore Root "localhost"
echo ✅ Nettoyage terminé
echo.

:: ================================
:: Extraction du certificat depuis Docker
:: ================================
echo 📥 Extraction du certificat depuis le container...
docker cp eshop-api-1:/https/aspnetapp.crt eshop-api-new.crt
if not exist "eshop-api-new.crt" (
    echo ❌ Erreur : impossible d'extraire le certificat
    pause
    exit /b 1
)
echo ✅ Certificat copié : eshop-api-new.crt
echo.

:: ================================
:: Installation du certificat dans Windows
:: ================================
echo 🔧 Ajout au store Windows...
certutil -addstore -f Root eshop-api-new.crt
echo 🔍 Vérification de l'installation...
certutil -store Root | findstr "localhost"
if %errorlevel% equ 0 (
    echo ✅ Certificat installé avec succès
) else (
    echo ❌ Erreur lors de l'installation
)
echo.

:: ================================
:: Nettoyage du fichier temporaire
:: ================================
echo 🗑️  Suppression du fichier temporaire...
del eshop-api-new.crt
echo ✅ Nettoyage terminé
echo.

:: ================================
:: Redémarrage des services système
:: ================================
echo 🔄 Redémarrage du service de chiffrement (CryptSvc)...
powershell -Command "Stop-Service -Name 'CryptSvc' -Force"
powershell -Command "Start-Service -Name 'CryptSvc'"
ipconfig /flushdns
echo ✅ Services redémarrés et DNS flush
echo.

:: ================================
:: Fin du script
:: ================================
echo ================================================
echo ✅ CONFIGURATION TERMINEE AVEC SUCCES !
echo ================================================
echo.
echo 🌐 Ouvrez maintenant : https://localhost:7002
echo 🔒 Plus d'avertissement "Non sécurisé" !
echo.

echo Fermeture automatique dans 5 secondes...
timeout /t 10 /nobreak >nul
exit
