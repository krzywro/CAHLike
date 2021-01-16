using System;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Shared.Contracts.ClientMessages.Table
{
    public interface ITablePlayerRequestScores : ITablePlayerMessage
    {
        Task RequestScores(Guid game);
    }
}
