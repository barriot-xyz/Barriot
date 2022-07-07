using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barriot.Extensions
{
    public static class DiscordRestExtensions
    {
        private static RestApplication? _application;

        /// <summary>
        ///     Checks if the provided user is the owner of the application.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static async Task<bool> IsApplicationOwnerAsync(this DiscordRestClient client, ulong userId)
        {
            if (_application is null)
                _application = await client.GetApplicationInfoAsync();

            if (_application.Owner.Id == userId)
                return true;
            return false;
        }
    }
}
