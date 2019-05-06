using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CrazyflieLib.Drivers;

namespace CrazyflieLib.crtp
{
    class _SharedRadio
    {
        public _SharedRadio(int devid)
        {
            radio = new Crazyradio();
            radio.init(devid);
        }

        public Crazyradio radio;
        public int usage_counter = 0;
    };

    class _RadioManager
    {
        public _RadioManager(int devid, int channel = 0, Crazyradio.DATA_RATE datarate = Crazyradio.DATA_RATE.DR_250KPS, long address = 0xE7E7E7E7E7)
        {
            _devid = devid;
            _channel = channel;
            _datarate = datarate;
            _address = address;

            lock (_radios)
            {
                _SharedRadio radio;
                if (_radios.ContainsKey(devid))
                    radio = _radios[devid];
                else
                {
                    radio = new _SharedRadio(devid);
                    _radios.Add(devid, radio);
                }
                radio.usage_counter++;
            }
        }

        public void close()
        {
            lock (_radios)
            {
                _SharedRadio radio = _radios[_devid];
                radio.usage_counter--;
                if (radio.usage_counter == 0)
                {
                    radio.radio.close();
                    _radios.Remove(_devid);
                }
            }
        }

        public Crazyradio enter()
        {
            lock (_radios)
            {
                _SharedRadio radio = _radios[_devid];
                radio.radio.set_channel(_channel);
                radio.radio.set_data_rate(_datarate);
                radio.radio.set_address(_address);
                return radio.radio;
            }
        }

        int _devid;
        int _channel;
        Crazyradio.DATA_RATE _datarate;
        long _address;
        static Dictionary<int, _SharedRadio> _radios = new Dictionary<int, _SharedRadio>();
    }
    
    public class RadioDriver
    {
        public class radio_interface
        {
            public Crazyradio.DATA_RATE rate;
            public int channel;

            public radio_interface(Crazyradio.DATA_RATE rate, int channel)
            {
                this.rate = rate;
                this.channel = channel;
            }
        };

        public string uri;

        _RadioManager _radio_manager = null;
        public Queue<CRTPPacket> in_queue = new Queue<CRTPPacket>();
        public Queue<CRTPPacket> out_queue = new Queue<CRTPPacket>();
        Thread _thread;
        public bool needs_resending = true;

        int _retry_before_disconnect = 100;

        int _curr_up = 0;
        int _curr_down = 0;

        bool _has_safelink = false;

        public static RadioDriver get_link_driver(string uri)
        {
            RadioDriver instance = new RadioDriver();
            instance.connect(uri);
            return instance;
        }

        public int connect(string uri)
        {
            Regex reg = new Regex("^radio://([0-9]+)(/(([0-9]+))(/((250K|1M|2M))?(/([A-F0-9]+))?)?)?$");
            Match match = reg.Match(uri);
            if (!match.Success)
                return 1;
            this.uri = uri;
            int channel = int.Parse(match.Groups[3].Value);
            if (channel == 0)
                channel = 2;
            Crazyradio.DATA_RATE datarate = Crazyradio.DATA_RATE.DR_2MPS;
            if (match.Groups[7].Value == "250K")
                datarate = Crazyradio.DATA_RATE.DR_250KPS;
            else if (match.Groups[7].Value == "1M")
                datarate = Crazyradio.DATA_RATE.DR_1MPS;
            else if (match.Groups[7].Value == "2M")
                datarate = Crazyradio.DATA_RATE.DR_2MPS;
            long addr = Convert.ToInt64(match.Groups[9].Value, 16);
            long address = addr != 0 ? addr : 0xE7E7E7E7E7;
            if (_radio_manager == null)
                _radio_manager = new _RadioManager(int.Parse(match.Groups[1].Value), channel, datarate, address);
            else
                return 2;
            Crazyradio cradio = _radio_manager.enter();
            if (cradio.version >= 40)
                cradio.set_arc(3);

            _thread = new Thread(run);
            _thread.Name = uri;
            _thread.Start();

            return 0;
        }

        public CRTPPacket receive_packet(int time = 0)
        {
            if (in_queue.Count > 0)
                return in_queue.Dequeue();
            else
            {
                if (time > 0)
                {
                    int steps = time / 10;
                    for (int i = 0; i < steps; i++)
                    {
                        Thread.Sleep(1);
                        if (in_queue.Count > 0)
                            return in_queue.Dequeue();
                    }
                }
            }
            return null;
        }

        public void send_packet(CRTPPacket pk)
        {
            long tick = DateTime.Now.Ticks;
            while (DateTime.Now.Ticks - tick < 20000000)
            {
                if (out_queue.Count == 0)
                {
                    out_queue.Enqueue(pk);
                    return;
                }
            }
            Console.WriteLine("RadioDriver: Could not send packet to copter");
        }

        public void close()
        {
            _thread.Abort();

            if(_radio_manager != null)
            {
                _radio_manager.close();
                _radio_manager = null;
            }

            in_queue.Clear();
            out_queue.Clear();
        }

        public int scan_interface(long address, out radio_interface[] result)
        {
            if (_radio_manager == null)
                _radio_manager = new _RadioManager(0);

            Crazyradio cradio = _radio_manager.enter();

            cradio.set_address(address);
            cradio.set_arc(1);

            cradio.set_data_rate(Crazyradio.DATA_RATE.DR_250KPS);
            int[] result1, result2, result3;
            int cnt1 = _scan_radio_channels(cradio, out result1);
            cradio.set_data_rate(Crazyradio.DATA_RATE.DR_1MPS);
            int cnt2 = _scan_radio_channels(cradio, out result2);
            cradio.set_data_rate(Crazyradio.DATA_RATE.DR_2MPS);
            int cnt3 = _scan_radio_channels(cradio, out result3);

            int total = cnt1 + cnt2 + cnt3;

            radio_interface[] interfaces = new radio_interface[total];
            int i = 0;
            while (i < cnt1)
            {
                interfaces[i] = new radio_interface(Crazyradio.DATA_RATE.DR_250KPS, result1[i]);
                i++;
            }
            while (i < cnt1 + cnt2)
            {
                interfaces[i] = new radio_interface(Crazyradio.DATA_RATE.DR_1MPS, result2[i - cnt1]);
                i++;
            }
            while (i < total)
            {
                interfaces[i] = new radio_interface(Crazyradio.DATA_RATE.DR_2MPS, result3[i - cnt1 - cnt2]);
                i++;
            }
            result = interfaces;

            _radio_manager.close();
            _radio_manager = null;

            return total;
        }

        private int _scan_radio_channels(Crazyradio cradio, out int[] output, int start = 0, int stop = 125)
        {
            return cradio.scan_channels(start, stop, new byte[1] { 0xFF }, out output);
        }

        private _radio_ack _send_packet_safe(Crazyradio cr, byte[] packet)
        {
            packet[0] &= 0xF3;
            packet[0] |= (byte)(_curr_up << 3 | _curr_down << 2);
            _radio_ack resp = cr.send_packet(packet);
            if (resp != null && resp.ack && resp.data != null && resp.data.Length > 0 && ((resp.data[0] & 4) == (_curr_down << 2)))
                _curr_down = 1 - _curr_down;
            if (resp.ack)
                _curr_up = 1 - _curr_up;
            return resp;
        }

        public void run()
        {
            byte[] dataOut = new byte[] { 0xFF };
            bool wait = false;
            int emptyCtr = 0;

            Crazyradio cradio = _radio_manager.enter();
            for (int i = 0; i < 10; i++)
            {
                _radio_ack resp = cradio.send_packet(new byte[] { 0xFF, 0x05, 0x01 });
                if (resp.data != null && resp.data.SequenceEqual(new byte[] { 0xFF, 0x05, 0x01 }))
                {
                    _has_safelink = true;
                    break;
                }
            }
            this.needs_resending = !this._has_safelink;

            _send_packet_safe(cradio, new byte[] { 0xF3 });
            _send_packet_safe(cradio, new byte[] { 0x5D, 0x05 });

            while (true)
            {
                _radio_ack ackStatus;
                cradio = _radio_manager.enter();
                if (_has_safelink)
                    ackStatus = _send_packet_safe(cradio, dataOut);
                else
                    ackStatus = cradio.send_packet(dataOut);

                if (ackStatus == null)
                {
                    Console.WriteLine("Dongle reported ACK status == null");
                    continue;
                }

                if (!ackStatus.ack)
                {
                    _retry_before_disconnect--;
                    if (_retry_before_disconnect == 0)
                    {
                        Console.WriteLine("Too many packets lost");
                        continue;
                    }
                }
                _retry_before_disconnect = 100;

                byte[] data = ackStatus.data;
                if (data != null && data.Length > 0)
                {
                    in_queue.Enqueue(new CRTPPacket(data[0], data.Skip(1).ToArray()));
                    wait = false;
                    emptyCtr = 0;
                }
                else
                {
                    emptyCtr += 1;
                    if (emptyCtr > 10)
                    {
                        emptyCtr = 10;
                        wait = true;
                    }
                    else
                    {
                        wait = false;
                    }
                }

                CRTPPacket outPacket = null;
                if (out_queue.Count > 0)
                    outPacket = out_queue.Dequeue();
                else
                {
                    if (wait)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            Thread.Sleep(1);
                            if (out_queue.Count > 0)
                            {
                                outPacket = out_queue.Dequeue();
                                break;
                            }
                        }
                    }
                }

                if (outPacket != null)
                {
                    List<byte> tmp = new List<byte>(outPacket.data.Length + 1) { outPacket.header };
                    tmp.AddRange(outPacket.data);
                    dataOut = tmp.ToArray();
                }
                else
                {
                    dataOut = new byte[] { 0xFF };
                }
            }
        }
    }
}
