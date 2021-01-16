using System;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Shared.Contracts.ClientMessages.Table
{
    public interface ITablePlayerRequestHand : ITablePlayerMessage
    {
        Task RequestHand(Guid game);
    }
}
