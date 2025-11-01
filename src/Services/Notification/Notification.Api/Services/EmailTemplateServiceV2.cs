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
                        subject = "Verifica tu Correo Electrónico - ECommerce";
                        htmlBody = RenderEmailVerificationTemplate(dataDict);
                        textBody = $"Hola {GetValue(dataDict, "FirstName")}, verifica tu email con el token: {GetValue(dataDict, "ConfirmationToken")}";
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

            // Find all {{variable}} patterns in the template
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

            // Handle {{#each}} blocks for arrays (simplified version)
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
                        blockData["this"] = item.ToString();

                        var renderedBlock = RenderTemplateWithHandlebars(blockTemplate, blockData);
                        renderedBlocks.Append(renderedBlock);
                        index++;
                    }

                    result = result.Replace(match.Value, renderedBlocks.ToString());
                }
            }

            return result;
        }

        // Los templates HTML completos se incluyen aquí con lógica de reemplazo de variables
        private string RenderEmailVerificationTemplate(Dictionary<string, object> data)
        {
            // Prepare data with computed values
            var firstName = GetValue(data, "FirstName");
            var confirmationToken = GetValue(data, "ConfirmationToken");
            var userId = GetValue(data, "UserId");
            var baseUrl = GetValue(data, "baseUrl");

            // Generar enlace al API Gateway endpoint con URL encoding
            var apiGatewayUrl = _configuration.GetValue<string>("ApiGatewayUrl") ?? "http://localhost:10000";
            var encodedToken = Uri.EscapeDataString(confirmationToken);
            var encodedUserId = Uri.EscapeDataString(userId);
            var verificationLink = $"{apiGatewayUrl}/v1/identity/confirm-email?userId={encodedUserId}&token={encodedToken}";

            // Try to load from file first
            var template = LoadTemplateFromFile("email-confirmation.html");

            if (template == null)
            {
                _logger.LogWarning("Could not load email-confirmation.html, using fallback template");
                // Fallback to inline template
                return $@"<html><body><h1>Verifica tu correo</h1><p>Hola {firstName},</p><p>Por favor verifica tu correo haciendo clic en el siguiente enlace:</p><a href=""{verificationLink}"">Verificar Email</a></body></html>";
            }

            // Add computed values to data
            data["verificationLink"] = verificationLink;

            // Render template with data
            var renderedTemplate = RenderTemplateWithHandlebars(template, data);

            // Replace AuthApp with ECommerce branding
            renderedTemplate = renderedTemplate.Replace("AuthApp", "ECommerce");

            return renderedTemplate;
        }

        private string RenderPasswordChangedTemplate(Dictionary<string, object> data)
        {
            // Try to load from file first
            var template = LoadTemplateFromFile("password-changed.html");

            if (template == null)
            {
                _logger.LogWarning("Could not load password-changed.html, using fallback template");
                var firstName = GetValue(data, "FirstName");
                return $@"<html><body><h1>Contraseña Cambiada</h1><p>Hola {firstName},</p><p>Tu contraseña ha sido cambiada exitosamente.</p></body></html>";
            }

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

        // Placeholder para los otros templates...
        private string Render2FAEnabledTemplate(Dictionary<string, object> data) => RenderEmailVerificationTemplate(data);
        private string Render2FABackupCodesTemplate(Dictionary<string, object> data) => RenderEmailVerificationTemplate(data);
        private string Render2FADisabledTemplate(Dictionary<string, object> data) => RenderEmailVerificationTemplate(data);
        private string RenderDefaultTemplate(Dictionary<string, object> data) => RenderEmailVerificationTemplate(data);
    }
}
