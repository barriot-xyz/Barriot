using Barriot.Application.Interactions.Converters;
using MongoDB.Bson;
using Transference;
using Transference.Discord;

namespace Barriot.Application.Interactions
{
    public static class InteractionExtensions
    {
        public static async Task ConfigureInteractionsAsync(WebApplication app)
        {
            var service = app.Services.GetRequiredService<InteractionService>();

            service.AddTypeConverter<ulong>(new UlongConverter());
            service.AddTypeConverter<TimeSpan>(new TimeSpanConverter());
            service.AddTypeConverter<Calculation>(new CalculationConverter());

            service.AddComponentTypeConverter<TimeSpan>(new TimeSpanComponentConverter());
            service.AddComponentTypeConverter<Color>(new ColorComponentConverter());

            service.AddTypeReader<ObjectId>(new ObjectIdComponentConverter());
            service.AddTypeReader<Guid>(new Converters.GuidConverter());

            service.AddGenericTypeReader(typeof(Pointer<>), typeof(UlongPointerReader<>));

            await service.AddModulesAsync(typeof(Program).Assembly, app.Services);

            if (app.Configuration.GetValue<bool>("BuildCommands"))
                await service.RegisterCommandsGloballyAsync();
        }
    }
}
