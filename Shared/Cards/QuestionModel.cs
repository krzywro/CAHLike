using System;

namespace KrzyWro.CAH.Shared.Cards
{
    public class QuestionModel : CardModel
    {
        public int AnswerCards { get; } = 1;

        public QuestionModel(Guid id, string text, int answerCards) : base(id, text)
        {
            AnswerCards = answerCards;
        }
    }
}
