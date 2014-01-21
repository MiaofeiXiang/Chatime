using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Chatime.Class
{
    public delegate void MessageReceivedEventHandler(string hostname, IPAddress RemoteIP, string msg);
    public delegate void OnLineEventHandler(string hostname, IPAddress RemoteIP);
    public delegate void OffLineEventHandler(string hostname, IPAddress RemoteIP);

    public delegate void FileAcceptEventHandler(string hostname, string fileName, IPAddress remoteIP);
    public delegate void FileRefuseEventHandler(string hostname, string fileName, IPAddress remoteIP);
    public delegate void FileSendReqEventHandler(string hostname, string fileName, IPAddress remoteIP);

    /// <summary>
    /// Version:1.0
    /// Date:2013/07/28
    /// Author:Miaofei Xiang
    /// <para>Define the Udp Message Receiver Class</para>
    /// </summary>
    public class UdpReceiver
    {
        public event MessageReceivedEventHandler RemoteMessageReceived; // Remote chat message     
        public event OffLineEventHandler RemoteOffLine; // Remote offLine   
        public event OnLineEventHandler RemoteOnLine; //Remote online

        public event FileAcceptEventHandler FileAccept; //Remote file accepttance
        public event FileRefuseEventHandler FileRefuse;  //Remote file refusal
        public event FileSendReqEventHandler FileSendReq; //Remote file sending request
   
        private Socket soc = null;

        private Thread workerThread = null;


        public UdpReceiver(Socket udpSck)
        {
            soc = udpSck;
        }

        ~UdpReceiver()
        {
            StopListen();
        }
        /// <summary>
        /// Start listening to the UDP port
        /// </summary>
        public void StartListen()         
        {
            ThreadStart start = new ThreadStart(ListenIncomingMsg); 
            workerThread = new Thread(start); 
            workerThread.IsBackground = true; 
            workerThread.Start();
            
        }
        /// <summary>
        /// Receive UDP message and call the corresponding event handler
        /// </summary>
        private void ListenIncomingMsg()
        {
            byte[] buffer = new byte[soc.ReceiveBufferSize];
            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            try
            {
                while (true)
                {
                    int len = soc.ReceiveFrom(buffer, ref remoteEP);
                    UdpDatagram msg = UdpDatagram.Convert(Encoding.UTF8.GetString(buffer, 0, len));
                    switch (msg.Type)
                    {
                        case UdpDatagramType.Chat: RemoteMessageReceived(msg.HostName, IPAddress.Parse(msg.FromAddress), msg.Message); break;
                        case UdpDatagramType.OnLine: RemoteOnLine(msg.HostName, IPAddress.Parse(msg.FromAddress)); break;
                        case UdpDatagramType.OffLine: RemoteOffLine(msg.HostName, IPAddress.Parse(msg.FromAddress)); break;
                        case UdpDatagramType.FAccept: FileAccept(msg.HostName, msg.Message, IPAddress.Parse(msg.FromAddress)); break;
                        case UdpDatagramType.FRefuse: FileRefuse(msg.HostName, msg.Message, IPAddress.Parse(msg.FromAddress)); break;
                        case UdpDatagramType.FSendReq: FileSendReq(msg.HostName, msg.Message, IPAddress.Parse(msg.FromAddress)); break;
                    }
                }
            }
            catch (SocketException)
            {

            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("Error Type:{0};Message:{1}", e.GetType(), e.Message));
            }
        }
        /// <summary>
        /// Stop the UDP port listening  
        /// </summary>
        public void StopListen()        
        {
            try
            {
                if (workerThread != null)
                    workerThread.Abort();
            }
            catch (ThreadAbortException)
            {
            }
        }
    }
}
