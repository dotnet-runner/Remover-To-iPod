using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;
using SpotifyApiWorker.Exceptions;

namespace SpotifyApiWorker.Middlewares;

public sealed class GlobalExceptionHandlerMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch(Exception ex)
        {
            httpContext.Response.StatusCode = ex switch
            {
                NoAuthorizationCodeException or APIException 
                    => StatusCodes.Status400BadRequest,
                AccessTokenException
                    => StatusCodes.Status401Unauthorized,
                AuthorizationCodeTokenException 
                    => StatusCodes.Status403Forbidden,
                
                _ => StatusCodes.Status500InternalServerError
            };
            
            await httpContext.Response.WriteAsJsonAsync(
                new ProblemDetails
                {
                    Type = ex.GetType().Name,
                    Title = "An error occurred while processing your request!",
                    Detail = ex.Message
                }
                );
        }
    }
}