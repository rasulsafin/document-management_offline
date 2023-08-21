using System;

namespace Brio.Docs.Dtos
{
    public class ConfigDto
    {
        public Uri BaseAddressForApi { get; set; } = new Uri("https://localhost:5001");
    }
}
