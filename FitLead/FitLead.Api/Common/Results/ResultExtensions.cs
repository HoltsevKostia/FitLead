using FitLead.Application.Common.Errors;
using FitLead.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;
using AppError = FitLead.Application.Common.Errors.Error;

namespace FitLead.Api.Common.Results
{
    public static class ResultExtensions
    {
        // For commands returning Result (no payload)
        public static IActionResult ToActionResult(this Result result, ControllerBase controller)
        {
            if (result.IsSuccess)
                return controller.NoContent();

            return ToProblem(controller, result.Error);
        }

        // For queries / commands returning payload
        public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
        {
            if (result.IsSuccess)
                return controller.Ok(result.Value);

            return ToProblem(controller, result.Error);
        }

        // For POST create with Location header
        public static IActionResult ToCreatedAtAction<T>(
            this Result<T> result,
            ControllerBase controller,
            string actionName,
            object? routeValues = null)
        {
            if (result.IsSuccess)
                return controller.CreatedAtAction(actionName, routeValues, result.Value);

            return ToProblem(controller, result.Error);
        }

        // For POST create without Location (optional helper)
        public static IActionResult ToCreated<T>(this Result<T> result, ControllerBase controller)
        {
            if (result.IsSuccess)
                return controller.StatusCode(StatusCodes.Status201Created, result.Value);

            return ToProblem(controller, result.Error);
        }

        private static IActionResult ToProblem(ControllerBase controller, AppError? error)
        {
            if (error is null)
            {
                var pd = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Unexpected error",
                    Detail = "Result failure did not contain an error."
                };
                return new ObjectResult(pd) { StatusCode = pd.Status };
            }

            var status = error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

            var problem = new ProblemDetails
            {
                Status = status,
                Title = error.Type.ToString(),
                Detail = error.Message
            };

            problem.Extensions["errorCode"] = error.Code;
            if (error.Metadata is not null)
            {
                foreach (var entry in error.Metadata)
                {
                    if (problem.Extensions.ContainsKey(entry.Key))
                        continue;

                    problem.Extensions[entry.Key] = entry.Value;
                }
            }

            return new ObjectResult(problem) { StatusCode = status };
        }
    }
}
