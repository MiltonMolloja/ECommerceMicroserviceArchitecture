using System.Text.Json;
using System.Text.Json.Serialization;

namespace Service.Common.Mapping
{
    public static class DtoMapperExtension
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static T MapTo<T>(this object value)
        {
            return JsonSerializer.Deserialize<T>(
                JsonSerializer.Serialize(value, _options),
                _options
            );
        }
    }
}
