using MRS.DocumentManagement.Base;
using MRS.DocumentManagement.Interface.Dtos;
using System;
using System.Collections.Generic;

namespace MRS.DocumentManagement.Models
{
    public class ObjectiveModel : BaseViewModel
    {

        public static explicit operator ObjectiveModel(ObjectiveDto ident) => new ObjectiveModel(ident);

        public ObjectiveDto dto;
        public ObjectiveModel(ObjectiveDto dto)
        {
            this.dto = dto;
        }

        public ObjectiveModel()
        {
            this.dto = new ObjectiveDto();
        }

        public int ID
        {
            get => (int)dto.ID;
            set
            {
                dto.ID = (ID<ObjectiveDto>)value;
                OnPropertyChanged();
            }
        }
        public int AuthorID
        {
            get => (int)dto.AuthorID;
            set
            {
                dto.AuthorID = (ID<UserDto>)value;
                OnPropertyChanged();
            }
        }
        public int ObjectiveTypeID
        {
            get => (int)dto.ObjectiveTypeID;
            set
            {
                dto.ObjectiveTypeID = (ID<ObjectiveTypeDto>)value;
                OnPropertyChanged();
            }
        }

        public int? ParentObjectiveID
        {
            get => (int?)dto.ParentObjectiveID;
            set
            {
                dto.ParentObjectiveID = (ID<ObjectiveDto>?)value;
                OnPropertyChanged();
            }
        }

        public int ProjectID
        {
            get => (int)dto.ProjectID;
            set
            {
                dto.ProjectID = (ID<ProjectDto>)value;
                OnPropertyChanged();
            }
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

        public string Description
        {
            get => dto.Description;
            set
            {
                dto.Description = value;
                OnPropertyChanged();
            }
        }

        public DateTime CreationDate
        {
            get => dto.CreationDate;
            set
            {
                dto.CreationDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime DueDate
        {
            get => dto.DueDate;
            set
            {
                dto.DueDate = value;
                OnPropertyChanged();
            }
        }
        public ObjectiveStatus Status
        {
            get => dto.Status;
            set
            {
                dto.Status = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<ItemDto> Items
        {
            get => dto.Items;
            set
            {
                dto.Items = value;
                OnPropertyChanged();
            }
        }
    }
}
