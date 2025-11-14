#!/bin/bash

echo "🔐 Initialisation SSL..."
mkdir -p /https

# ✅ Créer un fichier de configuration SSL avec SAN
cat > /https/ssl.conf << EOF
[req]
default_bits = 2048
prompt = no
default_md = sha256
distinguished_name = dn
x509_extensions = v3_req

[dn]
C = FR
ST = Bruxelles
L = Bruxelles
O = EShop
CN = localhost

[v3_req]
keyUsage = keyEncipherment, dataEncipherment, digitalSignature
extendedKeyUsage = serverAuth
subjectAltName = @alt_names

[alt_names]
DNS.1 = localhost
DNS.2 = host.docker.internal
IP.1 = 127.0.0.1
EOF

# ✅ Générer le certificat AVEC la configuration SAN
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
    -keyout /https/aspnetapp.key \
    -out /https/aspnetapp.crt \
    -config /https/ssl.conf \
    -extensions v3_req

echo "✅ Certificat généré avec SAN"

# Vérifier le certificat
echo "📋 Vérification du certificat :"
openssl x509 -in /https/aspnetapp.crt -text -noout | grep -A 5 "Subject Alternative Name"

# 2. Créer la version PKCS12
openssl pkcs12 -export -out /https/aspnetapp.pfx \
    -inkey /https/aspnetapp.key \
    -in /https/aspnetapp.crt \
    -passout pass:${CERT_PASSWORD}


echo "✅ PKCS12 créé"

# 3. Ajouter le certificat au store de confiance
cp /https/aspnetapp.crt /usr/local/share/ca-certificates/eshop-api.crt
update-ca-certificates

echo "✅ Certificat ajouté au store de confiance"
echo "🎉 SSL initialisé avec succès"