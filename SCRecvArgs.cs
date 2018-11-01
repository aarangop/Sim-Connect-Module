using LockheedMartin.Prepar3D.SimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimConnectModule
{
    public class SimConnectDataRecvArgs
    {
        public SIMCONNECT_RECV_SIMOBJECT_DATA RecvData;

        public SimConnectDataRecvArgs(SIMCONNECT_RECV_SIMOBJECT_DATA recvData)
        {
            RecvData = recvData;
        }
    }
}
