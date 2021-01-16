using KrzyWro.CAH.Shared.Dto;
using System.Collections.Generic;

namespace KrzyWro.CAH.Shared.Contracts.ServerMessages.Games
{
    public interface IGameSendList : IServerMessage<List<TableEntry>>
    {
    }
}
