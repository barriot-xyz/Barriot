namespace Barriot.Application.Interactions.Converters
{
    public class PointerConverter : TypeReader<Pointer>
    {
        public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, string option, IServiceProvider services)
        {
            if (Guid.TryParse(option, out Guid id))
                if (Pointer.TryParse(id, out var value))
                    return Task.FromResult(TypeConverterResult.FromSuccess(value));
            return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, "Failed to convert from guid."));
        }
    }
}
