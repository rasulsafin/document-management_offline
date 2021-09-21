﻿using System.Runtime.Serialization;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models
{
    [DataContract]
    public class Location : Object<Location.LocationAttributes, object>
    {
        public override string Type
        {
            get => Constants.LOCATION_TYPE;
            set { }
        }

        [DataContract]
        public class LocationAttributes : AAttributes
        {
            [DataMember(Name = "id")]
            public string Id { get; set; }

            [DataMember(Name = "parentId")]
            public object ParentID { get; set; }

            [DataMember(Name = "type")]
            public string Type { get; set; }

            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "description")]
            public string Description { get; set; }

            [DataMember(Name = "barcode")]
            public object BarCode { get; set; }

            [DataMember(Name = "order")]
            public int Order { get; set; }

            [DataMember(Name = "documentCount")]
            public int DocumentCount { get; set; }

            [DataMember(Name = "areaDefined")]
            public bool AreaDefined { get; set; }
        }
    }
}
