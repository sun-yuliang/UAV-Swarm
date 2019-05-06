using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyflieLib.Crazyflie
{
    public class PlatformService
    {
        private Crazyflie _cf;

        private bool _has_protocol_version;
        private int _protocolVersion;
        private object _callback;

        public PlatformService(Crazyflie crazyflie = null)
        {
            _cf = crazyflie;

            _has_protocol_version = false;
            _protocolVersion = -1;
            _callback = null;
        }

        public void fetch_platform_informations(int callback)
        {

        }

        public void set_continous_wave(bool enabled)
        {
            
        }

        public int get_protocol_version()
        {
            return _protocolVersion;
        }

        private void _request_protocol_version()
        {

        }

        private void _crt_service_callback(byte[] pk)
        {

        }

        private void _platform_callback(byte[] pk)
        {

        }
    }
}
