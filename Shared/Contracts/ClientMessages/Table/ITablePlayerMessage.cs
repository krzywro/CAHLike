using System;

namespace KrzyWro.CAH.Shared.Contracts.ClientMessages.Table
{
    public interface ITablePlayerMessage : IClientMessage<Guid>
    {
    }
    public interface ITablePlayerMessage<T1> : ITablePlayerMessage
    {
    }
    public interface ITablePlayerMessage<T1, T2> : ITablePlayerMessage<T1>
    {
    }
}
