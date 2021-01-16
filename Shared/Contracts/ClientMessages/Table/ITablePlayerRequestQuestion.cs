using System;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Shared.Contracts.ClientMessages.Table
{
    public interface ITablePlayerRequestQuestion : ITablePlayerMessage
    {
        Task RequestQuestion(Guid game);
    }
}
