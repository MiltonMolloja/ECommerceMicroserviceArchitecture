const crypto = require('crypto');

// Header
const header = {
    alg: "HS256",
    typ: "JWT"
};

// Payload - 1 año de duración
const now = Math.floor(Date.now() / 1000);
const oneYear = 365 * 24 * 60 * 60;
const exp = now + oneYear;

const payload = {
    nameid: "65812A2A-2B92-4357-A48F-E3C9FD5755BE",
    email: "admin@gmail.com",
    unique_name: "Admin",
    family_name: "Administrator",
    nbf: now,
    exp: exp,
    iat: now
};

// Secret key
const secret = "molloja-ecommerce-secret-key-super-secure-2025-minimum-32chars";

// Encode to base64url
function base64urlEncode(str) {
    return Buffer.from(JSON.stringify(str))
        .toString('base64')
        .replace(/\+/g, '-')
        .replace(/\//g, '_')
        .replace(/=/g, '');
}

const headerEncoded = base64urlEncode(header);
const payloadEncoded = base64urlEncode(payload);

// Create signature
const signatureInput = `${headerEncoded}.${payloadEncoded}`;
const signature = crypto
    .createHmac('sha256', secret)
    .update(signatureInput)
    .digest('base64')
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=/g, '');

// Create JWT
const jwt = `${headerEncoded}.${payloadEncoded}.${signature}`;

console.log("Token JWT (válido por 1 año):");
console.log(jwt);
console.log("\nFecha de creación:", new Date(now * 1000).toISOString());
console.log("Fecha de expiración:", new Date(exp * 1000).toISOString());
