using GuessifyBackend.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace GuessifyBackend.Handlers
{
    public class ErrorHandlingGameHubFilter : IHubFilter
    {

        private readonly ILogger<GlobalExceptionHandler> _logger;


        public ErrorHandlingGameHubFilter(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }



        public async ValueTask<object?> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object?>> next)
        {
            var hub = invocationContext.Hub as GameHub;
            try
            {
                return await next(invocationContext);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred in GameHub.");
                if (hub != null)
                {
                    await hub.Clients.Caller.ExceptionThrown(ex.Message);
                }
                throw;
            }


        }
    }
}
