using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Api.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly ILogger<EmailTemplateService> _logger;

        public EmailTemplateService(ILogger<EmailTemplateService> logger)
        {
            _logger = logger;
        }

        public Task<(string subject, string htmlBody, string textBody)> RenderTemplateAsync(string templateName, object data)
        {
            try
            {
                // Convertir data a dictionary para fácil acceso
                Dictionary<string, object> dataDict;

                // Si ya es un diccionario, usarlo directamente
                if (data is Dictionary<string, object> dict)
                {
                    dataDict = dict;
                }
                else if (data is System.Text.Json.JsonElement jsonElement)
                {
                    // Manejar JsonElement de System.Text.Json
                    dataDict = new Dictionary<string, object>();
                    foreach (var prop in jsonElement.EnumerateObject())
                    {
                        dataDict[prop.Name] = prop.Value.ToString();
                    }
                }
                else
                {
                    // Si es un objeto normal, usar reflexión
                    var properties = data.GetType().GetProperties();
                    dataDict = new Dictionary<string, object>();
                    foreach (var prop in properties)
                    {
                        dataDict[prop.Name] = prop.GetValue(data);
                    }
                }

                string subject, htmlBody, textBody;

                switch (templateName.ToLower())
                {
                    case "email-confirmation":
                        subject = "Confirma tu email - ECommerce";
                        htmlBody = RenderEmailConfirmationTemplate(dataDict);
                        textBody = $"Hola {GetValue(dataDict, "FirstName")}, por favor confirma tu email usando el siguiente token: {GetValue(dataDict, "ConfirmationToken")}";
                        break;

                    case "password-reset":
                        subject = "Recuperación de contraseña - ECommerce";
                        htmlBody = RenderPasswordResetTemplate(dataDict);
                        textBody = $"Hola {GetValue(dataDict, "FirstName")}, usa este token para recuperar tu contraseña: {GetValue(dataDict, "ResetToken")}";
                        break;

                    case "password-reset-confirmation":
                        subject = "Tu contraseña ha sido cambiada - ECommerce";
                        htmlBody = RenderPasswordResetConfirmationTemplate(dataDict);
                        textBody = $"Hola {GetValue(dataDict, "FirstName")}, tu contraseña ha sido cambiada exitosamente.";
                        break;

                    case "password-changed":
                        subject = "Contraseña actualizada - ECommerce";
                        htmlBody = RenderPasswordChangedTemplate(dataDict);
                        textBody = $"Hola {GetValue(dataDict, "FirstName")}, tu contraseña ha sido actualizada exitosamente.";
                        break;

                    case "2fa-enabled":
                        subject = "Autenticación de dos factores activada - ECommerce";
                        htmlBody = Render2FAEnabledTemplate(dataDict);
                        textBody = $"Hola {GetValue(dataDict, "FirstName")}, la autenticación de dos factores ha sido activada en tu cuenta.";
                        break;

                    case "2fa-disabled":
                        subject = "Autenticación de dos factores desactivada - ECommerce";
                        htmlBody = Render2FADisabledTemplate(dataDict);
                        textBody = $"Hola {GetValue(dataDict, "FirstName")}, la autenticación de dos factores ha sido desactivada en tu cuenta.";
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

        private string GetValue(Dictionary<string, object> data, string key)
        {
            return data.ContainsKey(key) ? data[key]?.ToString() : "";
        }

        private string RenderEmailConfirmationTemplate(Dictionary<string, object> data)
        {
            var firstName = GetValue(data, "FirstName");
            var confirmationToken = GetValue(data, "ConfirmationToken");
            var userId = GetValue(data, "UserId");
            var expirationHours = GetValue(data, "ExpirationHours");

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 4px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #777; }}
        .token {{ background-color: #e0e0e0; padding: 10px; border-radius: 4px; font-family: monospace; word-break: break-all; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>¡Bienvenido a ECommerce!</h1>
        </div>
        <div class=""content"">
            <h2>Hola {firstName},</h2>
            <p>Gracias por registrarte en nuestra plataforma. Para completar tu registro, necesitamos que confirmes tu dirección de email.</p>
            <p>Por favor, usa el siguiente token de confirmación:</p>
            <div class=""token"">{confirmationToken}</div>
            <p><strong>UserId:</strong> {userId}</p>
            <p>Este token expirará en {expirationHours} horas.</p>
            <p>Si no creaste esta cuenta, puedes ignorar este email.</p>
        </div>
        <div class=""footer"">
            <p>© 2025 ECommerce. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string RenderPasswordResetTemplate(Dictionary<string, object> data)
        {
            var firstName = GetValue(data, "FirstName");
            var resetToken = GetValue(data, "ResetToken");
            var email = GetValue(data, "Email");
            var ipAddress = GetValue(data, "IpAddress");
            var expirationMinutes = GetValue(data, "ExpirationMinutes");

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #FF9800; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #FF9800; color: white; text-decoration: none; border-radius: 4px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #777; }}
        .token {{ background-color: #e0e0e0; padding: 10px; border-radius: 4px; font-family: monospace; word-break: break-all; }}
        .warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 10px; margin: 10px 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Recuperación de Contraseña</h1>
        </div>
        <div class=""content"">
            <h2>Hola {firstName},</h2>
            <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta.</p>
            <p>Usa el siguiente token para restablecer tu contraseña:</p>
            <div class=""token"">{resetToken}</div>
            <p><strong>Email:</strong> {email}</p>
            <p>Este token expirará en {expirationMinutes} minutos.</p>
            <div class=""warning"">
                <strong>Información de seguridad:</strong><br>
                Solicitud desde la IP: {ipAddress}
            </div>
            <p>Si no solicitaste este cambio, ignora este email y tu contraseña permanecerá sin cambios.</p>
        </div>
        <div class=""footer"">
            <p>© 2025 ECommerce. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string RenderPasswordResetConfirmationTemplate(Dictionary<string, object> data)
        {
            var firstName = GetValue(data, "FirstName");

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #777; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>✓ Contraseña Restablecida</h1>
        </div>
        <div class=""content"">
            <h2>Hola {firstName},</h2>
            <p>Tu contraseña ha sido restablecida exitosamente.</p>
            <p>Ahora puedes iniciar sesión con tu nueva contraseña.</p>
            <p>Si no realizaste este cambio, contacta inmediatamente con soporte.</p>
        </div>
        <div class=""footer"">
            <p>© 2025 ECommerce. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string RenderPasswordChangedTemplate(Dictionary<string, object> data)
        {
            var firstName = GetValue(data, "FirstName");

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #777; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Contraseña Actualizada</h1>
        </div>
        <div class=""content"">
            <h2>Hola {firstName},</h2>
            <p>Tu contraseña ha sido actualizada exitosamente.</p>
            <p>Todas tus sesiones activas han sido cerradas por seguridad.</p>
            <p>Si no realizaste este cambio, contacta inmediatamente con soporte.</p>
        </div>
        <div class=""footer"">
            <p>© 2025 ECommerce. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string Render2FAEnabledTemplate(Dictionary<string, object> data)
        {
            var firstName = GetValue(data, "FirstName");

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #777; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🔒 Autenticación de Dos Factores Activada</h1>
        </div>
        <div class=""content"">
            <h2>Hola {firstName},</h2>
            <p>La autenticación de dos factores ha sido activada en tu cuenta.</p>
            <p>Tu cuenta ahora está más segura. Necesitarás tu dispositivo de autenticación cada vez que inicies sesión.</p>
            <p>Guarda tus códigos de respaldo en un lugar seguro.</p>
        </div>
        <div class=""footer"">
            <p>© 2025 ECommerce. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string Render2FADisabledTemplate(Dictionary<string, object> data)
        {
            var firstName = GetValue(data, "FirstName");

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #FF9800; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #777; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Autenticación de Dos Factores Desactivada</h1>
        </div>
        <div class=""content"">
            <h2>Hola {firstName},</h2>
            <p>La autenticación de dos factores ha sido desactivada en tu cuenta.</p>
            <p>Tu cuenta ya no requiere verificación de dos pasos.</p>
            <p>Si no realizaste este cambio, activa 2FA inmediatamente y contacta con soporte.</p>
        </div>
        <div class=""footer"">
            <p>© 2025 ECommerce. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string RenderDefaultTemplate(Dictionary<string, object> data)
        {
            var sb = new StringBuilder();
            sb.Append(@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background-color: #2196F3; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background-color: #f9f9f9; }
        .footer { text-align: center; padding: 20px; font-size: 12px; color: #777; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Notificación de ECommerce</h1>
        </div>
        <div class=""content"">");

            foreach (var kvp in data)
            {
                sb.Append($"<p><strong>{kvp.Key}:</strong> {kvp.Value}</p>");
            }

            sb.Append(@"
        </div>
        <div class=""footer"">
            <p>© 2025 ECommerce. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>");

            return sb.ToString();
        }
    }
}
