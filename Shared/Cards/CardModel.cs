using System;

namespace KrzyWro.CAH.Shared.Cards
{
    public abstract class CardModel
    {
        public Guid Id { get; }
        public string Text { get; }

        protected CardModel(Guid id, string text)
        {
            Id = id;
            Text = text;
        }
    }
}
