using Barriot.Application.Interactions.Attributes;
using Barriot.Extensions;
using Barriot.Models.Files;
using Barriot.Pagination;
using System.Diagnostics;

namespace Barriot.Application.Interactions
{
    /// <summary>
    ///     Represents a non-generic Barriot module base that implements <see cref="BarriotInteractionContext"/>.
    /// </summary>
    public class BarriotModuleBase : RestInteractionModuleBase<BarriotInteractionContext>
    {
        /// <summary>
        ///     A logger accessible from the interaction source to send log messages to the console interface.
        /// </summary>
        protected ILogger<BarriotModuleBase> Logger { get; }

        public BarriotModuleBase(ILogger<BarriotModuleBase> logger)
        {
            Logger = logger;
        }

        #region Execution

        private static int CalculateTier(long currentPoints, ref int ranking)
        {
            int tier = 0;
            while (currentPoints >= ranking)
            {
                ranking *= 2;
                tier++;
            }
            ranking /= 2;
            return tier;
        }

        private bool CanAssignFlag(UserFlag newFlag)
        {
            List<UserFlag> flags = Context.Member.Flags;
            UserFlag[] newFlags = { newFlag };

            if (!flags.Any(x => x.Title == newFlag.Title))
            {
                flags.RemoveAll(x => x.Type == newFlag.Type);
                Context.Member.Flags = new(flags.Concat(newFlags));
                return true;
            }
            return false;
        }

        private bool _hasWonGame = false;

        public void WonGame()
        {
            if (_hasWonGame)
                throw new InvalidOperationException("A user cannot win a game twice in a row.");
            _hasWonGame = true;
        }

        public override async Task AfterExecuteAsync(ICommandInfo command)
        {
            if (_hasWonGame)
            {
                Context.Member.GamesWon++;

                int ranking = 10;
                int tier = CalculateTier(Context.Member.GamesWon, ref ranking);

                if (tier is not (0 or > 10) && CanAssignFlag(UserFlag.CreateChampion(tier, ranking)))
                    await FollowupAsync(
                        text: $":star: **Congratulations!** *You have won over ` {ranking} ` challenges and have been granted a new acknowledgement!*" +
                        $"\n\n> You can find your acknowledgements by executing ` /statistics `",
                        ephemeral: true);
            }

            if (command is ComponentCommandInfo or ModalCommandInfo)
            {
                Context.Member.ButtonsPressed++;

                int ranking = 300;
                int tier = CalculateTier(Context.Member.ButtonsPressed, ref ranking);

                if (tier is not (0 or > 10) && CanAssignFlag(UserFlag.CreateComponent(tier, ranking)))
                    await FollowupAsync(
                        text: $":star: **Congratulations!** *You have pressed over ` {ranking} ` buttons and have been granted a new acknowledgement!*" +
                        $"\n\n> You can find your acknowledgements by executing ` /statistics `",
                        ephemeral: true);
            }

            else
            {
                if (Context.Member.Inbox.Any() && command.Name != "inbox")
                    await FollowupAsync(
                        text: ":speech_balloon: **You have unread mail!** Please use ` /inbox ` to read this mail.",
                        ephemeral: true);

                Context.Member.LastCommand = command.Name;
                Context.Member.CommandsExecuted++;

                int ranking = 150;
                int tier = CalculateTier(Context.Member.CommandsExecuted, ref ranking);

                if (tier is not (0 or > 10) && CanAssignFlag(UserFlag.CreateCommand(tier, ranking)))
                    await FollowupAsync(
                        text: $":star: **Congratulations!** *You have executed over ` {ranking} ` commands and been granted a new acknowledgement!*" +
                        $"\n\n> You can find your acknowledgements by executing ` /statistics `",
                        ephemeral: true);
            }

            if (Context.Member.UserName is "Unknown")
                Context.Member.UserName = $"{Context.User.Username}#{Context.User.Discriminator}";
        }

        #endregion

        #region DeferAsync

        /// <summary>
        ///     Defers a loading setter to the current <see cref="ComponentInteractionAttribute"/> marked method sources from.
        /// </summary>
        /// <param name="ephemeral"></param>
        /// <returns>An asynchronous <see cref="Task"/> with no return type.</returns>
        /// <exception cref="InvalidCastException">Thrown if this method is called on an unsupported type of interaction.</exception>
        public async Task DeferLoadingAsync(bool ephemeral = false)
        {
            if (Context.Interaction is not RestMessageComponent component)
                throw new InvalidCastException($"{nameof(DeferLoadingAsync)} can only be executed for a {nameof(RestMessageComponent)}");

            var payload = component.DeferLoading(ephemeral);

            await Context.InteractionResponseCallback(payload);
        }

        #endregion

        #region UpdateAsync

        /// <summary>
        ///     Updates the interaction the current <see cref="ComponentInteractionAttribute"/> marked method sources from.
        /// </summary>
        /// <param name="text">The text to update to.</param>
        /// <param name="components">The components to update to.</param>
        /// <param name="embed">The embed to update to.</param>
        /// <returns>An asynchronous <see cref="Task"/> with no return type.</returns>
        /// <exception cref="InvalidCastException">Thrown if this method is called on an unsupported type of interaction.</exception>
        public async Task UpdateAsync(string? text = null, ComponentBuilder? components = null, EmbedBuilder? embed = null)
        {
            if (Context.Interaction is not RestMessageComponent component)
                throw new InvalidCastException($"{nameof(UpdateAsync)} can only be executed for a {nameof(RestMessageComponent)}");

            var payload = component.Update(x =>
            {
                if (text is not null)
                    x.Content = text;

                if (components is not null)
                    x.Components = components.Build();
                else
                    x.Components = new ComponentBuilder().Build();

                if (embed is not null)
                {
                    var footerText = $"Brought to you by Rozen.";
                    if (embed.Footer is not null)
                    {
                        embed.Footer.Text += " | " + footerText;
                    }
                    else
                        embed.WithFooter(footerText);

                    if (embed.Color is null)
                        embed.WithColor(new Color(Context.Member.Color));

                    x.Embed = embed.Build();
                }
                else
                    x.Embed = null;
            });

            await Context.InteractionResponseCallback(payload);
        }

        /// <summary>
        ///     Updates the interaction the current <see cref="ComponentInteractionAttribute"/> marked method sources from.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="header"></param>
        /// <param name="context"></param>
        /// <param name="description"></param>
        /// <param name="components">The components to update to.</param>
        /// <param name="embed">The embed to update to.</param>
        /// <returns>An asynchronous <see cref="Task"/> with no return type.</returns>
        /// <exception cref="InvalidCastException">Thrown if this method is called on an unsupported type of interaction.</exception>
        public async Task UpdateAsync(MessageFormat format, string header, string? context = null, string? description = null, ComponentBuilder? components = null, EmbedBuilder? embed = null)
        {
            var tb = new TextBuilder()
                .WithResult(format)
                .WithHeader(header)
                .WithDescription(description)
                .WithContext(context);

            await UpdateAsync(
                text: tb.Build(),
                components: components,
                embed: embed);
        }

        /// <summary>
        ///     Updates the interaction the current <see cref="ComponentInteractionAttribute"/> marked method sources from with an error.
        /// </summary>
        /// <param name="error">The error message to send.</param>
        /// <param name="context"></param>
        /// <param name="description"></param>
        /// <returns>An asynchronous <see cref="Task"/> with no return type.</returns>
        /// <exception cref="InvalidCastException">Thrown if this method is called on an unsupported type of interaction.</exception>
        public async Task UpdateAsync(string error, string? context = null, string? description = null)
        {
            var tb = new TextBuilder()
                .WithResult(MessageFormat.Failure)
                .WithHeader(error)
                .WithContext(context)
                .WithDescription(description);

            await UpdateAsync(
                text: tb.Build());
        }

        /// <summary>
        ///     Updates the interaction the current <see cref="ComponentInteractionAttribute"/> marked method sources from with an error.
        /// </summary>
        /// <param name="error"></param>
        /// <param name="parameter"></param>
        /// <returns>An asynchronous <see cref="Task"/> with no return type.</returns>
        /// <exception cref="InvalidCastException">Thrown if this method is called on an unsupported type of interaction.</exception>
        public async Task UpdateAsync(ErrorInfo error, string parameter = "")
        {
            var text = FileExtensions.GetError(error, parameter);

            await UpdateAsync(
                text: text);
        }

        /// <summary>
        ///     Updates the interaction the current <see cref="ComponentInteractionAttribute"/> marked method sources from with a page.
        /// </summary>
        /// <param name="page">The page to send.</param>
        /// <param name="context"></param>
        /// <param name="description"></param>
        /// <returns>An asynchronous <see cref="Task"/> with no return type.</returns>
        public async Task UpdateAsync(Page page, string header, string? context = null)
        {
            var tb = new TextBuilder()
                .WithResult(MessageFormat.List)
                .WithHeader(header)
                .WithContext(context);

            await UpdateAsync(
                text: tb.Build(),
                components: page.Component,
                embed: page.Embed);
        }

        #endregion

        #region RespondAsync

        public async Task RespondAsync(string text, ComponentBuilder? components = null, EmbedBuilder? embed = null, bool ephemeral = false)
        {
            if (embed is not null)
            {
                var footerText = $"Brought to you by Rozen.";

                if (embed.Footer is not null)
                {
                    embed.Footer.Text += " | " + footerText;
                }
                else
                    embed.WithFooter(footerText);

                if (embed.Color is null)
                    embed.WithColor(new Color(Context.Member.Color));
            }

            await base.RespondAsync(
                text: text,
                components: components?.Build(),
                embed: embed?.Build(),
                ephemeral: ephemeral);
        }

        /// <summary>
        ///     Responds to the current <see cref="RestInteraction"/>.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="header"></param>
        /// <param name="context"></param>
        /// <param name="description"></param>
        /// <param name="components">The components to send.</param>
        /// <param name="embed">The embed to send.</param>
        /// <param name="ephemeral">If the message should be ephemerally sent.</param>
        /// <returns>An asynchronous <see cref="Task"/> with no return type.</returns>
        public async Task RespondAsync(MessageFormat format, string header, string? context = null, string? description = null, ComponentBuilder? components = null, EmbedBuilder? embed = null, bool? ephemeral = null)
        {
            var tb = new TextBuilder()
                .WithResult(format)
                .WithHeader(header)
                .WithDescription(description)
                .WithContext(context);

            if (format == MessageFormat.Failure || format == MessageFormat.List)
                ephemeral = true;

            else
                ephemeral ??= Context.Member.DoEphemeral;

            await RespondAsync(
                text: tb.Build(),
                components: components,
                embed: embed,
                ephemeral: ephemeral.Value);
        }

        /// <summary>
        ///     Responds to the current <see cref="RestInteraction"/> with an error.
        /// </summary>
        /// <param name="error">The error to send.</param>
        /// <param name="context">Error context if applicable.</param>
        /// <param name="description"></param>
        /// <returns>An asynchronous <see cref="Task"/> with no return type.</returns>
        public async Task RespondAsync(string error, string? context = null, string? description = null)
        {
            var tb = new TextBuilder()
                .WithResult(MessageFormat.Failure)
                .WithHeader(error)
                .WithContext(context)
                .WithDescription(description);

            await base.RespondAsync(
                text: tb.Build(),
                ephemeral: true);
        }

        /// <summary>
        ///     Responds to the current <see cref="RestInteraction"/> with an error.
        /// </summary>
        /// <param name="error"></param>
        /// <param name="parameter"></param>
        /// <returns>An asynchronous <see cref="Task"/> with no return type.</returns>
        public async Task RespondAsync(ErrorInfo error, string parameter = "")
        {
            var text = FileExtensions.GetError(error, parameter);

            await RespondAsync(
                text: text);
        }

        /// <summary>
        ///     Responds to the current <see cref="RestInteraction"/> with a page.
        /// </summary>
        /// <param name="page">The page to send.</param>
        /// <param name="header"></param>
        /// <param name="context"></param>
        /// <returns>An asynchronous <see cref="Task"/> with no return type.</returns>
        public async Task RespondAsync(Page page, string header, string? context = null)
        {
            var tb = new TextBuilder()
                .WithResult(MessageFormat.List)
                .WithHeader(header)
                .WithContext(context);

            await RespondAsync(
                text: tb.Build(),
                components: page.Component,
                embed: page.Embed,
                ephemeral: true);
        }

        #endregion

        #region FollowupAsync

        /// <summary>
        ///     Follows up to the current <see cref="RestInteraction"/>.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="header"></param>
        /// <param name="context"></param>
        /// <param name="description"></param>
        /// <param name="components">The components to send.</param>
        /// <param name="embed">The embed to send.</param>
        /// <returns>An asynchronous <see cref="Task"/> with no return type.</returns>
        public async Task FollowupAsync(MessageFormat format, string header, string? context = null, string? description = null, ComponentBuilder? components = null, EmbedBuilder? embed = null)
        {
            if (embed is not null)
            {
                var footerText = $"Brought to you by Rozen.";
                if (embed.Footer is not null)
                {
                    embed.Footer.Text += " | " + footerText;
                }
                else
                    embed.WithFooter(footerText);

                if (embed.Color is null)
                    embed.WithColor(new Color(Context.Member.Color));
            }

            var tb = new TextBuilder()
                .WithResult(format)
                .WithHeader(header)
                .WithDescription(description)
                .WithContext(context);

            await base.FollowupAsync(
                text: tb.Build(),
                components: components?.Build(),
                embed: embed?.Build());
        }

        /// <summary>
        ///     Follows up to the current <see cref="RestInteraction"/> with an error.
        /// </summary>
        /// <param name="error">The error to send.</param>
        /// <param name="context"></param>
        /// <param name="description"></param>
        /// <returns>An asynchronous <see cref="Task"/> with no return type.</returns>
        public async Task FollowupAsync(string error, string? context = null, string? description = null)
        {
            var tb = new TextBuilder()
                .WithResult(MessageFormat.Failure)
                .WithHeader(error)
                .WithContext(context)
                .WithDescription(description);

            await base.FollowupAsync(
                text: tb.Build());
        }

        /// <summary>
        ///     Follows up to the current <see cref="RestInteraction"/> with an error.
        /// </summary>
        /// <param name="error"></param>
        /// <param name="parameter"></param>
        /// <returns>An asynchronous <see cref="Task"/> with no return type.</returns>
        public async Task FollowupAsync(ErrorInfo error, string parameter = "")
        {
            var text = FileExtensions.GetError(error, parameter);

            await FollowupAsync(
                text: text);
        }

        #endregion
    }
}
