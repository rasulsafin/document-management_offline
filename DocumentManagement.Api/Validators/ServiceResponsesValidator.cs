using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace MRS.DocumentManagement.Api.Validators
{
    internal static class ServiceResponsesValidator
    {
        internal static IActionResult ValidateId<T>(ID<T> id)
        {
            if (id == ID<T>.InvalidID)
                return new BadRequestResult();

            return new OkObjectResult(id);
        }

        internal static IActionResult ValidateCollection<T>(IEnumerable<T> collection)
        {
            return new OkObjectResult(collection);
        }

        internal static IActionResult ValidateFoundObject<T>(T foundObject)
        {
            if (foundObject == null)
                return new NotFoundResult();

            return new OkObjectResult(foundObject);
        }

        internal static IActionResult ValidateFoundRelatedResult(bool result)
        {
            if (result)
                return new OkObjectResult(result);
            else
                return new NotFoundObjectResult(result);
        }

        /// <summary>
        /// Generates problem report response as of <see href="https://tools.ietf.org/html/rfc7807"/>.
        /// </summary>
        /// <param name="controller">Controller instance.</param>
        /// <param name="statusCode">The HTTP status code generated for this occurrence of the problem.</param>
        /// <param name="problemType">A short, human-readable summary of the problem type.
        /// It SHOULD NOT change from occurrence to occurrence of the problem, except for purposes of localization.</param>
        /// <param name="details">A human-readable explanation specific to this occurrence of the problem.</param>
        /// <returns>The created ObjectResult for the response.</returns>
        internal static IActionResult CreateProblemResult(ControllerBase controller, int statusCode, string problemType, string details)
            => controller.Problem(detail: details, statusCode: statusCode, title: problemType, type: "about:blank");
    }
}
