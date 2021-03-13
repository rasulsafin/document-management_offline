using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection
{
    public static class ConnectionCreator
    {
        private static readonly string SEARCH_PATTERN = "*DocumentManagement.Connection.*.dll";

        private static Dictionary<string, Type> connections;

        public static IConnection GetConnection(ConnectionType connectionType)
        {
            if (connections == null)
                GetAllConnectionTypes();

            Type type = null;
            if (!(connections?.TryGetValue(connectionType.Name, out type) ?? false))
                return null;

            return Activator.CreateInstance(type) as IConnection;
        }

        public static List<ConnectionTypeExternalDto> GetAllConnectionTypes()
        {
            connections = new Dictionary<string, Type>();

            var list = new List<ConnectionTypeExternalDto>();
            var listOfTypes = System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, SEARCH_PATTERN)
                        .SelectMany(x => Assembly.Load(AssemblyName.GetAssemblyName(x)).GetTypes())
                        .Where(x => typeof(IConnection).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);

            foreach (Type type in listOfTypes)
            {
                var connection = Activator.CreateInstance(type);
                var method = type.GetMethod(nameof(IConnection.GetConnectionType));
                var result = method?.Invoke(connection, null) as ConnectionTypeExternalDto;

                if (result == null)
                    continue;

                list.Add(result);
                connections.Add(result.Name, type);
            }

            return list;
        }
    }
}
