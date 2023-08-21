using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Dtos.ForApi.Records;
using Brio.Docs.Client.Exceptions;
using Brio.Docs.Client.Filters;
using Brio.Docs.Client.Services.ForApi;
using Brio.Docs.Client.Sorts;
using Brio.Docs.Common;
using Brio.Docs.Common.ForApi;
using Brio.Docs.Database;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace Brio.Docs.Services.ForApi
{
    public class ObjectiveForApiService : IObjectiveForApiService, IDisposable
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly HttpClient httpClient;
        private readonly IConfigService configService;

        public ObjectiveForApiService(DMContext context, IConfigService configService)
        {
            this.context = context;

            this.configService = configService;

            httpClient = new HttpClient
            {
                BaseAddress = this.configService.Config.BaseAddressForApi,
            };

            var mapConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<StatusEnum, ObjectiveStatus>()
                    .ConvertUsing(status => MapStatusEnumToObjectiveStatus(status));

                cfg.CreateMap<ObjectiveStatus, StatusEnum>()
                    .ConvertUsing(objectiveStatus => MapObjectiveStatusToStatusEnum(objectiveStatus));

                cfg.CreateMap<ObjectiveToCreateDto, RecordToCreateForApiDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Title))
                    .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => (long)src.ProjectID))
                    .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreationDate))
                    .ForMember(dest => dest.FixDate, opt => opt.MapFrom(src => src.DueDate))
                    .ForMember(dest => dest.CreatedById, opt => opt.MapFrom(src => (long)src.AuthorID))
                    .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => (long)src.ParentObjectiveID))
                    ;

            });

            mapper = mapConfiguration.CreateMapper();
        }

        public async Task<ObjectiveToListDto> Add(ObjectiveToCreateDto data)
        {
            var objToCreate = mapper.Map<RecordToCreateForApiDto>(data);

            // Assigning some values.

            // Suppose priority is normal. (default value)
            objToCreate.Priority = PriorityEnum.Medium;

            var jsonData = JsonConvert.SerializeObject(objToCreate);

            var httpCont = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var responseM = await httpClient.PostAsync("api/record", httpCont);

            if (responseM.IsSuccessStatusCode)
            {
                return mapper.Map<ObjectiveToListDto>(objToCreate);
            }
            else
            {
                throw new Exception("Internal server error at endpoint: ObjectiveForApiService.Add");
            }
        }

        public async Task<IEnumerable<ID<ObjectiveDto>>> Remove(ID<ObjectiveDto> objectiveID)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Update(ObjectiveDto objectiveData)
        {
            throw new NotImplementedException();
        }

        public async Task<ObjectiveDto> Find(ID<ObjectiveDto> objectiveID)
        {
            throw new NotImplementedException();
        }

        public async Task<PagedListDto<ObjectiveToListDto>> GetObjectives(ID<ProjectDto> projectId, ObjectiveFilterParameters filter, SortParameters sort)
        {
            var responseM = await httpClient.GetAsync($"api/record/get_records/{projectId}");

            if (responseM.IsSuccessStatusCode)
            {
                var content = await responseM.Content.ReadAsStringAsync();

                var records = JsonConvert.DeserializeObject<IEnumerable<RecordToReadForApiDto>>(content);

                // Hard Code: Add converters
                var projectsToReturn = records.Select(rec => new ObjectiveToListDto()
                {
                    ID = (ID<ObjectiveDto>)rec.Id,
                    Title = rec.Name,
                    Status = MapStatusEnumToObjectiveStatus(rec.Status),
                    CreationDate = rec.CreatedAt,
                    DueDate = rec.FixDate,
                }).ToList();

                return new PagedListDto<ObjectiveToListDto> { PageData = new PagedDataDto() { PageSize = filter.PageSize, TotalCount = projectsToReturn.Count }, Items = projectsToReturn };
            }
            else
            {
                throw new DocumentManagementException("ObjectiveForApiService.GetObjectives - exception");
            }
        }

        public async Task<IEnumerable<ObjectiveToSelectionDto>> GetObjectivesForSelection(ID<ProjectDto> projectID, ObjectiveFilterParameters filter)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ObjectiveToLocationDto>> GetObjectivesWithLocation(ID<ProjectDto> projectID, string itemName, ObjectiveFilterParameters filter)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<SubobjectiveDto>> GetObjectivesByParent(ID<ObjectiveDto> parentID)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ObjectiveBimParentDto>> GetParentsOfObjectivesBimElements(ID<ProjectDto> projectID)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private ObjectiveStatus MapStatusEnumToObjectiveStatus(StatusEnum status)
        {
            switch (status)
            {
                case StatusEnum.InProgress:
                    return ObjectiveStatus.InProgress;
                case StatusEnum.NotDefined:
                    return ObjectiveStatus.Undefined;
                case StatusEnum.Open:
                    return ObjectiveStatus.Open;
                case StatusEnum.Ready:
                    return ObjectiveStatus.Ready;
                case StatusEnum.Moved:
                    return ObjectiveStatus.Late;
                case StatusEnum.Closed:
                    return ObjectiveStatus.Closed;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private StatusEnum MapObjectiveStatusToStatusEnum(ObjectiveStatus objectiveStatus)
        {
            switch (objectiveStatus)
            {
                case ObjectiveStatus.Undefined:
                    return StatusEnum.NotDefined;
                case ObjectiveStatus.Open:
                    return StatusEnum.Open;
                case ObjectiveStatus.InProgress:
                    return StatusEnum.InProgress;
                case ObjectiveStatus.Ready:
                    return StatusEnum.Ready;
                case ObjectiveStatus.Late:
                    return StatusEnum.Moved;
                case ObjectiveStatus.Closed:
                    return StatusEnum.Closed;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
