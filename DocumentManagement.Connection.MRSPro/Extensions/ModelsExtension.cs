using System;
using System.Linq;
using MRS.DocumentManagement.Connection.MrsPro.Interfaces;
using static MRS.DocumentManagement.Connection.MrsPro.Constants;

namespace MRS.DocumentManagement.Connection.MrsPro.Extensions
{
    internal static class ModelsExtension
    {
        internal static string GetExternalId(this IElement element)
        {
            return $"{element.Ancestry}{ID_PATH_SPLITTER}{element.Id}{ID_SPLITTER}{element.Type.ToUpper()}";
        }

        internal static string GetParentProjectId(this IElementObject element)
        {
            // from "/5ebb7cb7782f96000146e7f3:ORGANIZATION/5ebbff9021ccb400017d707b:PROJECT"
            // need "5ebbff9021ccb400017d707b"
            var projectId = element.Ancestry;
            var splitId = projectId?
                .Split(ID_PATH_SPLITTER, StringSplitOptions.RemoveEmptyEntries);

            if (splitId == null || splitId.Length == 0)
                return null;

            var parentProject = splitId?.Length > 1 ? splitId[1] : splitId[0];
            return parentProject
                .Split(ID_SPLITTER)?
                .ElementAt(0);
        }
    }
}
