using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Chatime.Class
{
    public class SendFileInfo
    {
        public string localfilePath
        {
            get;
            set;
        }

        public IPAddress recvIP
        {
            get;
            set;
        }

        public int FileNo
        {
            get;
            set;
        }

        public bool? isAccept
        {
            get;
            set;
        }

        public SendFileInfo(string localfilePath, IPAddress recvIP, int FileNo)
        {
            this.localfilePath = localfilePath;
            this.recvIP = recvIP;
            this.FileNo = FileNo;
            this.isAccept = null;
        }
    }

    public class RecvFileInfo
    {
        public string filename
        {
            get;
            set;
        }

        public string localsavePath
        {
            get;
            set;
        }

        public IPAddress sendIP
        {
            get;
            set;
        }
    }

    public class SendFileInfoList
    {
        private List<SendFileInfo> FileInfoList;

        private int FileNo;

        public SendFileInfoList()
        {
            FileNo = 0;
        }
        public void Add(string localfilePath, IPAddress recvIP)
        {
            FileInfoList.Add(new SendFileInfo(localfilePath,recvIP,FileNo++));
        }
       /* public bool AcceptFile(int acceptFileNo)
        {
            
        }
        public bool RefuseFile(int refuseFileNo)
        {

        }*/
    }
}
