namespace Common.RateLimiting
{
    public class RateLimitingSettings
    {
        public bool EnableRateLimiting { get; set; } = true;

        // Configuración para endpoints de autenticación (login, register)
        public AuthenticationRateLimitSettings Authentication { get; set; } = new();

        // Configuración para endpoints generales de API
        public GeneralRateLimitSettings General { get; set; } = new();

        // Configuración para endpoints de lectura (GET)
        public ReadRateLimitSettings Read { get; set; } = new();

        // Configuración para endpoints de escritura (POST, PUT, DELETE)
        public WriteRateLimitSettings Write { get; set; } = new();
    }

    public class AuthenticationRateLimitSettings
    {
        public int PermitLimit { get; set; } = 5;  // 5 intentos
        public int WindowInSeconds { get; set; } = 60;  // por minuto
    }

    public class GeneralRateLimitSettings
    {
        public int PermitLimit { get; set; } = 100;  // 100 requests
        public int WindowInSeconds { get; set; } = 60;  // por minuto
    }

    public class ReadRateLimitSettings
    {
        public int PermitLimit { get; set; } = 200;  // 200 requests
        public int WindowInSeconds { get; set; } = 60;  // por minuto
    }

    public class WriteRateLimitSettings
    {
        public int PermitLimit { get; set; } = 50;  // 50 requests
        public int WindowInSeconds { get; set; } = 60;  // por minuto
    }
}
