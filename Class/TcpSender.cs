using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Chatime.Class
{
    /// <summary>
    /// Version:1.0
    /// Date:2013/07/28
    /// Author:Miaofei Xiang
    /// <para> Define the TCP file sending class</para>
    /// </summary>
    public class TcpSender
    {
        private Socket soc = null;

        public event SendFailHandler SendFail;

        public TcpSender(Socket tcpSoc)
        {
            soc = tcpSoc;
        }

        public void SendMessage(TcpMessage msg, IPEndPoint epRemote)
        {
            if (soc.Connected)
            {
                soc.Shutdown(SocketShutdown.Send);
                soc.Disconnect(true);            
            }
            try
            {
                soc.BeginConnect(epRemote as EndPoint, new AsyncCallback(MessageCallBack), msg);
            }
            catch (Exception ex)
            {
                if (SendFail != null)
                {
                    SendFail(String.Format("Send Failed! {0}", ex.Message));
                }
            }           
        }

        private void MessageCallBack(IAsyncResult aResult)
        {
            soc.EndConnect(aResult);
            TcpMessage msg = (TcpMessage)aResult.AsyncState;
            soc.SendFile(msg.filePath, msg.fileinfobuffer, null, TransmitFileOptions.UseDefaultWorkerThread);
        }
    }
}
