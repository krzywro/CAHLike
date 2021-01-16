using System;

namespace KrzyWro.CAH.Shared.Dto
{
    public class TableEntry
    {
        public Guid GameId { get; set; }
        public DateTimeOffset Created { get; set; }
    }
}
