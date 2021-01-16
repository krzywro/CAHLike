using System;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Shared.Contracts.ClientMessages.Game
{
    public interface ILobbyJoin : IClientMessage<Guid>
    {
        Task Join(Guid playerId);
    }
}
