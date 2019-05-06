using LibUsbDotNet;
using LibUsbDotNet.Main;
using System;
using System.Linq;

namespace CrazyflieLib.Drivers
{
    public class _radio_ack
    {
        public bool ack = false;
        public bool powerDet = false;
        public int retry = 0;
        public byte[] data = null;
    };

    public class Crazyradio
    {
        public enum DATA_RATE
        {
            DR_250KPS,
            DR_1MPS,
            DR_2MPS
        };

        public enum POWER
        {
            P_M18DBM,
            P_M12DBM,
            P_M6DBM,
            P_0DBM
        };

        public enum RADIO_ERR
        {
            DONGLE_NOT_FOUND,
            OUT_OF_RANGE,
            GET_DEVICE_FAILED,
            FIRMWARE_OUT_OF_DATE
        };

        internal enum CrazyradioRequest
        {
            SET_RADIO_CHANNEL = 0x01,
            SET_RADIO_ADDRESS =	0x02,
            SET_DATA_RATE	  =	0x03,
            SET_RADIO_POWER	  = 0x04,
            SET_RADIO_ARD	  =	0x05,
            SET_RADIO_ARC	  =	0x06,
            ACK_ENABLE		  =	0x10,
            SET_CONT_CARRIER  =	0x20,
            SCANN_CHANNELS	  =	0x21,
            LAUNCH_BOOTLOADER =	0xFF
        };

        public int current_channel = 0;
        public long current_address = 0;
        public DATA_RATE current_datarate = DATA_RATE.DR_250KPS;
        public int arc = 0;

        UsbDevice MyUsbDevice;
        UsbEndpointWriter writer;
        UsbEndpointReader reader;

        public ushort version;

        public int init(int device, int devid = 0)
        {
            UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(0x1915, 0x7777);
            var crazyRadiosRegDeviceList = UsbDevice.AllDevices.FindAll(MyUsbFinder);
            foreach (UsbRegistry reg in crazyRadiosRegDeviceList)
            {
                if (reg.Device != null)
                {
                    MyUsbDevice = reg.Device;
                    break;
                }
            }
            if (MyUsbDevice == null)
                return 1;

            IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
            if (!ReferenceEquals(wholeUsbDevice, null))
            {
                wholeUsbDevice.SetConfiguration(1);
                wholeUsbDevice.ClaimInterface(0);
            }

            writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);
            reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);

            version = (ushort)(MyUsbDevice.Info.Descriptor.BcdDevice & 0xFF);
            if (version < 30)
                return 2;

            this.set_data_rate(DATA_RATE.DR_2MPS);
            this.set_channel(2);
            this.arc = -1;
            if (this.version >= 40)
            {
                this.set_cont_carrier(false);
                this.set_address(0xE7E7E7E7E7);
                this.set_power(POWER.P_0DBM);
                this.set_arc(3);
                this.set_ard_bytes(32);
                this.set_ack_enable(true);
            }
            return 0;
        }

        public void close()
        {
            MyUsbDevice.Close();

            this.current_channel = 0;
            this.current_address = 0xE7E7E7E7E7;
            this.current_datarate = DATA_RATE.DR_250KPS;
        }

        private void ControlTransferOut(CrazyradioRequest request, short value, short index, short length, byte[] data)
        {
            ControlTransfer(UsbRequestType.TypeVendor, UsbRequestRecipient.RecipDevice, UsbEndpointDirection.EndpointOut, (byte)request, value, index, length, data);
        }

        private void ControlTransferIn(CrazyradioRequest request, short value, short index, short length, byte[] data)
        {
            ControlTransfer(UsbRequestType.TypeVendor, UsbRequestRecipient.RecipDevice, UsbEndpointDirection.EndpointIn, (byte)request, value, index, length, data);
        }

        private bool ControlTransfer(UsbRequestType requestType, UsbRequestRecipient requestRecipient, UsbEndpointDirection requestDirection, byte request, short value, short index, short length, byte[] data)
        {
            var lengthTransferred = -1;

            var requestTypeByte = (byte)requestType;
            var requestRecipientByte = (byte)requestRecipient;
            var requestDirectionByte = (byte)requestDirection;
            var fullRequestTypeByte = (byte)(requestTypeByte | requestRecipientByte | requestDirectionByte);
            var setupPacket = new UsbSetupPacket(fullRequestTypeByte, request, value, index, length);

            if (data == null) data = new byte[0];
            return MyUsbDevice.ControlTransfer(ref setupPacket, data, data.Length, out lengthTransferred);
        }

        public void set_channel(int channel)
        {
            if (channel != this.current_channel)
            {
                ControlTransferOut(CrazyradioRequest.SET_RADIO_CHANNEL, (short)channel, 0, 0, null);
                this.current_channel = channel;
            }
        }

        public void set_address(long address)
        {
            if (address != this.current_address)
            {
                ControlTransferOut(CrazyradioRequest.SET_RADIO_ADDRESS, 0, 0, 5, BitConverter.GetBytes(address).Take(5).Reverse().ToArray());
                this.current_address = address;
            }
        }

        public void set_data_rate(DATA_RATE datarate)
        {
            if (datarate != this.current_datarate)
            {
                ControlTransferOut(CrazyradioRequest.SET_DATA_RATE, (short)datarate, 0, 0, null);
                this.current_datarate = datarate;
            }
        }

        public void set_power(POWER power)
        {
            ControlTransferOut(CrazyradioRequest.SET_RADIO_POWER, (short)power, 0, 0, null);
        }

        public void set_arc(int arc)
        {
            ControlTransferOut(CrazyradioRequest.SET_RADIO_ARC, (short)arc, 0, 0, null);
        }

        public void set_ard_time(int us)
        {
            // Auto Retransmit Delay:
            // 0000 - Wait 250uS
            // 0001 - Wait 500uS
            // 0010 - Wait 750uS
            // ........
            // 1111 - Wait 4000uS

            // Round down, to value representing a multiple of 250uS

            int t = us / 250 - 1;
            if (t < 0) t = 0;
            if (t > 0xF) t = 0xF;
            ControlTransferOut(CrazyradioRequest.SET_RADIO_ARD, (short)t, 0, 0, null);
        }

        public void set_ard_bytes(int nbytes)
        {
            ControlTransferOut(CrazyradioRequest.SET_RADIO_ARD, (short)(0x80 | nbytes), 0, 0, null);
        }

        public void set_cont_carrier(bool active)
        {
            ControlTransferOut(CrazyradioRequest.SET_CONT_CARRIER, (short)(active ? 1 : 0), 0, 0, null);
        }

        public void set_ack_enable(bool enable)
        {
            ControlTransferOut(CrazyradioRequest.ACK_ENABLE, (short)(enable ? 1 : 0), 0, 0, null);
        }

        public int scan_selected(int selected)
        {
            return 0;
        }

        public int scan_channels(int start, int stop, byte[] packet, out int[] output)
        {
            int count = stop - start + 1;
            int result = 0;
            int[] hit = new int[count];
            for (int i = start; i <= stop; i++)
            {
                this.set_channel(i);
                _radio_ack status = this.send_packet(packet);
                if (status.ack)
                {
                    hit[result] = i;
                    result++;
                }
            }
            int[] realhit = new int[result];
            for (int i = 0; i < result; i++)
                realhit[i] = hit[i];
            output = realhit;
            return result;
        }

        public _radio_ack send_packet(byte[] dataOut)
        {
            _radio_ack ackIn = null;
            int transferred;
            writer.Write(dataOut, 1000, out transferred);
            if (transferred == dataOut.Length)
            {
                byte[] data = new byte[64];
                reader.Read(data, 1000, out transferred);
                if (transferred > 0)
                {
                    ackIn = new _radio_ack();
                    if (data[0] != 0)
                    {
                        ackIn.ack = (data[0] & 0x01) != 0;
                        ackIn.powerDet = (data[0] & 0x02) != 0;
                        ackIn.retry = data[0] >> 4;
                        ackIn.data = data.Take(transferred).Skip(1).ToArray();
                    }
                    else
                    {
                        ackIn.retry = this.arc;
                    }
                }
            }
            return ackIn;
        }
    }
}
