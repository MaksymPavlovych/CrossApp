using Core;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api;

public sealed class ApiExceptionHandler(IProblemDetailsService problems) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            DomainException => (StatusCodes.Status409Conflict, "Порушено бізнес-правило."),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Некоректний запит."),
            _ => (0, "")
        };

        if (status == 0) return false; // інші винятки піде обробляти стандартний обробник (500)

        context.Response.StatusCode = status;
        return await problems.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = exception is DomainException ? exception.Message : null
            }
        });
    }
}
