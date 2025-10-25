import json
import base64
import hmac
import hashlib
from datetime import datetime, timedelta

# Header
header = {
    "alg": "HS256",
    "typ": "JWT"
}

# Payload - 1 año de duración
now = datetime.utcnow()
exp = now + timedelta(days=365)

payload = {
    "nameid": "65812A2A-2B92-4357-A48F-E3C9FD5755BE",
    "email": "admin@gmail.com",
    "unique_name": "Admin",
    "family_name": "Administrator",
    "nbf": int(now.timestamp()),
    "exp": int(exp.timestamp()),
    "iat": int(now.timestamp())
}

# Secret key
secret = "molloja-ecommerce-secret-key-super-secure-2025-minimum-32chars"

# Encode header and payload
def base64url_encode(data):
    json_data = json.dumps(data, separators=(',', ':')).encode('utf-8')
    return base64.urlsafe_b64encode(json_data).rstrip(b'=').decode('utf-8')

header_encoded = base64url_encode(header)
payload_encoded = base64url_encode(payload)

# Create signature
message = f"{header_encoded}.{payload_encoded}".encode('utf-8')
signature = hmac.new(secret.encode('utf-8'), message, hashlib.sha256).digest()
signature_encoded = base64.urlsafe_b64encode(signature).rstrip(b'=').decode('utf-8')

# Create JWT
jwt_token = f"{header_encoded}.{payload_encoded}.{signature_encoded}"

print(f"Token JWT (válido por 1 año):")
print(jwt_token)
print(f"\nFecha de creación: {now.isoformat()}Z")
print(f"Fecha de expiración: {exp.isoformat()}Z")
