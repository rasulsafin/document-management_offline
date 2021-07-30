using System;
using System.Linq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility.Mapping.Converters
{
    public class BimElementObjectiveTypeConverter : ITypeConverter<BimElementExternalDto, BimElementObjective>
    {
        private readonly DMContext dbContext;
        private readonly ILogger<BimElementObjectiveTypeConverter> logger;

        public BimElementObjectiveTypeConverter(DMContext dbContext, ILogger<BimElementObjectiveTypeConverter> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            logger.LogTrace("BimElementObjectiveTypeConverter created");
        }

        public BimElementObjective Convert(
            BimElementExternalDto source,
            BimElementObjective destination,
            ResolutionContext context)
        {
            logger.LogTrace(
                "Convert started with source: {@Source} & destination: {@Destination}",
                source,
                destination);
            var exist = dbContext.BimElements
               .FirstOrDefault(x => x.ParentName == source.ParentName && x.GlobalID == source.GlobalID);
            logger.LogDebug("Found bim element: {@BimElement}", exist);

            return new BimElementObjective
            {
                BimElement = exist ?? new BimElement
                {
                    ParentName = source.ParentName,
                    GlobalID = source.GlobalID,
                },
            };
        }
    }
}