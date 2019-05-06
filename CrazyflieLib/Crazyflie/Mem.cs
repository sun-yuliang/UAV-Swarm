using CrazyflieLib.crtp;
using CrazyflieLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyflieLib.Crazyflie
{
    public class MemoryElement
    {
        public enum TYPE
        {
            TYPE_I2C = 0,
            TYPE_1W = 1,
            TYPE_DRIVER_LED = 0x10,
            TYPE_LOCO = 0x11,
            TYPE_TRAJ = 0x12,
            TYPE_LOCO2 = 0x13
        };

        public int id;
        public TYPE type;
        public int size;
        public Memory mem_handler;

        public MemoryElement(int id, TYPE type, int size, Memory mem_handler)
        {
            this.id = id;
            this.type = type;
            this.size = size;
            this.mem_handler = mem_handler;
        }

        static string type_to_string(TYPE t)
        {
            switch (t)
            {
                case TYPE.TYPE_I2C:
                    return "I2C";
                case TYPE.TYPE_1W:
                    return "1-wire";
                case TYPE.TYPE_DRIVER_LED:
                    return "LED driver";
                case TYPE.TYPE_LOCO:
                    return "Loco Positioning";
                case TYPE.TYPE_TRAJ:
                    return "Trajectory";
                case TYPE.TYPE_LOCO2:
                    return "Loco Positioning 2";
                default:
                    return "Unknown";
            }
        }

        public void new_data(object[] args)
        {
            new_data2((MemoryElement)args[0], (int)args[1], (byte[])args[2]);
        }

        private void new_data2(MemoryElement mem, int addr, byte[] data)
        {

        }
    }

    public class LED
    {
        public int r = 0;
        public int g = 0;
        public int b = 0;
        public int intensity = 100;

        void set(int r, int g, int b, int intensity = -1)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            if (intensity != -1)
                this.intensity = intensity;
        }
    }

    public class LEDDriverMemory : MemoryElement
    {
        private Action _update_finished_cb;
        private Action<MemoryElement, int> _write_finished_cb;

        public LED[] leds;
        public bool valid;

        public LEDDriverMemory(int id, TYPE type, int size, Memory mem_handler) : base(id, type, size, mem_handler)
        {
            leds = new LED[12];
        }

        public void write_data(Action<MemoryElement, int> write_finished_cb)
        {
            _write_finished_cb = write_finished_cb;
            var data = new List<byte>();
            foreach (var led in leds)
            {
                int R5 = ((((led.r & 0xFF) * 249 + 1014) >> 11) & 0x1F) * led.intensity / 100;
                int G6 = ((((led.g & 0xFF) * 253 + 505) >> 10) & 0x3F) * led.intensity / 100;
                int B5 = ((((led.b & 0xFF) * 249 + 1014) >> 11) & 0x1F) * led.intensity / 100;
                int tmp = (R5 << 11) | (G6 << 5) | (B5 << 0);
                data.Add((byte)(tmp >> 8));
                data.Add((byte)(tmp & 0xFF));
            }
            mem_handler.write(this, 0, data.ToArray(), true);
        }

        public void update(Action update_finished_cb)
        {
            if (update_finished_cb != null)
            {
                _update_finished_cb = update_finished_cb;
                valid = false;
                mem_handler.read(this, 0, 16);
            }
        }

        public void write_done(MemoryElement mem, int addr)
        {
            if (_write_finished_cb != null && mem.id == id)
            {
                _write_finished_cb(this, addr);
            }
        }
    }

    public class I2CElement : MemoryElement
    {
        private Action _update_finished_cb;
        private Action _write_finished_cb;
        public Dictionary<string, object> elements;
        public bool valid;
        public byte[] datav0;

        public I2CElement(int id, TYPE type, int size, Memory mem_handler) : base(id, type, size, mem_handler)
        {
            elements = new Dictionary<string, object>();
            valid = false;
        }

        public void new_data(object[] args)
        {
            new_data2((MemoryElement)args[0], (int)args[1], (byte[])args[2]);
        }

        private void new_data2(MemoryElement mem, int addr, byte[] data)
        {
            if (mem.id == this.id)
            {
                if(addr == 0)
                {
                    bool done = false;

                    if (BitConverter.ToInt32(data.Take(4).ToArray(), 0) == 0xBC)
                    {
                        elements.Add("version", data[5]);
                        elements.Add("radio_channel", data[6]);
                        elements.Add("radio_speed", data[7]);
                        elements.Add("pitch_trim", BitConverter.ToSingle(data.Skip(7).Take(4).ToArray(), 0));
                        elements.Add("roll_trim", BitConverter.ToSingle(data.Skip(11).Take(4).ToArray(), 0));

                        if ((byte)elements["version"] == 0)
                        {
                            done = true;
                        }
                        else if ((byte)elements["version"] == 1)
                        {
                            this.datav0 = data;
                            //this.mem_handler.
                        }
                    }
                }
            }
        }
    }

//     public class OWElement : MemoryElement
//     {
//         public int pid;
//         public string name;
//     }

    public class AnchorData
    {

    }

//     public class LocoMemory : MemoryElement
//     {
// 
//     }

    public class AnchorData2
    {

    }

//     public class LocoMemory2 : MemoryElement
//     {
// 
//     }

    public class Poly4D
    {

    }

//     public class TrajectoryMemory : MemoryElement
//     {
// 
//     }

    public class _ReadRequest
    {
        public MemoryElement mem;
        public int addr;
        private int _bytes_left;
        public byte[] data;
        public Crazyflie cf;

        private int _current_addr;

        public _ReadRequest(MemoryElement mem, int addr, int length, Crazyflie cf)
        {
            this.mem = mem;
            this.addr = addr;
            this._bytes_left = length;
            this.data = new byte[0];
            this.cf = cf;

            this._current_addr = addr;
        }

        public void start()
        {
            _request_new_chunk();
        }

        public void resend()
        {
            _request_new_chunk();
        }

        private void _request_new_chunk()
        {
            int new_len = _bytes_left;
            if (new_len > 20)
                new_len = 20;

            var pk = new CRTPPacket();
            pk.set_header(CRTPPort.MEM, 1);
            var data = new List<byte>(6);
            data.Add((byte)mem.id);
            data.AddRange(BitConverter.GetBytes(_current_addr));
            data.Add((byte)new_len);
            pk.data = data.ToArray();
            cf.send_packet(pk, data.Take(5).ToArray(), false, 1000);
        }

        public bool add_data(int addr, byte[] data)
        {
            int data_len = data.Length;
            if (addr != _current_addr)
                return false;

            var tmp = this.data.ToList();
            tmp.AddRange(data);
            this.data = tmp.ToArray();
            _bytes_left -= data_len;
            _current_addr += data_len;

            if (_bytes_left > 0)
            {
                _request_new_chunk();
                return false;
            }
            else
                return true;
        }
    }

    public class _WriteRequest
    {
        public MemoryElement mem;
        public int addr;
        private int _bytes_left;
        private byte[] _data;
        public byte[] data;
        public Crazyflie cf;

        private int _current_addr;

        private CRTPPacket _sent_packet;
        private byte[] _sent_reply;

        private int _addr_add;

        public _WriteRequest(MemoryElement mem, int addr, byte[] data, Crazyflie cf)
        {
            this.mem = mem;
            this.addr = addr;
            this._bytes_left = data.Length;
            this._data = data;
            this.data = new byte[0];
            this.cf = cf;

            this._current_addr = addr;

            this._addr_add = 0;
        }

        public void start()
        {
            _write_new_chunk();
        }

        public void resend()
        {
            cf.send_packet(_sent_packet, _sent_reply, false, 1000);
        }

        private void _write_new_chunk()
        {
            int new_len = _data.Length;
            if (new_len > 25)
                new_len = 25;

            var data = _data.Take(new_len);
            _data = _data.Skip(new_len).ToArray();

            var pk = new CRTPPacket();
            pk.set_header(CRTPPort.MEM, 2);
            var tmp = new List<byte>(5);
            tmp.Add((byte)mem.id);
            tmp.AddRange(BitConverter.GetBytes(_current_addr));
            _sent_reply = tmp.ToArray();
            tmp.AddRange(data);
            pk.data = tmp.ToArray();
            _sent_packet = pk;
            cf.send_packet(pk, _sent_reply, false, 1000);

            _addr_add = data.Count();
        }

        public bool write_done(int addr)
        {
            if (addr != _current_addr)
                return false;

            if (_data.Length > 0)
            {
                _current_addr += _addr_add;
                _write_new_chunk();
                return false;
            }
            else
                return true;
        }
    }

    public class Memory
    {
        public Caller mem_added_cb;
        public Caller mem_read_cb;
        public Caller mem_write_cb;

        public Crazyflie cf;
        public List<MemoryElement> mems;

        private Action _refresh_callback;
        private int _fetch_id;
        public int nbr_of_mems;
        private int _ow_mem_fetch_index;
        private Dictionary<int, _ReadRequest> _read_requests;
        private Dictionary<int, List<_WriteRequest>> _write_requests;
        private Dictionary<int, int> _ow_mems_left_to_update;
        private bool _getting_count;

        public Memory(Crazyflie crazyflie = null)
        {
            mem_added_cb = new Caller();
            mem_read_cb = new Caller();
            mem_write_cb = new Caller();

            cf = crazyflie;
            cf.add_port_callback((byte)CRTPPort.MEM, _new_packet_cb);
            cf.disconnected.add_callback(_disconnected);
            _clear_state();
        }

        public void _clear_state()
        {
            mems = new List<MemoryElement>();
            _refresh_callback = null;
            _fetch_id = 0;
            nbr_of_mems = 0;
            _ow_mem_fetch_index = 0;
            _read_requests = new Dictionary<int, _ReadRequest>();
            _write_requests = new Dictionary<int, List<_WriteRequest>>();
            _ow_mems_left_to_update = new Dictionary<int, int>();
        }

        public void _mem_update_done(MemoryElement mem)
        {
            if (_ow_mems_left_to_update.Keys.Contains(mem.id))
                _ow_mems_left_to_update.Remove(mem.id);

            if (_ow_mems_left_to_update.Count == 0)
            {
                if (_refresh_callback != null)
                {
                    _refresh_callback();
                    _refresh_callback = null;
                }
            }
        }

        public MemoryElement get_mem(int id)
        {
            foreach (var m in mems)
            {
                if (m.id == id)
                    return m;
            }
            return null;
        }

        public List<MemoryElement> get_mems(MemoryElement.TYPE type)
        {
            var ret = new List<MemoryElement>();

            foreach (var m in mems)
            {
                if (m.type == type)
                    ret.Add(m);
            }

            return ret;
        }

//         public OWElement ow_search(int pid, string name)
//         {
//             foreach (OWElement m in get_mems(MemoryElement.TYPE.TYPE_1W))
//             {
//                 if (m.pid == pid || m.name == name)
//                     return m;
//             }
//             return null;
//         }

        public bool write(MemoryElement memory, int addr, byte[] data, bool flush_queue = false)
        {
            var wreq = new _WriteRequest(memory, addr, data, cf);
            if (!_write_requests.Keys.Contains(memory.id))
                _write_requests.Add(memory.id, new List<_WriteRequest>());

            if (flush_queue)
                _write_requests[memory.id] = _write_requests[memory.id].Take(1).ToList();
            _write_requests[memory.id].Add(wreq);
            if (_write_requests[memory.id].Count == 1)
                wreq.start();

            return true;
        }

        public bool read(MemoryElement memory, int addr, int length)
        {
            if (_read_requests.Keys.Contains(memory.id))
                return false;

            var rreq = new _ReadRequest(memory, addr, length, cf);
            _read_requests.Add(memory.id, rreq);

            rreq.start();

            return true;
        }

        public void refresh(Action refresh_done_callback)
        {
            _refresh_callback = refresh_done_callback;
            _fetch_id = 0;
            foreach (var m in mems)
            {
                mem_read_cb.remove_callback(m.new_data);
                switch (m.GetType().Name)
                {
                    case "LEDDriverMemory":
                        //((LEDDriverMemory)m).
                        break;
                }
            }
        }

        private void _disconnected(object uri)
        {

        }

        private void _new_packet_cb(CRTPPacket packet)
        {

        }

    }
}
