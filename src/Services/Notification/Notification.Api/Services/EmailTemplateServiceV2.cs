using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Notification.Api.Services
{
    public class EmailTemplateServiceV2 : IEmailTemplateService
    {
        private readonly ILogger<EmailTemplateServiceV2> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;
        private readonly Dictionary<string, string> _templateCache = new Dictionary<string, string>();

        public EmailTemplateServiceV2(
            ILogger<EmailTemplateServiceV2> logger,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            _logger = logger;
            _configuration = configuration;
            _environment = environment;
        }

        public Task<(string subject, string htmlBody, string textBody)> RenderTemplateAsync(string templateName, object data)
        {
            try
            {
                // Convertir data a dictionary
                Dictionary<string, object> dataDict = ConvertToDict(data);

                // Añadir variables globales
                dataDict["year"] = DateTime.Now.Year.ToString();
                dataDict["baseUrl"] = _configuration.GetValue<string>("BaseUrl") ?? "https://ecommerce.com";

                string subject, htmlBody, textBody;

                switch (templateName.ToLower())
                {
                    case "email-confirmation":
                    case "email-verification":
                        {
                            subject = "Verifica tu Correo Electrónico - ECommerce";
                            // Generar el link de verificación
                            var verificationLink = GenerateVerificationLink(dataDict);
                            dataDict["verificationLink"] = verificationLink;
                            htmlBody = RenderEmailVerificationTemplate(dataDict);
                            textBody = $"Hola {GetValue(dataDict, "FirstName")},\n\nGracias por registrarte en ECommerce.\n\nPor favor verifica tu correo haciendo clic en el siguiente enlace:\n{verificationLink}\n\nEste enlace expirará en 24 horas.\n\nSaludos,\nEl equipo de ECommerce";
                        }
                        break;

                    case "password-reset":
                        subject = "Restablece tu Contraseña - ECommerce";
                        htmlBody = RenderPasswordResetTemplate(dataDict);
                        textBody = $"Hola {GetValue(dataDict, "FirstName")}, usa este token para restablecer tu contraseña: {GetValue(dataDict, "ResetToken")}";
                        break;

                    case "password-changed":
                        subject = "Contraseña Cambiada Exitosamente - ECommerce";
                        htmlBody = RenderPasswordChangedTemplate(dataDict);
                        textBody = $"Hola {GetValue(dataDict, "FirstName")}, tu contraseña ha sido cambiada exitosamente.";
                        break;

                    case "2fa-enabled":
                        subject = "Autenticación de Dos Factores Habilitada - ECommerce";
                        htmlBody = Render2FAEnabledTemplate(dataDict);
                        textBody = $"Hola {GetValue(dataDict, "FirstName")}, la autenticación de dos factores ha sido activada.";
                        break;

                    case "2fa-backup-codes":
                        subject = "Códigos de Respaldo 2FA - ECommerce";
                        htmlBody = Render2FABackupCodesTemplate(dataDict);
                        textBody = $"Hola {GetValue(dataDict, "FirstName")}, aquí están tus códigos de respaldo 2FA.";
                        break;

                    case "new-session-alert":
                        subject = "Nueva Sesión Detectada - ECommerce";
                        htmlBody = RenderNewSessionAlertTemplate(dataDict);
                        textBody = $"Hola {GetValue(dataDict, "FirstName")}, se detectó una nueva sesión en tu cuenta.";
                        break;

                    case "2fa-disabled":
                        subject = "Autenticación de Dos Factores Desactivada - ECommerce";
                        htmlBody = Render2FADisabledTemplate(dataDict);
                        textBody = $"Hola {GetValue(dataDict, "FirstName")}, la autenticación de dos factores ha sido desactivada.";
                        break;

                    case "purchase-confirmation":
                    case "order-confirmation":
                        subject = $"Confirmación de Compra - Pedido {GetValue(dataDict, "OrderNumber")} - ECommerce";
                        htmlBody = RenderPurchaseConfirmationTemplate(dataDict);
                        textBody = $"Hola {GetValue(dataDict, "CustomerName")}, tu pedido {GetValue(dataDict, "OrderNumber")} ha sido confirmado.";
                        break;

                    case "payment-failed":
                        subject = $"Pago Rechazado - Pedido {GetValue(dataDict, "OrderNumber")} - Acción Requerida";
                        htmlBody = RenderPaymentFailedTemplate(dataDict);
                        textBody = $"Hola {GetValue(dataDict, "CustomerName")}, tu pago para el pedido {GetValue(dataDict, "OrderNumber")} fue rechazado.";
                        break;

                    case "refund-processed":
                        subject = $"Reembolso Procesado - {GetValue(dataDict, "RefundNumber")} - ECommerce";
                        htmlBody = RenderRefundProcessedTemplate(dataDict);
                        textBody = $"Hola {GetValue(dataDict, "CustomerName")}, tu reembolso {GetValue(dataDict, "RefundNumber")} ha sido procesado.";
                        break;

                    case "welcome-email":
                    case "welcomeemail":
                        subject = "¡Bienvenido a ECommerce!";
                        htmlBody = RenderWelcomeEmailTemplate(dataDict);
                        textBody = $"Hola {GetValue(dataDict, "CustomerName")}, ¡bienvenido a ECommerce! Gracias por registrarte.";
                        break;

                    case "cart-abandoned":
                    case "cartabandoned":
                        subject = "¡No olvides tu carrito! - ECommerce";
                        htmlBody = RenderCartAbandonedTemplate(dataDict);
                        textBody = $"Hola {GetValue(dataDict, "CustomerName")}, tienes productos esperándote en tu carrito.";
                        break;

                    case "order-cancelled":
                    case "ordercancelled":
                        subject = $"Pedido Cancelado - {GetValue(dataDict, "OrderNumber")} - ECommerce";
                        htmlBody = RenderOrderCancelledTemplate(dataDict);
                        textBody = $"Hola {GetValue(dataDict, "CustomerName")}, tu pedido {GetValue(dataDict, "OrderNumber")} ha sido cancelado.";
                        break;

                    case "order-shipped":
                    case "ordershipped":
                        subject = $"¡Tu pedido está en camino! - {GetValue(dataDict, "OrderNumber")} - ECommerce";
                        htmlBody = RenderOrderShippedTemplate(dataDict);
                        textBody = $"Hola {GetValue(dataDict, "CustomerName")}, tu pedido {GetValue(dataDict, "OrderNumber")} ha sido enviado.";
                        break;

                    case "order-delivered":
                    case "orderdelivered":
                        subject = $"¡Pedido Entregado! - {GetValue(dataDict, "OrderNumber")} - ECommerce";
                        htmlBody = RenderOrderDeliveredTemplate(dataDict);
                        textBody = $"Hola {GetValue(dataDict, "CustomerName")}, tu pedido {GetValue(dataDict, "OrderNumber")} ha sido entregado.";
                        break;

                    case "review-request":
                    case "reviewrequest":
                        subject = "¿Qué te pareció tu compra? - ECommerce";
                        htmlBody = RenderReviewRequestTemplate(dataDict);
                        textBody = $"Hola {GetValue(dataDict, "CustomerName")}, nos encantaría conocer tu opinión sobre los productos que compraste.";
                        break;

                    default:
                        _logger.LogWarning($"Template not found: {templateName}");
                        subject = "Notificación - ECommerce";
                        htmlBody = RenderDefaultTemplate(dataDict);
                        textBody = "Notificación de ECommerce";
                        break;
                }

                return Task.FromResult((subject, htmlBody, textBody));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error rendering template {templateName}");
                throw;
            }
        }

        private Dictionary<string, object> ConvertToDict(object data)
        {
            if (data is Dictionary<string, object> dict)
            {
                return dict;
            }
            else if (data is System.Text.Json.JsonElement jsonElement)
            {
                var result = new Dictionary<string, object>();
                foreach (var prop in jsonElement.EnumerateObject())
                {
                    result[prop.Name] = prop.Value.ToString();
                }
                return result;
            }
            else
            {
                var properties = data.GetType().GetProperties();
                var result = new Dictionary<string, object>();
                foreach (var prop in properties)
                {
                    result[prop.Name] = prop.GetValue(data);
                }
                return result;
            }
        }

        private string GetValue(Dictionary<string, object> data, string key)
        {
            // Try exact match first
            if (data.ContainsKey(key))
                return data[key]?.ToString() ?? "";

            // Try case-insensitive search
            var foundKey = data.Keys.FirstOrDefault(k => k.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (foundKey != null)
                return data[foundKey]?.ToString() ?? "";

            return "";
        }

        private string ReplaceVariables(string template, Dictionary<string, object> data)
        {
            var result = template;
            foreach (var kvp in data)
            {
                result = result.Replace($"{{{{{kvp.Key}}}}}", kvp.Value?.ToString() ?? "");
            }
            return result;
        }

        private string LoadTemplateFromFile(string templateFileName)
        {
            try
            {
                // Check cache first
                if (_templateCache.ContainsKey(templateFileName))
                {
                    return _templateCache[templateFileName];
                }

                // Get template file path
                var templatePath = Path.Combine(_environment.ContentRootPath, "Templates", templateFileName);

                if (!File.Exists(templatePath))
                {
                    _logger.LogWarning($"Template file not found: {templatePath}");
                    return null;
                }

                // Read template content
                var templateContent = File.ReadAllText(templatePath);

                // Cache it
                _templateCache[templateFileName] = templateContent;

                _logger.LogInformation($"Template loaded successfully: {templateFileName}");
                return templateContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading template file: {templateFileName}");
                return null;
            }
        }

        private string RenderTemplateWithHandlebars(string template, Dictionary<string, object> data)
        {
            var result = template;

            // Handle {{#each}} blocks for arrays FIRST (before simple variable replacement)
            var eachPattern = @"\{\{#each\s+(\w+)\}\}(.*?)\{\{/each\}\}";
            var eachMatches = Regex.Matches(result, eachPattern, RegexOptions.Singleline);

            foreach (Match match in eachMatches)
            {
                var arrayName = match.Groups[1].Value;
                var blockTemplate = match.Groups[2].Value;

                if (data.ContainsKey(arrayName) && data[arrayName] is IEnumerable<object> items)
                {
                    var renderedBlocks = new StringBuilder();
                    var index = 0;

                    foreach (var item in items)
                    {
                        var blockData = new Dictionary<string, object>(data);
                        blockData["@index"] = (index + 1).ToString();

                        // If item is a dictionary or has properties, add them to blockData
                        if (item is Dictionary<string, object> itemDict)
                        {
                            foreach (var kvp in itemDict)
                            {
                                blockData[kvp.Key] = kvp.Value;
                            }
                        }
                        else if (item != null)
                        {
                            // Try to extract properties from the object
                            var properties = item.GetType().GetProperties();
                            foreach (var prop in properties)
                            {
                                blockData[prop.Name] = prop.GetValue(item);
                            }
                            blockData["this"] = item.ToString();
                        }

                        var renderedBlock = RenderSimpleVariables(blockTemplate, blockData);
                        renderedBlocks.Append(renderedBlock);
                        index++;
                    }

                    result = result.Replace(match.Value, renderedBlocks.ToString());
                }
            }

            // Find all {{variable}} patterns in the template
            result = RenderSimpleVariables(result, data);

            return result;
        }

        private string RenderSimpleVariables(string template, Dictionary<string, object> data)
        {
            var result = template;
            var variablePattern = @"\{\{(\w+)\}\}";
            var matches = Regex.Matches(result, variablePattern);

            // Replace each variable found
            foreach (Match match in matches)
            {
                var variableName = match.Groups[1].Value;
                var variableValue = GetValue(data, variableName);

                if (!string.IsNullOrEmpty(variableValue))
                {
                    result = result.Replace($"{{{{{variableName}}}}}", variableValue);
                }
            }

            return result;
        }

        // Los templates HTML completos se incluyen aquí con lógica de reemplazo de variables
        private string RenderEmailVerificationTemplate(Dictionary<string, object> data)
        {
            // Prepare data with computed values
            var firstName = GetValue(data, "FirstName");
            var verificationLink = GetValue(data, "verificationLink");

            // Try to load from file first
            var template = LoadTemplateFromFile("email-confirmation.html");

            if (template == null)
            {
                _logger.LogWarning("Could not load email-confirmation.html, using fallback template");
                // Fallback to inline template
                return $@"<html><body><h1>Verifica tu correo</h1><p>Hola {firstName},</p><p>Por favor verifica tu correo haciendo clic en el siguiente enlace:</p><a href=""{verificationLink}"">Verificar Email</a></body></html>";
            }

            // Render template with data
            var renderedTemplate = RenderTemplateWithHandlebars(template, data);

            // Replace AuthApp with ECommerce branding
            renderedTemplate = renderedTemplate.Replace("AuthApp", "ECommerce");

            return renderedTemplate;
        }

        private string RenderPasswordChangedTemplate(Dictionary<string, object> data)
        {
            // Prepare data with computed values
            var firstName = GetValue(data, "FirstName");
            var ipAddress = GetValue(data, "IpAddress");

            // Parse change time
            DateTime changeTime = DateTime.UtcNow;
            if (data.ContainsKey("ChangeTime"))
            {
                if (data["ChangeTime"] is DateTime dt)
                    changeTime = dt;
                else if (DateTime.TryParse(data["ChangeTime"]?.ToString(), out DateTime parsed))
                    changeTime = parsed;
            }

            // Format date and time
            var date = changeTime.ToString("dd/MM/yyyy");
            var time = changeTime.ToString("HH:mm:ss");

            // Try to load from file first
            var template = LoadTemplateFromFile("password-changed.html");

            if (template == null)
            {
                _logger.LogWarning("Could not load password-changed.html, using fallback template");
                return $@"<html><body><h1>Contraseña Cambiada</h1><p>Hola {firstName},</p><p>Tu contraseña ha sido cambiada exitosamente.</p><p>Fecha: {date} {time}</p><p>IP: {ipAddress}</p></body></html>";
            }

            // Add computed values to data
            data["firstName"] = firstName;
            data["date"] = date;
            data["time"] = time;
            data["ipAddress"] = ipAddress ?? "Unknown";
            data["location"] = "Unknown"; // TODO: Implement IP geolocation
            data["device"] = GetValue(data, "Device") ?? "Unknown";

            // Render template with data
            var renderedTemplate = RenderTemplateWithHandlebars(template, data);

            // Replace AuthApp with ECommerce branding
            renderedTemplate = renderedTemplate.Replace("AuthApp", "ECommerce");

            return renderedTemplate;
        }

        private string RenderPasswordResetTemplate(Dictionary<string, object> data)
        {
            // Prepare data with computed values
            var firstName = GetValue(data, "FirstName");
            var resetToken = GetValue(data, "ResetToken");
            var email = GetValue(data, "Email");
            var ipAddress = GetValue(data, "IpAddress");
            var baseUrl = GetValue(data, "baseUrl");

            // Parse request time
            DateTime requestTime = DateTime.UtcNow;
            if (data.ContainsKey("RequestTime"))
            {
                if (data["RequestTime"] is DateTime dt)
                    requestTime = dt;
                else if (DateTime.TryParse(data["RequestTime"]?.ToString(), out DateTime parsed))
                    requestTime = parsed;
            }

            // Format date and time
            var date = requestTime.ToString("dd/MM/yyyy");
            var time = requestTime.ToString("HH:mm:ss");

            // Generate reset link to frontend
            var frontendUrl = _configuration.GetValue<string>("FrontendUrl") ?? "http://localhost:4400";
            var encodedToken = Uri.EscapeDataString(resetToken);
            var encodedEmail = Uri.EscapeDataString(email);
            var resetLink = $"{frontendUrl}/auth/reset-password?token={encodedToken}&email={encodedEmail}";

            // Try to load from file first
            var template = LoadTemplateFromFile("password-reset.html");

            if (template == null)
            {
                _logger.LogWarning("Could not load password-reset.html, using fallback template");
                return $@"<html><body><h1>Restablecer Contraseña</h1><p>Hola {firstName},</p><p>Por favor restablece tu contraseña haciendo clic en el siguiente enlace:</p><a href=""{resetLink}"">Restablecer Contraseña</a></body></html>";
            }

            // Add computed values to data
            data["firstName"] = firstName;
            data["resetLink"] = resetLink;
            data["date"] = date;
            data["time"] = time;
            data["ipAddress"] = ipAddress ?? "Unknown";
            data["location"] = "Unknown"; // TODO: Implement IP geolocation if needed

            // Render template with data
            var renderedTemplate = RenderTemplateWithHandlebars(template, data);

            // Replace AuthApp with ECommerce branding
            renderedTemplate = renderedTemplate.Replace("AuthApp", "ECommerce");

            return renderedTemplate;
        }

        private string RenderNewSessionAlertTemplate(Dictionary<string, object> data)
        {
            // Prepare data with computed values
            var firstName = GetValue(data, "FirstName");
            var date = GetValue(data, "Date");
            var time = GetValue(data, "Time");
            var device = GetValue(data, "Device");
            var browser = GetValue(data, "Browser");
            var location = GetValue(data, "Location");
            var ipAddress = GetValue(data, "IpAddress");
            var confirmLink = GetValue(data, "ConfirmLink");
            var secureLink = GetValue(data, "SecureLink");
            var baseUrl = GetValue(data, "baseUrl");

            // Try to load from file first
            var template = LoadTemplateFromFile("new-session-alert.html");

            if (template == null)
            {
                _logger.LogWarning("Could not load new-session-alert.html, using fallback template");
                return $@"<html><body><h1>Nueva Sesión Detectada</h1><p>Hola {firstName},</p><p>Se ha detectado un nuevo inicio de sesión en tu cuenta desde {device} ({browser}).</p><p>IP: {ipAddress}</p><p>Fecha: {date} {time}</p></body></html>";
            }

            // Add all data to dictionary
            data["firstName"] = firstName;
            data["date"] = date;
            data["time"] = time;
            data["device"] = device;
            data["browser"] = browser;
            data["location"] = location;
            data["ipAddress"] = ipAddress;
            data["confirmLink"] = confirmLink;
            data["secureLink"] = secureLink;

            // Render template with data
            var renderedTemplate = RenderTemplateWithHandlebars(template, data);

            // Replace AuthApp with ECommerce branding
            renderedTemplate = renderedTemplate.Replace("AuthApp", "ECommerce");

            return renderedTemplate;
        }

        private string Render2FAEnabledTemplate(Dictionary<string, object> data)
        {
            // Prepare data with computed values
            var firstName = GetValue(data, "FirstName");
            var baseUrl = GetValue(data, "baseUrl");

            // Try to load from file first
            var template = LoadTemplateFromFile("2fa-enabled.html");

            if (template == null)
            {
                _logger.LogWarning("Could not load 2fa-enabled.html, using fallback template");
                return $@"<html><body><h1>2FA Habilitada</h1><p>Hola {firstName},</p><p>La autenticación de dos factores ha sido habilitada exitosamente en tu cuenta.</p></body></html>";
            }

            // Add computed values to data
            data["firstName"] = firstName;

            // Render template with data
            var renderedTemplate = RenderTemplateWithHandlebars(template, data);

            // Replace AuthApp with ECommerce branding
            renderedTemplate = renderedTemplate.Replace("AuthApp", "ECommerce");

            return renderedTemplate;
        }

        private string Render2FABackupCodesTemplate(Dictionary<string, object> data)
        {
            // Prepare data with computed values
            var firstName = GetValue(data, "FirstName");
            var baseUrl = GetValue(data, "baseUrl");

            // Try to load from file first
            var template = LoadTemplateFromFile("2fa-backup-codes.html");

            if (template == null)
            {
                _logger.LogWarning("Could not load 2fa-backup-codes.html, using fallback template");
                return $@"<html><body><h1>Códigos de Respaldo 2FA</h1><p>Hola {firstName},</p><p>Aquí están tus códigos de respaldo para 2FA. Guárdalos en un lugar seguro.</p></body></html>";
            }

            // Add computed values to data
            data["firstName"] = firstName;

            // Render template with data
            var renderedTemplate = RenderTemplateWithHandlebars(template, data);

            // Replace AuthApp with ECommerce branding
            renderedTemplate = renderedTemplate.Replace("AuthApp", "ECommerce");

            return renderedTemplate;
        }

        private string Render2FADisabledTemplate(Dictionary<string, object> data)
        {
            // No template file exists, using inline HTML
            var firstName = GetValue(data, "FirstName");
            var baseUrl = GetValue(data, "baseUrl");
            var year = DateTime.Now.Year;

            return $@"<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>2FA Deshabilitada - ECommerce</title>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; padding: 40px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
        h1 {{ color: #232F3E; text-align: center; }}
        .icon {{ text-align: center; font-size: 64px; margin: 20px 0; }}
        .warning {{ background: #FFF3CD; border-left: 4px solid #FFC107; padding: 15px; margin: 20px 0; border-radius: 4px; }}
        .warning p {{ color: #856404; margin: 0; }}
        p {{ color: #555; line-height: 1.6; }}
        .footer {{ margin-top: 40px; text-align: center; color: #666; font-size: 13px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""icon"">⚠️</div>
        <h1>Autenticación de Dos Factores Deshabilitada</h1>
        <p>Hola {firstName},</p>
        <p>Te informamos que la autenticación de dos factores (2FA) ha sido deshabilitada en tu cuenta de ECommerce.</p>
        <div class=""warning"">
            <p><strong>⚠️ Advertencia de Seguridad:</strong> Tu cuenta ahora está menos protegida. Recomendamos habilitar 2FA nuevamente para mayor seguridad.</p>
        </div>
        <p>Si no realizaste este cambio, contacta inmediatamente con soporte.</p>
        <p style=""margin-top: 30px;"">Saludos,<br><strong>El equipo de ECommerce</strong></p>
        <div class=""footer"">
            <p>© {year} ECommerce, Inc. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string RenderPurchaseConfirmationTemplate(Dictionary<string, object> data)
        {
            // Prepare data with computed values
            var customerName = GetValue(data, "CustomerName");
            var orderNumber = GetValue(data, "OrderNumber");
            var subtotal = GetValue(data, "Subtotal");
            var shippingCost = GetValue(data, "ShippingCost");
            var tax = GetValue(data, "Tax");
            var total = GetValue(data, "Total");
            var estimatedDelivery = GetValue(data, "EstimatedDelivery");
            var baseUrl = GetValue(data, "baseUrl");

            // Generate URLs
            var frontendUrl = _configuration.GetValue<string>("FrontendUrl") ?? "http://localhost:4400";
            var trackOrderUrl = $"{frontendUrl}/orders/{orderNumber}/track";
            var receiptUrl = $"{frontendUrl}/orders/{orderNumber}/receipt";
            var myAccountUrl = $"{frontendUrl}/account";
            var myOrdersUrl = $"{frontendUrl}/orders";
            var supportUrl = $"{frontendUrl}/support";
            var returnsUrl = $"{frontendUrl}/returns";

            // Try to load from file first
            var template = LoadTemplateFromFile("purchase-confirmation.html");

            if (template == null)
            {
                _logger.LogWarning("Could not load purchase-confirmation.html, using fallback template");
                return $@"<html><body><h1>¡Compra Confirmada!</h1><p>Hola {customerName},</p><p>Tu pedido {orderNumber} ha sido confirmado.</p><p>Total: {total}</p></body></html>";
            }

            // Add computed values to data
            data["CustomerName"] = customerName;
            data["OrderNumber"] = orderNumber;
            data["Subtotal"] = subtotal;
            data["ShippingCost"] = shippingCost ?? "Gratis";
            data["Tax"] = tax;
            data["Total"] = total;
            data["EstimatedDelivery"] = estimatedDelivery ?? "3-5 días hábiles";
            data["TrackOrderUrl"] = trackOrderUrl;
            data["ReceiptUrl"] = receiptUrl;
            data["MyAccountUrl"] = myAccountUrl;
            data["MyOrdersUrl"] = myOrdersUrl;
            data["SupportUrl"] = supportUrl;
            data["ReturnsUrl"] = returnsUrl;

            // Render template with data
            var renderedTemplate = RenderTemplateWithHandlebars(template, data);

            return renderedTemplate;
        }

        private string RenderPaymentFailedTemplate(Dictionary<string, object> data)
        {
            // Prepare data with computed values
            var customerName = GetValue(data, "CustomerName");
            var orderNumber = GetValue(data, "OrderNumber");
            var attemptDate = GetValue(data, "AttemptDate");
            var amount = GetValue(data, "Amount");
            var paymentMethod = GetValue(data, "PaymentMethod");
            var failureReason = GetValue(data, "FailureReason");
            var baseUrl = GetValue(data, "baseUrl");

            // Generate URLs
            var frontendUrl = _configuration.GetValue<string>("FrontendUrl") ?? "http://localhost:4400";
            var retryPaymentUrl = $"{frontendUrl}/orders/{orderNumber}/payment";
            var viewOrderUrl = $"{frontendUrl}/orders/{orderNumber}";
            var myAccountUrl = $"{frontendUrl}/account";
            var myOrdersUrl = $"{frontendUrl}/orders";
            var supportUrl = $"{frontendUrl}/support";
            var helpCenterUrl = $"{frontendUrl}/help";

            // Try to load from file first
            var template = LoadTemplateFromFile("payment-failed.html");

            if (template == null)
            {
                _logger.LogWarning("Could not load payment-failed.html, using fallback template");
                return $@"<html><body><h1>Pago Rechazado</h1><p>Hola {customerName},</p><p>Tu pago para el pedido {orderNumber} fue rechazado.</p><p>Razón: {failureReason}</p><p><a href=""{retryPaymentUrl}"">Reintentar Pago</a></p></body></html>";
            }

            // Add computed values to data
            data["CustomerName"] = customerName;
            data["OrderNumber"] = orderNumber;
            data["AttemptDate"] = attemptDate ?? DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            data["Amount"] = amount;
            data["PaymentMethod"] = paymentMethod;
            data["FailureReason"] = failureReason ?? "No pudimos procesar el pago. Por favor intenta nuevamente o contacta con soporte.";
            data["RetryPaymentUrl"] = retryPaymentUrl;
            data["ViewOrderUrl"] = viewOrderUrl;
            data["MyAccountUrl"] = myAccountUrl;
            data["MyOrdersUrl"] = myOrdersUrl;
            data["SupportUrl"] = supportUrl;
            data["HelpCenterUrl"] = helpCenterUrl;

            // Render template with data
            var renderedTemplate = RenderTemplateWithHandlebars(template, data);

            return renderedTemplate;
        }

        private string RenderRefundProcessedTemplate(Dictionary<string, object> data)
        {
            // Prepare data with computed values
            var customerName = GetValue(data, "CustomerName");
            var refundNumber = GetValue(data, "RefundNumber");
            var refundReason = GetValue(data, "RefundReason");
            var processedDate = GetValue(data, "ProcessedDate");
            var refundMethod = GetValue(data, "RefundMethod");
            var refundAmount = GetValue(data, "RefundAmount");
            var orderNumber = GetValue(data, "OrderNumber");
            var purchaseDate = GetValue(data, "PurchaseDate");
            var baseUrl = GetValue(data, "baseUrl");

            // Generate URLs
            var frontendUrl = _configuration.GetValue<string>("FrontendUrl") ?? "http://localhost:4400";
            var viewDetailsUrl = $"{frontendUrl}/refunds/{refundNumber}";
            var myAccountUrl = $"{frontendUrl}/account";
            var myReturnsUrl = $"{frontendUrl}/returns";
            var supportUrl = $"{frontendUrl}/support";
            var faqUrl = $"{frontendUrl}/faq";

            // Try to load from file first
            var template = LoadTemplateFromFile("refund-processed.html");

            if (template == null)
            {
                _logger.LogWarning("Could not load refund-processed.html, using fallback template");
                return $@"<html><body><h1>Reembolso Procesado</h1><p>Hola {customerName},</p><p>Tu reembolso {refundNumber} ha sido procesado exitosamente.</p><p>Monto: {refundAmount}</p></body></html>";
            }

            // Add computed values to data
            data["CustomerName"] = customerName;
            data["RefundNumber"] = refundNumber;
            data["RefundReason"] = refundReason ?? "Producto Defectuoso";
            data["ProcessedDate"] = processedDate ?? DateTime.Now.ToString("dd/MM/yyyy - HH:mm:ss");
            data["RefundMethod"] = refundMethod;
            data["RefundAmount"] = refundAmount;
            data["OrderNumber"] = orderNumber;
            data["PurchaseDate"] = purchaseDate;
            data["ReturnReceivedDate"] = GetValue(data, "ReturnReceivedDate") ?? DateTime.Now.AddDays(-2).ToString("dd/MM/yyyy");
            data["ProductVerifiedDate"] = GetValue(data, "ProductVerifiedDate") ?? DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy");
            data["RefundProcessedDate"] = GetValue(data, "RefundProcessedDate") ?? DateTime.Now.ToString("dd/MM/yyyy");
            data["ViewDetailsUrl"] = viewDetailsUrl;
            data["SupportUrl"] = supportUrl;
            data["MyAccountUrl"] = myAccountUrl;
            data["MyReturnsUrl"] = myReturnsUrl;
            data["FaqUrl"] = faqUrl;

            // Render template with data
            var renderedTemplate = RenderTemplateWithHandlebars(template, data);

            return renderedTemplate;
        }

        private string GenerateVerificationLink(Dictionary<string, object> data)
        {
            var confirmationToken = GetValue(data, "ConfirmationToken");
            var userId = GetValue(data, "UserId");
            var apiGatewayUrl = _configuration.GetValue<string>("ApiGatewayUrl") ?? "http://localhost:10000";
            var encodedToken = Uri.EscapeDataString(confirmationToken);
            var encodedUserId = Uri.EscapeDataString(userId);
            return $"{apiGatewayUrl}/v1/identity/confirm-email?userId={encodedUserId}&token={encodedToken}";
        }

        private string RenderWelcomeEmailTemplate(Dictionary<string, object> data)
        {
            var customerName = GetValue(data, "CustomerName");
            var email = GetValue(data, "Email");
            var registeredAt = GetValue(data, "RegisteredAt");
            var year = DateTime.Now.Year;

            // Try to load from file first
            var template = LoadTemplateFromFile("welcome-email.html");

            if (template != null)
            {
                data["customerName"] = customerName;
                data["email"] = email;
                data["registeredAt"] = registeredAt;
                return RenderTemplateWithHandlebars(template, data);
            }

            // Fallback template
            return $@"<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Bienvenido - ECommerce</title>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; padding: 40px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
        h1 {{ color: #232F3E; text-align: center; }}
        .icon {{ text-align: center; font-size: 64px; margin: 20px 0; }}
        p {{ color: #555; line-height: 1.6; }}
        .btn {{ display: inline-block; background: #FF9900; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; margin-top: 20px; }}
        .footer {{ margin-top: 40px; text-align: center; color: #666; font-size: 13px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""icon"">🎉</div>
        <h1>¡Bienvenido a ECommerce!</h1>
        <p>Hola <strong>{customerName}</strong>,</p>
        <p>¡Gracias por registrarte en ECommerce! Estamos emocionados de tenerte como parte de nuestra comunidad.</p>
        <p>Ahora puedes:</p>
        <ul>
            <li>Explorar miles de productos</li>
            <li>Guardar tus favoritos</li>
            <li>Recibir ofertas exclusivas</li>
            <li>Realizar compras seguras</li>
        </ul>
        <p style=""text-align: center;"">
            <a href=""https://ecommerce.com"" class=""btn"">Comenzar a Comprar</a>
        </p>
        <p style=""margin-top: 30px;"">Saludos,<br><strong>El equipo de ECommerce</strong></p>
        <div class=""footer"">
            <p>© {year} ECommerce, Inc. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string RenderCartAbandonedTemplate(Dictionary<string, object> data)
        {
            var customerName = GetValue(data, "CustomerName");
            var cartTotal = GetValue(data, "CartTotal");
            var itemCount = GetValue(data, "ItemCount");
            var itemsList = GetValue(data, "ItemsList");
            var cartUrl = GetValue(data, "CartUrl");
            var year = DateTime.Now.Year;

            // Try to load from file first
            var template = LoadTemplateFromFile("cart-abandoned.html");

            if (template != null)
            {
                data["customerName"] = customerName;
                data["cartTotal"] = cartTotal;
                data["itemCount"] = itemCount;
                data["itemsList"] = itemsList;
                data["cartUrl"] = cartUrl;
                return RenderTemplateWithHandlebars(template, data);
            }

            // Fallback template
            return $@"<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Tu carrito te espera - ECommerce</title>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; padding: 40px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
        h1 {{ color: #232F3E; text-align: center; }}
        .icon {{ text-align: center; font-size: 64px; margin: 20px 0; }}
        p {{ color: #555; line-height: 1.6; }}
        .btn {{ display: inline-block; background: #FF9900; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; margin-top: 20px; }}
        .cart-summary {{ background: #f9f9f9; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .footer {{ margin-top: 40px; text-align: center; color: #666; font-size: 13px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""icon"">🛒</div>
        <h1>¡Tu carrito te espera!</h1>
        <p>Hola <strong>{customerName}</strong>,</p>
        <p>Notamos que dejaste algunos productos en tu carrito. ¡No te preocupes, los guardamos para ti!</p>
        <div class=""cart-summary"">
            <p><strong>Productos en tu carrito ({itemCount}):</strong></p>
            <p>{itemsList}</p>
            <p><strong>Total:</strong> {cartTotal}</p>
        </div>
        <p style=""text-align: center;"">
            <a href=""{cartUrl}"" class=""btn"">Completar mi Compra</a>
        </p>
        <p style=""margin-top: 30px;"">Saludos,<br><strong>El equipo de ECommerce</strong></p>
        <div class=""footer"">
            <p>© {year} ECommerce, Inc. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string RenderOrderCancelledTemplate(Dictionary<string, object> data)
        {
            var customerName = GetValue(data, "CustomerName");
            var orderNumber = GetValue(data, "OrderNumber");
            var amount = GetValue(data, "Amount");
            var cancellationReason = GetValue(data, "CancellationReason");
            var cancelledAt = GetValue(data, "CancelledAt");
            var year = DateTime.Now.Year;

            // Try to load from file first
            var template = LoadTemplateFromFile("order-cancelled.html");

            if (template != null)
            {
                data["customerName"] = customerName;
                data["orderNumber"] = orderNumber;
                data["amount"] = amount;
                data["cancellationReason"] = cancellationReason;
                data["cancelledAt"] = cancelledAt;
                return RenderTemplateWithHandlebars(template, data);
            }

            // Fallback template
            return $@"<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Pedido Cancelado - ECommerce</title>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; padding: 40px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
        h1 {{ color: #232F3E; text-align: center; }}
        .icon {{ text-align: center; font-size: 64px; margin: 20px 0; }}
        p {{ color: #555; line-height: 1.6; }}
        .order-details {{ background: #f9f9f9; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .btn {{ display: inline-block; background: #FF9900; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; margin-top: 20px; }}
        .footer {{ margin-top: 40px; text-align: center; color: #666; font-size: 13px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""icon"">❌</div>
        <h1>Pedido Cancelado</h1>
        <p>Hola <strong>{customerName}</strong>,</p>
        <p>Te confirmamos que tu pedido ha sido cancelado.</p>
        <div class=""order-details"">
            <p><strong>Número de Pedido:</strong> {orderNumber}</p>
            <p><strong>Monto:</strong> {amount}</p>
            <p><strong>Motivo:</strong> {cancellationReason}</p>
            <p><strong>Fecha de Cancelación:</strong> {cancelledAt}</p>
        </div>
        <p>Si se realizó algún cargo, el reembolso será procesado en los próximos 5-10 días hábiles.</p>
        <p style=""text-align: center;"">
            <a href=""https://ecommerce.com"" class=""btn"">Seguir Comprando</a>
        </p>
        <p style=""margin-top: 30px;"">Saludos,<br><strong>El equipo de ECommerce</strong></p>
        <div class=""footer"">
            <p>© {year} ECommerce, Inc. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string RenderOrderShippedTemplate(Dictionary<string, object> data)
        {
            var customerName = GetValue(data, "CustomerName");
            var orderNumber = GetValue(data, "OrderNumber");
            var trackingNumber = GetValue(data, "TrackingNumber");
            var carrier = GetValue(data, "Carrier");
            var trackingUrl = GetValue(data, "TrackingUrl");
            var shippedAt = GetValue(data, "ShippedAt");
            var estimatedDelivery = GetValue(data, "EstimatedDelivery");
            var shippingAddress = GetValue(data, "ShippingAddress");
            var year = DateTime.Now.Year;

            // Try to load from file first
            var template = LoadTemplateFromFile("order-shipped.html");

            if (template != null)
            {
                data["customerName"] = customerName;
                data["orderNumber"] = orderNumber;
                data["trackingNumber"] = trackingNumber;
                data["carrier"] = carrier;
                data["trackingUrl"] = trackingUrl;
                data["shippedAt"] = shippedAt;
                data["estimatedDelivery"] = estimatedDelivery;
                data["shippingAddress"] = shippingAddress;
                return RenderTemplateWithHandlebars(template, data);
            }

            // Fallback template
            return $@"<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Tu pedido está en camino - ECommerce</title>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; padding: 40px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
        h1 {{ color: #232F3E; text-align: center; }}
        .icon {{ text-align: center; font-size: 64px; margin: 20px 0; }}
        p {{ color: #555; line-height: 1.6; }}
        .tracking-box {{ background: #e8f5e9; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #4CAF50; }}
        .shipping-details {{ background: #f9f9f9; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .btn {{ display: inline-block; background: #FF9900; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; margin-top: 20px; }}
        .footer {{ margin-top: 40px; text-align: center; color: #666; font-size: 13px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""icon"">📦</div>
        <h1>¡Tu pedido está en camino!</h1>
        <p>Hola <strong>{customerName}</strong>,</p>
        <p>¡Buenas noticias! Tu pedido ha sido enviado y está en camino.</p>
        <div class=""tracking-box"">
            <p><strong>Número de Seguimiento:</strong> {trackingNumber}</p>
            <p><strong>Transportista:</strong> {carrier}</p>
            <p style=""text-align: center; margin-top: 15px;"">
                <a href=""{trackingUrl}"" class=""btn"">Rastrear Envío</a>
            </p>
        </div>
        <div class=""shipping-details"">
            <p><strong>Pedido:</strong> {orderNumber}</p>
            <p><strong>Fecha de Envío:</strong> {shippedAt}</p>
            <p><strong>Entrega Estimada:</strong> {estimatedDelivery}</p>
            <p><strong>Dirección de Entrega:</strong> {shippingAddress}</p>
        </div>
        <p style=""margin-top: 30px;"">Saludos,<br><strong>El equipo de ECommerce</strong></p>
        <div class=""footer"">
            <p>© {year} ECommerce, Inc. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string RenderOrderDeliveredTemplate(Dictionary<string, object> data)
        {
            var customerName = GetValue(data, "CustomerName");
            var orderNumber = GetValue(data, "OrderNumber");
            var deliveredAt = GetValue(data, "DeliveredAt");
            var receivedBy = GetValue(data, "ReceivedBy");
            var reviewUrl = GetValue(data, "ReviewUrl");
            var year = DateTime.Now.Year;

            // Try to load from file first
            var template = LoadTemplateFromFile("order-delivered.html");

            if (template != null)
            {
                data["customerName"] = customerName;
                data["orderNumber"] = orderNumber;
                data["deliveredAt"] = deliveredAt;
                data["receivedBy"] = receivedBy;
                data["reviewUrl"] = reviewUrl;
                return RenderTemplateWithHandlebars(template, data);
            }

            // Fallback template
            return $@"<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Pedido Entregado - ECommerce</title>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; padding: 40px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
        h1 {{ color: #232F3E; text-align: center; }}
        .icon {{ text-align: center; font-size: 64px; margin: 20px 0; }}
        p {{ color: #555; line-height: 1.6; }}
        .success-box {{ background: #e8f5e9; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #4CAF50; }}
        .btn {{ display: inline-block; background: #FF9900; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; margin-top: 20px; }}
        .footer {{ margin-top: 40px; text-align: center; color: #666; font-size: 13px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""icon"">✅</div>
        <h1>¡Pedido Entregado!</h1>
        <p>Hola <strong>{customerName}</strong>,</p>
        <p>Tu pedido ha sido entregado exitosamente.</p>
        <div class=""success-box"">
            <p><strong>Pedido:</strong> {orderNumber}</p>
            <p><strong>Entregado el:</strong> {deliveredAt}</p>
            <p><strong>Recibido por:</strong> {receivedBy}</p>
        </div>
        <p>¡Esperamos que disfrutes tu compra! Nos encantaría conocer tu opinión.</p>
        <p style=""text-align: center;"">
            <a href=""{reviewUrl}"" class=""btn"">Dejar una Reseña</a>
        </p>
        <p style=""margin-top: 30px;"">Saludos,<br><strong>El equipo de ECommerce</strong></p>
        <div class=""footer"">
            <p>© {year} ECommerce, Inc. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string RenderReviewRequestTemplate(Dictionary<string, object> data)
        {
            var customerName = GetValue(data, "CustomerName");
            var orderNumber = GetValue(data, "OrderNumber");
            var year = DateTime.Now.Year;

            // Try to load from file first
            var template = LoadTemplateFromFile("review-request.html");

            if (template != null)
            {
                data["customerName"] = customerName;
                data["orderNumber"] = orderNumber;
                return RenderTemplateWithHandlebars(template, data);
            }

            // Fallback template
            return $@"<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>¿Qué te pareció tu compra? - ECommerce</title>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; padding: 40px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
        h1 {{ color: #232F3E; text-align: center; }}
        .icon {{ text-align: center; font-size: 64px; margin: 20px 0; }}
        p {{ color: #555; line-height: 1.6; }}
        .stars {{ text-align: center; font-size: 32px; margin: 20px 0; }}
        .btn {{ display: inline-block; background: #FF9900; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; margin-top: 20px; }}
        .footer {{ margin-top: 40px; text-align: center; color: #666; font-size: 13px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""icon"">⭐</div>
        <h1>¿Qué te pareció tu compra?</h1>
        <p>Hola <strong>{customerName}</strong>,</p>
        <p>Esperamos que estés disfrutando de los productos de tu pedido <strong>{orderNumber}</strong>.</p>
        <p>Tu opinión es muy importante para nosotros y para otros compradores. ¿Podrías tomarte un momento para dejarnos una reseña?</p>
        <div class=""stars"">⭐⭐⭐⭐⭐</div>
        <p style=""text-align: center;"">
            <a href=""https://ecommerce.com/orders/{orderNumber}/review"" class=""btn"">Escribir Reseña</a>
        </p>
        <p style=""margin-top: 30px;"">¡Gracias por tu tiempo!<br><strong>El equipo de ECommerce</strong></p>
        <div class=""footer"">
            <p>© {year} ECommerce, Inc. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string RenderDefaultTemplate(Dictionary<string, object> data)
        {
            var firstName = GetValue(data, "FirstName") ?? "Usuario";
            var year = DateTime.Now.Year;

            return $@"<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Notificación - ECommerce</title>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; padding: 40px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
        h1 {{ color: #232F3E; text-align: center; }}
        p {{ color: #555; line-height: 1.6; }}
        .footer {{ margin-top: 40px; text-align: center; color: #666; font-size: 13px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <h1>Notificación de ECommerce</h1>
        <p>Hola {firstName},</p>
        <p>Has recibido una notificación de tu cuenta de ECommerce.</p>
        <p style=""margin-top: 30px;"">Saludos,<br><strong>El equipo de ECommerce</strong></p>
        <div class=""footer"">
            <p>© {year} ECommerce, Inc. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
