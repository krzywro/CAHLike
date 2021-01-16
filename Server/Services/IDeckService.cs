using KrzyWro.CAH.Shared.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Server.Services
{
    public interface IDeckService
    {
        Task EnsureDeck();
        Task ResetDeck();
        Task PrepareNextQuestion();
        Task<QuestionModel> PeekQuestion();
        Task<AnswerModel> PopAnswer();
        Task<Stack<QuestionModel>> RequestQuestionDeck();
        Task<Stack<AnswerModel>> RequestAnswerDeck();
    }
}
