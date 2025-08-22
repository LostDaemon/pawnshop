using PawnShop.Models.Characters;

namespace PawnShop.Services
{
    public interface IPlayerService
    {
        Player Player { get; }
        void InitializePlayer();
    }
}
