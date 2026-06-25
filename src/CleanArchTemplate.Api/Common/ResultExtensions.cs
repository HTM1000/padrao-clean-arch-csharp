using CleanArchTemplate.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchTemplate.Api.Common;

/// <summary>Converte <see cref="Result"/> em <see cref="IActionResult"/> mapeando o tipo de erro para HTTP.</summary>
public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result, ControllerBase controller)
    {
        ArgumentNullException.ThrowIfNull(controller);
        return result.IsSuccess ? controller.NoContent() : Problem(result.Error, controller);
    }

    public static IActionResult ToActionResult<TValue>(this Result<TValue> result, ControllerBase controller)
    {
        ArgumentNullException.ThrowIfNull(controller);
        return result.IsSuccess ? controller.Ok(result.Value) : Problem(result.Error, controller);
    }

    private static ObjectResult Problem(Error error, ControllerBase controller)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        };

        return controller.Problem(detail: error.Message, statusCode: statusCode, title: error.Code);
    }
}
