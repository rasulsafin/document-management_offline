using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public class ConnectionInfoToCreateDto : IConnectionInfoDto
    {
        public ID<ConnectionTypeDto> ConnectionTypeID { get; set;  }

        public ID<UserDto> UserID { get; set;  }

        public IDictionary<string, string> AuthFieldValues { get; set;  }
    }
}
