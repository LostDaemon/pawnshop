using PawnShop.Models.Characters;

namespace PawnShop.Services
{
    public interface ICustomerFactoryService
    {
        Customer GenerateRandomCustomer();
    }
}