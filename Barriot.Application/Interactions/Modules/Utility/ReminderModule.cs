using Barriot.Extensions;
using Barriot.Application.Interactions.Attributes;
using Barriot.Models.Files;
using Barriot.Pagination;
using MongoDB.Bson;

namespace Barriot.Application.Interactions.Modules
{
    [IgnoreBlacklistedUsers]
    public class ReminderModule : BarriotModuleBase
    {
        public ReminderModule(ILogger<BarriotModuleBase> logger) : base(logger)
        {

        }

        [SlashCommand("remind", "Creates a reminder for a set time with provided message.")]
        public async Task RemindAsync(
            [Summary("time", "The time until this reminder")] TimeSpan spanUntil,
            [Summary("message", "What you want to be reminded about")] string message,
            [Summary("frequency", "How many times this reminder should be repeated")] int frequency = 1,
            [Summary("span", "What the time between each repetition should be")] TimeSpan? timeBetween = null)
        {
            if (frequency <= 0)
                frequency = 1;

            if (spanUntil == TimeSpan.Zero)
                await RespondAsync(
                    text: FileExtensions.GetError(ErrorInfo.InvalidTimeSpan, "time until this reminder is sent"),
                    ephemeral: true);

            else if (timeBetween is null && frequency > 1)
                await RespondAsync(
                    text: FileExtensions.GetError(ErrorInfo.InvalidTimeSpan, "time between reminders"),
                    ephemeral: true);

            else if (timeBetween is not null && timeBetween?.TotalMinutes < 5d)
                await RespondAsync(
                    text: ":x: **The timespan can not be shorter than 5 minutes!**",
                    ephemeral: true);

            else
            {
                if (!Context.Interaction.IsDMInteraction)
                {
                    try
                    {
                        var embed = new EmbedBuilder()
                            .WithDescription(FileExtensions.GetEmbedContent(EmbedInfo.ReminderCheckUp))
                            .WithFooter("Make sure you keep your DM's open to receive it!");

                        await Context.User.SendMessageAsync(
                            text: ":wave: **Hi, just checking up on you!**",
                            embed: embed.Build());
                    }
                    catch
                    {
                        await RespondAsync(
                            text: $":x: **Reminder creation failed!** {FileExtensions.GetError(ErrorInfo.ReminderSendFailed)}",
                            ephemeral: true);
                        return;
                    }
                }

                await RemindEntity.CreateAsync(message, spanUntil, Context.User.Id, frequency, timeBetween);

                await RespondAsync(
                    text: $":thumbsup: **Got it!** I will remind you to {message} in {spanUntil.ToReadable()}" +
                    $"{((frequency > 1) ? $"\n\n> This reminder will repeat {frequency} time(s) every {timeBetween?.ToReadable()}." : "")}",
                    ephemeral: Context.Member.DoEphemeral);
            }
        }

        [SlashCommand("reminders", "Lists your current reminders.")]
        public async Task ListRemindersAsync(
            [Summary("page", "The reminders page")] int page = 1,
            [Summary("query", "The query to search reminders by")] string search = "")
        {
            if (search.Length > 50)
                await RespondAsync(
                    error: "Your search query is beyond the maximum query limit!");

            else
            {
                var value = await ListRemindersInternal(search, page);

                if (value is not null)
                    await RespondAsync(
                        page: value.Value,
                        header: "Your reminders:",
                        context: string.IsNullOrEmpty(search) ? null : $"Only reminders matching \"{search}\" will be displayed.");

                else
                    await RespondAsync(
                        error: "You have no reminders!",
                        context: "Use ` /remind ` to set reminders.");
            }
        }

        [ComponentInteraction("reminders-list:*,*")]
        public async Task ListRemindersFromButtonAsync(string search, int page)
        {
            var value = await ListRemindersInternal(search, page);

            if (value is not null)
                await UpdateAsync(
                    page: value.Value,
                    header: "Your reminders:",
                    context: string.IsNullOrEmpty(search) ? null : $"Only reminders matching \"{search}\" will be displayed.");

            else
                await UpdateAsync(
                    error: string.IsNullOrEmpty(search) ? "You have no reminders!" : $"No reminders were found matching \"{search}\"!",
                    context: "Use ` /remind ` to set reminders.");

        }

        private async Task<Page?> ListRemindersInternal(string search, int page)
        {
            if (page < 1)
                page = 1;

            var reminders = await RemindEntity.GetManyAsync(Context.User);

            if (reminders.Any())
            {
                if (!Paginator<RemindEntity>.TryGet(out var paginator))
                {
                    paginator = new PaginatorBuilder<RemindEntity>()
                        .WithPages(x =>
                        {
                            string sendRepeat = "";
                            if (x.Frequency > 1)
                                sendRepeat = $"\n⤷ *Set to repeat {x.Frequency} more time(s).";
                            return new($"{x.Expiration} (UTC)", x.Message ?? "No message set" + sendRepeat);
                        })
                        .WithCustomId("reminders-list")
                        .Build();
                }
                var value = paginator.GetPage(page, reminders);

                if (value is null)
                    return null;

                value!.Value.Component.WithButton("Delete reminders from this page", $"reminders-deleting:{page},{search}", ButtonStyle.Secondary);

                return value;
            }

            return null;
        }

        [ComponentInteraction("reminders-deleting:*,*")]
        public async Task DeletingRemindersAsync(int page, string search)
        {
            var reminders = await RemindEntity.GetManyAsync(Context.User);

            if (!reminders.Any())
                await UpdateAsync(
                    error: "You have no reminders to delete!");

            if (!string.IsNullOrEmpty(search))
            {
                reminders = reminders.Where(x => x.Message.Contains(search)).ToList();

                if (!reminders.Any())
                    await UpdateAsync(
                        error: "You have no reminders to delete matching your query!");
            }

            else
            {
                var sb = new SelectMenuBuilder()
                    .WithMinValues(1)
                    .WithCustomId("reminders-deleted")
                    .WithPlaceholder("Select 1 or more reminders to delete.");

                int index = page * 10 - 10;

                var range = reminders.GetRange(index, reminders.Count - index);
                for (int i = 0; i < range.Count; i++)
                {
                    if (i == 10)
                        break;
                    sb.AddOption(range[i].Expiration.ToString(), range[i].ObjectId.ToString(), range[i].Message.Reduce(100));
                }

                sb.WithMaxValues(sb.Options.Count);

                var cb = new ComponentBuilder()
                    .WithSelectMenu(sb);

                await UpdateAsync(
                    format: MessageFormat.Deleting,
                    header: "Delete reminders:",
                    context: "Select the reminders you want to delete in the dropdown below.",
                    components: cb);
            }
        }

        [ComponentInteraction("reminders-deleted")]
        public async Task DeletedRemindersAsync(ObjectId[] selectedValues)
        {
            var reminders = await RemindEntity.GetManyAsync(Context.User);

            if (!reminders.Any())
                await UpdateAsync(
                    error: "You have no reminders to delete!");

            else
            {
                foreach (var value in selectedValues)
                {
                    var reminder = reminders.First(x => x.ObjectId == value);

                    if (reminder is not null)
                        await reminder.DeleteAsync();
                }
                await UpdateAsync(
                    format: MessageFormat.Success,
                    header: $"Succesfully removed {selectedValues.Length} reminder(s).");
            }
        }
    }
}
