using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyflieLib.crtp
{
    public enum CRTPPort
    {
        CONSOLE = 0x00,
        PARAM = 0x02,
        COMMANDER = 0x03,
        MEM = 0x04,
        LOGGING = 0x05,
        LOCALIZATION = 0x06,
        COMMANDER_GENERIC = 0x07,
        SETPOINT_HL = 0x08,
        PLATFORM = 0x0D,
        DEBUGDRIVER = 0x0E,
        LINKCTRL = 0x0F,
        ALL = 0xFF
    };

    public class CRTPPacket
    {
        public int size;
        public byte[] data;
        public byte header;
        private byte _port;
        private byte _channel;

        public CRTPPacket(byte header = 0, byte[] data = null)
        {
            this.size = 0;
            this.data = new byte[] { };
            this.header = (byte)(header | 0x3 << 2);
            this._port = (byte)((header & 0xF0) >> 4);
            this._channel = (byte)(header & 0x03);
            if (data != null)
                this.data = data;
        }

        public byte get_header()
        {
            this._update_header();
            return this.header;
        }

        public void set_header(byte port, byte channel)
        {
            this._port = port;
            this.channel = channel;
            this._update_header();
        }

        public void set_header(CRTPPort port, byte channel)
        {
            this._port = (byte)port;
            this.channel = channel;
            this._update_header();
        }

        private void _update_header()
        {
            this.header = (byte)((this._port & 0x0f) << 4 | 3 << 2 | (this.channel & 0x03));
        }

        public List<byte> datal
        {
            get
            {
                return this.data.ToList();
            }
        }

        public byte port
        {
            get
            {
                return this._port;
            }
            set
            {
                this._port = value;
                this._update_header();
            }
        }

        public byte channel
        {
            get
            {
                return this._channel;
            }
            set
            {
                this._channel = value;
                this._update_header();
            }
        }
    }
}
