using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Chatime.Class
{
    /// <summary>
    /// Version:1.0
    /// Date:2013/07/28
    /// Author:Miaofei Xiang
    /// <para>Define the Talker Class which manages the socket resources and the transfer process</para>
    /// </summary>
    public class Talker
    {
        private TcpSender tcpSender;
        private TcpReceiver tcpReceiver;
        private UdpSender udpSender;
        private UdpReceiver udpReceiver;

        private Socket udpSck = null;

        private Socket tcpSck = null;

        public readonly int tcport = 8899;

        public readonly int udport = 8090;

        public readonly int udpRecBufferSize = 5000;

        public readonly int udpSendBufferSize = 5000;

        public readonly int tcpRecBufferSize = 5000;

        public readonly int tcpSendBufferSize = 5000;

        public readonly IPAddress GroupIPAddress = IPAddress.Parse("239.255.255.255"); //multicast IP group address

        public IPAddress LocalIPAddress;

        #region Talker provides event accessors to external GUI Form class
        public event SendFailHandler SendFail
        {
            add
            {
                tcpSender.SendFail += value;
                udpSender.SendFail += value;
            }
            remove
            {
                tcpSender.SendFail -= value;
                udpSender.SendFail -= value;
            }
        }
        public event MessageReceivedEventHandler RemoteMessageReceived 
        {
            add
            {
                udpReceiver.RemoteMessageReceived += value;
            }
            remove
            {
                udpReceiver.RemoteMessageReceived -= value;
            }
        }

        public event OffLineEventHandler RemoteOffLine  
        {
            add
            {
                udpReceiver.RemoteOffLine += value;
            }
            remove
            {
                udpReceiver.RemoteOffLine -= value;
            }
        }
        public event OnLineEventHandler RemoteOnLine 
        {
            add
            {
                udpReceiver.RemoteOnLine += value;
            }
            remove
            {
                udpReceiver.RemoteOnLine -= value;
            }
        }
        public event FileAcceptEventHandler FileAccept
        {
            add
            {
                udpReceiver.FileAccept += value;
            }
            remove
            {
                udpReceiver.FileAccept -= value;
            }
        }
        public event FileRefuseEventHandler FileRefuse
        {
            add
            {
                udpReceiver.FileRefuse += value;
            }
            remove
            {
                udpReceiver.FileRefuse -= value;
            }
        }
        public event FileSendReqEventHandler FileSendReq
        {
            add
            {
                udpReceiver.FileSendReq += value;
            }
            remove
            {
                udpReceiver.FileSendReq -= value;
            }
        }
        public event FileReadyToRecEventHandler FileReadyRec
        {
            add
            {
                tcpReceiver.FileReadyRec += value;
            }
            remove
            {
                tcpReceiver.FileReadyRec -= value;
            }
        }
        public event FileReceivingEventHandler FileReceiving
        {
            add
            {
                tcpReceiver.FileReceiving += value;
            }
            remove
            {
                tcpReceiver.FileReceiving -= value;
            }
        }
        public event FileReceivedEventHandler FileReceived
        {
            add
            {
                tcpReceiver.FileReceived += value;
            }
            remove
            {
                tcpReceiver.FileReceived -= value;
            }
        }
        public event FileRecFailedEventHandler FileRecFailed
        {
            add
            {
                tcpReceiver.FileRecFailed += value;
            }
            remove
            {
                tcpReceiver.FileRecFailed -= value;
            }
        }
        #endregion

        public Talker()
        {
            string LocalIP = GetLocalIP();
            LocalIPAddress = IPAddress.Parse(LocalIP);
            IPEndPoint tcpLocalEp = new IPEndPoint(LocalIPAddress, tcport);
            IPEndPoint udpLocalEp = new IPEndPoint(LocalIPAddress, udport);
            tcpSck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpSck.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            tcpSck.SendBufferSize = tcpSendBufferSize;
            tcpSck.ReceiveBufferSize = tcpRecBufferSize;
            tcpSck.Bind(tcpLocalEp);

            udpSck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSck.SetSocketOption(SocketOptionLevel.Udp, SocketOptionName.NoDelay, 1);
            udpSck.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            udpSck.SendBufferSize = udpSendBufferSize;
            udpSck.ReceiveBufferSize = udpRecBufferSize;
            udpSck.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 1);
            udpSck.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new
            MulticastOption(GroupIPAddress, LocalIPAddress)); //Join in the multicast IP group
            udpSck.MulticastLoopback = false; //Don't loop back the udp message to local machine
            udpSck.Bind(udpLocalEp);

            tcpSender = new TcpSender(tcpSck);
            tcpReceiver = new TcpReceiver(tcpSck);
            udpSender = new UdpSender(udpSck);
            udpReceiver = new UdpReceiver(udpSck);
        }
         ~Talker()
        {
            CloseTalker();
        }
        /// <summary>
        /// Stop the port listening and release all the resources 
        /// </summary>
         public void CloseTalker()
         {
             CloseUdpRec();
             tcpSck.Close();
             udpSck.Close();
         }
        /// <summary>
        /// Get the local IP address as a string
        /// </summary>
        /// <returns>IP address string</returns>
        private string GetLocalIP()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)//Find the ipv4 IPAddress for host
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }
        /// <summary>
        /// Multicast the UDP notice
        /// </summary>
        /// <param name="notice">OnLine,OffLine</param>
        public void MultiCastNotice(UdpDatagramType notice)
        {
            IPEndPoint groupEP = new IPEndPoint(GroupIPAddress, udport);
            udpSender.MultiCast(new UdpDatagram(notice,"",LocalIPAddress.ToString(),GroupIPAddress.ToString()), groupEP);
        }
        /// <summary>
        /// Send the file transfer related notice to a recipient
        /// </summary>
        /// <param name="notice">FSendReq,FAccept,FRefuse</param>
        /// <param name="filePath">Sending file path</param>
        /// <param name="recIP">Recipient IP address</param>
        public void SendFileNotice(UdpDatagramType notice, string filePath, IPAddress recIP)
        {
            IPEndPoint recEP = new IPEndPoint(recIP, udport);
            udpSender.SendMessage(new UdpDatagram(notice, filePath, LocalIPAddress.ToString(), recIP.ToString()), recEP);
        }
        /// <summary>
        /// Send group text message
        /// </summary>
        /// <param name="txtmsg">text message string</param>
        public void SendGroupTextMsg(string txtmsg)
        {
            IPEndPoint groupEP = new IPEndPoint(GroupIPAddress, udport);
            udpSender.MultiCast(new UdpDatagram(txtmsg, LocalIPAddress.ToString(), GroupIPAddress.ToString()), groupEP);
        }
        /// <summary>
        /// Send private message to a recipient
        /// </summary>
        /// <param name="txtmsg">text message string</param>
        /// <param name="recIP">recipient IP address</param>
        public void SendPrivateTextMsg(string txtmsg, IPAddress recIP)
        {
            IPEndPoint personEP = new IPEndPoint(recIP,udport);
            udpSender.SendMessage(new UdpDatagram(txtmsg, LocalIPAddress.ToString(), recIP.ToString()), personEP);
        }
        /// <summary>
        /// Receive remote file
        /// </summary>
        /// <param name="filePath">User-defined local saved path</param>
        /// <param name="remotefilePath">Remote file path</param>
        /// <param name="remoteIP">Sender IP address</param>
        public void RecFile(string filePath, string remotefilePath, IPAddress remoteIP)
        {
            tcpReceiver.ReceiveFile(filePath, remotefilePath, remoteIP);
        }
        /// <summary>
        /// Send file to a recipient
        /// </summary>
        /// <param name="filePath">Local file path</param>
        /// <param name="recIP">Recipient IP address</param>
        public void SendFile(string filePath, IPAddress recIP)
        {
            IPEndPoint remoteEP = new IPEndPoint(recIP, tcport);
            tcpSender.SendMessage(new TcpMessage(filePath), remoteEP);
        }
       /// <summary>
       /// Start the UDP port listening on udpReceiver
       /// </summary>
        public void OpenUdpRec()
        {
            udpReceiver.StartListen();
        }
        /// <summary>
        /// Close the UDP port listening on udpReceiver
        /// </summary>
        public void CloseUdpRec()
        {
            udpReceiver.StopListen();
        }
    }
}
