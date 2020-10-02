using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MRS.Bim.Tools;

namespace MRS.Bim.DocumentManagement
{
    public class ConnectionHandler : IConnection
    {
        public event Action<Project> ProjectPicked = null;

        public event Action<ConnectionState> ConnectionStateChanged = null;

        public event Action<ConnectionType> ConnectionTypeChanged = null;

        public ConnectionState ConnectionState
        {
            get => connectionState;
            private set
            {
                connectionState = value;
                ConnectionStateChanged?.Invoke(value);
            }
        }

        public ConnectionType ConnectionType
        {
            get => connectionType;
            private set
            {
                connectionType = value;
                ConnectionTypeChanged?.Invoke(value);
            }
        }

        public ConnectionInfo Info { get => Connection.Info; set => Connection.Info = value; }

        public Project CurrentProject { get; private set; }

        private ConnectionState connectionState;

        private ConnectionType connectionType;

        private IConnection Connection { get; set; }

        private dynamic PickedProjectId { get; set; }

        private readonly List<object> buffer = new List<object>();
        private readonly Semaphore semaphore = new Semaphore(1, 1);
        private readonly object lockerBuffer = new object();
        private IProgressing progressor;

        private IProgressing Progressor => progressor ?? (progressor = BimEnvironment.Instance.GetProgressor());

        #region Singletone

        public static ConnectionHandler Instance => instance ?? (instance = new ConnectionHandler());

        private static ConnectionHandler instance = null;

        #endregion

        public void PickProject(Project project)
        {
            Connection.PickProject(project);
            PickedProjectId = project.ID;
            ProjectPicked?.Invoke(project);
        }

        //TODO: improve this.
        public void SetConnectionType(string connection)
        {
            switch (connection)
            {
                case "TDMS ONLINE":
                    Connection = new TdmsConnection(Progressor);
                    ConnectionType = ConnectionType.Online;
                    break;
                case "TDMS OFFLINE":
                    Connection = new OfflineConnection("TDMS.db", Progressor);
                    ConnectionType = ConnectionType.Offline;
                    break;
                case "YandexDisk ONLINE":
                    Connection = new YandexDiskConnection(Progressor);
                    ConnectionType = ConnectionType.Online;
                    break;
                case "YandexDisk OFFLINE":
                    Connection = new OfflineConnection("YandexDisk.db", Progressor);
                    ConnectionType = ConnectionType.Offline;
                    break;
                default:
                    Disconnect();
                    Connection = null;
                    break;
            }
        }

        public void Cancel()
            => Connection?.Cancel();

        #region CONNECT METHODS

        //TODO: DB handler
        //TODO: ConnectionTypes handler


        public async Task<(bool, string)> Connect(dynamic param)
        {
            ConnectionState = ConnectionState.Loading;
            var result = Connection != null ? await Connection?.Connect(param) : (false, (string) null);
            ConnectionState = result.Item1 ? ConnectionState.Connected : ConnectionState.NotConnected;
            return result;
        }

        public void Disconnect()
        {
            ConnectionState = ConnectionState.NotConnected;
            // TODO:
            //Connection?.Disconnect();
        }

        #endregion

        #region ADD METHODS

        public async Task<bool> Add(DMAction[] actions)
            => Connection != null && await Connection?.Add(actions);

        public async Task<bool> Add(DMFile file)
        {
            if (Connection == null)
                return false;
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Add;
            var result = await AddAction(nameof(Add), file) && await Connection.Add(file);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<bool> Add(DMFile[] files)
        {
            if (Connection == null)
                return false;
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Add;
            var result = await AddAction(nameof(Add), (object) files) && await Connection.Add(files);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<Issue> Add(Issue issue)
        {
            if (Connection == null)
                return null;
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Add;
            await AddAction(nameof(Add), issue);
            var result = await Connection.Add(issue);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<bool> Add(Issue[] issues)
        {
            if (Connection == null)
                return false;
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Add;
            var result = await AddAction(nameof(Add), (object) issues) && await Connection.Add(issues);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<bool> Add(Job job)
        {
            if (Connection == null)
                return false;
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Add;
            var result = await AddAction(nameof(Add), job) && await Connection.Add(job);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<bool> Add(Job[] jobs)
        {
            if (Connection == null)
                return false;
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Add;
            var result = await AddAction(nameof(Add), (object) jobs) && await Connection.Add(jobs);
            GetOutOfQueue(key);
            return result;
        }

        #endregion

        #region CHANGE METHODS

        public async Task<bool> Change(DMFile file)
        {
            if (Connection == null)
                return false;
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Update;
            var result = await AddAction(nameof(Change), file) && await Connection.Change(file);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<bool> Change(DMFile[] files)
        {
            if (Connection == null)
                return false;
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Update;
            var result = await AddAction(nameof(Change), (object) files) && await Connection.Change(files);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<bool> Change(Issue issue)
        {
            if (Connection == null)
                return false;
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Update;
            var result = await AddAction(nameof(Change), issue) && await Connection.Change(issue);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<bool> Change(Issue[] issues)
        {
            if (Connection == null)
                return false;
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Update;
            var result = await AddAction(nameof(Change), (object) issues) && await Connection.Change(issues);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<bool> Change(Job job)
        {
            if (Connection == null)
                return false;
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Update;
            var result = await AddAction(nameof(Change), job) && await Connection.Change(job);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<bool> Change(Job[] jobs)
        {
            if (Connection == null)
                return false;
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Update;
            var result = await AddAction(nameof(Change), (object) jobs) && await Connection.Change(jobs);
            GetOutOfQueue(key);
            return result;
        }

        #endregion

        #region FILES' MANIPULATION METHODS

        public async Task<bool> Upload(string id, DMFile file)
        {
            if (Connection == null)
                return false;
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Upload;
            var result = await AddAction(nameof(Upload), id, file) && await Connection.Upload(id, file);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<bool> Upload(string id, DMFile[] files)
        {
            if (Connection == null)
                return false;
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Upload;
            var result = await AddAction(nameof(Upload), id, files) && await Connection.Upload(id, files);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<bool> Upload()
        {
            var result = true;

            if (Connection == null)
                return false;
          
            Progressor.ProgressType = ProgressType.Upload;
            result = await Connection.Upload();

            return result;
        }

        public async Task<bool> Download(string id, DMFile file)
        {
            if (Connection == null)
                return false;
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Download;
            var result = await Connection.Download(id, file);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<bool> Download(string id, DMFile[] files)
        {
            if (Connection == null)
                return false;
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Download;
            var result = await Connection.Download(id, files);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<bool> Download()
        {
            if (Connection == null)
                return false;
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Download;
            var result = await Connection.Download();
            GetOutOfQueue(key);
            return result;
        }

        #endregion

        #region GET METHODS

        public async Task<(bool, DMFile)> GetDMFile(string id, string fileId)
        {
            if (Connection == null)
                return (false, null);
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Get;
            var result = await Connection.GetDMFile(id, fileId);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<(bool, DMFile[])> GetDMFiles(string id, DMFileType type)
        {
            if (Connection == null)
                return (false, null);
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Get;
            var result = await Connection.GetDMFiles(id, type);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<(bool, Issue)> GetIssue(string id)
        {
            if (Connection == null)
                return (false, null);
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Get;
            var result = await Connection.GetIssue(id);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<(bool, Issue[])> GetIssues(string id)
        {
            if (Connection == null)
                return (false, null);

            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Get;
            var result = await Connection.GetIssues(id);
            GetOutOfQueue(key);

            return result;
        }

        public async Task<(bool, Job)> GetJob(string id)
        {
            if (Connection == null)
                return (false, null);

            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Get;
            var result = await Connection.GetJob(id);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<(bool, Job[])> GetJobs(string id)
        {
            if (Connection == null)
                return (false, null);
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Get;
            var result = await Connection.GetJobs(id);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<(bool, Project)> GetProject(string id)
        {
            if (Connection == null)
                return (false, null);
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Get;
            var result = await Connection.GetProject(id);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<(bool, Project[])> GetProjects(string id)
        {
            if (Connection == null)
                return (false, null);
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Get;
            var result = await Connection.GetProjects(id);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<(bool, DMAccount)> GetAccountInfo()
        {
            if (Connection == null)
                return (false, null);
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Get;
            var result = await Connection.GetAccountInfo();
            GetOutOfQueue(key);
            return result;
        }

        public async Task<(bool, Dictionary<string, DMItem[]>)> GetEnums()
        {
            if (Connection == null)
                return (false, null);

            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Get;
            var result = await Connection.GetEnums();
            GetOutOfQueue(key);
            return result;
        }

        #endregion

        #region REMOVE METHODS

        public async Task<bool> Remove(DMAction lastAction)
            => Connection != null && await Connection.Remove(lastAction);

        public async Task<bool> Remove(DMAction[] actions)
            => Connection != null && await Connection.Remove(actions);

        public async Task<bool> Remove(DMFile file)
        {
            if (Connection == null)
                return false;
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Remove;
            var result = await AddAction(nameof(Remove), file) && await Connection.Remove(file);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<bool> Remove(DMFile[] files)
        {
            if (Connection == null)
                return false;
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Remove;
            var result = await AddAction(nameof(Remove), (object) files) && await Connection.Remove(files);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<bool> Remove(Issue issue)
        {
            if (Connection == null)
                return false;
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Remove;
            var result = await AddAction(nameof(Remove), issue) && await Connection.Remove(issue);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<bool> Remove(Issue[] issues)
        {
            if (Connection == null)
                return false;
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Remove;
            var result = await AddAction(nameof(Remove), (object) issues) && await Connection.Remove(issues);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<bool> Remove(Job job)
        {
            if (Connection == null)
                return false;
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Remove;
            var result = await AddAction(nameof(Remove), job) && await Connection.Remove(job);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<bool> Remove(Job[] jobs)
        {
            if (Connection == null)
                return false;
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Remove;
            var result = await AddAction(nameof(Remove), (object) jobs) && await Connection.Remove(jobs);
            GetOutOfQueue(key);
            return result;
        }

        public async Task<bool> Remove(Project project)
        {
            if (Connection == null)
                return false;
            
            var key = WaitMyTurn();
            Progressor.ProgressType = ProgressType.Remove;
            var result = await AddAction(nameof(Remove), project) && await Connection.Remove(project);
            GetOutOfQueue(key);
            return result;
        }

        #endregion

        #region PRIVATE METHODS

        private async Task<bool> AddAction(string methodName, params object[] parameters)
        {
            if (Connection is OfflineConnection)
            {
                var action = new DMAction
                {
                    ProjectId = PickedProjectId,
                    MethodName = methodName,
                    Parameters = parameters
                };
                return await Add(new[] {action});
            }

            return true;
        }
        
        private void ChangeConnectionState(ConnectionState value)
            => ConnectionState = value;


        private object WaitMyTurn()
        {
            var key = new object();

            lock (lockerBuffer)
                buffer.Add(key);

            bool notMy;
            semaphore.WaitOne();

            do
            {
                semaphore.Release();
                semaphore.WaitOne();
                lock (lockerBuffer)
                    notMy = buffer[0] != key;
            } while (notMy);
            
            return key;
        }

        private void GetOutOfQueue(object key)
        {
            lock (lockerBuffer)
                buffer.Remove(key);

            semaphore.Release();
        }

        #endregion
    }
}