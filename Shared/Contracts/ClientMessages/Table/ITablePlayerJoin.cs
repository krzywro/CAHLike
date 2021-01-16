using System;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Shared.Contracts.ClientMessages.Table
{
    public interface ITablePlayerJoin : ITablePlayerMessage<Guid>
    {
        Task Join(Guid game, Guid playerId);
    }
}
