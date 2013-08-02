using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Chatime.Class
{
    public delegate void SendFailHandler(string err);

    /// <summary>
    /// ClassName:UdpSender
    /// Version:1.0
    /// Date:2013/07/28
    /// Author:Miaofei Xiang
    /// <para>Define the Udp Message Sender Class</para>
    /// </summary>
    public class UdpSender
    {
        private Socket soc = null;

        public event SendFailHandler SendFail;

        public UdpSender(Socket udpSck)
        {
            soc = udpSck;
        }
        /// <summary>
        /// Send UDP group message to all users
        /// </summary>
        /// <param name="Msg">UDPDatagram group message</param>
        /// <param name="groupEP">Multicast group address</param>
        public void MultiCast(UdpDatagram Msg, IPEndPoint groupEP)
        {
            byte[] msg = Encoding.UTF8.GetBytes(Msg.ToString());
            soc.SendTo(msg, groupEP);
        }
        /// <summary>
        /// Send UDP private message to a specific recipient
        /// </summary>
        /// <param name="txtMsg">UDPDatagram private message</param>
        /// <param name="epRemote">Recipient address</param>
        public void SendMessage(UdpDatagram txtMsg, IPEndPoint epRemote)
        {        
            byte[] data = new byte[soc.SendBufferSize];
            string txtMsgStr = txtMsg.ToString();
            int leftLen = txtMsgStr.Length;
            int sendLen = 0;
            
            try
            {
                while (leftLen != 0)
                {
                    if (leftLen >= soc.SendBufferSize)
                    {
                        data = Encoding.UTF8.GetBytes(txtMsgStr.Substring(sendLen, soc.SendBufferSize));
                        soc.SendTo(data, soc.SendBufferSize, SocketFlags.None, epRemote);
                        leftLen -= soc.SendBufferSize;
                        sendLen += soc.SendBufferSize;
                    }
                    else
                    {
                        data = Encoding.UTF8.GetBytes(txtMsgStr.Substring(sendLen, leftLen));
                        soc.SendTo(data, leftLen, SocketFlags.None, epRemote);
                        leftLen = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                if (SendFail != null)
                {
                    SendFail(String.Format("Message Send Failed! " + ex.Message));
                }
            }
        }
    }
}
