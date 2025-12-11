using GuessifyBackend.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace GuessifyBackend.Handlers
{
    public class ErrorHandlingLobbyHubFilter : IHubFilter
    {

        private readonly ILogger<GlobalExceptionHandler> _logger;


        public ErrorHandlingLobbyHubFilter(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }



        public async ValueTask<object?> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object?>> next)
        {
            var hub = invocationContext.Hub as LobbyHub;
            try
            {
                return await next(invocationContext);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred in LobbyHub.");
                if (hub != null)
                {
                    await hub.Clients.Caller.ExceptionThrown(ex.Message);
                }
                throw;
            }


        }
    }
}
