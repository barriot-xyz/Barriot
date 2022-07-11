namespace Barriot.Application.Services.Entities
{
    public class InboxService : IConfigurableService
    {
        public InboxService()
        {

        }

        public async Task ConfigureAsync()
        {
            await PopulateInboxAsync();
        }

        private static async Task PopulateInboxAsync()
        {
            List<string> messages = new();
            foreach (var file in Directory.GetFiles(Path.Combine("Files", "Messages")))
            {
                messages.Add(string.Join("\n", File.ReadAllLines(file)));

                File.Delete(file);
            }

            if (!messages.Any())
                return;

            var users = await UserEntity.GetAllAsync();

            foreach (var user in users)
                user.Inbox = new(user.Inbox.Concat(messages));
        }
    }
}
