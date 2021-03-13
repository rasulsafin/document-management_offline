using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MRS.DocumentManagement.Api.Validators;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Services;
using MRS.DocumentManagement.Utility;

namespace MRS.DocumentManagement.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            string connection = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<Database.DMContext>(options => options.UseSqlite(connection));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = AuthOptions.ISSUER,
                        ValidateAudience = true,
                        ValidAudience = AuthOptions.AUDIENCE,
                        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = false,
                    };
                });

            // Mapping
            services.AddTransient<DynamicFieldTypeConverter>();
            services.AddTransient<DynamicFieldDtoTypeConverter>();
            services.AddTransient<BimElementObjectiveTypeConverter>();

            services.AddTransient<ConnectionTypeAppPropertiesResolver>();
            services.AddTransient<ConnectionTypeDtoAppPropertiesResolver>();
            services.AddTransient<ConnectionInfoAuthFieldValuesResolver>();
            services.AddTransient<ConnectionInfoDtoAuthFieldValuesResolver>();

            services.AddTransient<DynamicFieldEnumerationTypePropertyResolver>();
            services.AddTransient<DynamicFieldExternalDtoValueResolver>();
            services.AddTransient<DynamicFieldValuePropertyResolver>();
            services.AddTransient<DynamicFieldValueResolver>();

            services.AddTransient<ObjectiveExternalDtoProjectIdResolver>();
            services.AddTransient<ObjectiveExternalDtoObjectiveTypeResolver>();
            services.AddTransient<ObjectiveExternalDtoObjectiveTypeIDResolver>();
            services.AddTransient<ObjectiveObjectiveTypeResolver>();
            services.AddTransient<ObjectiveProjectIDResolver>();
            services.AddTransient<ObjectiveExternalDtoProjectResolver>();
            services.AddTransient<BimElementObjectiveTypeConverter>();

            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

            services.AddControllers().AddNewtonsoftJson(opt =>
            {
                opt.SerializerSettings.ReferenceLoopHandling =
                    Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });
            services.AddCors();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "0.8.0",
                    Title = "BRIO DM",
                    Description = "DM API details",
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddScoped<ItemHelper>();
            services.AddScoped<DynamicFieldHelper>();

            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<IConnectionService, ConnectionService>();
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<IObjectiveService, ObjectiveService>();
            services.AddScoped<IObjectiveTypeService, ObjectiveTypeService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IConnectionTypeService, ConnectionTypeService>();

            services.AddSingleton<CryptographyHelper>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
            });
            app.UseRouting();
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            // TODO: uncomment and add Authenticate attribute to all controllers when roles are ready
            // app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
