using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Chatime.Class
{
    public delegate void FileSentFinishEventHandler();
    /// <summary>
    /// Version:1.0
    /// Date:2013/07/28
    /// Author:Miaofei Xiang
    /// <para> Define the TCP file sending class</para>
    /// </summary>
    public class TcpSender
    {
        private Socket soc = null;

        private int sendBuffersize;

        public event SendFailHandler SendFail;

        public event FileSentFinishEventHandler FileSentFinish;

        public TcpSender(Socket tcpSoc)
        {
            this.sendBuffersize = tcpSoc.SendBufferSize;
        }

        public void SendMessage(TcpMessage msg, IPEndPoint epRemote)
        {
            soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            soc.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            soc.SendBufferSize = sendBuffersize;
            try
            {
                soc.BeginConnect(epRemote as EndPoint, new AsyncCallback(MessageCallBack), msg);
            }
            catch (Exception ex)
            {
                soc.Close();
                if (SendFail != null)
                {
                    SendFail(String.Format("Send Failed! {0}", ex.Message));
                }
            }           
        }

        private void MessageCallBack(IAsyncResult aResult)
        {
            try
            {
                soc.EndConnect(aResult);
                TcpMessage msg = (TcpMessage)aResult.AsyncState;
                soc.SendFile(msg.filePath, msg.fileinfobuffer, null, TransmitFileOptions.UseDefaultWorkerThread);
                FileSentFinish();
            }
            catch (Exception ex)
            {                
                if (SendFail != null)
                {
                    SendFail(String.Format("Send Failed! {0}", ex.Message));
                }
            }
            finally
            {
                soc.Shutdown(SocketShutdown.Both);
                soc.Disconnect(false);
                soc.Close();
            }
        }
    }
}
