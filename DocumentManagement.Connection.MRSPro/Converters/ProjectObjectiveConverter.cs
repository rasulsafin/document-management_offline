using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.MrsPro.Converters
{
    internal class ProjectObjectiveConverter : IConverter<Project, ObjectiveExternalDto>
    {
        public Task<ObjectiveExternalDto> Convert(Project from)
        {
            throw new NotImplementedException();
        }
    }
}
