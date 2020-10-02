using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MRS.Bim.DocumentManagement.Tdms;
using MRS.Bim.Tools;

namespace MRS.Bim.DocumentManagement
{
    public class TdmsConnection : AOnlineConnection
    {
        private readonly TDMSClient client = TDMSClient.instance;
        private readonly Dictionary<(string, Type), (string methodName, string parameterType)> methodsContainer =
                DetectMethods();

        public TdmsConnection(IProgressing progress) : base(@"TDMS.db")
            => client.Progress = progress;

        #region IConnection Methods

        #region Add Methods

        public override void PickProject(Project project)
        { }

        public override async Task<bool> Add(DMFile file) => await Receive<bool>(file); // Add(DMFile)

        public override async Task<bool> Add(DMFile[] files) => await Receive<bool>(files); // Add(DMFile[])

        public override async Task<Issue> Add(Issue issue) => await Receive<Issue>(issue); // Add(Issue)

        public override async Task<bool> Add(Issue[] issues) => await Receive<bool>(issues); // Add(Issue[])

        public override async Task<bool> Add(Job job) => await Receive<bool>(job); // Add(Job)

        public override async Task<bool> Add(Job[] jobs) => await Receive<bool>(jobs); // Add(Job[])

        #endregion

        public override void Cancel() => client.Cancel();

        #region Change Methods

        public override async Task<bool> Change(DMFile file) => await Receive<bool>(file); // Change(DMFile)

        public override async Task<bool> Change(DMFile[] files) => await Receive<bool>(files); // Change(DMFile[])

        public override async Task<bool> Change(Issue issue) => await Receive<bool>(issue); // Change(Issue)

        public override async Task<bool> Change(Issue[] issues) => await Receive<bool>(issues); // Change(Issue[])

        public override async Task<bool> Change(Job job) => await Receive<bool>(job); // Change(Job)

        public override async Task<bool> Change(Job[] jobs) => await Receive<bool>(jobs); // Change(Job[])

        #endregion

        #region Connect Methods

        public override async Task<(bool, string)> Connect(dynamic parameter)
        {
            var parameters = (string[])parameter;
            return await Receive<(bool, string)>((parameters[0], parameters[1], parameters[2], parameters[3])); // Connect((String, String, String, String))
        }

        public override void Disconnect() => Receive<bool>(true).ConfigureAwait(false); // Disconnect(Boolean)

        #endregion

        #region Upload Methods

        public override async Task<bool> Upload(string id, DMFile file) => await Receive<bool>((id, file)); // Upload((String, DMFile))

        public override async Task<bool> Upload(string id, DMFile[] files) => await Receive<bool>((id, files)); // Upload((String, DMFile[]))

        #endregion

        #region Download Methods

        public override async Task<bool> Download(string id, DMFile file) => await Receive<bool>((id, file)); // Download((id, DMFile))

        public override async Task<bool> Download(string id, DMFile[] files) => await Receive<bool>((id, files)); // Download((id, DMFile[]))

        #endregion

        #region Get Methods

        public override async Task<(bool, DMFile)> GetDMFile(string id, string fileId)
        {
            try
            {
                var result = await Receive<DMFile>((id, fileId)); // GetDMFile((String, String))
                return (result != null, result);
            }
            catch (OperationCanceledException)
            {
                return (false, null);
            }
        }

        public override async Task<(bool, DMFile[])> GetDMFiles(string id, DMFileType type)
        {
            try
            {
                var result = await Receive<DMFile[]>((id, type)); // GetDMFiles(String, DMFileType)
                return (result != null, result);
            }
            catch (OperationCanceledException)
            {
                return (false, null);
            }
        }

        public override async Task<(bool, Issue)> GetIssue(string id)
        {
            try
            {
                var result = await Receive<Issue>(id); // GetIssue(String)
                return (result != null, result);
            }
            catch (OperationCanceledException)
            {
                return (false, null);
            }
        }

        public override async Task<(bool, Issue[])> GetIssues(string id)
        {
            try
            {
                var result = await Receive<Issue[]>(id); // GetIssues(String)
                return (result != null, result);
            }
            catch (OperationCanceledException)
            {
                return (false, null);
            }
        }

        public override async Task<(bool, Job)> GetJob(string id)
        {
            try
            {
                var result = await Receive<Job>(id); // GetJob(String)
                return (result != null, result);
            }
            catch (OperationCanceledException)
            {
                return (false, null);
            }
        }

        public override async Task<(bool, Job[])> GetJobs(string id)
        {
            try
            {
                var result = await Receive<Job[]>(id); // GetJobs(String)
                return (result != null, result);
            }
            catch (OperationCanceledException)
            {
                return (false, null);
            }
        }

        public override async Task<(bool, Project)> GetProject(string id)
        {
            try
            {
                var result = await Receive<Project>(id); // GetProject(String)
                return (result != null, result);
            }
            catch (OperationCanceledException)
            {
                return (false, null);
            }
        }

        public override async Task<(bool, Project[])> GetProjects(string id)
        {
            try
            {
                var result = await Receive<Project[]>(id); // GetProjects(String)
                return (result != null, result);
            }
            catch (OperationCanceledException)
            {
                return (false, null);
            }
        }

        public override async Task<(bool, DMAccount)> GetAccountInfo()
        {
            try
            {
                var result = await Receive<DMAccount>(true); // GetAccountInfo(Boolean)
                return (result != null, result);
            }
            catch (OperationCanceledException)
            {
                return (false, null);
            }
        }

        public override async Task<(bool, Dictionary<string, DMItem[]>)> GetEnums()
        {
            try
            {
                var result = await Receive<Dictionary<string, DMItem[]>>(true); // GetEnums(Boolean)

                if (Info == null)
                    Info = new ConnectionInfo();
                Info.Enums = result;

                return (result != null, result);
            }
            catch (OperationCanceledException)
            {
                return (false, null);
            }
        }

        #endregion

        #region Remove Methods

        public override async Task<bool> Remove(DMFile file) => await Receive<bool>(file); // Remove(DMFile)

        public override async Task<bool> Remove(DMFile[] files) => await Receive<bool>(files); // Remove(DMFile[])

        public override async Task<bool> Remove(Issue issue) => await Receive<bool>(issue); // Remove(Issue)

        public override async Task<bool> Remove(Issue[] issues) => await Receive<bool>(issues); // Remove(Issue[])

        public override async Task<bool> Remove(Job job) => await Receive<bool>(job); // Remove(Job)

        public override async Task<bool> Remove(Job[] jobs) => await Receive<bool>(jobs); // Remove(Job[])

        public override async Task<bool> Remove(Project project) => await Receive<bool>(project); // Remove(Project)

        #endregion

        #endregion

        #region Private Methods

        private static Dictionary<(string, Type), (string, string)> DetectMethods()
        {
            var dictionary = new Dictionary<(string Name, Type ParameterType), (string, string)>();

            foreach (var method in typeof(ITdmsConnection).GetMethods())
            {
                var parameterType = method.GetParameters()[0].ParameterType;
                dictionary.Add((method.Name, parameterType), (method.ToShortString(), parameterType.ToShortString()));
            }

            return dictionary;
        }

        /// <summary>
        /// Call a method (with a same name by default) at the server.
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="defaultValue"></param>
        /// <param name="methodName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private async Task<T> Receive<T>(object parameter,
                                         T defaultValue = default,
                                         [CallerMemberName] string methodName = null)
        {
            try
            {
                var (methodToCall, parameterType) = methodsContainer[(methodName, parameter.GetType())];
                var a = await client.SendData(methodToCall, parameterType, parameter);
                return a != null && a is T result ? result : defaultValue;
            }
            catch (OperationCanceledException)
            {
                await Receive<bool>(true, methodName: nameof(ITdmsConnection.Cancel));
                throw;
            }
            catch (KeyNotFoundException)
            {
                if (methodName == null || parameter == null)
                    throw new NullReferenceException();
                throw;
            }
        }

        #endregion
    }
}