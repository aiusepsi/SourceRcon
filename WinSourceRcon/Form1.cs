using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using SourceRconLib;

namespace WinSourceRcon
{
    public partial class Form1 : Form
    {
        Rcon sr;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            sr = new Rcon();
            sr.ConnectionSuccess += new BoolInfo(sr_ConnectionSuccess);
            sr.ServerOutput += new RconOutput(sr_ServerOutput);
            sr.Errors += new RconOutput(sr_Errors);
        }

        void sr_Errors(MessageCode code, string data)
        {
            MethodInvoker m = () => 
            {
                OutputBox.SelectionColor = Color.Red;
                OutputBox.SelectedText = "} " + code.ToString() + "\n" + (data == null ? "" : "} " + data + "\n");
            };
            this.Invoke(m);
        }

        void sr_ServerOutput(MessageCode code, string data)
        {
            MethodInvoker m = () => { if (data != null) OutputBox.AppendText(data); };
            this.Invoke(m);
        }

        void sr_ConnectionSuccess(bool info)
        {
            if (info)
            {
                MethodInvoker m = () =>
                    {
                        SendInputButton.Enabled = true;
                        OutputBox.Enabled = true;
                        InputBox.Enabled = true;

                        ConnectButton.Text = "Disconnect";
                        ConnectButton.Click += Disconnect_Click;
                        ConnectButton.Click -= button2_Click;
                    };
                this.Invoke(m);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            IPAddress ip = IPAddress.Parse(IPBox.Text);
            int port = int.Parse(PortBox.Text);

            IPEndPoint ipe = new IPEndPoint(ip, port);

            sr.Connect(ipe, PasswordBox.Text);
        }

        private void Disconnect_Click(object sender, EventArgs e)
        {
            sr.Disconnect();
            ConnectButton.Text = "Connect";
            ConnectButton.Click += button2_Click;
            ConnectButton.Click -= Disconnect_Click;
        }

        private void SendInputButton_Click(object sender, EventArgs e)
        {
            sr.ServerCommand(InputBox.Text);
            OutputBox.SelectionColor = Color.Blue;
            OutputBox.SelectedText = "] " + InputBox.Text + "\n";
            InputBox.Clear();
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                SendInputButton.PerformClick();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sr.Dispose();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            sr = new Rcon();
            sr.ConnectionSuccess += new BoolInfo(sr_ConnectionSuccess);
            sr.ServerOutput += new RconOutput(sr_ServerOutput);
            sr.Errors += new RconOutput(sr_Errors);
        }

    }
}
