using WPFStorage.Base;
using MRS.DocumentManagement.Interface.Dtos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MRS.DocumentManagement.Models
{
    public class ObjectiveModel : BaseViewModel
    {

        public static explicit operator ObjectiveModel(ObjectiveDto ident) => new ObjectiveModel(ident);

        public ObjectiveDto dto;
        private List<ItemDto> items;

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

        public ObservableCollection<ItemModel> GetItems()
        {
            var _items = new ObservableCollection<ItemModel>();
            _items.CollectionChanged += Items_CollectionChanged;
            foreach (var item in dto.Items)
            {
                _items.Add(new ItemModel(item));
            }
            return _items;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {

            }
            if (e.OldItems != null)
            {

            }
        }

        public ItemModel GetItem(ID<ItemDto> itemID)
        {
            if (itemID.IsValid && dto.Items != null)
            {
                foreach (var item in dto.Items)
                {
                    if (item.ID == itemID)
                        return new ItemModel(item);
                }
            }
            return null;
        }

        public void SetItem(ItemModel item)
        {
            ID<ItemDto> itemID = item.dto.ID;
            if (itemID.IsValid)
            {
                if (dto.Items == null)
                    dto.Items = items = new List<ItemDto>();
                foreach (var itemDto in dto.Items)
                {
                    if (itemDto.ID == itemID)
                    {
                        itemDto.ExternalItemId = item.ExternalItemId;
                        itemDto.ItemType = item.ItemType;
                        itemDto.Name = item.Name;
                    }
                }
                items.Add(item.dto);
            }
        }


        public ObservableCollection<ObjectiveModel> SubObjectives { get; set; } = new ObservableCollection<ObjectiveModel>();


    }
}
