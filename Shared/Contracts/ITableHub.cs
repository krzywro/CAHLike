using KrzyWro.CAH.Shared.Contracts.ClientMessages.Table;

namespace KrzyWro.CAH.Shared.Contracts
{
    public interface ITableHub :
        ITablePlayerJoin,
        ITablePlayerRequestQuestion,
        ITablePlayerRequestScores,
        ITablePlayerRequestHand,
        ITablePlayerSendAnswers,
        ITableMasterSendAnswers
    {
        public static readonly string Path = "/tablehub";
    }
}
