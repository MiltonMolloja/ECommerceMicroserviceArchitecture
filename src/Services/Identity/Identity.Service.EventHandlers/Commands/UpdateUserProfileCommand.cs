using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Identity.Service.EventHandlers.Commands
{
    public class UpdateUserProfileCommand : IRequest<bool>
    {
        /// <summary>
        /// UserId obtenido del JWT token - No se envía en el body
        /// </summary>
        [JsonIgnore]
        public string UserId { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }
    }
}
