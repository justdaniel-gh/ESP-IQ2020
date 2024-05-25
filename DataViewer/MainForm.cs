﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataViewer
{
    public partial class MainForm : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private byte[] readBuffer = new byte[2000];
        private int readBufferLen = 0;
        private StreamWriter outputFile = null;
        private int scanAddress = 0;
        private int connectionState = 0;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateInfo();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mainDataPacketTextBox.Clear();
            mainRawDataTextBox.Clear();
        }

        private void addPacketToStore(string packet)
        {
            if (InvokeRequired) { try { Invoke(new AppendTextHandler(addPacketToStore), packet); } catch (Exception) { } return; }
            foreach (ListViewItem i in packetsListView.Items) { if (i.Text == packet) { return; } }
            ListViewItem l = new ListViewItem(packet);
            packetsListView.Items.Add(l);
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            if (client == null)
            {
                string[] split = addressTextBox.Text.Split(':');
                if (split.Length != 2) return;

                ushort port = 0;
                if (ushort.TryParse(split[1], out port) == false) return;

                client = new TcpClient();
                client.BeginConnect(IPAddress.Parse(split[0]), port, OnConnectSink, null);
                connectionState = 1;
                UpdateInfo();
            }
            else
            {
                Disconnect();
            }
            UpdateInfo();
        }

        public delegate void AppendTextHandler(string msg);

        private void AppendRawDataText(string msg)
        {
            if (InvokeRequired) { try { Invoke(new AppendTextHandler(AppendRawDataText), msg); } catch (Exception) { } return; }
            mainRawDataTextBox.AppendText(msg + "\r\n");
            //if (outputFile != null) { outputFile.WriteLine(DateTime.Now.ToString() + " " + msg); outputFile.Flush(); }
        }

        private void AppendPacketDataText(string msg)
        {
            if (InvokeRequired) { try { Invoke(new AppendTextHandler(AppendPacketDataText), msg); } catch (Exception) { } return; }
            mainDataPacketTextBox.AppendText(msg + "\r\n");
            if (outputFile != null) { outputFile.WriteLine(DateTime.Now.ToString() + " " + msg); outputFile.Flush(); }
        }

        public delegate void UpdateInfoHandler();
        private void UpdateInfo()
        {
            if (InvokeRequired) { Invoke(new UpdateInfoHandler(UpdateInfo)); return; }
            connectButton.Text = (stream == null) ? "Connect" : "Disconnect";
            addressTextBox.Enabled = (stream == null);
            hexTextBox.Enabled = sendButton.Enabled = (stream != null);
            rawSendTextBox.Enabled = rawSendButton.Enabled = (stream != null);
            if (connectionState == 0) { mainToolStripStatusLabel.Text = "Disconnected"; }
            if (connectionState == 1) { mainToolStripStatusLabel.Text = "Connecting"; }
            if (connectionState == 2) { mainToolStripStatusLabel.Text = "Connected"; }
        }

        public delegate void DisconnectHandler();
        private void Disconnect()
        {
            if (InvokeRequired) { Invoke(new DisconnectHandler(Disconnect)); return; }
            if (stream != null) { stream.Close(); stream.Dispose(); stream = null; }
            client = null;
            readBufferLen = 0;
            connectionState = 0;
            UpdateInfo();
        }

        private void OnConnectSink(IAsyncResult ar)
        {
            if (client == null) return;

            // Accept the connection
            try { client.EndConnect(ar); } catch (Exception) { Disconnect(); return; }

            connectionState = 2;
            stream = client.GetStream();
            stream.BeginRead(readBuffer, readBufferLen, readBuffer.Length - readBufferLen, new AsyncCallback(ResponseSink), this);
            UpdateInfo();
        }

        private void ResponseSink(IAsyncResult ar)
        {
            if (client == null) return;
            if (stream == null) return;

            // Read the data
            int len = 0;
            try { len = stream.EndRead(ar); } catch (Exception) { }
            if (len == 0) { Disconnect(); return; }
            AppendRawDataText(" <-- " + ConvertByteArrayToHexString(readBuffer, readBufferLen, len));
            readBufferLen += len;

            // Process the data
            int consumed = 0;
            while ((consumed = processData(readBuffer, readBufferLen)) > 0) {
                if ((consumed > 0) && (consumed < readBufferLen))
                {
                    for (var i = 0; i < (readBufferLen - consumed); i++) { readBuffer[i] = readBuffer[i + consumed]; }
                }
                readBufferLen -= consumed;
            }

            // Read again
            stream.BeginRead(readBuffer, readBufferLen, readBuffer.Length - readBufferLen, new AsyncCallback(ResponseSink), this);
        }

        private int processData(byte[] data, int len)
        {
            //AppendText(" <-- " + ConvertByteArrayToHexString(data, 0, len));
            if (len < 5) return 0;
            if (data[0] != 0x1c) { AppendPacketDataText("Invalid data: " + ConvertByteArrayToHexString(data, 0, len)); return len; }
            int datalen = data[3];
            int totallen = datalen + 6;
            if (len < (datalen + 6)) { AppendPacketDataText("Incomplete data: " + ConvertByteArrayToHexString(data, 0, len)); return len; }
            byte checksum = ComputeChecksum(data, 1, datalen + 5);
            if (checksum != data[datalen + 5]) { AppendPacketDataText("Invalid checksum: " + ConvertByteArrayToHexString(data, 0, len)); return len; }
            //AppendText("Checksum " + checksum + " / " + data[datalenpadded + 4] + ", len: " + datalenpadded);
            //AppendText(" <-- " + ConvertByteArrayToHexString(data, 1, datalen + 3));
            //if ((data[1] == 0x99) || (data[2] == 0x99)) {
            string t = ConvertByteArrayToHexString(data, 1, 1) + " " + ConvertByteArrayToHexString(data, 2, 1) + " " + ConvertByteArrayToHexString(data, 4, 1) + " " + ConvertByteArrayToHexString(data, 5, datalen);
            addPacketToStore(t);
            AppendPacketDataText(" <-- " + t);
            //}
            return totallen;
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            if (stream == null) return;

            string t = hexTextBox.Text;
            t = t.Replace("<", "").Replace("-", "").Replace(":", "").Replace(" ", "").Replace(",", "");
            byte[] raw = ConvertHexStringToByteArray(t);
            if (raw.Length < 4) return;
            byte dst = raw[0];
            byte src = raw[1];
            byte op = raw[2];
            byte[] data = new byte[raw.Length - 3];
            Array.Copy(raw, 3, data, 0, raw.Length - 3);
            int totallen = 6 + data.Length;
            byte[] packet = new byte[totallen];
            packet[0] = 0x1C;
            packet[1] = dst;
            packet[2] = src;
            packet[3] = (byte)(data.Length);
            packet[4] = op;
            data.CopyTo(packet, 5);
            packet[packet.Length - 1] = ComputeChecksum(packet, 1, packet.Length - 1);
            //AppendText(" RAW --> " + ConvertByteArrayToHexString(packet, 0, packet.Length));
            AppendPacketDataText(" --> " + ConvertByteArrayToHexString(packet, 1, 1) + " " + ConvertByteArrayToHexString(packet, 2, 1) + " " + ConvertByteArrayToHexString(packet, 4, 1) + " " + ConvertByteArrayToHexString(packet, 5, data.Length));
            AppendRawDataText(" --> " + ConvertByteArrayToHexString(packet, 0, packet.Length));
            stream.WriteAsync(packet, 0, packet.Length);
        }

        private byte[] ConvertHexStringToByteArray(string hex)
        {
            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException("Hex string must have an even length.");
            }

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                string byteValue = hex.Substring(i, 2);
                bytes[i / 2] = Convert.ToByte(byteValue, 16);
            }
            return bytes;
        }

        private string ConvertByteArrayToHexString(byte[] bytes, int offset, int len)
        {
            char[] c = new char[len * 2];
            byte b;
            for (int i = 0; i < len; i++)
            {
                b = ((byte)(bytes[i + offset] >> 4));
                c[i * 2] = (char)(b > 9 ? b + 0x37 : b + 0x30);
                b = ((byte)(bytes[i + offset] & 0xF));
                c[i * 2 + 1] = (char)(b > 9 ? b + 0x37 : b + 0x30);
            }
            return new string(c);
        }

        private byte ComputeChecksum(byte[] bytes, int start, int end)
        {
            //AppendText(string.Format("ComputeChecksum: {0}, {1}", start, end));
            uint sum = 0;
            for (var i = start; i < end; i++) { sum += bytes[i]; }
            sum ^= 0xFFFFFFFF;
            return (byte)(sum & 0xFF);
        }

        private void logToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (outputFile == null)
            {
                outputFile = new StreamWriter(Path.Combine(Application.StartupPath, "LogFile.txt"));
            }
            else
            {
                outputFile.Close();
                outputFile.Dispose();
                outputFile = null;
            }
            logToFileToolStripMenuItem.Checked = (outputFile != null);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (outputFile != null)
            {
                outputFile.Close();
                outputFile.Dispose();
                outputFile = null;
            }
        }

        private void rawSendButton_Click(object sender, EventArgs e)
        {
            if (stream == null) return;

            string t = rawSendTextBox.Text;
            t = t.Replace("<", "").Replace("-", "").Replace(":", "").Replace(" ", "").Replace(",", "");
            byte[] packet = ConvertHexStringToByteArray(t);
            AppendRawDataText(" --> " + ConvertByteArrayToHexString(packet, 0, packet.Length));
            stream.WriteAsync(packet, 0, packet.Length);
        }

        private void scanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scanAddress = 0;
            scanTimer.Enabled = true;
        }

        private void scanTimer_Tick(object sender, EventArgs e)
        {
            if ((scanAddress == 255) || (stream == null)) { scanTimer.Enabled = false; return; }

            /*
            byte[] data = ConvertHexStringToByteArray(hexTextBox.Text);
            AppendText(" --> " + ConvertByteArrayToHexString(data, 0, data.Length));
            stream.WriteAsync(data, 0, data.Length);
            */
            string t = hexTextBox.Text;
            t = t.Replace("<", "").Replace("-", "").Replace(":", "").Replace(" ", "");
            string[] tx = t.Split(',');
            if (tx.Length != 4) return;
            byte[] dst = ConvertHexStringToByteArray(tx[0]);
            byte[] src = ConvertHexStringToByteArray(tx[1]);
            byte[] op = ConvertHexStringToByteArray(tx[2]);
            byte[] data = ConvertHexStringToByteArray(tx[3]);
            if ((dst.Length != 1) || (src.Length != 1) || (op.Length != 1) || (data.Length < 1) || (data.Length > 255)) return;
            int totallen = 6 + data.Length;
            byte[] packet = new byte[totallen];
            packet[0] = 0x1C;
            packet[1] = (byte)scanAddress;
            packet[2] = src[0];
            packet[3] = (byte)(data.Length);
            packet[4] = op[0];
            data.CopyTo(packet, 5);
            packet[packet.Length - 1] = ComputeChecksum(packet, 1, packet.Length - 1);
            //AppendText(" RAW --> " + ConvertByteArrayToHexString(packet, 0, packet.Length));
            AppendPacketDataText(" --> " + ConvertByteArrayToHexString(packet, 1, 1) + " " + ConvertByteArrayToHexString(packet, 2, 1) + " " + ConvertByteArrayToHexString(packet, 4, 1) + " " + ConvertByteArrayToHexString(packet, 5, data.Length));
            AppendRawDataText(" --> " + ConvertByteArrayToHexString(packet, 0, packet.Length));
            stream.WriteAsync(packet, 0, packet.Length);

            scanAddress++;
        }
    }
}
