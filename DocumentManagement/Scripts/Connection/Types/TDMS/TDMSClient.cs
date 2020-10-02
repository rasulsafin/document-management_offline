using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MRS.Bim.Tools;
using MRS.Bim.DocumentManagement.Tdms.Helpers;
using Timer = MRS.Bim.Tools.Timer;

namespace MRS.Bim.DocumentManagement.Tdms
{
    public class TDMSClient 
    {
        private System.Diagnostics.Process process = null;
        private NetManager client;
        private JSONObject json = new JSONObject();

        public bool isConnectedToServer = false;

        public object result;

        public static TDMSClient instance;

        public bool IsOver = false;
        public Timer timer = new Timer();
        private CancellationTokenSource cts;

        public IProgressing Progress { get; set; }

        public void Awake()
        {
            string filePath = Path.Combine(BimEnvironment.Instance.StreamingAssetsPath, "TDMS Server/TDMSRPCServer.exe");
            try
            {
                var proccesses = System.Diagnostics.Process.GetProcessesByName("TDMSRPCServer");
                if (proccesses.Length > 0)
                    process = proccesses[0];
                else
                    process = System.Diagnostics.Process.Start(filePath);
            }
            catch (Exception e)
            {
            }

            instance = this;
            EventBasedNetListener listener = new EventBasedNetListener();
            client = new NetManager(listener);
            client.Start();

            Connect("localhost" /* host ip or name */, 3333 /* port */, "TDMS" /* text key or NetDataWriter */);

            listener.NetworkReceiveEvent += OnRecieve;
            Task.Run(PollEvents);
        }

        private void PollEvents()
        {
            while (true)
            {
                Task.Delay(150).Wait();
                client.PollEvents();
            }
        }
        

        public async void Connect(string host, int port, string key)
        {
            await Task.Run(() =>
            {
                try
                {
                    client.Connect(host, port, key);
                    while (!IsOver)
                    {
                        Thread.Sleep(1);
                    }
                }
                catch (Exception e)
                {
                }
            });
        }

        public async Task<object> SendData(string name, string type, object o)
        {
            Progress.Clear();

            if (name != "Exit")
                timer.SetTimer(10, () =>
                {
                    Progress.Clear();
                    // ConnectionService.GetInstance().Reset();
                });

            if (!Progress.IsCanceled)
                Progress.AddLayer(100);

            cts = Progress.CancellationTokenSource;
            var cancellationToken = cts.Token;

            await Task.Run(() =>
            {
                IsOver = false;
                Send(json.ConvertToJson(name, type, o));
                while (!IsOver)
                {
                    Thread.Sleep(1);
                    if (cancellationToken.IsCancellationRequested)
                        IsOver = true;

                    cancellationToken.ThrowIfCancellationRequested();
                }
            }, cancellationToken);

            Progress.Clear();
            return result;
        }

        public void Cancel()
        {
            // Запрашиваем отмену операции
            if (!cts.IsCancellationRequested)
                cts.Cancel();
        }

        public void Send(string data)
        {
            if (isConnectedToServer)
            {
                try
                {
                    var peer = client.FirstPeer;
                    NetDataWriter writer = new NetDataWriter();
                    writer.Reset();
                    writer.Put(data);
                    peer.Send(writer, DeliveryMethod.ReliableOrdered);
                }
                catch (Exception e)
                {
                    result = null;
                    ConnectionHandler.Instance.Disconnect();
                    Reset();
                    // timer.StopTimer();
                    IsOver = true;
                }
            }
        }

        public void OnRecieve(NetPeer fromPeer, NetDataReader dataReader, DeliveryMethod deliveryMethod)
        {
            var data = json.ConvertFromJson(dataReader.GetString());
            var answer = data.Item1;

            result = data.Item2;

            switch (answer)
            {
                case "Received":
                    timer.StopTimer();
                    break;
                case "Progress":
                    if (!Progress.IsCanceled)
                        Progress?.Set(result is float r ? r : -1.0f);
                    break;
                case "ConnectedToServer":
                    isConnectedToServer = bool.Parse(result.ToString());
                    goto default;
                default:
                    Progress?.Clear();
                    IsOver = true;
                    break;
            }
        }

        public void Exit()
        {
            Send(json.ConvertToJson("Exit", "Boolean", true));
        }

        private void Reset()
        {
            isConnectedToServer = false;
            client.DisconnectAll();
            client.Stop();
            if (!process.HasExited)
            {
                process.Kill();
            }

            string filePath = Path.Combine(BimEnvironment.Instance.StreamingAssetsPath, "TDMS Server/TDMSRPCServer.exe");

            var proccesses = System.Diagnostics.Process.GetProcessesByName("TDMSRPCServer");
            if (proccesses.Length > 0)
                process = proccesses[0];
            else
                process = System.Diagnostics.Process.Start(filePath);

            client.Start();
            Connect("localhost", 3333, "TDMS");
        }
    }
}