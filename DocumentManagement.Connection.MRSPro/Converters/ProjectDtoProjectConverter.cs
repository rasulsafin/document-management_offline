﻿using Brio.Docs.Connections.MrsPro.Models;
using Brio.Docs.Interface;
using Brio.Docs.Interface.Dtos;
using System;
using System.Threading.Tasks;

namespace Brio.Docs.Connections.MrsPro.Converters
{
    public class ProjectDtoProjectConverter : IConverter<ProjectExternalDto, Project>
    {
        public async Task<Project> Convert(ProjectExternalDto from)
        {
            throw new NotImplementedException();
        }
    }
}
