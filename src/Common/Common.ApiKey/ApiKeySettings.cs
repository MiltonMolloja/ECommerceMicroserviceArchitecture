namespace Common.ApiKey
{
    public class ApiKeySettings
    {
        public bool EnableApiKeyValidation { get; set; } = true;
        public string ApiKey { get; set; } = string.Empty;
        public List<string> ValidApiKeys { get; set; } = new();
    }
}
