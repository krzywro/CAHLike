using KrzyWro.CAH.Server.Helpers;
using KrzyWro.CAH.Shared.Cards;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Server.Services
{
    public class DeckService : IDeckService
    {
        private readonly IDistributedCache _cache;
        private readonly IHostEnvironment _hostEnvironment;

        public DeckService(IDistributedCache cache, IHostEnvironment hostEnvironment)
        {
            _cache = cache;
            _hostEnvironment = hostEnvironment;
        }

        public async Task EnsureDeck()
        {
            var questions = await _cache.GetStringAsync("questions");
            var answers = await _cache.GetStringAsync("answers");

            if (string.IsNullOrEmpty(questions) || string.IsNullOrEmpty(answers))
                await ResetDeck();
        }

        public async Task<QuestionModel> PeekQuestion()
        {
            await EnsureDeck();
            var questions = await _cache.GetStringAsync("questions");
            var deck = JsonSerializer.Deserialize<Stack<QuestionModel>>(questions);
            return deck.Peek();
        }

        public async Task<AnswerModel> PopAnswer()
        {
            await EnsureDeck();
            var answers = await _cache.GetStringAsync("answers");
            var deck = JsonSerializer.Deserialize<Stack<AnswerModel>>(answers);
            var answer = deck.Pop();
            answers = JsonSerializer.Serialize(deck);
            await _cache.SetStringAsync("answers", answers);

            return answer;
        }

        public async Task PrepareNextQuestion()
        {
            await EnsureDeck();
            var questions = await _cache.GetStringAsync("questions");
            var deck = JsonSerializer.Deserialize<Stack<QuestionModel>>(questions);
            deck.Pop();
            questions = JsonSerializer.Serialize(deck);
            await _cache.SetStringAsync("questions", questions);
        }

        public async Task ResetDeck()
        {
            string contentRootPath = _hostEnvironment.ContentRootPath;
            var JSON = File.ReadAllText(Path.Join(contentRootPath, "Deck.json"));
            var deck = JsonSerializer.Deserialize<DeckModel>(JSON);

            var questions = JsonSerializer.Serialize(deck.Questions.Shuffle());
            await _cache.SetStringAsync("questions", questions);

            var answers = JsonSerializer.Serialize(deck.Answers.Shuffle());
            await _cache.SetStringAsync("answers", answers);
        }
    }
}
