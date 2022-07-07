using Barriot.Application.Interactions.Attributes;

namespace Barriot.Application.Interactions.Modules
{
    [IgnoreBlacklistedUsers]
    public class ProfileModule : BarriotModuleBase
    {
        public ProfileModule(ILogger<BarriotModuleBase> logger) : base(logger)
        {

        }

        [AllowAPI(true)]
        [SlashCommand("profile", "Views your or another user's profile.")]
        public async Task SlashProfileAsync(RestUser? user = null)
            => await ProfileAsync(user ?? Context.User);

        [AllowAPI(true)]
        [UserCommand("Profile")]
        public async Task ProfileAsync(RestUser user)
        {
            bool isSelfUser = user.Id == Context.User.Id;

            var member = isSelfUser 
                ? Context.Member 
                : await UserEntity.GetAsync(user.Id);

            var bumps = await BumpsEntity.GetAsync(user.Id);

            var eb = new EmbedBuilder();
            var cb = new ComponentBuilder();

            cb.WithButton("View stats", $"stats:{Context.User.Id},{user.Id}");
            cb.WithButton("View acknowledgements", $"ack:{Context.User.Id},{user.Id}");

            eb.AddField("Bumps:", $"Received ` {bumps.ReceivedBumps} `", true);
            eb.AddField("_ _", $"` {bumps.BumpsToGive} ` bumps to give out.", true);

            if (member.Flags.Any())
            {
                eb.AddField("Acknowledgements:", $"Owns ` {member.Flags.Count} ` acknowledgements in total.");

                if (member.FeaturedFlag is not null)
                    eb.AddField("Featured acknowledgement:", $"**{member.FeaturedFlag}**\n > {member.FeaturedFlag.Description}");
            }

            if (member.Votes > 0)
            {
                eb.AddField("Votes:", $"Voted ` {member.Votes} ` times in total.", true);

                if (member.MonthlyVotes > 0)
                    eb.AddField("_ _", $"` {member.MonthlyVotes} ` times in the last month.", true);
            }

            await RespondAsync(
                format: "bust_in_silhouette",
                header: $"{user.Username}#{user.Discriminator}'s profile:",
                embed: eb,
                components: cb);
        }

        [DoUserCheck]
        [ComponentInteraction("stats:*,*")]
        public async Task StatsAsync(ulong _, ulong targetId)
        {
            var cb = new ComponentBuilder()
                .WithButton("View acknowledgements", $"ack:{Context.User.Id},{targetId}");

            if (Context.Member.Flags.Any(x => x.Type is FlagType.Developer))
            {
                if (targetId != Context.User.Id)
                    cb.WithButton("Blacklist user", $"blacklist:{Context.User.Id},{targetId}", ButtonStyle.Danger);
            }

            var target = await UserEntity.GetAsync(targetId);

            var eb = new EmbedBuilder()
                .AddField("Latest command", $"` {target.LastCommand} `")
                .AddField("Commands", $"Executed ` {target.CommandsExecuted} ` command{((target.CommandsExecuted != 1) ? "s" : "")}.")
                .AddField("Components", $"Clicked ` {target.ButtonsPressed} ` component{((target.ButtonsPressed != 1) ? "s" : "")}.")
                .AddField("Challenges", $"Won ` {target.GamesWon} ` challenge{((target.GamesWon != 1) ? "s" : "")}.");

            await UpdateAsync(
                format: "bar_chart",
                header: $"<@{targetId}>'s Barriot command statistics:",
                embed: eb,
                components: cb);
        }

        [DoUserCheck]
        [ComponentInteraction("ack:*,*")]
        public async Task AcknowledgementsAsync(ulong _, ulong targetId)
        {
            var user = await UserEntity.GetAsync(targetId);

            var eb = new EmbedBuilder()
                .WithColor(Color.Gold);

            if (user.Flags.Any())
                foreach (var flag in user.Flags)
                {
                    bool inline = true;
                    switch (flag.Type)
                    {
                        case FlagType.Component:
                        case FlagType.Command:
                        case FlagType.Champion:
                            inline = false;
                            break;
                        default:
                            break;
                    }
                    eb.AddField(flag.ToString(), flag.Description, inline);
                }
            else
                eb.WithDescription("This user has no acknowledgments.");

            var cb = new ComponentBuilder()
                .WithButton("View stats", $"stats:{Context.User.Id},{targetId}");

            if ((await Context.Client.GetApplicationInfoAsync()).Owner.Id == Context.User.Id)
            {
                cb = new();
                cb.WithButton("Add acknowledgement", $"flag-creating:{Context.User.Id},{targetId}", ButtonStyle.Success);
                cb.WithButton("Remove acknowledgement(s)", $"flag-deleting:{Context.User.Id},{targetId}", ButtonStyle.Danger);
            }

            await UpdateAsync(
                format: "medal",
                header: $"<@{targetId}>'s Acknowledgements.",
                context: "Rewarded for contributions, regular use of the bot, donations and more.",
                embed: eb,
                components: cb);
        }
    }
}
