CERT_DIR=/etc/nginx/certs

echo "=== Debug information ==="
echo "Current directory: $(pwd)"
echo "Certificate directory: $CERT_DIR"
echo "Contents of $CERT_DIR:"
ls -la $CERT_DIR/ 2>/dev/null || echo "Cannot access certificate directory"
echo "========================"

while [ ! -f "$CERT_DIR/localhost.crt" ] || [ ! -f "$CERT_DIR/localhost.key" ]; do
  echo "Waiting for SSL certificates..."
  echo "Files in $CERT_DIR:"
  ls -la $CERT_DIR/ 2>/dev/null || echo "Directory not accessible"
  sleep 2
done

echo "SSL certificates found, starting Nginx..."
nginx -g "daemon off;"