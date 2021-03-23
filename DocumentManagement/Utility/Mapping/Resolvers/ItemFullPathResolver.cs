using System.Linq;
using AutoMapper;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class ItemFullPathResolver : IValueResolver<Item, ItemExternalDto, string>
    {
        private readonly DMContext dbContext;

        public ItemFullPathResolver(DMContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public string Resolve(Item source, ItemExternalDto destination, string destMember, ResolutionContext context)
        {
            var projectID = source.Project?.ID ?? source.ProjectID ?? source.Objectives.First().Objective.ProjectID;
            var project = dbContext.Projects
                .Where(x => x.ID == projectID)
                .FirstOrDefault();
            return PathHelper.GetFullPath(project.Title, source.RelativePath);
        }
    }
}
