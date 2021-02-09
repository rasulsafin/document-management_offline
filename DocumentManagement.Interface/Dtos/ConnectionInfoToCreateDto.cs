﻿using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public class ConnectionInfoToCreateDto
    {
        public ID<ConnectionTypeDto> ConnectionTypeID { get; set;  }

        public ID<UserDto> UserID { get; set;  }

        public IReadOnlyCollection<string> AuthData { get; set;  }
    }
}
