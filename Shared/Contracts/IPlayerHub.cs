using KrzyWro.CAH.Shared.Contracts.ClientMessages.Player;

namespace KrzyWro.CAH.Shared.Contracts
{
    public interface IPlayerHub :
        IPlayerRegister
    {
        public static readonly string Path = "/playerhub";
    }
}
