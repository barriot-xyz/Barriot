namespace Barriot.Application.Services.Entities
{
    public class GameService : IConfigurableService
    {
        private readonly System.Timers.Timer _timer;

        public GameService()
        {
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

        private static async Task OnElapsedAsync(System.Timers.ElapsedEventArgs _)
        {
            await TicTacToeEntity.DeleteManyAsync();
            await ConnectEntity.DeleteManyAsync();
        }
    }
}