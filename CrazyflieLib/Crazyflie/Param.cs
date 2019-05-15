using CrazyflieLib.crtp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CrazyflieLib.Crazyflie
{
    class ParamTocElement
    {
        public const int RW_ACCESS = 0;
        public const int RO_ACCESS = 1;

        public enum ELEMENT_TYPE
        {
            int8_t,
            int16_t,
            int32_t,
            int64_t,
            float_t = 6,
            double_t,
            uint8_t,
            uint16_t,
            uint32_t,
            uint64_t
        };

        public int ident;
        public string group;
        public string name;
        public byte metadata;
        public ELEMENT_TYPE type;
        public int access;

        public ParamTocElement(int ident = 0, byte[] data = null)
        {
            this.ident = ident;
            if (data != null)
            {
                string s = Encoding.Default.GetString(data);
                string[] strs = s.Split('\0');
                this.group = strs[0];
                this.name = strs[1];

                metadata = data[0];

                type = (ELEMENT_TYPE)(metadata & 0x0F);
                if ((metadata & 0x40) != 0)
                    access = ParamTocElement.RO_ACCESS;
                else
                    access = ParamTocElement.RW_ACCESS;
            }
        }

        public string get_readable_access()
        {
            if (access == ParamTocElement.RO_ACCESS)
                return "RO";
            return "RW";
        }
    }

    public class Param
    {
        enum STATE
        {
            IDLE,
            WAIT_TOC,
            WAIT_READ,
            WAIT_WRITE
        }

        public enum CHANNEL
        {
            TOC_CHANNEL,
            READ_CHANNEL,
            WRITE_CHANNEL
        }

        public Toc toc;

        public Crazyflie cf;
        private bool _useV2;
        public _ParamUpdater param_updater;

        public bool is_updated;

        public Param(Crazyflie crazyflie)
        {
            toc = new Toc();

            cf = crazyflie;
            _useV2 = false;
            param_updater = new _ParamUpdater(cf, _useV2, null);

            is_updated = false;
        }

        public void request_update_of_all_params()
        {
            foreach (var group in toc.toc.Keys)
                foreach (var name in toc.toc[group].Keys)
                    this.request_param_update(String.Format("{0}.{1}", group, name));
        }

        private bool _check_if_all_updated()
        {
            return true;
        }

        private void _param_updated(CRTPPacket pk)
        {

        }

        public void remove_update_callback()
        {

        }

        public void add_update_callback()
        {

        }

        public void refresh_toc(Action refresh_done_callback, int toc_cache)
        {
            _useV2 = this.cf.platform.get_protocol_version() >= 4;
            TocFetcher toc_fetcher = new TocFetcher(this.cf, this, (byte)CRTPPort.PARAM, this.toc, refresh_done_callback, 0);
            toc_fetcher.start();
        }

        private void _disconnected(string uri)
        {
            this.param_updater.close();

            toc.clear();
        }

        public void request_param_update(string complete_name)
        {
            this.param_updater.request_param_update(toc.get_element_id(complete_name));
        }

        public void set_value(string complete_name, string value)
        {
            ParamTocElement element = (ParamTocElement)toc.get_element_by_complete_name(complete_name);

            if (element != null && element.access == ParamTocElement.RW_ACCESS)
            {
                int varid = element.ident;
                CRTPPacket pk = new CRTPPacket();
                pk.set_header(CRTPPort.PARAM, (byte)CHANNEL.WRITE_CHANNEL);
                List<byte> data = new List<byte>() { };
                if (this._useV2)
                    data.AddRange(BitConverter.GetBytes((ushort)varid));
                else
                    data.AddRange(BitConverter.GetBytes((byte)varid));

                switch (element.type)
                {
                    case ParamTocElement.ELEMENT_TYPE.int8_t:
                        data.AddRange(BitConverter.GetBytes(char.Parse(value)));
                        break;
                    case ParamTocElement.ELEMENT_TYPE.int16_t:
                        data.AddRange(BitConverter.GetBytes(short.Parse(value)));
                        break;
                    case ParamTocElement.ELEMENT_TYPE.int32_t:
                        data.AddRange(BitConverter.GetBytes(int.Parse(value)));
                        break;
                    case ParamTocElement.ELEMENT_TYPE.int64_t:
                        data.AddRange(BitConverter.GetBytes(long.Parse(value)));
                        break;
                    case ParamTocElement.ELEMENT_TYPE.float_t:
                        data.AddRange(BitConverter.GetBytes(float.Parse(value)));
                        break;
                    case ParamTocElement.ELEMENT_TYPE.double_t:
                        data.AddRange(BitConverter.GetBytes(double.Parse(value)));
                        break;
                    case ParamTocElement.ELEMENT_TYPE.uint8_t:
                        data.AddRange(BitConverter.GetBytes(byte.Parse(value)));
                        break;
                    case ParamTocElement.ELEMENT_TYPE.uint16_t:
                        data.AddRange(BitConverter.GetBytes(ushort.Parse(value)));
                        break;
                    case ParamTocElement.ELEMENT_TYPE.uint32_t:
                        data.AddRange(BitConverter.GetBytes(uint.Parse(value)));
                        break;
                    case ParamTocElement.ELEMENT_TYPE.uint64_t:
                        data.AddRange(BitConverter.GetBytes(ulong.Parse(value)));
                        break;
                    default:
                        break;
                }
                this.param_updater.request_param_setvalue(pk);
            }
        }
    }

    public class _ParamUpdater
    {
        public Thread th;
        public Crazyflie cf;
        private bool _useV2;
        public Action<CRTPPacket> updated_callback;
        public Queue<CRTPPacket> request_queue;
        private bool _should_close;
        private int _req_param;

        public _ParamUpdater(Crazyflie cf, bool useV2, Action<CRTPPacket> updated_callback)
        {
            this.cf = cf;
            this._useV2 = useV2;
            this.updated_callback = updated_callback;
            this.request_queue = new Queue<CRTPPacket>();
            this.cf.add_port_callback((byte)CRTPPort.PARAM, _new_packet_cb);
            this._should_close = false;
            this._req_param = -1;
        }

        public void start()
        {
            th = new Thread(run);
            th.Start();
        }

        public void close()
        {
            th.Abort();
            request_queue.Clear();
        }

        public void request_param_setvalue(CRTPPacket pk)
        {
            request_queue.Enqueue(pk);
        }

        private void _new_packet_cb(CRTPPacket pk)
        {
            if(pk.channel == (byte)Param.CHANNEL.READ_CHANNEL || pk.channel == (byte)Param.CHANNEL.WRITE_CHANNEL)
            {
                int var_id;
                if (_useV2)
                {
                    var_id = BitConverter.ToUInt16(pk.data, 0);
                    if (pk.channel == (byte)Param.CHANNEL.READ_CHANNEL)
                    {
                        List<byte> data = new List<byte>();
                        data.AddRange(pk.data.Take(2));
                        data.AddRange(pk.data.Skip(3));
                        pk.data = data.ToArray();
                    }
                }
                else
                {
                    var_id = pk.data[0];
                }
                if (pk.channel == (byte)Param.CHANNEL.TOC_CHANNEL && this._req_param == var_id && pk != null)
                {
                    this.updated_callback(pk);
                    this._req_param = -1;
                }
            }
        }

        public void request_param_update(int var_id)
        {
            CRTPPacket pk = new CRTPPacket();
            pk.set_header(CRTPPort.PARAM, (byte)Param.CHANNEL.READ_CHANNEL);
            if (this._useV2)
                pk.data = BitConverter.GetBytes((ushort)var_id);
            else
                pk.data = BitConverter.GetBytes((byte)var_id);
            this.request_queue.Enqueue(pk);
        }

        public void run()
        {
            while (!_should_close)
            {
                CRTPPacket pk = request_queue.Dequeue();
                if (this.cf.link != null)
                {
                    if(this._useV2)
                    {
                        this._req_param = BitConverter.ToUInt16(pk.data.Take(2).ToArray(), 0);
                        this.cf.send_packet(pk, pk.data.Take(2).ToArray());
                    }
                    else
                    {
                        this._req_param = pk.data[0];
                        this.cf.send_packet(pk, pk.data.Take(1).ToArray());
                    }
                }
            }
        }
    }
}
