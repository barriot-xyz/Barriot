using Barriot.Extensions;
using Barriot.Application.Interactions.Attributes;

namespace Barriot.Application.Interactions.Modules
{
    // TODO, rework SEND
    [IgnoreBlacklistedUsers]
    public class GamesModule : BarriotModuleBase
    {
        public GamesModule(ILogger<BarriotModuleBase> logger) : base(logger)
        {

        }

        [SlashCommand("riddle", "Gets a random riddle.")]
        public async Task RiddleAsync()
        {
            var file = FileExtensions.GetDataFromFile("Riddles");

            var cb = new ComponentBuilder()
                .WithButton("Answer", $"riddle:{Context.User.Id},{file.Index}");

            await RespondAsync(
                format: MessageFormat.Question,
                header: "Answer me this:",
                description: file.SelectedLine.Split('|').First(),
                components: cb);
        }

        [DoUserCheck]
        [ComponentInteraction("riddle:*,*")]
        public async Task RiddleAsync(ulong _, int riddleId)
            => await RespondAsync(
                format: "eyes",
                header: "The answer to your riddle is:",
                description: FileExtensions.GetDataFromFile("Riddles").Lines[riddleId].Split('|').Last());

        [SlashCommand("question", "Give me a question, I'll answer!")]
        public async Task QuestionAsync(
            [Summary("question", "The question to ask")] string? _ = null)
            => await RespondAsync(
                format: "speech_balloon",
                header: FileExtensions.GetDataFromFile("Answers").SelectedLine);

        [SlashCommand("random-fact", "A random fact")]
        public async Task RandomFactAsync()
            => await RespondAsync(
                format: "bulb",
                header: "Did you know that:",
                description: FileExtensions.GetDataFromFile("Facts").SelectedLine);

        [SlashCommand("showerthought", "Ever thought about something odd in the shower? I certainly did!")]
        public async Task ShowerThoughtsAsync()
            => await RespondAsync(
                format: "thinking",
                header: "Have you ever thought about:",
                description: FileExtensions.GetDataFromFile("Thoughts").SelectedLine);

        [SlashCommand("dadjoke", "They're pretty bad tbh...")]
        public async Task DadJokeAsync()
            => await RespondAsync(
                format: "man_facepalming",
                header: "Heres a good one:",
                description: FileExtensions.GetDataFromFile("Jokes").SelectedLine);

        [SlashCommand("ping", "Pong! See if the bot works. If this command fails, all is lost...")]
        public async Task PingAsync()
            => await RespondAsync(
                format: "ping_pong",
                header: "Pong!");

        [SlashCommand("coinflip", "Flips a coin.")]
        public async Task CoinFlipAsync()
            => await RespondAsync(
                text: (new Random().Next(2) < 1)
                    ? ":coin: **Heads!**"
                    : ":coin: **Tails!**");

        [SlashCommand("dice", "Throws a dice.")]
        public async Task DiceAsync()
            => await RespondAsync(
                format: "game_die",
                header: $"{(new Random().Next(7) + 1)}");
    }
}
