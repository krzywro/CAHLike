using KrzyWro.CAH.Shared.Cards;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Krzywro.CAH.DeckCreator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length != 3)
                Console.Error.WriteLine("Nieprawidłowa liczba argumentów");

            if (!File.Exists(args[0]))
                Console.Error.WriteLine("Plik pytań nie istnieje");

            if (!File.Exists(args[0]))
                Console.Error.WriteLine("Plik odpowiedzi nie istnieje");

            var questions = await File.ReadAllLinesAsync(args[0]);
            var answers = await File.ReadAllLinesAsync(args[1]);

            var deck = new DeckModel();

            foreach (var line in questions)
            {
                var splitted = line.Split('|');
                var answercards = splitted.Length > 1 ? int.Parse(splitted[1]) : 1;
                deck.Questions.Add(new QuestionModel { Id = Guid.NewGuid(), Text = splitted[0], AnswerCards = answercards });
            }

            foreach (var line in answers)
                deck.Answers.Add(new AnswerModel { Id = Guid.NewGuid(), Text = line });

            using (var outputFile = File.OpenWrite(args[2]))
            {
                await JsonSerializer.SerializeAsync(outputFile, deck, new JsonSerializerOptions() { WriteIndented = true });
            }
        }
    }
}
