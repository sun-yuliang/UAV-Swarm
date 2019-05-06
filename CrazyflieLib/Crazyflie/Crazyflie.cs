using System;
using System.Collections.Generic;
using System.Threading;
using CrazyflieLib.crtp;
using CrazyflieLib.Util;

namespace CrazyflieLib.Crazyflie
{
    public enum State
    {
        DISCONNECTED,
        INITIALIZED,
        CONNECTED,
        SETUP_FINISHED
    };

    public class Crazyflie
    {
        public Caller disconnected;
        public Caller connection_lost;
        public Caller link_established;
        public Caller connection_requested;
        public Caller connected;
        public Caller connection_failed;
        public Caller packet_received;
        public Caller packet_sent;
        public Caller link_quality_updated;

        public State state;

        public RadioDriver link;
        public _IncomingPacketHandler incomming;
        public Param param;
        public PlatformService platform;

        public string link_uri;

        public DateTime connected_ts;

        public Crazyflie(RadioDriver link = null)
        {
            this.state = State.DISCONNECTED;

            this.link = link;

            this.incomming = new _IncomingPacketHandler(this);
            this.incomming.start();

            this.param = new Param(this);

            this.platform = new PlatformService(this);

            this.link_uri = "";

            this.connected_ts = DateTime.MinValue;
        }

        public void open_link(string link_uri)
        {
            this.state = State.INITIALIZED;
            this.link_uri = link_uri;

            link = crtp.RadioDriver.get_link_driver(link_uri);
        }

        public void close_link()
        {
            if (this.link != null)
            {
                //this.commander.send_setpoint(0, 0, 0, 0);
                this.link.close();
                this.link = null;
            }
        }

        public bool is_connected()
        {
            return this.connected_ts != DateTime.MinValue;
        }

        public void add_port_callback(byte port, Action<CRTPPacket> cb)
        {
            this.incomming.add_port_callback(port, cb);
        }

        public void remove_port_callback(byte port, Action<CRTPPacket> cb)
        {
            this.incomming.remove_port_callback(port, cb);
        }

        private void _param_toc_updated_cb()
        {
            this.param.request_update_of_all_params();
        }

        private void _mems_updated_cb()
        {
            this.param.refresh_toc(_param_toc_updated_cb, 0);
        }

        private void _log_toc_updated_cb()
        {

        }

        public void send_packet(CRTPPacket pk, byte[] expected_reply = null, bool resend = false, int timeout = 200)
        {
            if (this.link != null)
            {
                if (expected_reply != null && expected_reply.Length > 0 && !resend && this.link.needs_resending)
                {

                }
                else if (resend)
                {

                }
                this.link.send_packet(pk);
            }
        }
    }

    class _CallbackContainer
    {
        public byte port;
        public byte port_mask;
        public byte channel;
        public byte channel_mask;
        public Action<CRTPPacket> callback;

        public _CallbackContainer(byte port, byte port_mask, byte channel, byte channel_mask, Action<CRTPPacket> callback)
        {
            this.port = port;
            this.port_mask = port_mask;
            this.channel = channel;
            this.channel_mask = channel_mask;
            this.callback = callback;
        }
    }

    public class _IncomingPacketHandler
    {
        Thread thread;
        Crazyflie cf;
        List<_CallbackContainer> cb;

        public _IncomingPacketHandler(Crazyflie cf)
        {
            this.cf = cf;
            this.cb = new List<_CallbackContainer>();
        }

        public void start()
        {
            thread = new Thread(run);
            thread.Start();
        }

        public void add_port_callback(byte port, Action<CRTPPacket> cb)
        {
            add_header_callback(cb, port, 0, 0xFF, 0);
        }

        public void remove_port_callback(byte port, Action<CRTPPacket> cb)
        {
            foreach (_CallbackContainer port_callback in this.cb)
            {
                if (port_callback.port == port && port_callback.callback == cb)
                    this.cb.Remove(port_callback);
            }
        }

        public void add_header_callback(Action<CRTPPacket> cb, byte port, byte channel, byte port_mask = 0xFF, byte channel_mask = 0xFF)
        {
            this.cb.Add(new _CallbackContainer(port, port_mask, channel, channel_mask, cb));
        }

        public void run()
        {
            while (true)
            {
                if(this.cf.link == null)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                CRTPPacket pk = this.cf.link.receive_packet(1000);

                if (pk == null)
                    continue;

                //this.cf.packet_received.call(pk);

                bool found = false;
                foreach(var cb in this.cb)
                {
                    if(cb.port == (pk.port & cb.port_mask) && cb.channel == (pk.channel & cb.channel_mask))
                    {
                        cb.callback(pk);

                        if (cb.port != 0xFF)
                            found = true;
                    }
                }

                if (!found)
                    ;
            }
        }
    }
}
