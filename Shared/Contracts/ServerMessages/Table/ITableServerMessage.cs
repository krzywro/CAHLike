using System;

namespace KrzyWro.CAH.Shared.Contracts.ServerMessages.Table
{
    public interface ITableServerMessage : IServerMessage<Guid>
    {
    }
    public interface ITableServerMessage<T1> : IServerMessage<Guid, T1>
    {
    }
    public interface ITableServerMessage<T1, T2> : IServerMessage<Guid, T1, T2>
    {
    }
}
