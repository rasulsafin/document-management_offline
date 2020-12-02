using Microsoft.AspNetCore.Mvc;
using MRS.DocumentManagement.Interface;
using System.Collections.Generic;
using System.Linq;

namespace DocumentManagement.Api.Validators
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
            if (collection.Count() == 0)
                return new NotFoundResult();

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
    }
}
