using System;
using System.Linq;
using AutoMapper;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class BimElementObjectiveTypeConverter : ITypeConverter<BimElementExternalDto, BimElementObjective>
    {
        private readonly DMContext dbContext;

        public BimElementObjectiveTypeConverter(DMContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public BimElementObjective Convert(
            BimElementExternalDto source,
            BimElementObjective destination,
            ResolutionContext context)
        {
            var exist = dbContext.BimElements
               .FirstOrDefault(x => x.ParentName == source.ParentName && x.GlobalID == source.GlobalID);

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
