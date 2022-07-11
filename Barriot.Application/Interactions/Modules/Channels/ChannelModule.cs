using Barriot.Application.Interactions.Attributes;
using Barriot.Application.Interactions.Modals;

namespace Barriot.Application.Interactions.Modules
{
    // TODO, rework SEND
    [EnabledInDm(false)]
    [RequireBotPermission(ChannelPermission.ManageChannels)]
    [IgnoreBlacklistedUsers]
    public class ChannelModule : BarriotModuleBase
    {
        public ChannelModule(ILogger<BarriotModuleBase> logger) : base(logger)
        {

        }

        [AllowAPI(true)]
        [SlashCommand("channel", "Manages this channel.")]
        public async Task ManageChannelAsync()
        {
            var perms = (Context.Channel as ITextChannel)!.PermissionOverwrites.Where(x => x.TargetId == Context.Guild.Id);

            bool isLocked = false;
            if (perms.Any() && perms.First().Permissions.SendMessages == PermValue.Deny)
                isLocked = true;

            var cb = new ComponentBuilder()
                .WithButton($"{(isLocked ? "Unlock" : "Lock")} channel", $"channel-lock:{Context.User.Id},{isLocked}", isLocked ? ButtonStyle.Success : ButtonStyle.Danger)
                .WithButton("Delete channel", $"channel-delete:{Context.User.Id}", ButtonStyle.Danger)
                .WithButton("Prune channel", $"channel-prune:{Context.User.Id}", ButtonStyle.Danger);

            await RespondAsync(
                format: MessageFormat.Important,
                header: "Manage this channel.",
                context: "Please select an action below:",
                components: cb,
                ephemeral: true);
        }

        [AllowAPI(true)]
        [DoUserCheck]
        [ComponentInteraction("channel-lock:*,*")]
        public async Task LockAsync(ulong _, bool isLocked)
        {
            if (Context.Channel is RestTextChannel channel)
            {
                if (!isLocked)
                {
                    await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(sendMessages: PermValue.Deny));

                    await UpdateAsync(
                        format: MessageFormat.NotAllowed,
                        header: "Successfully locked channel.",
                        context: "You can unlock the channel by executing `/channel` again.");
                }
                else
                {
                    await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(sendMessages: PermValue.Inherit));

                    await UpdateAsync(
                        format: MessageFormat.Success,
                        header: "Successfully unlocked channel.");
                }
            }
            else
                throw new InvalidOperationException();
        }

        [AllowAPI(true)]
        [DoUserCheck]
        [ComponentInteraction("channel-delete:*")]
        public async Task DeleteAsync(ulong _)
        {
            if (Context.Channel is RestTextChannel channel)
            {
                await UpdateAsync(
                    format: MessageFormat.Success,
                    header: "Deleting channel...");

                await Task.Delay(5000);

                await channel.DeleteAsync(new() { AuditLogReason = $"Deleted channel as requested by: {Context.User.Username}#{Context.User.Discriminator}" });
            }
            else
                throw new InvalidOperationException();
        }

        [AllowAPI(true)]
        [DoUserCheck]
        [ComponentInteraction("channel-prune:*")]
        public async Task StartPruneAsync(ulong _)
        {
            var mb = new ModalBuilder()
                .WithTitle("Delete messages")
                .WithCustomId("channel-prune-finalize")
                .AddTextInput("Amount of messages to delete:", "entry", TextInputStyle.Short, "1", 1, 3, true, "1");

            await RespondWithModalAsync(mb.Build());
        }

        [AllowAPI(true)]
        [ModalInteraction("channel-prune-finalize")]
        public async Task FinalizePruneAsync(QueryModal<string> modal)
        {
            if (uint.TryParse(modal.Result, out var result) && result <= 100)
            {
                if (Context.Channel is RestTextChannel channel)
                {
                    var messages = await channel.GetMessagesAsync((int)result).FlattenAsync();

                    await channel.DeleteMessagesAsync(messages);

                    await RespondAsync(
                        format: MessageFormat.Success,
                        header: $"Succesfully attempted to delete {result} messages from this channel.",
                        description: "If not all messages disappeared, they may be older than 14 days old. Older than 14 days, Barriot will not remove messages.",
                        ephemeral: true);
                }
                else
                    throw new InvalidOperationException();
            }
            else
                await RespondAsync(
                    error: "The amount of messages to prune is invalid.",
                    context: $"Your value: `{modal.Result}` does not match the required 1-100.");
        }
    }
}
