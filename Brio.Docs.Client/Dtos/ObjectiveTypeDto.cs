﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace Brio.Docs.Client.Dtos
{
    public class ObjectiveTypeDto
    {
        public ID<ObjectiveTypeDto> ID { get; set; }

        public string Name { get; set; }

        public ICollection<DynamicFieldDto> DefaultDynamicFields { get; set; }
    }
}
