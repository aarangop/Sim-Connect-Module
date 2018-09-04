using LockheedMartin.Prepar3D.SimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimConnectModule
{
    public class ScRecvArgs
    {
        public SIMCONNECT_RECV RecvData;

        public ScRecvArgs(SIMCONNECT_RECV recvData)
        {
            RecvData = recvData;
        }
    }
}
