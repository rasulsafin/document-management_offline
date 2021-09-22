using System;
using System.Linq;
using AutoMapper;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Client.Dtos;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Utility.Mapping.Converters
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
