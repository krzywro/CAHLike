using KrzyWro.CAH.Shared.Contracts.ClientMessages;

namespace KrzyWro.CAH.Shared.Contracts
{
    public interface IPlayerHub :
        IPlayerHubPickAnswer,
        IPlayerHubRegisterPlayer,
        IPlayerHubRequestHand,
        IPlayerHubRequestQuestion,
        IPlayerHubRequestScores,
        IPlayerHubSendAnswers
    {
        public static readonly string Path = "/playerhub";
    }
}
