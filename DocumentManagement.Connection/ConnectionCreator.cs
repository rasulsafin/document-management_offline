using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection
{
    public static class ConnectionCreator
    {
        private static readonly string METHOD_NAME = "GetConnectionType";
        private static readonly string SEARCH_PATTERN = "*DocumentManagement.Connection.*.dll";

        private static Dictionary<string, Type> connections;

        public static IConnection GetConnection(ConnectionTypeDto connectionTypeDto)
        {
            if (connections == null)
                GetAllConnectionTypes();

            if (!connections.TryGetValue(connectionTypeDto.Name, out Type type))
                return null;

            return Activator.CreateInstance(type) as IConnection;
        }

        public static List<ConnectionTypeDto> GetAllConnectionTypes()
        {
            connections = new Dictionary<string, Type>();

            var list = new List<ConnectionTypeDto>();
            var listOfTypes = System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, SEARCH_PATTERN)
                        .SelectMany(x => Assembly.Load(AssemblyName.GetAssemblyName(x)).GetTypes())
                        .Where(x => typeof(IConnection).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);

            foreach (Type type in listOfTypes)
            {
                object connection = Activator.CreateInstance(type);
                MethodInfo method = type.GetMethod(METHOD_NAME);
                object result = method.Invoke(connection, null);
                ConnectionTypeDto t = result as ConnectionTypeDto;
                list.Add(t);
                connections.Add(t.Name, type);
            }

            return list;
        }
    }
}
