using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MRS.DocumentManagement.Interface.Dtos;
using WPFStorage.Base;

namespace MRS.DocumentManagement.Models
{
    public class ObjectiveModel : BaseViewModel
    {
        public ObjectiveDto dto;
        private List<ItemDto> items;
        private UserModel author;

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
                OnPropertyChanged("Author");
            }
        }

        public UserModel Author
        {
            get
            {
                if (author == null)
                {
                    foreach (var user in ObjectModel.Users)
                    {
                        if (user.ID == AuthorID)
                        {
                            author = user;
                            break;
                        }
                    }
                }

                return author;
            }

            set
            {
                author = value;
                dto.AuthorID = (ID<UserDto>)author.ID;
                OnPropertyChanged();
                OnPropertyChanged("AuthorID");
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

        public ObservableCollection<ObjectiveModel> SubObjectives { get; set; } = new ObservableCollection<ObjectiveModel>();

        public static explicit operator ObjectiveModel(ObjectiveDto ident) => new ObjectiveModel(ident);

        public ObservableCollection<ItemModel> GetItems()
        {
            var itemsCollect = new ObservableCollection<ItemModel>();
            itemsCollect.CollectionChanged += Items_CollectionChanged;
            foreach (var item in dto.Items)
            {
                itemsCollect.Add(new ItemModel(item));
            }

            return itemsCollect;
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

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
            }

            if (e.OldItems != null)
            {
            }
        }
    }
}
