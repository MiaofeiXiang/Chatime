using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
//Reference: bbs.csdn.net/topics/39030797 posted by user[yyl8781697]
namespace Chatime.Class
{
    /// <summary>
    /// Define the type of UdpDatagram
    /// </summary>
    public enum UdpDatagramType
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
            msg.AppendFormat("Type={0},", this.Type.ToString());
            msg.AppendFormat("FromAddress={0},", this.FromAddress);
            msg.AppendFormat("ToAddress={0},", this.ToAddress);
            msg.AppendFormat("HostName={0},", this.HostName);
            msg.AppendFormat("Message={0}", this.Message);
         
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
            IDictionary<string, string> idict = new Dictionary<string, string>();
            string[] strlist = netstr.Split(',');
            for (int i = 0; i < strlist.Length; i++)
            {
                string[] info = strlist[i].Split('='); //put corresponding message data field into dictionary object
                idict.Add(info[0], info[1]);
            }

            data.Type = (UdpDatagramType)Enum.Parse(typeof(UdpDatagramType), idict["Type"]);
            data.FromAddress = idict["FromAddress"];
            data.ToAddress = idict["ToAddress"];  
            data.HostName = idict["HostName"];  
            data.Message = idict["Message"];

            return data;
        }
    }

}
