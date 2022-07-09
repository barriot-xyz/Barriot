using Barriot.Application.Interactions.Attributes;
using Barriot.Pagination;
using Barriot.Models;
using MongoDB.Bson;
using Barriot.Extensions;
using Barriot.Application.Interactions.Modals;

namespace Barriot.Application.Interactions.Modules
{
    [IgnoreBlacklistedUsers]
    public class PinModule : BarriotModuleBase
    {
        public PinModule(ILogger<BarriotModuleBase> logger) : base(logger)
        {

        }

        [MessageCommand("Pin")]
        public async Task PinAsync(IMessage message)
        {
            var url = message.GetJumpUrl(Context.Interaction.IsDMInteraction, Context.Interaction.ChannelId, Context.Interaction.GuildId);

            var mb = new ModalBuilder()
                .WithTitle("Add a note to this pin:")
                .WithCustomId($"pin-finish:{url}")
                .AddTextInput("Note:", "entry", TextInputStyle.Short, "This is pretty funny", 1, 200);

            await RespondWithModalAsync(mb.Build());
        }

        [ModalInteraction("pin-finish:*")]
        public async Task PinFinalizeAsync(string url, QueryModal<string> modal)
        {
            if (!JumpUrl.TryParse(url, out var messageUrl))
            {
                await RespondAsync(
                    error: "An unexpected error occurred while parsing this message!",
                    context: "Please report this behavior.");
                return;
            }

            var pins = await PinEntity.GetManyAsync(Context.User.Id);

            if (pins.Any(x => x.MessageId == messageUrl.MessageId))
            {
                await RespondAsync(
                    error: "You already pinned this message!",
                    context: "View your messages by executing ` /pins `.");
                return;
            }

            if (string.IsNullOrEmpty(modal.Result))
            {
                await RespondAsync(
                    error: "The note you added to this pin is empty!");
                return;
            }

            await PinEntity.CreateAsync(Context.User.Id, messageUrl, modal.Result);

            await RespondAsync(
                format: MessageFormat.Success,
                header: "Succesfully created pin!",
                context: "View your pins by executing ` /pins `.",
                description: $"**Note:** {modal.Result}\n> **Message link:** {messageUrl}");
        }

        [SlashCommand("pins", "View all your current pins.")]
        public async Task ListPinsAsync(
            [Summary("page", "The page you want to view")] int page = 1,
            [Summary("query", "Search for specific pins")] string search = "")
        {
            var value = await ListPinsInternalAsync(page, search);

            if (value is not null)
                await RespondAsync(
                    page: value.Value,
                    header: "A list of your pins:",
                    context: string.IsNullOrEmpty(search) ? null : $"This list only matches entries tied to {search}");

            else
                await RespondAsync(
                    error: string.IsNullOrEmpty(search) ? "You currently have no pins!" : $"No pins were found that match \"{search}\"!",
                    context: "Use ` Pin ` in message apps to add a new pin.");
        }

        [ComponentInteraction("pin-list:*,*")]
        public async Task ListPinsFromButtonAsync(string search, int page)
        {
            var value = await ListPinsInternalAsync(page, search);

            if (value is not null)
                await UpdateAsync(
                    page: value.Value,
                    header: "A list of your pins:",
                    context: string.IsNullOrEmpty(search) ? null : $"This list only matches entries tied to {search}");

            else
                await UpdateAsync(
                    error: "You currently have no pins!",
                    context: "Use ` Pin ` in message apps to add a new pin.");
        }

        private async Task<Page?> ListPinsInternalAsync(int page, string search)
        {
            if (page < 1)
                page = 1;

            var pins = await PinEntity.GetManyAsync(Context.User.Id);

            if (pins.Any())
            {
                if (!Paginator<PinEntity>.TryGet(out var paginator))
                {
                    paginator = new PaginatorBuilder<PinEntity>()
                        .WithCustomId("pin-list")
                        .WithPages(x =>
                        {
                            var pinnedSince = DateTime.UtcNow - x.PinDate;

                            string description = "";
                            if (!string.IsNullOrEmpty(x.Reason))
                                description = $"> **Note:** {x.Reason} \n";

                            description += $"> **Jump to message:** {x.Url}";

                            return new($"{pinnedSince.ToReadable()} ago.", description);
                        })
                        .Build();
                }
                var value = paginator.GetPage(page, pins, search, (x, s) => x.Where(x => x.Reason.Contains(s)).ToList());

                if (value is null)
                    return null;

                value.Value.Component.WithButton("Delete pins", $"pins-delete:{page}", ButtonStyle.Danger);
                value.Value.Component.WithButton("Modify pins", $"pins-edit:{page}", ButtonStyle.Secondary);

                return value;
            }

            return null;
        }

        [ComponentInteraction("pins-edit:*")]
        public async Task EditPinsAsync(int page)
        {
            var pins = await PinEntity.GetManyAsync(Context.User.Id);

            if (pins.Any())
            {
                var sb = new SelectMenuBuilder()
                    .WithMinValues(1)
                    .WithMaxValues(1)
                    .WithCustomId("pins-editing")
                    .WithPlaceholder("Select the pin you want to modify.");

                int index = page * 10 - 10;

                var range = pins.GetRange(index, pins.Count - index);
                for (int i = 0; i < range.Count; i++)
                {
                    if (i is 10)
                        break;
                    sb.AddOption(range[i].PinDate.ToString(), range[i].ObjectId.ToString(), range[i].MessageId.ToString());
                }

                var cb = new ComponentBuilder()
                    .WithSelectMenu(sb);

                await UpdateAsync(
                    format: "pen_ballpoint",
                    header: "Select a pin to edit the note for:",
                    components: cb);
            }
            else
                await UpdateAsync(
                    error: "You have no pins to modify!",
                    context: "The page you selected from is outdated and does not contain any pins.");
        }

        [ComponentInteraction("pins-editing")]
        public async Task EditingPinsAsync(ObjectId[] selectedValues)
        {
            var mb = new ModalBuilder()
                .WithTitle("Edit the note to this pin:")
                .WithCustomId($"pins-edited:{selectedValues[0]}")
                .AddTextInput("Note:", "entry", TextInputStyle.Short, "", 1, 200);

            await RespondWithModalAsync(mb.Build());
        }

        [ModalInteraction("pins-edited:*")]
        public async Task EditedPinsAsync(ObjectId id, QueryModal<string> modal)
        {
            var pins = await PinEntity.GetManyAsync(Context.User.Id);

            var pin = pins.First(x => x.ObjectId == id);

            if (pin is not null)
            {
                if (string.IsNullOrEmpty(modal.Result))
                {
                    await RespondAsync(
                        error: "Unable to modify a pin reason with an empty message.", 
                        context: "Please try again while specifying a message.");
                    return;
                }

                var eb = new EmbedBuilder()
                    .WithUrl(pin.Url)
                    .WithTitle("Click here to jump to message.")
                    .AddField("Before:", string.IsNullOrEmpty(pin.Reason) ? "_ _" : pin.Reason)
                    .AddField("After", modal.Result);

                pin.Reason = modal.Result;

                await RespondAsync(
                    format: MessageFormat.Success,
                    header: "Succesfully modified pin reason:",
                    embed: eb);
            }
            else
                await RespondAsync(
                    error: "The pin you tried to edit does not exist!",
                    context: "This could happen because you deleted the pin before editing it.");
        }

        [ComponentInteraction("pins-delete:*")]
        public async Task DeletePinsAsync(int page)
        {
            var pins = await PinEntity.GetManyAsync(Context.User.Id);

            if (pins.Any())
            {
                var sb = new SelectMenuBuilder()
                    .WithMinValues(1)
                    .WithCustomId("pins-deleted")
                    .WithPlaceholder("Select 1 or more pins to delete.");

                int index = page * 10 - 10;

                var range = pins.GetRange(index, pins.Count - index);
                for (int i = 0; i < range.Count; i++)
                {
                    if (i is 10)
                        break;
                    sb.AddOption(range[i].PinDate.ToString(), range[i].ObjectId.ToString(), range[i].MessageId.ToString());
                }

                sb.WithMaxValues(sb.Options.Count);

                var cb = new ComponentBuilder()
                    .WithSelectMenu(sb);

                await UpdateAsync(
                    format: MessageFormat.Deleting,
                    header: "Deleting pins:",
                    components: cb);
            }
            else
                await UpdateAsync(
                    error: "You have no pins to delete!",
                    context: "The page you selected from is outdated and does not contain any pins.");
        }

        [ComponentInteraction("pins-deleted")]
        public async Task DeletedPinsAsync(ObjectId[] selectedValues)
        {
            var pins = await PinEntity.GetManyAsync(Context.User.Id);

            if (!pins.Any())
                await UpdateAsync(
                    error: "You have no pins to delete!");

            else
            {
                foreach (var value in selectedValues)
                {
                    var pin = pins.First(x => x.ObjectId == value);

                    if (pin is not null)
                        await pin.DeleteAsync();
                }
                await UpdateAsync(
                    format: MessageFormat.Success,
                    header: $"Succesfully removed {selectedValues.Length} pin(s).");
            }
        }
    }
}
