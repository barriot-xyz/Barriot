namespace Barriot.Application.Services.Entities
{
    public class ReminderService : IConfigurableService
    {
        private readonly System.Timers.Timer _timer;
        private readonly DiscordRestClient _client;

        public ReminderService(DiscordRestClient client)
        {
            _client = client;
            _timer = new(5000)
            {
                AutoReset = true,
                Enabled = true,
            };
        }

        public async Task ConfigureAsync()
        {
            _timer.Elapsed += async (_, x) => await OnElapsedAsync(x);
            _timer.Start();
            await Task.CompletedTask;
        }

        private async Task OnElapsedAsync(System.Timers.ElapsedEventArgs _)
        {
            var reminders = await RemindEntity.GetManyAsync(DateTime.UtcNow);

            foreach (var reminder in reminders) // comes in at 1
            {
                reminder.Frequency--; // decrease frequency, now 0
                reminder.Expiration += reminder.SpanToRepeat; // irrelevant to later check.

                var user = await _client.GetUserAsync(reminder.UserId);

                try
                {
                    await user.SendMessageAsync($":alarm_clock: **Reminder!**\n\n> {reminder.Message}");
                }
                catch { }

                if (reminder.Frequency <= 0) // if 0 or lower, delete.
                    await reminder.DeleteAsync();
            }
        }
    }
}
