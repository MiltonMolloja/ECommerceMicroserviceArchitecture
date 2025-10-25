using System;

namespace Notification.Domain
{
    public class NotificationTemplate
    {
        #region Properties - Persistidas

        public int TemplateId { get; set; }
        public NotificationType Type { get; set; }
        public string TemplateKey { get; set; } // order_placed, order_shipped, etc.

        public string TitleTemplate { get; set; } // "Your order #{{orderNumber}} has been placed!"
        public string MessageTemplate { get; set; } // "Thank you {{customerName}}, your order..."

        public NotificationChannel Channel { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        #endregion

        #region Business Methods

        /// <summary>
        /// Renderiza el título del template con las variables proporcionadas
        /// Usa sintaxis Mustache: {{variable}}
        /// </summary>
        public string RenderTitle(System.Collections.Generic.Dictionary<string, object> variables)
        {
            if (string.IsNullOrEmpty(TitleTemplate))
                return string.Empty;

            var result = TitleTemplate;
            foreach (var variable in variables)
            {
                var placeholder = $"{{{{{variable.Key}}}}}";
                result = result.Replace(placeholder, variable.Value?.ToString() ?? string.Empty);
            }
            return result;
        }

        /// <summary>
        /// Renderiza el mensaje del template con las variables proporcionadas
        /// Usa sintaxis Mustache: {{variable}}
        /// </summary>
        public string RenderMessage(System.Collections.Generic.Dictionary<string, object> variables)
        {
            if (string.IsNullOrEmpty(MessageTemplate))
                return string.Empty;

            var result = MessageTemplate;
            foreach (var variable in variables)
            {
                var placeholder = $"{{{{{variable.Key}}}}}";
                result = result.Replace(placeholder, variable.Value?.ToString() ?? string.Empty);
            }
            return result;
        }

        /// <summary>
        /// Activa el template
        /// </summary>
        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Desactiva el template
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}
