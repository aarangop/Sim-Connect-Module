using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockheedMartin.Prepar3D.SimConnect;
using PilotAssistDll.Helpers;
using PilotAssistModels;

namespace SimConnectModule
{
    public static class ScManagedLib
    {
        #region Fields

        // SimConnect object
        private static SimConnect _simConnect = null;
        private const int _WM_USER_SIMCONNECT = 0x0403;
        private static SCConnectionStatus _connStatus = SCConnectionStatus.Disconnected;
        private static IntPtr _windowHandle;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static bool _stopDataRequestLoop = true;
        private static bool _sendingHudData = false;

        #endregion

        #region Properties

        #region Private Properties

        #endregion

        #region Public Properties
        public static SCConnectionStatus ConnectionStatus { get => _connStatus; }

        public static int WM_USER_SIMCONNECT { get => _WM_USER_SIMCONNECT; }

        public static SimConnect SimConnectInstance { get => _simConnect; }

        public static bool SendingHudData { get => _sendingHudData; }

        #endregion

        #endregion

        #region Events

        public static event EventHandler<SimConnectDataRecvArgs> OnRecvData;

        public delegate void OnSimConnectRecv();

        public static event OnSimConnectRecv OnRecvOpen;
        public static event OnSimConnectRecv OnRecvQuit;

        #endregion

        #region Enums
        public enum SCConnectionStatus
        {
            Connected,
            Disconnected,
            Connecting
        }

        #endregion

        #region Event Handlers

        private static void _simConnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            ScData.ProcessDataRecv(data.dwData[0], (SIMVAR_CATEGORY)data.dwDefineID);

            SimConnectDataRecvArgs args = new SimConnectDataRecvArgs(data);

            log.Info(data);

            OnRecvData?.Invoke(sender, args);
        }

        private static void _simConnect_OnRecvSimObjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            ScData.ProcessDataRecv(data.dwData[0], (SIMVAR_CATEGORY)data.dwDefineID);

            SimConnectDataRecvArgs args = new SimConnectDataRecvArgs(data);

            OnRecvData?.Invoke(sender, args);
        }

        public async static void _simConnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            _connStatus = SCConnectionStatus.Connected;
            await RegisterAllStructs();
            OnRecvOpen?.Invoke();
        }

        public static void RequestAllSimData()
        {
            ScData.RequestDataAllDataStructs(_simConnect, _WM_USER_SIMCONNECT);
        }

        public async static void _simConnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {

            CloseConnection();
            _connStatus = SCConnectionStatus.Disconnected;
            OnRecvQuit?.Invoke();

            await Task.Run(async () => await AttemptConnection());
        }

        public static void SetWindowHandle(IntPtr handle)
        {
            _windowHandle = handle;
        }

        #endregion

        #region Public Methods

        public static async Task AttemptConnection()
        {
            while (_simConnect == null)
            {

                if (_windowHandle != IntPtr.Zero)
                {
                    try
                    {
                        _simConnect = new SimConnect("Managed Data Request", _windowHandle, _WM_USER_SIMCONNECT, null, 0);
                        
                        // Subscribe to Open and Quit events
                        _simConnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(_simConnect_OnRecvOpen);
                        _simConnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(_simConnect_OnRecvQuit);

                        // catch a simobject data request
                        _simConnect.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler(_simConnect_OnRecvSimobjectDataBytype);
                        _simConnect.OnRecvSimobjectData += new SimConnect.RecvSimobjectDataEventHandler(_simConnect_OnRecvSimObjectData);
                    }
                    catch (Exception e)
                    {
                        string message = e.Message;
                    }
                }
                await Task.Delay(1000);
            }
        }

        public static void ReceiveMessage()
        {
            if (_simConnect != null)
            {
                _simConnect.ReceiveMessage();
            }
        }

        public static void CloseConnection()
        {
            if (_simConnect != null)
            {
                _simConnect.Dispose();
                _simConnect = null;
            }
        }

        public static bool IsDataRequestLoopActive()
        {
            return !_stopDataRequestLoop;
        }

        public static async Task StartDataRequestLoop()
        {
            // Avoid calling this function simultaneously
            if (IsDataRequestLoopActive()) return;

            _stopDataRequestLoop = false;

            while (!_stopDataRequestLoop)
            {
                ScManagedLib.RequestSimData();

                await Task.Delay(200);
            }
        }

        public static void StartHudDataStream()
        {
            OnRecvData += SendHudData;
            _sendingHudData = true;
        }

        public static void StopHudDataStream()
        {
            OnRecvData -= SendHudData;
            _sendingHudData = false;
        }

        public static void SendHudData(object obj, SimConnectDataRecvArgs args)
        {
            if (!MqttManager.Client.IsConnected || !_sendingHudData) return;

            byte[] msg =  ModelSerializer.StrucToByteArray(ScData.ACInstrData);

            MqttManager.Client.Publish(MqttTopics.ServerPublishTopics[MqttTopics.ServerPublish.HudData], msg);
            
        }

        public static void StopDataRequestLoop()
        {
            _stopDataRequestLoop = true;
        }

        public static async Task RegisterDataStruct(SIMVAR_CATEGORY category)
        {
            await ScData.RegisterStruct(_simConnect, category);
        }

        public async static Task RegisterAllStructs()
        {
            await ScData.RegisterStruct(_simConnect, SIMVAR_CATEGORY.ENGINE_DATA);
            await ScData.RegisterStruct(_simConnect, SIMVAR_CATEGORY.AIRCRAFT_MISCELANEOUS);
            await ScData.RegisterStruct(_simConnect, SIMVAR_CATEGORY.CONTROLS);
            await ScData.RegisterStruct(_simConnect, SIMVAR_CATEGORY.FLIGHT_INSTRUMENTATION);
            await ScData.RegisterStruct(_simConnect, SIMVAR_CATEGORY.LANDING_GEAR);
        }

        public static void RequestSimData(SIMVAR_CATEGORY category)
        {
            _simConnect.RequestDataOnSimObjectType(category, category, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
        }

        public static void RequestSimData()
        {
            foreach (KeyValuePair<SIMVAR_CATEGORY, bool> cat in ScData.RegisteredDataStructs)
            {
                _simConnect.RequestDataOnSimObjectType(cat.Key, cat.Key, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
            }
        }

        #endregion
    }
}
