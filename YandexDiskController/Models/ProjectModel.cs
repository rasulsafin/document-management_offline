using DocumentManagement.Base;
using MRS.DocumentManagement.Interface.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentManagement.Models
{
    public class ProjectModel : BaseViewModel
    {
        ProjectDto dto;

        public ProjectModel(ProjectDto dto)
        {
            this.dto = dto;
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
            //set
            //{
            //    dto.Title = value;
            //    OnPropertyChanged();
            //}
        }

        
    }
}
