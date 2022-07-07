using Barriot.Application.Interactions.Attributes;
using Barriot.Extensions;

namespace Barriot.Application.Interactions.Modules
{
    [IgnoreBlacklistedUsers]
    [EnabledInDm(false)]
    public class BumpModule : BarriotModuleBase
    {
        public BumpModule(ILogger<BarriotModuleBase> logger) : base(logger)
        {
        }

        [SlashCommand("daily", "Claims daily rewards.")]
        public async Task DailyAsync()
        {
            var redeemer = await BumpsEntity.GetAsync(Context.User.Id);

            if (redeemer.CanRedeem())
            {
                redeemer.LastRedeemed = DateTime.UtcNow;
                redeemer.BumpsToGive++;

                await RespondAsync(
                    format: MessageFormat.Success,
                    header: "Daily bump redeemed!");
            }
            else
            {
                var span = redeemer.LastRedeemed - DateTime.UtcNow.AddDays(-1);

                await RespondAsync(
                    format: MessageFormat.Failure,
                    header: "Daily bump has already been redeemed!",
                    context: $"Please try again in {(span.ToReadable())}");
            }
        }

        [AllowAPI(true)]
        [SlashCommand("bump", "Bump another user!")]
        public async Task BumpAsync(RestUser user)
        {
            if (user.IsBot || user.IsWebhook)
                await RespondAsync(
                    format: MessageFormat.Failure,
                    header: "You cannot bump a bot!");

            else if (Context.User.Id == user.Id)
                await RespondAsync(
                    format: MessageFormat.Failure,
                    header: "You cannot bump yourself!");

            else
            {
                var bumps = await BumpsEntity.GetAsync(user.Id);
                var giver = await BumpsEntity.GetAsync(Context.User.Id);

                bool permissionToIgnore = Context.Member.HasFlag(FlagType.Developer);

                if (!permissionToIgnore && giver.BumpsToGive <= 0)
                    await RespondAsync(
                        format: MessageFormat.Failure,
                        header: "You cannot bump someone if you have no bumps to give!",
                        context: giver.CanRedeem() ? "Use `/daily` to redeem a bump!" : null);

                else
                {
                    if (!permissionToIgnore)
                        giver.BumpsToGive--;

                    bumps.ReceivedBumps++;

                    ComponentBuilder? cb = null;
                    if (permissionToIgnore || giver.BumpsToGive >= 1)
                        cb = new ComponentBuilder()
                            .WithButton("Bump again!", $"bump:{Context.User.Id},{user.Id},{1}");

                    await RespondAsync(
                        format: MessageFormat.BumpGiven,
                        header: $"Succesfully bumped <@{user.Id}>!",
                        components: cb ?? null,
                        ephemeral: false);
                }
            }
        }

        [DoUserCheck]
        [ComponentInteraction("bump:*,*,*")]
        public async Task BumpFromButtonAsync(ulong _, ulong userId, int count)
        {
            var bumps = await BumpsEntity.GetAsync(userId);
            var giver = await BumpsEntity.GetAsync(Context.User.Id);

            bool permissionToIgnore = Context.Member.HasFlag(FlagType.Developer);

            count++;

            if (!permissionToIgnore)
                giver.BumpsToGive--;
            bumps.ReceivedBumps++;

            ComponentBuilder? cb = null;
            if (permissionToIgnore || giver.BumpsToGive >= 1)
                cb = new ComponentBuilder()
                    .WithButton("Bump again!", $"bump:{Context.User.Id},{userId},{count}");

            await UpdateAsync(
                format: MessageFormat.BumpGiven,
                header: $"Succesfully bumped <@{userId}> {count} times!",
                components: cb ?? null);
        }
    }
}
