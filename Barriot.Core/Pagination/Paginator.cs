namespace Barriot.Pagination
{
    public class Paginator<T>
    {
        private const int pageSize = 10;

        private static Paginator<T>? instance;

        private readonly Func<T, PageFieldBuilder> _valueFormatter;
        private readonly string _customId;

        internal Paginator(Func<T, PageFieldBuilder> valueFormatter, string customId)
        {
            _valueFormatter = valueFormatter;
            _customId = customId;

            instance = this;
        }

        /// <summary>
        ///     Attempts to grab a page from the paginator.
        /// </summary>
        /// <param name="pageNumber">The page number to create a page for.</param>
        /// <param name="entries">The entries for all pages.</param>
        /// <returns>A <see cref="Page"/> for the respective <paramref name="pageNumber"/></returns>
        public Page? GetPage(int pageNumber, List<T> entries, string search = "", Func<List<T>, string, List<T>>? query = null)
        {
            bool isSearching = !string.IsNullOrEmpty(search);

            if (isSearching)
            {
                if (query is null)
                    throw new ArgumentNullException(nameof(query), "Cannot be null if search parameter is provided");

                entries = query(entries, search);
            }

            if (!entries.Any())
                return null;

            var maxPages = (int)Math.Ceiling((double)entries.Count / pageSize);

            if (pageNumber > maxPages)
                pageNumber = maxPages;

            var index = (pageNumber * pageSize) - pageSize;

            var toGather = pageSize;
            if (index + pageSize >= entries.Count)
                toGather = entries.Count - index;

            var eb = new EmbedBuilder();

            var range = entries.GetRange(index, toGather);

            foreach (var entry in range)
            {
                var formatter = _valueFormatter(entry);
                eb.AddField(formatter.Title, formatter.Value, formatter.DoInline);
            }

            var cid = _customId + ":";

            if (isSearching)
                cid += (search + ",");

            var cb = new ComponentBuilder()
                .WithButton(
                    label: "Previous page",
                    customId: cid + $"{pageNumber - 1}",
                    style: ButtonStyle.Danger,
                    disabled: pageNumber <= 1)
                .WithButton(
                    label: "Next page",
                    customId: cid + $"{pageNumber + 1}",
                    style: ButtonStyle.Primary,
                    disabled: pageNumber >= maxPages);

            eb.WithFooter($"Page {pageNumber}/{maxPages}");
            
            return new Page(eb, cb); // continue
        }

        /// <summary>
        ///     Checks to see if a paginator exists for the specific generic paginator.
        /// </summary>
        /// <returns></returns>
        public static bool Exists()
            => instance is not null;

        /// <summary>
        ///     Tries to get a paginator from it's generic parameter. 
        /// </summary>
        /// <param name="paginator"></param>
        /// <returns></returns>
        public static bool TryGet(out Paginator<T> paginator)
        {
            if (instance is not null)
            {
                paginator = instance;
                return true;
            }
            else
            {
                paginator = null!;
                return false;
            }
        }
    }
}
