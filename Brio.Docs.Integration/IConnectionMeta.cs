using System;
using Brio.Docs.Integration.Dtos;

namespace Brio.Docs.Integration.Client
{
    /// <summary>
    /// The interface for register external connection in the system.
    /// </summary>
    public interface IConnectionMeta
    {
        /// <summary>
        /// Get the information about the current Connection.
        /// </summary>
        /// <returns>Filed ConnectionTypeDto.</returns>
        ConnectionTypeExternalDto GetConnectionTypeInfo();

        /// <summary>
        /// Gets type of class that implements IConnection.
        /// </summary>
        /// <returns>The type of the class that implements IConnection interface.</returns>
        Type GetIConnectionType();
    }
}
