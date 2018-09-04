using System;
using System.Threading.Tasks;
using LockheedMartin.Prepar3D.SimConnect;

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

        #endregion

        #region Properties

        #region Private Properties

        #endregion

        #region Public Properties
        public static SCConnectionStatus ConnectionStatus { get => _connStatus; }

        public static int WM_USER_SIMCONNECT { get => _WM_USER_SIMCONNECT; }

        public static SimConnect SimConnectInstance { get => _simConnect; }

        #endregion

        #endregion

        #region Events

        public static event EventHandler<ScRecvArgs> OnRecvData;

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
            ScData.ProcessDataRecv(data);

            ScRecvArgs args = new ScRecvArgs(data);

            OnRecvData?.Invoke(sender, args);
        }

        public static void _simConnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            _connStatus = SCConnectionStatus.Connected;

            // Register all available data structs when the connection is opened.
            ScData.RegisterDataStructs(_simConnect);
            OnRecvOpen?.Invoke();
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

        public static void RegisterDataStruct()
        {
            ScData.RegisterStruct(_simConnect, SIMVAR_CATEGORY.ENGINE_DATA);

            _simConnect.RequestDataOnSimObjectType(SIMVAR_CATEGORY.ENGINE_DATA, SIMVAR_CATEGORY.ENGINE_DATA, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
        }

        #endregion
    }
}
