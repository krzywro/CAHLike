using System;

namespace KrzyWro.CAH.Shared.Cards
{
    public abstract class CardModel
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
    }
}
