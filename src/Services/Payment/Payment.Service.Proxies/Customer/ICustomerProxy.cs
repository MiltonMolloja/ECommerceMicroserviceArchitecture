using System.Threading.Tasks;

namespace Payment.Service.Proxies.Customer
{
    public interface ICustomerProxy
    {
        Task<CustomerDto> GetCustomerByIdAsync(int customerId);
    }

    public class CustomerDto
    {
        public int ClientId { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
