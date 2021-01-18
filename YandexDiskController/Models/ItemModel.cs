using System.Xml.Serialization;
using MRS.DocumentManagement.Interface.Dtos;
using WPFStorage.Base;

namespace MRS.DocumentManagement.Models
{
    public class ItemModel : BaseViewModel
    {
        public static explicit operator ItemModel(ItemDto ident) => new ItemModel(ident);

        [XmlIgnore]
        public ItemDto dto;
        private bool isObjective;

        public ItemModel(ItemDto dto)
        {
            this.dto = dto;
        }

        public ItemModel()
        {
            this.dto = new ItemDto();
        }

        public string Name
        {
            get => dto.Name;
            set
            {
                dto.Name = value;
                OnPropertyChanged();
            }
        }

        public bool IsObjective 
        {
            get => isObjective;
            set
            {
                isObjective = value;
                OnPropertyChanged();
            }
        }

        public string ExternalItemId
        {
            get => dto.ExternalItemId;
            set
            {
                dto.ExternalItemId = value;
                OnPropertyChanged();
            }
        }

        public ItemTypeDto ItemType
        {
            get => dto.ItemType;
            set
            {
                dto.ItemType = value;
                OnPropertyChanged();
            }
        }

        public int ID
        {
            get => (int)dto.ID;
            set
            {
                dto.ID = (ID<ItemDto>)value;
                OnPropertyChanged();
            }
        }
    
}
}
