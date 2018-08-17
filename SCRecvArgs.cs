using LockheedMartin.Prepar3D.SimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimConnectModule
{
    public class SCRecvEventArgs
    {
        public SIMCONNECT_RECV RecvData;

        public SCRecvEventArgs(SIMCONNECT_RECV recvData)
        {
            RecvData = recvData;
        }
    }
}
