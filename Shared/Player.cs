using System;
using System.Collections.Generic;
using System.Text;

namespace KrzyWro.CAH.Shared
{
    public class Player
    {
        public Guid Id { get; }
        public string Name { get; set; } = "New Player";

        public Player(Guid id) => Id = id;
    }
}
