using System;
using MRS.DocumentManagement;
using MRS.DocumentManagement.Connection.MrsPro;
using MRS.DocumentManagement.Connection.MrsPro.Converters;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Connection.MrsPro.Services;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MrsProServiceCollectionExtensions
    {
        public static IServiceCollection AddMrsPro(this IServiceCollection services)
        {
            services.AddScoped<MrsProHttpConnection>();
            services.AddScoped<MrsProConnection>();
            services.AddScoped<MrsProConnectionContext>();

            services.AddScoped<AuthenticationService>();
            services.AddScoped<ProjectsService>();
            services.AddScoped<UsersService>();
            services.AddScoped<IssuesService>();
            services.AddScoped<AttachmentsService>();

            services.AddScoped<IssuesDecorator>();
            services.AddScoped<ProjectElementsDecorator>();

            services.AddConverter<Issue, ObjectiveExternalDto, IssueObjectiveConverter>();
            services.AddConverter<ObjectiveExternalDto, Issue, ObjectiveIssueConverter>();
            services.AddConverter<string, ObjectiveStatus, StatusObjectiveStatusConverter>();
            services.AddConverter<ObjectiveStatus, string, ObjectiveStatusStatusConverter>();
            services.AddConverter<Project, ObjectiveExternalDto, ProjectObjectiveConverter>();
            services.AddConverter<ObjectiveExternalDto, Project, ObjectiveProjectConverter>();
            services.AddConverter<string, (string id, string type), ExternalIdTypeIdConverter>();

            services.AddScoped<Func<MrsProConnectionContext>>(x => x.GetService<MrsProConnectionContext>);

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
