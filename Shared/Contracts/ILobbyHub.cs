using KrzyWro.CAH.Shared.Contracts.ClientMessages.Game;

namespace KrzyWro.CAH.Shared.Contracts
{
    public interface ILobbyHub :
        ILobbyJoin,
        IRequestGameList,
        IRequestGameCreation
    {
        public static readonly string Path = "/lobbyhub";
    }
}
