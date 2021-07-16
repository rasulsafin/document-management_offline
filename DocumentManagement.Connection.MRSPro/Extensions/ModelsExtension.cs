using System;
using System.Linq;
using MRS.DocumentManagement.Connection.MrsPro.Interfaces;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.MrsPro.Constants;

namespace MRS.DocumentManagement.Connection.MrsPro.Extensions
{
    internal static class ModelsExtension
    {
        internal static ProjectExternalDto ToDto(this Project project)
        {
            return new ProjectExternalDto
            {
                ExternalID = project.Id,
                Title = project.Name,
            };
        }

        internal static string GetExternalId(this IElementObject element)
        {
            var type = element.Type == ISSUE_TYPE ? TASK :
                PROJECT;
            return $"{element.Ancestry}{ID_PATH_SPLITTER}{element.Id}{type}";
        }

        internal static string GetParentProjectId(this IElementObject element)
        {
            // from "/5ebb7cb7782f96000146e7f3:ORGANIZATION/5ebbff9021ccb400017d707b:PROJECT"
            // need "5ebbff9021ccb400017d707b"
            var projectId = element.Ancestry;
            return projectId?
                .Split(ID_PATH_SPLITTER, StringSplitOptions.RemoveEmptyEntries)?
                .ElementAt(1)?
                .Split(ID_SPLITTER)?
                .ElementAt(0);
        }
    }
}
