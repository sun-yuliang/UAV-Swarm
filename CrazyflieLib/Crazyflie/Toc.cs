using CrazyflieLib.crtp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyflieLib.Crazyflie
{
    enum CMD_TOC
    {
        CMD_TOC_ELEMENT,
        CMD_TOC_INFO,
        CMD_TOC_ITEM_V2,
        CMD_TOC_INFO_V2
    };

    enum TOC_STATE
    {
        IDLE,
        GET_TOC_INFO,
        GET_TOC_ELEMENT
    };

    //Container for TocElements.
    public class Toc
    {
        public Dictionary<string, Dictionary<string, object>> toc = new Dictionary<string, Dictionary<string, object>>();

        //Clear the TOC
        public void clear()
        {
            toc.Clear();
        }

        //Add a new TocElement to the TOC container.
        public void add_element(object obj)
        {
            if (obj.GetType().Name == "ParamTocElement")
            {
                ParamTocElement element = (ParamTocElement)obj;
                if (toc.Keys.Contains(element.group))
                    toc[element.group].Add(element.name, obj);
                else
                    toc.Add(element.group, new Dictionary<string, object>() { { element.name, element } });
            }
        }

        //Get a TocElement element identified by complete name from the container.
        public object get_element_by_complete_name(string complete_name)
        {
            return this.get_element_by_id(this.get_element_id(complete_name));
        }

        //Get the TocElement element id-number of the element with the supplied name.
        public int get_element_id(string complete_name)
        {
            string[] str = complete_name.Split('.');
            if (str.Length != 0)
                return -1;
            var element = get_element(str[0], str[1]);
            if (element.GetType().Name == "ParamTocElement")
                return ((ParamTocElement)element).ident;
            else
                return -1;
        }

        //Get a TocElement element identified by name and group from the container.
        public object get_element(string group, string name)
        {
            try
            {
                return toc[group][name];
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        //Get a TocElement element identified by index number from the container.
        public object get_element_by_id(int ident)
        {
            foreach (var group in toc.Keys)
            {
                foreach(var name in toc[group].Keys)
                {
                    var element = toc[group][name];
                    if (element.GetType().Name == "ParamTocElement")
                        if (((ParamTocElement)element).ident == ident)
                            return element;
                }
                    
            }
            return null;
        }
    }

    //Fetches TOC entries from the Crazyflie
    class TocFetcher
    {
        public Crazyflie cf;
        public byte port;
        private int _crc;
        public object requested_index;
        public object nbr_of_items;
        public TOC_STATE state;
        public Toc toc;
        private int _toc_cache;
        public Action finished_callback;
        public object element_class;
        bool _useV2;

        public TocFetcher(Crazyflie crazyflie, object element_class, byte port, Toc toc_holder, Action finished_callback, int toc_cache)
        {
            this.cf = crazyflie;
            this.port = port;
            this._crc = 0;
            this.requested_index = null;
            this.nbr_of_items = null;
            this.state = TOC_STATE.IDLE;
            this.toc = toc_holder;
            this._toc_cache = toc_cache;
            this.finished_callback = finished_callback;
            this.element_class = element_class;
            this._useV2 = false;
        }

        public void start()
        {
            this._useV2 = this.cf.platform.get_protocol_version() >= 4;

            this.cf.add_port_callback(port, this._new_packet_cb);

            this.state = TOC_STATE.GET_TOC_INFO;

            CRTPPacket pk = new CRTPPacket();
            pk.set_header(port, 0); //TOC_CHANNEL
            if (this._useV2)
            {
                pk.data = new byte[] { (byte)CMD_TOC.CMD_TOC_INFO_V2 };
                this.cf.send_packet(pk, new byte[] { (byte)CMD_TOC.CMD_TOC_INFO_V2 });
            }
            else
            {
                pk.data = new byte[] { (byte)CMD_TOC.CMD_TOC_INFO_V2 };
                this.cf.send_packet(pk, new byte[] { (byte)CMD_TOC.CMD_TOC_INFO });
            }
        }

        private void _toc_fetch_finished()
        {
            this.cf.remove_port_callback(this.port, this._new_packet_cb);
            this.finished_callback();
        }

        private void _new_packet_cb(CRTPPacket packet)
        {
            byte chan = packet.channel;
            if (chan != 0)
                return;
            byte[] payload = packet.data.Skip(1).ToArray();

            if (state == TOC_STATE.GET_TOC_INFO)
            {
                if(_useV2)
                {

                }
                else
                {

                }
            }
            else if(state == TOC_STATE.GET_TOC_ELEMENT)
            {

            }
        }

        private void _request_toc_element(int index)
        {
            CRTPPacket pk = new CRTPPacket();
            pk.set_header(this.port, 0);
            if (_useV2)
            {
                pk.data = new byte[] { (byte)CMD_TOC.CMD_TOC_ITEM_V2, (byte)(index & 0xFF), (byte)((index >> 8) & 0xFF) };
                this.cf.send_packet(pk, pk.data);
            }
            else
            {
                pk.data = new byte[] { (byte)CMD_TOC.CMD_TOC_ELEMENT, (byte)index };
                this.cf.send_packet(pk, pk.data);
            }
        }
    }
}
