using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Utils
{
    public class ProjectsMapper : IConverter<IReadOnlyCollection<ProjectExternalDto>, IReadOnlyCollection<Project>>
    {
        private readonly ILogger<ProjectsMapper> logger;
        private readonly IMapper mapper;

        public ProjectsMapper(ILogger<ProjectsMapper> logger, IMapper mapper)
        {
            this.logger = logger;
            this.mapper = mapper;
        }

        public Task<IReadOnlyCollection<Project>> Convert(IReadOnlyCollection<ProjectExternalDto> externalDtos)
        {
            logger.LogTrace("Map started for externalDtos: {@Dtos}", externalDtos);
            var result = mapper.Map<IReadOnlyCollection<Project>>(externalDtos);
            return Task.FromResult(result);
        }
    }
}
