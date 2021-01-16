using System;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Shared.Contracts.ClientMessages.Player
{
    public interface IPlayerRegister : IClientMessage<Guid, string>
    {
        Task RegisterPlayer(Guid playerId, string playerName);
    }
}
