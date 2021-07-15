using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro.Converters
{
    internal class ObjectiveProjectConverter : IConverter<ObjectiveExternalDto, Project>
    {
        public Task<Project> Convert(ObjectiveExternalDto from)
        {
            throw new NotImplementedException();
        }
    }
}
