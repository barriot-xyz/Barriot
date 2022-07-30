using System.Text.RegularExpressions;

namespace Barriot.Extensions
{
    public static class StringExtensions
    {
        private static readonly Regex _regex = new(
            @"(?<Prelink>\S+\s+\S*)?(?<OpenBrace><)?https?:\/\/(?:(?:ptb|canary)\.)?discord(?:app)?\.com\/channels\/(?<Location>\d+|@me)\/(?<ChannelId>\d+)\/(?<MessageId>\d+)\/?(?<CloseBrace>>)?(?<Postlink>\S*\s+\S+)?",
            RegexOptions.Compiled);

        /// <summary>
        ///     Reduces the length of the <paramref name="input"/> and appends the <paramref name="finalizer"/> to humanize the returned string.
        /// </summary>
        /// <remarks>
        ///     Returns the string unchanged if the length is less or equal to <paramref name="maxLength"/>.
        /// </remarks>
        /// <param name="input">The input string to reduce the length of.</param>
        /// <param name="maxLength">The max length the input string is allowed to be.</param>
        /// <param name="killAtWhitespace">Wether to kill the string at whitespace instead of cutting off at a word.</param>
        /// <param name="finalizer">The finalizer to humanize this string with.</param>
        /// <returns>The input string reduced to fit the length set by <paramref name="maxLength"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the length of maxlength is below 0 after the finalizer has been reduced from it.</exception>
        public static string Reduce(this string input, int maxLength, bool killAtWhitespace = false, string finalizer = "...")
        {
            if (input is null)
                return string.Empty + finalizer;

            if (input.Length > maxLength)
            {
                maxLength -= (finalizer.Length + 1); // reduce the length of the finalizer + a single integer to convert to valid range.

                if (maxLength < 1)
                    throw new ArgumentOutOfRangeException(nameof(maxLength));

                if (killAtWhitespace)
                {
                    var range = input.Split(' ');
                    for (int i = 2; input.Length + finalizer.Length > maxLength; i++) // set i as 2, 1 for index reduction, 1 for initial word removal, then increment.
                        input = string.Join(' ', range[..(range.Length - i)]);

                    input += finalizer;
                }
                return input[..maxLength] + finalizer;
            }
            else return input;
        }

        /// <summary>
        ///     Create a jump URL from the provided message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="isDm"></param>
        /// <param name="channelId"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public static string GetJumpUrl(this IMessage message, bool isDm, ulong? channelId, ulong? guildId = null)
            => $"https://discord.com/channels/{(isDm ? "@me" : $"{guildId}")}/{channelId}/{message.Id}";

        /// <summary>
        ///     Checks if the provided <paramref name="messageUrl"/> is a valid jump url.
        /// </summary>
        /// <param name="messageUrl"></param>
        /// <returns></returns>
        public static bool IsJumpUrl(this string messageUrl)
            => _regex.IsMatch(messageUrl);

        /// <summary>
        ///     Attempts to fetch a jump url from the provided message url.
        /// </summary>
        /// <param name="messageUrl"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool TryGetUrlData(this string messageUrl, out List<ulong> data)
        {
            data = new();

            if (!messageUrl.IsJumpUrl())
                return false;

            var matches = _regex.Matches(messageUrl);

            foreach (Match match in matches)
                foreach (Group group in match.Groups)
                    switch (group.Name)
                    {
                        case "Location":
                            if (ulong.TryParse(group.Value, out var ul))
                                data.Add(ul); // guild id
                            else data.Add(0); // dm
                            break;
                        case "ChannelId":
                            var channelId = ulong.Parse(group.Value);

                            data.Add(channelId);
                            break;
                        case "MessageId":
                            var messageId = ulong.Parse(group.Value);

                            data.Add(messageId);
                            break;
                    }

            return true;
        }
    }
}
