namespace Common.CorrelationId
{
    /// <summary>
    /// Interface para acceder al Correlation ID desde cualquier parte de la aplicación
    /// </summary>
    public interface ICorrelationIdAccessor
    {
        /// <summary>
        /// Obtiene el Correlation ID del request actual
        /// </summary>
        string? GetCorrelationId();
    }
}
