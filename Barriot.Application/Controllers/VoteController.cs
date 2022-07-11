using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Barriot.Application.Controllers.Args;
using Barriot.Application.Controllers.Builders;
using Barriot.Http;
using Barriot.Http.Json;

namespace Barriot.Application.Controllers
{
    [ApiController]
    [Route("votes")]
    public class VoteController : ControllerBase
    {
        private readonly ILogger<VoteController> _logger;
        private readonly IConfiguration _configuration;

        public VoteController(
            ILogger<VoteController> logger, 
            IConfiguration config)
        {
            _logger = logger;
            _configuration = config;
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync()
        {
            var auth = HttpContext.Request.Headers.Authorization;
            using var sr = new StreamReader(HttpContext.Request.Body);
            var body = await sr.ReadToEndAsync();

            if (auth[0] != _configuration["TGGAuthKey"])
            {
                _logger.LogError("Failure (Invalid authorization header)");

                return new ContentResultBuilder(401)
                    .WithPayload("Failed to authorize source")
                    .Build();
            }

            var result = JsonConvert.DeserializeObject<Vote>(body);

            if (result is null)
            {
                _logger.LogError("Failure (Unable to deserialize vote)");

                return new ContentResultBuilder(400)
                    .WithPayload("Failed to deserialize content")
                    .Build();
            }

            _logger.LogInformation("Success (Received vote for {})", result.UserId);

            var user = await UserEntity.GetAsync(result.UserId);

            var time = DateTime.UtcNow;
            var oldTime = user.LastVotedAt;

            if (oldTime.AddMonths(1).Month == time.Month)
            {
                if (user.MonthlyVotes is not 0)
                {
                    var flags = user.Flags;
                    if (DateTime.DaysInMonth(user.LastVotedAt.Year, user.LastVotedAt.Month) != user.MonthlyVotes)
                    {
                        flags.RemoveAll(x => x.Type is FlagType.TopVoter);
                        user.Flags = flags;
                    }
                    else
                    {
                        if (!user.Flags.Contains(UserFlag.TopVoter))
                        {
                            flags.Add(UserFlag.TopVoter);
                            user.Flags = flags;
                        }
                    }
                }

                user.MonthlyVotes = 1;
            }
            else
                user.MonthlyVotes++;

            user.LastVotedAt = DateTime.UtcNow;
            user.Votes++;

            var bumps = await BumpsEntity.GetAsync(user.UserId);

            bumps.BumpsToGive++;

            return new ContentResultBuilder(200)
                .WithPayload("Received vote")
                .Build();
        }
    }
}
