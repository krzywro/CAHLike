using KrzyWro.CAH.Shared.Cards;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Server.Services
{
    public interface IGamesService
    {
        Task<List<Table>> List();
        Task<Table> Create();
        Task<Table> Get(Guid guid);
    }

    public class GamesService : IGamesService
    {
        private readonly static ConcurrentBag<Table> _tables = new();

        private readonly IDeckService _deckService;

        public GamesService(IDeckService deckService)
        {
            _deckService = deckService;
        }

        public async Task<Table> Create()
        {
            var table = new Table();
            table.Id = Guid.NewGuid();
            table.QuestionDeck = await _deckService.RequestQuestionDeck();
            table.CurrentQuestion = table.QuestionDeck.Pop();
            table.AnswerDeck = new ConcurrentStack<AnswerModel>(await _deckService.RequestAnswerDeck());
            _tables.Add(table);

            return table;
        }

        public Task<Table> Get(Guid guid)
        {
            return Task.FromResult(_tables.SingleOrDefault(x => x.Id == guid));
        }

        public Task<List<Table>> List()
        {
            return Task.FromResult(_tables.ToList());
        }
    }

    public class Table
    {
        public Guid Id { get; set; }
        public ConcurrentBag<Guid> Players { get; set; } = new();
        public ConcurrentDictionary<Guid, int> Scores { get; set; } = new();
        public Guid CurrentMaster { get; set; } = Guid.Empty;
        public HashSet<Guid> PreviousMasters { get; set; } = new();
        public ConcurrentDictionary<Guid, List<AnswerModel>> Hands { get; set; } = new();
        public ConcurrentDictionary<Guid, List<AnswerModel>> CollectedAnswers { get; set; } = new();
        public QuestionModel CurrentQuestion { get; set; }
        public Stack<QuestionModel> QuestionDeck { get; set; } = new();
        public ConcurrentStack<AnswerModel> AnswerDeck { get; set; } = new();

    }
}
