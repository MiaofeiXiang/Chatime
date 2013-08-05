using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using Chatime.Class;
using System.IO;

namespace Chatime
{
    public partial class ChatForm : Form
    {
        private Talker Messenger;
        private string hostname;
        private Dictionary<string,IPAddress> UserList; //Store all the online users' host name and IPAddress
        private FileTransWindow receiveFileProg = null;
        private Thread recvFileProgthread = null;
        private bool isRecvFileProgReady;
        private FileTransWindow sendFileProg = null;        
        private Thread sendFileProgthread = null;
        private bool isSendFileProgReady;
        private FileSendWindow sendFileDialog;

        public ChatForm()
        {
            InitializeComponent();
        }

        private void ChatForm_Load(object sender, EventArgs e)
        {
            Messenger = new Talker();
            //Fire the event accessors provided in Talker class
            Messenger.SendFail += this.SendFailNotice;
            Messenger.RemoteMessageReceived += this.MessageReceived;
            Messenger.RemoteOffLine += this.RemoteOffLine;
            Messenger.RemoteOnLine += this.RemoteOnLine;
            Messenger.FileSendReq += this.FileSendReq;
            Messenger.FileAccept += this.FileAccept;
            Messenger.FileRefuse += this.FileRefuse;
            Messenger.FileReadyRec += this.FileReadyRec;
            Messenger.FileReceived += this.FileReceived;
            Messenger.FileRecFailed += this.FileRecFailed;
            Messenger.FileSent += FileSent;

            UserList = new Dictionary<string,IPAddress>();
            hostname = Dns.GetHostName();
            UserList.Add(hostname,Messenger.LocalIPAddress);
            list_User.Items.Add(hostname); //local host online
            Messenger.MultiCastNotice(UdpDatagramType.OnLine); //multicast online message 
            Messenger.OpenUdpRec(); //open UDP receiving service 
        }
       
        private void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult closeConfirm = DialogResult.OK;
            if (receiveFileProg != null || sendFileProg != null)
            {
                closeConfirm = MessageBox.Show("File Transmission not finished, confirm closing?", "Notice", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);    
            }

            if (closeConfirm == DialogResult.OK)
            {
                if (recvFileProgthread != null)
                    recvFileProgthread.Abort();
                if (sendFileProgthread != null)
                    sendFileProgthread.Abort();
                UserList.Remove(hostname);
                list_User.Items.Remove(hostname); //local host offline
                Messenger.MultiCastNotice(UdpDatagramType.OffLine); //multicast offline message
                Messenger.CloseTalker();             
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            string txtmsg = textMessage.Text;
            if (txtmsg != "")
            {
                Messenger.SendGroupTextMsg(txtmsg); //send group text message
                this.listMessage.Items.Add(String.Format("Me[{0}]:", DateTime.Now));
                this.listMessage.Items.Add(String.Format("{0}", txtmsg));
                textMessage.Clear();
            }
            else
                MessageBox.Show("Don't send blank message!");
        }

        private void buttonFile_Click(object sender, EventArgs e)
        {
            int ItemsSelectedNumber = this.list_User.SelectedItems.Count;
            if (ItemsSelectedNumber == 1)
            {
                string receiver = this.list_User.SelectedItem.ToString();
                sendFileDialog = new FileSendWindow();
                sendFileDialog.fileReadyToSend += TcpSendFile;
                sendFileDialog.Show();
            }
            else if (ItemsSelectedNumber == 0)
            {
                MessageBox.Show("Please first select a reciever from user list!");
            }
            else
            {
                MessageBox.Show("Please select only one reciever from user list!");
            }
        }
        private void TcpSendFile(string filepath)
        {
            Messenger.SendFileNotice(UdpDatagramType.FSendReq,filepath,UserList[(string)list_User.SelectedItem]);
        }
        private void SendFailNotice(string err)
        {
            MessageBox.Show(string.Format("Sent Failure. Error:{0}",err));
        }

        private void MessageReceived(string hostname, IPAddress remoteIP, string txtmsg)
        {
            if(UserList[hostname].ToString()==remoteIP.ToString()) //validate if the message from a user on the list
               this.BeginInvoke(new Action(() => { this.listMessage.Items.Add(String.Format("{0}[{1}]:", hostname, DateTime.Now)); this.listMessage.Items.Add(String.Format("{0}",txtmsg)); }));
        }

        private void RemoteOnLine(string hostname, IPAddress remoteIP)
        {
            if (!UserList.ContainsKey(hostname))
            {
                this.BeginInvoke(new Action(()=>Messenger.MultiCastNotice(UdpDatagramType.OnLine)));
                this.BeginInvoke(new Action(()=>UserList.Add(hostname,remoteIP)));
                this.BeginInvoke(new Action(()=>list_User.Items.Add(hostname)));
            }
        }

        private void RemoteOffLine(string hostname, IPAddress remoteIP)
        {
            if (UserList.ContainsKey(hostname))
            {
                this.BeginInvoke(new Action(() => UserList.Remove(hostname)));
                this.BeginInvoke(new Action(()=>list_User.Items.Remove(hostname)));
            }
        }

        private void FileSendReq(string remoteHostname, string remotefilePath, IPAddress remoteIP)
        {
            string filepath = "";
            string filename = Path.GetFileName(remotefilePath);
            try
            {
                if (MessageBox.Show(string.Format("User {0} wants to send file {1} to you.", remoteHostname, filename), "Notice", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    FolderBrowserDialog savePathDialog = new FolderBrowserDialog();
                    savePathDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    savePathDialog.InitializeLifetimeService();
                    DialogResult result = STAShowDialog(savePathDialog);
                             
                    if (result == DialogResult.OK)
                    { 
                        filepath = savePathDialog.SelectedPath;
                        this.receiveFileProg = new FileTransWindow("Receiving " + filename);
                        isRecvFileProgReady = false;
                        recvFileProgthread = new Thread(new ThreadStart(recvFileProgEntry));
                        recvFileProgthread.Start();
                        while (!isRecvFileProgReady) ;
                        Messenger.RecFile(filepath + "\\", remotefilePath, remoteIP);
                    }
                    else
                        Messenger.SendFileNotice(UdpDatagramType.FRefuse, remotefilePath, remoteIP);
                }
                else
                {
                    Messenger.SendFileNotice(UdpDatagramType.FRefuse, remotefilePath, remoteIP);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format(e.Message));
            }
        }

        private void recvFileProgEntry()
        {
            isRecvFileProgReady = true;
            this.receiveFileProg.ShowDialog();           
        }

        private class DialogState
        {
            public DialogResult result;
            public FolderBrowserDialog dialog;
            public void ThreadProcShowDialog()
            {
                result = dialog.ShowDialog();
            }
        }
        private DialogResult STAShowDialog(FolderBrowserDialog savePathDialog)
        {
            DialogState state = new DialogState();
            state.dialog = savePathDialog;
            System.Threading.Thread t = new System.Threading.Thread(state.ThreadProcShowDialog);
            t.SetApartmentState(System.Threading.ApartmentState.STA);
            t.Start();
            t.Join();
            return state.result;
        }

        private void FileReadyRec(string remotefilePath, IPAddress remoteIP)
        {
            Messenger.SendFileNotice(UdpDatagramType.FAccept, remotefilePath, remoteIP);
        }
        private void FileAccept(string remoteHostname, string filePath, IPAddress remoteIP)
        {
            //MessageBox.Show(String.Format("{0}[{2}] accept your file sending request:{1}", remoteHostname, filePath, remoteIP.ToString()));
            string filename = Path.GetFileName(filePath);
            this.sendFileProg = new FileTransWindow("Sending " + filename);
            isSendFileProgReady = false;
            sendFileProgthread = new Thread(new ThreadStart(sendFileProgEntry));
            sendFileProgthread.Start();
            while (!isSendFileProgReady) ;
            Messenger.SendFile(filePath, remoteIP);
        }

        private void sendFileProgEntry()
        {
            isSendFileProgReady = true;
            this.sendFileProg.ShowDialog(); 
        }

        private void FileSent()
        {
            if (sendFileProg != null)
                this.BeginInvoke(new Action(() => { this.sendFileProg.Close(); this.sendFileProg = null; }));
        }
        private void FileRefuse(string remoteHostname, string remotefilePath, IPAddress remoteIP)
        {
            MessageBox.Show(String.Format("{0} refused your file {1}.", remoteHostname, remotefilePath));
        }
        
        private void FileReceived()
        {
            if(receiveFileProg!=null)
                this.BeginInvoke(new Action(() => { this.receiveFileProg.Close(); this.receiveFileProg = null; }));
        }
        private void FileRecFailed(string filename, string err)
        {
            MessageBox.Show(string.Format("Receiving File {0} confront error. Error:{1}", filename, err));
        }

      
    }
}
