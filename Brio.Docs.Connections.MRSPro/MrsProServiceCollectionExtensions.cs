using System;
using System.Collections.Generic;
using Brio.Docs.Common;
using Brio.Docs.Connections.MrsPro;
using Brio.Docs.Connections.MrsPro.Converters;
using Brio.Docs.Connections.MrsPro.Interfaces;
using Brio.Docs.Connections.MrsPro.Models;
using Brio.Docs.Connections.MrsPro.Services;
using Brio.Docs.Integration;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MrsProServiceCollectionExtensions
    {
        public static IServiceCollection AddMrsPro(this IServiceCollection services)
        {
            services.AddScoped<MrsProHttpConnection>();
            services.AddScoped<MrsProConnection>();
            services.AddScoped<MrsProConnectionContext>();
            services.AddScoped<MrsProStorage>();

            services.AddScoped<AuthenticationService>();
            services.AddScoped<ProjectsService>();
            services.AddScoped<IssuesService>();
            services.AddScoped<AttachmentsService>();
            services.AddScoped<PlansService>();
            services.AddScoped<ItemService>();

            services.AddScoped<IssuesDecorator>();
            services.AddScoped<ProjectElementsDecorator>();
            services.AddScoped<ProjectsDecorator>();

            services.AddScoped<Func<MrsProConnectionContext>>(x => x.GetService<MrsProConnectionContext>);

            services.AddConverters();

            return services;
        }

        private static IServiceCollection AddConverters(this IServiceCollection services)
        {
            services.AddConverter<Issue, ObjectiveExternalDto, IssueObjectiveConverter>();
            services.AddConverter<ObjectiveExternalDto, Issue, ObjectiveIssueConverter>();
            services.AddConverter<string, ObjectiveStatus, StatusObjectiveStatusConverter>();
            services.AddConverter<ObjectiveStatus, string, ObjectiveStatusStatusConverter>();
            services.AddConverter<Project, ObjectiveExternalDto, ProjectObjectiveConverter>();
            services.AddConverter<ObjectiveExternalDto, Project, ObjectiveProjectConverter>();
            services.AddConverter<string, (string id, string type), ExternalIdTypeIdConverter>();
            services.AddConverter<Project, ProjectExternalDto, ProjectProjectDtoConverter>();
            services.AddConverter<ProjectExternalDto, Project, ProjectDtoProjectConverter>();

            services.AddConverter<IEnumerable<IElementAttachment>, ICollection<ItemExternalDto>, ElementAttachmentItemConverter>();
            return services;
        }

        private static IServiceCollection AddConverter<TFrom, TTo, TConverter>(this IServiceCollection services)
      where TConverter : class, IConverter<TFrom, TTo>
        {
            services.AddScoped<IConverter<TFrom, TTo>, TConverter>();
            return services;
        }
    }
}
