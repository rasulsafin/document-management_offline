using DocumentManagement.Base;
using MRS.DocumentManagement;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using System;
using System.Xml.Serialization;

namespace DocumentManagement.Models
{


    public class ProjectModel : BaseViewModel
    {

        public static explicit operator ProjectModel(ProjectDto ident) => new ProjectModel(ident);

        [XmlIgnore]
        public ProjectDto dto;

        public ProjectModel(ProjectDto dto)
        {
            this.dto = dto;
        }

        public ProjectModel()
        {
            this.dto = new ProjectDto();
        }

        public string Title
        {
            get => dto.Title;
            set
            {
                dto.Title = value;
                OnPropertyChanged();
            }
        }

        public int ID
        {
            get => (int)dto.ID;
            set
            {
                dto.ID = (ID<ProjectDto>)value;
                OnPropertyChanged();
            }
        }
    }
}
