using System;
using System.Threading.Tasks;
using LockheedMartin.Prepar3D.SimConnect;

namespace SimConnectModule
{
    public static class SimConnectManaged
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

        #endregion

        #endregion

        #region Events

        public static event EventHandler<SCRecvEventArgs> OnRecvData;

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
            SCRecvEventArgs args = new SCRecvEventArgs(data);

            OnRecvData?.Invoke(sender, args);
        }

        public static void _simConnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            _connStatus = SCConnectionStatus.Connected;
            OnRecvOpen?.Invoke();
        }

        public static void _simConnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            _connStatus = SCConnectionStatus.Disconnected;
            OnRecvQuit?.Invoke();
        }

        public static void SetWindowHandle(IntPtr handle)
        {
            _windowHandle = handle;
        }

        #endregion

        #region Public Methods


        public static async void AttemptConnection()
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
            _simConnect.ReceiveMessage();
        }

        public static void CloseConnection()
        {
            if (_simConnect != null)
            {
                _simConnect.Dispose();
                _simConnect = null;
            }
        }

        #endregion
    }
}
