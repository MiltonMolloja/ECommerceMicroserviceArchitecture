# MercadoPago - Tarjetas de Prueba

Este documento describe cómo probar diferentes escenarios de pago usando el **MockPaymentGateway** que simula el comportamiento de MercadoPago.

## 🎯 Configuración Actual

El sistema está configurado con:
- **Provider**: `Mock` (ver `appsettings.json` del Payment.Api)
- **Simulación**: Estados de MercadoPago basados en el nombre del titular

## 🧪 Nombres de Titular para Testing

El **nombre del titular de la tarjeta** determina el resultado del pago:

### ✅ Pagos Aprobados

| Nombre | Resultado | Descripción |
|--------|-----------|-------------|
| `APRO` | Aprobado | Pago procesado exitosamente |

### ❌ Pagos Rechazados

| Nombre | Código de Error | Descripción |
|--------|----------------|-------------|
| `CALL` | `cc_rejected_call_for_authorize` | Rechazado - Llamar para autorizar |
| `FUND` | `cc_rejected_insufficient_amount` | Rechazado por monto insuficiente |
| `SECU` | `cc_rejected_bad_filled_security_code` | Rechazado por código de seguridad inválido |
| `EXPI` | `cc_rejected_bad_filled_date` | Rechazado por fecha de expiración inválida |
| `FORM` | `cc_rejected_bad_filled_other` | Rechazado por error en formulario |
| `BLAC` | `cc_rejected_blacklist` | Rechazado - Tarjeta deshabilitada |
| `CARD` | `cc_rejected_invalid_installments` | Rechazado - Cuotas inválidas |
| `DUPL` | `cc_rejected_duplicated_payment` | Rechazado - Pago duplicado |
| `HIGH` | `cc_rejected_high_risk` | Rechazado - Alto riesgo |
| `OTHE` | `cc_rejected_other_reason` | Rechazado por error general |

### ⏳ Pagos Pendientes

| Nombre | Código de Estado | Descripción |
|--------|-----------------|-------------|
| `CONT` | `pending_contingency` | Pago pendiente de revisión |
| `PCONT` | `pending_contingency` | Pago pendiente de revisión |

## 📝 Ejemplo de Uso

### Escenario 1: Probar Rechazo por Fondos Insuficientes

1. En el checkout, ingresá:
   - **Número de tarjeta**: `5031 4332 1540 6351` (Mastercard de prueba)
   - **Nombre del titular**: `FUND`
   - **Fecha de expiración**: `11/25`
   - **CVV**: `123`
   - **Tipo de documento**: `DNI`
   - **Número de documento**: `12345678`

2. Al procesar el pago, verás:
   - ❌ Pago rechazado
   - Mensaje: "Rechazado por monto insuficiente"
   - Código: `cc_rejected_insufficient_amount`

### Escenario 2: Probar Rechazo por Tarjeta Deshabilitada

1. En el checkout, ingresá:
   - **Número de tarjeta**: `5031 4332 1540 6351`
   - **Nombre del titular**: `BLAC`
   - **Fecha de expiración**: `11/25`
   - **CVV**: `123`
   - **Tipo de documento**: `DNI`
   - **Número de documento**: `12345678`

2. Al procesar el pago, verás:
   - ❌ Pago rechazado
   - Mensaje: "Rechazado - Tarjeta deshabilitada"
   - Código: `cc_rejected_blacklist`

### Escenario 3: Probar Pago Aprobado

1. En el checkout, ingresá:
   - **Número de tarjeta**: `5031 4332 1540 6351`
   - **Nombre del titular**: `APRO`
   - **Fecha de expiración**: `11/25`
   - **CVV**: `123`
   - **Tipo de documento**: `DNI`
   - **Número de documento**: `12345678`

2. Al procesar el pago, verás:
   - ✅ Pago aprobado
   - Transaction ID generado
   - Orden completada

## 🔍 Logs del MockPaymentGateway

El MockPaymentGateway loggea información detallada:

```
[MOCK GATEWAY] Processing payment for amount 1500.00 ARS
[MOCK GATEWAY] CardholderName: BLAC, DNI: 12345678
[MOCK GATEWAY] ✓ Test cardholder name detected: 'BLAC' -> Status: rejected
[MOCK GATEWAY] ✗ Payment REJECTED - Rechazado - Tarjeta deshabilitada (Code: cc_rejected_blacklist)
```

Si usás un nombre que NO es de prueba:

```
[MOCK GATEWAY] Processing payment for amount 1500.00 ARS
[MOCK GATEWAY] CardholderName: JUAN PEREZ, DNI: 12345678
[MOCK GATEWAY] ⚠ Cardholder name 'JUAN PEREZ' is NOT a test name. Valid test names: APRO, CALL, FUND, SECU, EXPI, FORM, BLAC, CARD, DUPL, HIGH, OTHE, CONT, PCONT
[MOCK GATEWAY] Proceeding with default approval logic (non-test mode)
[MOCK GATEWAY] Payment succeeded - TransactionID: MOCK_1234567890_ABCD1234
```

## 🔄 Cambiar a MercadoPago Real

Para usar el gateway real de MercadoPago en lugar del Mock:

1. Editá `appsettings.json` del Payment.Api:

```json
"PaymentGateway": {
    "Provider": "MercadoPago"  // Cambiar de "Mock" a "MercadoPago"
}
```

2. Asegurate de tener configuradas las credenciales de MercadoPago:

```json
"MercadoPago": {
    "PublicKey": "APP_USR-tu-public-key",
    "AccessToken": "APP_USR-tu-access-token"
}
```

3. Con MercadoPago real, usá las [tarjetas de prueba oficiales](https://www.mercadopago.com.ar/developers/es/docs/your-integrations/test/cards)

## 📚 Tarjetas de Prueba de MercadoPago (Real)

Cuando uses el gateway real de MercadoPago, estas son las tarjetas de prueba:

### Mastercard
- **Número**: `5031 4332 1540 6351`
- **CVV**: Cualquier 3 dígitos
- **Fecha**: Cualquier fecha futura

### Visa
- **Número**: `4509 9535 6623 3704`
- **CVV**: Cualquier 3 dígitos
- **Fecha**: Cualquier fecha futura

### American Express
- **Número**: `3711 803032 57522`
- **CVV**: Cualquier 4 dígitos
- **Fecha**: Cualquier fecha futura

## ⚠️ Notas Importantes

1. **Case Sensitive**: Los nombres de prueba deben estar en MAYÚSCULAS (`APRO`, no `apro`)
2. **Exactitud**: El nombre debe ser exacto (`BLAC`, no `BLACK`)
3. **Modo Mock**: Solo funciona cuando `PaymentGateway:Provider` está configurado como `"Mock"`
4. **Logs**: Revisá los logs del Payment.Api para ver qué está pasando

## 🐛 Troubleshooting

### Problema: Usé "BLAC" pero el pago fue aprobado

**Causa**: El nombre no se está enviando correctamente desde el frontend, o el Payment.Api no está usando el MockGateway.

**Solución**:
1. Verificá los logs del Payment.Api
2. Asegurate que `PaymentGateway:Provider` sea `"Mock"` en `appsettings.json`
3. Verificá que el frontend esté enviando el `cardholderName` en el request

### Problema: No veo los logs del MockGateway

**Causa**: El nivel de logging está muy alto.

**Solución**: Editá `appsettings.json`:

```json
"Logging": {
    "LogLevel": {
        "Default": "Information",  // Cambiar de "Error" a "Information"
        "Payment.Service.Gateways.Mock": "Information"
    }
}
```

## 📖 Referencias

- [Documentación oficial de MercadoPago - Tarjetas de prueba](https://www.mercadopago.com.ar/developers/es/docs/your-integrations/test/cards)
- [Códigos de estado de MercadoPago](https://www.mercadopago.com.ar/developers/es/reference/payments/_payments/post)
