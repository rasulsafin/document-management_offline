using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Dtos.ForApi;
using Brio.Docs.Client.Dtos.ForApi.Project;
using Brio.Docs.Client.Dtos.ForApi.Projects;
using Brio.Docs.Client.Exceptions;
using Brio.Docs.Client.Services.ForApi;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Newtonsoft.Json;

namespace Brio.Docs.Services.ForApi
{
    public class ProjectForApiService : IProjectForApiService, IDisposable
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly HttpClient httpClient;
        private readonly IConfigService configService;

        public ProjectForApiService(DMContext context, IConfigService configService, IMapper mapper)
        {
            this.context = context;

            this.configService = configService;

            this.mapper = mapper;

            httpClient = new HttpClient
            {
                BaseAddress = this.configService.Config.BaseAddressForApi,
            };
        }

        // TODO
        public async Task<IEnumerable<ProjectToListDto>> GetAllProjects()
        {
            try
            {
                var responseM = await httpClient.GetAsync("api/project");

                var content = await responseM.Content.ReadAsStringAsync();

                var projects = JsonConvert.DeserializeObject<List<ProjectToListDto>>(content);
                return projects;
            }
            catch (DocumentManagementException ex)
            {
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<IEnumerable<ProjectToListDto>> GetUserProjects(ID<UserDto> userID)
        {
            try
            {
                var response = await httpClient.GetAsync($"api/project/for_user/{userID}");
                var content = await response.Content.ReadAsStringAsync();
                var projectsFromApi = JsonConvert.DeserializeObject<List<ProjectToReadForApiDto>>(content);

                // Hard Code: Add converters
                var projectsToReturn = projectsFromApi.Select(proj => new ProjectToListDto
                {
                    ID = (ID<ProjectDto>)proj.Id,
                    Title = proj.Title,
                    Items = proj.Items.Select(item => mapper.Map<ItemDto>(item)).ToList(),
                }).ToList();

                return projectsToReturn;
            }
            catch (DocumentManagementException ex)
            {
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        // TODO: CreatedById, UpdatedById, ItemIds ant etc.
        public async Task<ProjectToListDto> Add(ProjectToCreateDto projectToCreate)
        {
            try
            {
                var project = mapper.Map<Project>(projectToCreate);

                var projToCreateForApi = mapper.Map<ProjectToCreateForApiDto>(projectToCreate);

                // HardCode: make converters mb.
                var userFromDb = context.Users.Find((int)projectToCreate.AuthorID);

                // HardCode: Map UserForApi.
                var userForApi = new UserForApiDto()
                {
                    Id = (long)userFromDb.ID,
                    Name = userFromDb.Name,
                    Login = userFromDb.Login,
                    LastName = string.Empty,
                    OrganizationId = 1,
                };

                // Hard Code: make converters mb.
                projToCreateForApi.CreatedById = (long)projectToCreate.AuthorID;
                projToCreateForApi.UpdatedById = (long)projectToCreate.AuthorID;
                projToCreateForApi.UpdatedAt = DateTime.Now;
                projToCreateForApi.Items.Select(x => x.ProjectId = project.ID).ToList();

                projToCreateForApi.ItemIds = projectToCreate.Items.Select(i => (int)i.ID).ToList();
                projToCreateForApi.Users = new List<UserForApiDto>() { userForApi };
                projToCreateForApi.UserIds = new List<int>
                {
                    (int)projectToCreate.AuthorID,
                };

                var jsonData = JsonConvert.SerializeObject(projToCreateForApi);

                var httpContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

                var responseM = await httpClient.PostAsync("api/project", httpContent);

                if (responseM.IsSuccessStatusCode)
                {
                    project.Items = projectToCreate.Items.Select(i => mapper.Map<Database.Models.Item>(i)).ToList();

                    return mapper.Map<ProjectToListDto>(project);
                }
                else
                {
                    throw new DocumentManagementException("Couldn't establish connection with local API server. Status code 500");
                }
            }
            catch (DocumentManagementException ex)
            {
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        // TODO: Api doesn't have this endpoint
        public async Task<bool> Remove(ID<ProjectDto> projectID)
        {
            return false;
        }

        public async Task<bool> Update(ProjectDto project)
        {
            var projToUpd = mapper.Map<ProjectToUpdateForApi>(project);

            var jsonData = JsonConvert.SerializeObject(projToUpd);

            var httpContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var responseM = await httpClient.PutAsync("api/project", httpContent);

            return responseM.IsSuccessStatusCode;
        }

        public async Task<ProjectDto> Find(ID<ProjectDto> projectID)
        {
            // TODO
            var responseM = await httpClient.GetAsync($"api/project/{projectID}");
            var content = await responseM.Content.ReadAsStringAsync();

            var projFromApi = JsonConvert.DeserializeObject<ProjectToReadForApiDto>(content);

            var projToReturn = new ProjectDto()
            {
                ID = (ID<ProjectDto>)projFromApi.Id,
                Title = projFromApi.Title,
            };
            return projToReturn;
        }

        public async Task<IEnumerable<UserDto>> GetUsers(ID<ProjectDto> projectID)
        {
            return null;
        }

        public async Task<bool> LinkToUsers(ID<ProjectDto> projectID, IEnumerable<ID<UserDto>> users)
        {
            return true;
        }

        public async Task<bool> UnlinkFromUsers(ID<ProjectDto> projectID, IEnumerable<ID<UserDto>> users)
        {
            return true;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
