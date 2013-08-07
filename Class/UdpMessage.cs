using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
//Reference: bbs.csdn.net/topics/39030797 posted by user[yyl8781697]
namespace Chatime.Class
{
    /// <summary>
    /// Define the type of UdpDatagram
    /// </summary>
    public enum UdpDatagramType:byte
    {
        OnLine = 1, 

        OffLine,
        
        Chat,

        FSendReq,

        FAccept,

        FRefuse
    }

    /// <summary>
    /// Version:1.0
    /// Date:2013/07/28
    /// Author:Miaofei Xiang
    /// <para> Define the UdpDatagram Class</para>
    /// </summary>
    public class UdpDatagram
    {

        public UdpDatagramType Type
        {
            get; 
            protected set; 
        }

        public string FromAddress
        {
            get;
            protected set;
        }

        public string ToAddress
        {
            get;
            protected set;
        }

        public string HostName
        {
            get;
            protected set;
        }

        public string Message
        {
            get;
            protected set;
        }

        public UdpDatagram()
        {
        }

        public UdpDatagram(UdpDatagramType notice, string filePath, string FromAddr, string ToAddr)
        {
            Type = notice;
            FromAddress = FromAddr;
            ToAddress = ToAddr;
            HostName = Dns.GetHostName();
            Message = filePath;
        }

        public UdpDatagram(string textmessage, string FromAddr, string ToAddr)
        {
            Message = textmessage;
            FromAddress = FromAddr;
            ToAddress = ToAddr;
            HostName = Dns.GetHostName();
            Type = UdpDatagramType.Chat;
        }
        /// <summary>
        /// Override the ToString() method to convert UdpDatagram to string
        /// </summary>
        public override string ToString()
        {
            StringBuilder msg = new StringBuilder();
            msg.AppendFormat("{0},", this.Type.ToString());
            msg.AppendFormat("{0},", this.FromAddress);
            msg.AppendFormat("{0},", this.ToAddress);
            msg.AppendFormat("{0},", this.HostName.Length);
            msg.AppendFormat("{0},", this.HostName);
            msg.AppendFormat("{0}", this.Message);
         
            return msg.ToString();
        }
        /// <summary>
        /// Convert the network string to UdpDatagram
        /// </summary>
        /// <param name="netstr">Received network string</param>
        /// <returns>UdpDatagram Object</returns>
        public static UdpDatagram Convert(string netstr)
        {
            UdpDatagram data = new UdpDatagram();

            int ChCount = 0;
            int netstrLen = netstr.Length;
            int headfieldcount = 1;
            string typestr = "";
            string fromAddrstr = "";
            string toAddrstr = "";
            string hostnamestr = "";
            string hostnameLenstr = "";
            int hostnameLen = 0;
            string messagestr = "";


            for (; ChCount < netstrLen && headfieldcount!=6; ChCount++) //Extract all the headerfields
            {
                if (headfieldcount != 6&&netstr[ChCount] == ',')
                {
                    if (headfieldcount == 4)
                        hostnameLen = int.Parse(hostnameLenstr);
                    headfieldcount++;                    
                }
                else if(netstr[ChCount] != ',')
                {
                    switch (headfieldcount)
                    {
                        case 1: typestr += netstr[ChCount]; break;
                        case 2: fromAddrstr += netstr[ChCount]; break;
                        case 3: toAddrstr += netstr[ChCount]; break;
                        case 4: hostnameLenstr += netstr[ChCount]; break;
                        case 5: hostnamestr = netstr.Substring(ChCount, hostnameLen); ChCount += hostnameLen - 1; break;
                    }
                }
            }
            if(ChCount < netstrLen)
                messagestr = netstr.Substring(ChCount);
            data.Type = (UdpDatagramType)Enum.Parse(typeof(UdpDatagramType), typestr);
            data.FromAddress = fromAddrstr;
            data.ToAddress = toAddrstr;  
            data.HostName = hostnamestr;  
            data.Message = messagestr;
            return data;
        }
    }

}
