using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.IO;

namespace Chatime.Class
{
    public delegate void FileReadyToRecEventHandler(string remotefilePath, IPAddress remoteIP);
    public delegate void FileReceivingEventHandler(int progressbarportion);
    public delegate void FileReceivedEventHandler(string filename);
    public delegate void FileRecFailedEventHandler(string filename, string error);

    /// <summary>
    /// Version:1.0
    /// Date:2013/07/28
    /// Author:Miaofei Xiang
    /// <para>Define the TCP file receiving class</para>
    /// </summary>
    public class TcpReceiver
    {

        private Socket listener = null;

        private Thread RECt = null;

        private int sendBufferSize;

        private int recvBufferSize;

        private string remotefilePath;

        private IPAddress remoteIP;

        private string recFilepath;

        public event FileReadyToRecEventHandler FileReadyRec;
        public event FileReceivingEventHandler FileReceiving;
        public event FileReceivedEventHandler FileReceived;
        public event FileRecFailedEventHandler FileRecFailed;

        public TcpReceiver(Socket tcpSoc)
        {
            listener = tcpSoc;
            listener.Listen(1);
            sendBufferSize = tcpSoc.SendBufferSize;
            recvBufferSize = tcpSoc.ReceiveBufferSize;
        }

        ~TcpReceiver()
        {
            if (RECt != null)
                RECt.Abort();
        }
       
        public void ReceiveFile(string filepath, string remotefilePath, IPAddress IPRemote)
        {
            recFilepath = filepath;
            this.remotefilePath = remotefilePath;
            this.remoteIP = IPRemote;
            if (listener.Connected)
            {
                listener.Shutdown(SocketShutdown.Receive);
                listener.Disconnect(true);
            }
            try
            {
                RECt = new Thread(new ThreadStart(RecService));
                RECt.Start();            
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Problem receiving File. {0}", ex.Message));
            }
           
        }

        private void RecService()
        {
            string filename = "";
            int filenameLen = 0;
            byte[] buffer;
            byte[] nameBuffer = new byte[3];
            byte[] FileLenSentArr = new byte[10];
            long FileLenSent = 0;
            Socket client = null;
            NetworkStream s = null;
            try
            {
                FileReadyRec(this.remotefilePath, this.remoteIP);
                client = listener.Accept();
                client.ReceiveBufferSize = recvBufferSize;
                s = new NetworkStream(client);
                s.Read(nameBuffer, 0, 3);
                filenameLen = (int)nameBuffer[1];
                nameBuffer = new byte[filenameLen + 2];
                s.Read(nameBuffer, 0, filenameLen + 2);
                filename = Encoding.UTF8.GetString(nameBuffer, 1, filenameLen);

                s.Read(FileLenSentArr, 0, 10);
                FileLenSent = BitConverter.ToInt64(FileLenSentArr, 1);
                int readLen = 0;
                long leftLen = FileLenSent;
                BinaryWriter sw = new BinaryWriter(File.Open(recFilepath + filename, FileMode.Create));
                int progressbarportion = 1;
                int realRecBufferSize = Math.Min(sendBufferSize, recvBufferSize);
                long progressportion = FileLenSent / 100 / realRecBufferSize;
                long progressportionCount = 0;
                using (sw)
                {
                    if (FileLenSent <= realRecBufferSize)
                    {
                        buffer = new byte[FileLenSent];
                        readLen = s.Read(buffer, 0, (int)FileLenSent);
                        sw.Write(buffer, 0, readLen);
                    }
                    else
                    {
                        buffer = new byte[realRecBufferSize];
                        while (leftLen != 0)
                        {
                            if (leftLen >= realRecBufferSize)
                                readLen = s.Read(buffer, 0, realRecBufferSize);
                            else
                                readLen = s.Read(buffer, 0, (int)leftLen);
                            sw.Write(buffer, 0, readLen);
                            leftLen -= readLen;
                            if (readLen != 0 && progressportionCount++ == progressportion)
                            {
                                if (progressbarportion <= 100)
                                    FileReceiving(progressbarportion);
                                progressbarportion++;
                                progressportionCount = 0;
                            }
                        }
                    }
                    //FileReceiving(100);
                    //FileReceived(filename);
                    sw.Close();
                }

            }
            catch (Exception ex)
            {
                if (FileRecFailed != null)
                    FileRecFailed(filename, ex.Message.ToString());
            }
            finally
            {
                if (s != null)
                    s.Close();
                if (client != null)
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Disconnect(false);
                    client.Close();
                }
            }
        }
    }
}
