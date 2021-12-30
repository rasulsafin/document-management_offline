using System.Collections.Generic;
using Brio.Docs.Connections.LementPro.Models;
using Brio.Docs.Integration.Extensions;
using Brio.Docs.Integration.Utilities;

namespace Brio.Docs.Connections.LementPro.Utilities
{
    public static class LoggerUtilities
    {
        public static IEnumerable<GettingPropertyExpression> GetSensitiveProperties()
            => GettingPropertyExpression.CreateList()
               .AddProperty<AuthorizationData>(x => x.Password);
    }
}
