namespace Barriot.Application.Interactions
{
    /// <summary>
    ///     The context for a Barriot interaction, including the user entity that invoked it.
    /// </summary>
    public sealed class BarriotInteractionContext : RestInteractionContext
    {
        /// <summary>
        ///     The Barriot user entity for this interaction.
        /// </summary>
        public UserEntity Member { get; }

        /// <summary>
        ///     Creates a new context for upcoming interaction handlers to make use of.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="interaction"></param>
        /// <param name="responseCallback"></param>
        internal BarriotInteractionContext(UserEntity member, DiscordRestClient client, RestInteraction interaction, Func<string, Task> responseCallback)
            : base(client, interaction, responseCallback)
        {
            Member = member;
        }
    }
}
