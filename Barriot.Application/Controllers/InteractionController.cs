using Barriot.Application.Interactions;
using Microsoft.AspNetCore.Mvc;

namespace Barriot.Application.Controllers
{
    [ApiController]
    [Route("interactions")]
    public class InteractionController : ControllerBase
    {
        const string _contentType = "application/json";

        private readonly ILogger<InteractionController> _logger;
        private readonly DiscordRestClient _client;
        private readonly InteractionService _service;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly InteractionApiManager _apiManager;
        private readonly PostExecutionManager _postExecManager;

        public InteractionController(
            DiscordRestClient client, 
            InteractionService service, 
            ILogger<InteractionController> logger,
            IServiceProvider provider,
            IConfiguration config,
            InteractionApiManager apiManager,
            PostExecutionManager postExecManager)
        {
            _logger = logger;
            _client = client;
            _service = service;
            _serviceProvider = provider;
            _configuration = config;
            _apiManager = apiManager;
            _postExecManager = postExecManager;
        }

        [HttpGet]
        public IActionResult GetAsync()
            => Ok(Content("Interaction endpoint available.", _contentType));

        [HttpPost]
        public async Task PostAsync()
        {
            async Task ReturnAsync(int statusCode, string payload)
            {
                HttpContext.Response.StatusCode = statusCode;
                HttpContext.Response.ContentType = "application/json";
                await HttpContext.Response.WriteAsync(payload).ConfigureAwait(false);
                await HttpContext.Response.CompleteAsync().ConfigureAwait(false);
            }

            var signature = HttpContext.Request.Headers["X-Signature-Ed25519"];
            var timestamp = HttpContext.Request.Headers["X-Signature-Timestamp"];
            using var sr = new StreamReader(HttpContext.Request.Body);
            var body = await sr.ReadToEndAsync();

            if (!_client.IsValidHttpInteraction(_configuration["PublicToken"], signature, timestamp, body))
            {
                _logger.LogError("Failure (Invalid interaction signature)");
                await ReturnAsync(401, "Failed to verify interaction!");
            }

            RestInteraction interaction = await _client.ParseHttpInteractionAsync(_configuration["PublicToken"], signature, timestamp, body, _apiManager.Predicate);

            if (interaction is RestPingInteraction pingInteraction)
            {
                _logger.LogInformation("Successful (Ping)");
                await ReturnAsync(200, pingInteraction.AcknowledgePing());
            }

            var context = new BarriotInteractionContext(
                member: await UserEntity.GetAsync(interaction.User.Id),
                client: _client, 
                interaction: interaction, 
                responseCallback: async (str) => await ReturnAsync(200, str));

            var result = await _service.ExecuteCommandAsync(context, _serviceProvider);

            await _postExecManager.RunAsync(result, context);
        }
    }
}