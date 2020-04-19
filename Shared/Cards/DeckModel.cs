using System;
using System.Collections.Generic;
using System.Text;

namespace KrzyWro.CAH.Shared.Cards
{
    public class DeckModel
    {
        public List<QuestionModel> Questions { get; set; } = new List<QuestionModel>();
        public List<AnswerModel> Answers { get; set; } = new List<AnswerModel>();
    }
}
