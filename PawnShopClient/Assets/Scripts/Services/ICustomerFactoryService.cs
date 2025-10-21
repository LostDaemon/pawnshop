using PawnShop.Models.Characters;
using PawnShop.Models.Npc;

namespace PawnShop.Services
{
    public interface ICustomerFactoryService
    {
        Customer GenerateCustomer(NpcType customerType);
    }
}