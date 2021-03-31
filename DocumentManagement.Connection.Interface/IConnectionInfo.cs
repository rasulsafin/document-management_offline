using System;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Interface
{
    public interface IConnectionInfo
    {
        /// <summary>
        /// Get the information about the current Connection.
        /// </summary>
        /// <returns>Filed ConnectionTypeDto.</returns>
        ConnectionTypeExternalDto GetConnectionType();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Type GetTypeOfConnection();
    }
}
