namespace MRS.DocumentManagement.Interface.Dtos
{
    public struct ProjectToListDto
    {
        public ID<ProjectDto> ID { get; set; }
        public string Title { get; set; }
    }
}
