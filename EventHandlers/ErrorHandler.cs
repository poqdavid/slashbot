namespace SlashBot.EventHandlers;

public class ErrorHandler : IClientErrorHandler
{
    public ValueTask HandleEventHandlerError(string name, Exception exception, Delegate invokedDelegate, object sender, object args)
    {
        if (exception is BadRequestException badRequest)
        {
            Log.Error
            (
                "Event handler exception for event {Event} thrown from {Method} (defined in {DeclaryingType}):\n" +
                "A request was rejected by the Discord API.\n" +
                "  Errors: {Errors}\n" +
                "  Message: {JsonMessage}\n" +
                "  Stack trace: {Stacktrace}",
                name,
                invokedDelegate.Method,
                invokedDelegate.Method.DeclaringType,
                badRequest.Errors,
                badRequest.JsonMessage,
                badRequest.StackTrace
            );

            return ValueTask.CompletedTask;
        }

        Log.Error(exception, "Event handler exception for event {Event} thrown from {Method} (defined in {DeclaryingType}).",
            name, invokedDelegate.Method, invokedDelegate.Method.DeclaringType);

        return ValueTask.CompletedTask;
    }

    public ValueTask HandleGatewayError(Exception exception)
    {
        Log.Error(exception, "An error occurred in the DSharpPlus gateway.");

        return ValueTask.CompletedTask;
    }
}