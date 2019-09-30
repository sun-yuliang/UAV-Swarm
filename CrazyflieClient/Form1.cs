using System;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;
using CrazyflieLib.crtp;
using static CrazyflieLib.crtp.RadioDriver;
using System.Runtime.InteropServices;
using CrazyflieLib.Crazyflie;

namespace CrazyflieClient
{
    public partial class Form1 : Form
    {
        RadioDriver[] rd;
        Crazyflie[] cf;

        string[] uri =
        {
            "radio://0/100/2M/E7E7E7E703",
            "radio://0/100/2M/E7E7E7E704",
            "radio://0/100/2M/E7E7E7E705",
            "radio://0/100/2M/E7E7E7E706"
        };

        static string[] rate = { "250K", "1M", "2M" };
        const int NUMS = 4;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();

            radio_interface[] interfaces;
            long addr = Convert.ToInt64(textBox1.Text, 16);
            cf[0].link.scan_interface(addr, out interfaces);

            foreach(radio_interface flie in interfaces)
                comboBox1.Items.Add("radio://0/" + flie.channel + "/" + rate[(int)flie.rate] + "/" + addr.ToString("X8"));
            if (comboBox1.Items.Count != 0)
            {
                comboBox1.SelectedIndex = 0;
                button_connect.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button_connect.Enabled = false;
            for (int i = 0; i < NUMS; i++)
            {
                textBox3.AppendText(string.Format("{0} {1} {2} {3}" + Environment.NewLine, i, pos[i].x, pos[i].y, pos[i].z));
                cf[i].open_link(uri[i]);
            }
            button_fly.Enabled = true;
//             Thread th = new Thread(SendSetPoint);
//             th.Start();
        }

        CRTPPacket CommanderPacket(float roll, float pitch, float yaw, ushort thrust)
        {
            CRTPPacket packet = new CRTPPacket();
            packet.port = (byte)CRTPPort.COMMANDER;

            List<byte> data = new List<byte>(14) { };
            data.AddRange(BitConverter.GetBytes(roll));
            data.AddRange(BitConverter.GetBytes(-pitch));
            data.AddRange(BitConverter.GetBytes(yaw));
            data.AddRange(BitConverter.GetBytes(thrust));

            packet.data = data.ToArray();
            return packet;
        }

        CRTPPacket VelocityPacket(float vx, float vy, float vz, float yawrate)
        {
            CRTPPacket packet = new CRTPPacket();
            packet.port = (byte)CRTPPort.COMMANDER_GENERIC;

            List<byte> data = new List<byte>(17) { 0x01 };
            data.AddRange(BitConverter.GetBytes(vx));
            data.AddRange(BitConverter.GetBytes(vy));
            data.AddRange(BitConverter.GetBytes(vz));
            data.AddRange(BitConverter.GetBytes(yawrate));

            packet.data = data.ToArray();
            return packet;
        }
        CRTPPacket SetPointPacket(float x, float y, float z, int yaw)
        {
            CRTPPacket packet = new CRTPPacket();
            packet.port = (byte)CRTPPort.COMMANDER_GENERIC;

            List<byte> data = new List<byte>(17) { 7 };
            data.AddRange(BitConverter.GetBytes(x));
            data.AddRange(BitConverter.GetBytes(y));
            data.AddRange(BitConverter.GetBytes(z));
            data.AddRange(BitConverter.GetBytes(yaw));

            packet.data = data.ToArray();
            return packet;
        }

        /*
        private void SendSetPoint2()
        {
            Thread.Sleep(5000);
            //cf.send_packet(CommanderPacket(0, 0, 0, 0));
            while (true)
            {
                Console.WriteLine(thrust);
                //cf.send_packet(CommanderPacket(0, 0, 0, thrust));
                cf.send_packet(CommanderPacket(1.90f, 2.85f, 0, (int)(0.20 * 1000)));
                Thread.Sleep(10);
            }
            cf.close_link();
            button2.Enabled = true;
        }
        */
        class Pos
        {
            public float x;
            public float y;
            public float z;

            public Pos(float x, float y, float z)
            {
                this.x = x >= 0 ? x : 0;
                this.y = y >= 0 ? y : 0;
                this.z = z >= 0 ? z : 0;
            }
        }

        Pos[] pos = new Pos[]
        {
            new Pos(1f, 1f, 0),
            new Pos(3.5f, 1f, 0),
            new Pos(1f, 11f, 0),
            new Pos(3.5f, 11f, 0)
        };
        
        private void SendSetPoint()
        {
            //Thread.Sleep(5000);
            Console.WriteLine("Let's move out.");
            while (true)
            {
                for (int i = 0; i < NUMS; i++)
                    cf[i].send_packet(SetPointPacket(pos[i].x, pos[i].y, pos[i].z, 0));
                Thread.Sleep(50);
            }
            for (int i = 0; i < NUMS; i++)
                cf[i].close_link();
            Invoke(new Action(() => { button_connect.Enabled = true; }));
        }
/*
        private void SendVelocity()
        {
            Thread.Sleep(3000);
            cf.send_packet(VelocityPacket(0, 0, 0, 0));
            for (int i = 0; i < 100; i++)
            {
                cf.send_packet(VelocityPacket(0, 0, 0.3f, 0));
                Thread.Sleep(10);
            }
            for (int i = 0; i < 100; i++)
            {
                cf.send_packet(VelocityPacket(0, 0, 0, 0));
                Thread.Sleep(10);
            }
            for (int i = 0; i < 100; i++)
            {
                cf.send_packet(VelocityPacket(0, 0, -0.3f, 0));
                Thread.Sleep(10);
            }
            cf.send_packet(VelocityPacket(0, 0, 0, 0));
            cf.close_link();
            Invoke(new Action(() => { button2.Enabled = true; }));
        }
        */

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, KeyModifiers fsModifiers, Keys vk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [Flags()]
        public enum KeyModifiers
        {
            None = 0,
            Alt = 1,
            Ctrl = 2,
            Shift = 4,
            WindowsKey = 8
        }

        ushort thrust = 0;

        private void Form1_Load(object sender, EventArgs e)
        {
            //rd = new RadioDriver[NUMS];
            cf = new Crazyflie[NUMS];
            for (int i = 0; i < NUMS; i++)
            {
                //rd[i] = new RadioDriver();
                cf[i] = new Crazyflie();
            }

            RegisterHotKey(Handle, (int)Keys.Up, KeyModifiers.None, Keys.Up);
            RegisterHotKey(Handle, (int)Keys.Down, KeyModifiers.None, Keys.Down);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            switch (m.Msg)
            {
                case WM_HOTKEY:
                    switch ((Keys)m.WParam.ToInt32())
                    {
                        case Keys.Up:
                            if(thrust < 50000)
                                thrust += 1000;
                            break;
                        case Keys.Down:
                            if (thrust > 0)
                                thrust -= 1000;
                            break;
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            const int WM_KEYDOWN = 0x0100;

            if (checkbox_keyboard.Checked)
            {
                if (msg.Msg == WM_KEYDOWN && msg.HWnd == checkbox_keyboard.Handle)
                {
                    bool changed = true;
                    switch (keyData)
                    {
                        case Keys.W:
                            pos[0].x += 0.1f;
                            break;
                        case Keys.S:
                            pos[0].x -= 0.1f;
                            break;
                        case Keys.A:
                            pos[0].y += 0.1f;
                            break;
                        case Keys.D:
                            pos[0].y -= 0.1f;
                            break;
                        case Keys.Q:
                            break;
                        case Keys.E:
                            break;
                        case Keys.ShiftKey | Keys.Shift:
                            pos[0].z -= 0.1f;
                            break;
                        case Keys.Space:
                            pos[0].z += 0.1f;
                            break;
                        default:
                            changed = false;
                            break;
                    }
                    if (changed)
                    {
                        textBox3.Clear();
                        for (int i = 0; i < NUMS; i++)
                            textBox3.AppendText(string.Format("{0} {1} {2} {3}" + Environment.NewLine, i, pos[i].x, pos[i].y, pos[i].z));
                    }
                }
                return true;
            }
            return false;
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string[] strs = textBox2.Text.Split(' ');
                if (strs.Length == 4)
                {
                    pos[int.Parse(strs[0])] = new Pos(float.Parse(strs[1]), float.Parse(strs[2]), float.Parse(strs[3]));
                    textBox3.Clear();
                    for (int i = 0; i < NUMS; i++)
                        textBox3.AppendText(string.Format("{0} {1} {2} {3}" + Environment.NewLine, i, pos[i].x, pos[i].y, pos[i].z));
                }
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Force exit
            Environment.Exit(0);
        }

        class Command
        {
            public int id;
            public float x;
            public float y;
            public float z;

            public Command(int id, float x, float y, float z)
            {
                this.id = id;
                this.x = x >= 0 ? x : 0;
                this.y = y >= 0 ? y : 0;
                this.z = z >= 0 ? z : 0;
            }
        }
        List<Command> commands = new List<Command>();

        private void SequenceThread()
        {
            Thread.Sleep(5000);

            Thread th2 = new Thread(SendSetPoint);
            th2.Start();

            for (int i = 0; i < commands.Count; i++)
            {
                int id = commands[i].id;
                if (id == -1)
                    Thread.Sleep((int)commands[i].x);
                else
                    pos[id] = new Pos(commands[i].x, commands[i].y, commands[i].z);
            }
        }

        private void button_fly_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                button_fly.Enabled = false;
                System.IO.Stream file = openFileDialog1.OpenFile();
                byte[] file_buf = new byte[file.Length];
                file.Read(file_buf, 0, (int)file.Length);
                file.Close();
                string file_string = System.Text.Encoding.Default.GetString(file_buf);

                string[] lines = file_string.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                commands.Clear();
                foreach (string line in lines)
                {
                    string[] param = line.Split(' ');
                    commands.Add(new Command(int.Parse(param[0]), float.Parse(param[1]), float.Parse(param[2]), float.Parse(param[3])));
                }

                Thread th1 = new Thread(SequenceThread);
                th1.Start();
            }
        }
    }
}
