using System;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Shared.Contracts.ClientMessages
{
    public interface IPlayerHubRegisterPlayer : IClientMessage<Guid, string>
    {
        Task RegisterPlayer(Guid playerId, string playerName);
    }
}
