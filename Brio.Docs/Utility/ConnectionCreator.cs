using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Brio.Docs.Integration
{
    public static class ConnectionCreator
    {
        private static readonly string SEARCH_PATTERN = "*Brio.Docs.Connections.*.dll";

        private static readonly Lazy<IReadOnlyCollection<Assembly>> ASSEMBLIES_COLLECTION =
            new Lazy<IReadOnlyCollection<Assembly>>(
                () => Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, SEARCH_PATTERN)
                   .Select(x => Assembly.Load(AssemblyName.GetAssemblyName(x)))
                   .ToList());

        private static Dictionary<string, Type> connections;

        public static IEnumerable<MethodInfo> GetDependencyInjectionMethods()
            => ASSEMBLIES_COLLECTION.Value
               .SelectMany(x => x.GetTypes())
               .Where(
                    x => x.IsPublic && x.IsDefined(typeof(ExtensionAttribute)) &&
                        x.Namespace == typeof(IServiceCollection).Namespace)
               .SelectMany(
                    type => type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                       .Where(IsExtensionFor<IServiceCollection>));

        public static IEnumerable<MethodInfo> GetLoggerMethods()
            => ASSEMBLIES_COLLECTION.Value
               .SelectMany(x => x.GetTypes())
               .Where(x => x.IsPublic && x.IsDefined(typeof(ExtensionAttribute)))
               .SelectMany(
                    type => type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                       .Where(IsExtensionFor<LoggerConfiguration>));

        public static Type GetConnection(ConnectionType connectionType)
        {
            if (connections == null)
                GetAllConnectionTypes();

            Type type = null;
            return !(connections?.TryGetValue(connectionType.Name, out type) ?? false) ? null : type;
        }

        public static List<ConnectionTypeExternalDto> GetAllConnectionTypes()
        {
            connections = new Dictionary<string, Type>();

            var list = new List<ConnectionTypeExternalDto>();
            var listOfTypes = ASSEMBLIES_COLLECTION.Value
                        .SelectMany(x => x.GetTypes())
                        .Where(x => typeof(IConnectionMeta).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);

            foreach (Type type in listOfTypes)
            {
                var connection = Activator.CreateInstance(type);
                var method = type.GetMethod(nameof(IConnectionMeta.GetConnectionTypeInfo));
                var result = method?.Invoke(connection, null) as ConnectionTypeExternalDto;

                if (result == null)
                    continue;

                method = type.GetMethod(nameof(IConnectionMeta.GetIConnectionType));

                list.Add(result);
                connections.Add(result.Name, method?.Invoke(connection, null) as Type);
            }

            return list;
        }

        private static bool IsExtensionFor<T>(MethodInfo x)
        {
            if (!x.IsDefined(typeof(ExtensionAttribute)) ||
                x.ReturnType != typeof(T))
                return false;

            var parameters = x.GetParameters();
            return parameters.Length == 1 &&
                parameters[0].ParameterType == typeof(T);
        }
    }
}
