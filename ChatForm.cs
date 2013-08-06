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
    /// <summary>
    /// Main conversation windows form
    /// </summary>
    public partial class ChatForm : Form
    {
        private Talker Messenger;
        private string hostname;
        private Dictionary<string,IPAddress> UserList; //store all the online users' host name and IPAddress
        private FileTransWindow receiveFileProg = null; //Receive File Pop-Up Progress Window
        private Thread recvFileProgthread = null; //thread for Receive Progress Window
        private bool isRecvFileProgReady; //signal the main window the recvFileProgthread has started
        private FileTransWindow sendFileProg = null; //Send File Pop-Up Progress Window      
        private Thread sendFileProgthread = null; //thread for Send Progress Window
        private bool isSendFileProgReady; //signal the main window the sendFileProgthread has started
        private FileSendWindow sendFileDialog; //dialogue to select send file path
        /// <summary>
        /// chat form constructor
        /// </summary>
        public ChatForm()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Form load event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChatForm_Load(object sender, EventArgs e)
        {
            Messenger = new Talker();
            //fire the event accessors provided in Talker class
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
        /// <summary>
        /// Form closing event handler
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event argument</param>
        private void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult closeConfirm = DialogResult.OK;
            if (receiveFileProg != null || sendFileProg != null) //Transfer procedure ongoing
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
                e.Cancel = true; //cancel the form closing event
            }
        }
        /// <summary>
        /// Send group messages to all online users
        /// </summary>
        private void buttonSend_Click(object sender, EventArgs e)
        {
            string txtmsg = textMessage.Text;
            if (txtmsg != "")
            {
                Messenger.SendGroupTextMsg(txtmsg); 
                this.listMessage.Items.Add(String.Format("Me[{0}]:", DateTime.Now));
                this.listMessage.Items.Add(String.Format("{0}", txtmsg));
                textMessage.Clear();
            }
            else
                MessageBox.Show("Don't send blank message!");
        }
        /// <summary>
        /// Send file to selected user in the user list
        /// </summary>
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
        /// <summary>
        /// Send file-sending request to receiver
        /// </summary>
        /// <param name="filepath">local file path</param>
        private void TcpSendFile(string filepath)
        {
            Messenger.SendFileNotice(UdpDatagramType.FSendReq,filepath,UserList[(string)list_User.SelectedItem]);
        }
        /// <summary>
        /// Text message or file sending failure
        /// </summary>
        /// <param name="err">system exception error message</param>
        private void SendFailNotice(string err)
        {
            MessageBox.Show(string.Format("Sent Failure. Error:{0}",err));
        }
        /// <summary>
        /// Message received Event handler
        /// </summary>
        /// <param name="hostname">sender hostname</param>
        /// <param name="remoteIP">sender IP address</param>
        /// <param name="txtmsg">received text message</param>
        private void MessageReceived(string hostname, IPAddress remoteIP, string txtmsg)
        {
            if(UserList[hostname].ToString()==remoteIP.ToString()) //validate if the message from a user on the list
               this.BeginInvoke(new Action(() => { this.listMessage.Items.Add(String.Format("{0}[{1}]:", hostname, DateTime.Now)); this.listMessage.Items.Add(String.Format("{0}",txtmsg)); }));
        }
        /// <summary>
        /// Remote user online event handler
        /// </summary>
        /// <param name="hostname">remote hostname</param>
        /// <param name="remoteIP">remote IP address</param>
        private void RemoteOnLine(string hostname, IPAddress remoteIP)
        {
            if (!UserList.ContainsKey(hostname))
            {
                this.BeginInvoke(new Action(()=>Messenger.MultiCastNotice(UdpDatagramType.OnLine))); //multicast self-online message to new online user
                this.BeginInvoke(new Action(()=>UserList.Add(hostname,remoteIP)));
                this.BeginInvoke(new Action(()=>list_User.Items.Add(hostname)));
            }
        }
        /// <summary>
        /// Remote offline event handler
        /// </summary>
        /// <param name="hostname">remote hostname</param>
        /// <param name="remoteIP">remote IP address</param>
        private void RemoteOffLine(string hostname, IPAddress remoteIP)
        {
            if (UserList.ContainsKey(hostname))
            {
                this.BeginInvoke(new Action(() => UserList.Remove(hostname)));
                this.BeginInvoke(new Action(()=>list_User.Items.Remove(hostname)));
            }
        }
        /// <summary>
        /// Receive file sending request event handler
        /// </summary>
        /// <param name="remoteHostname">sender hostname</param>
        /// <param name="remotefilePath">sending file path</param>
        /// <param name="remoteIP">sender IP address</param>
        private void FileSendReq(string remoteHostname, string remotefilePath, IPAddress remoteIP)
        {
            string filepath = "";
            string filename = Path.GetFileName(remotefilePath);
            try
            {
                if (MessageBox.Show(string.Format("User {0} wants to send file {1} to you.", remoteHostname, filename), "Notice", MessageBoxButtons.YesNo) == DialogResult.Yes) //User accepts the file
                {
                    FolderBrowserDialog savePathDialog = new FolderBrowserDialog(); //open a dialogue for user to select file saving path
                    savePathDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    savePathDialog.InitializeLifetimeService();
                    DialogResult result = STAShowDialog(savePathDialog);
                             
                    if (result == DialogResult.OK)
                    { 
                        filepath = savePathDialog.SelectedPath;
                        this.receiveFileProg = new FileTransWindow("Receiving " + filename);
                        isRecvFileProgReady = false;
                        recvFileProgthread = new Thread(new ThreadStart(recvFileProgEntry));
                        recvFileProgthread.Start(); //start thread for receiving file progress window
                        while (!isRecvFileProgReady) ; //waiting for thread actually running 
                        Messenger.RecFile(filepath + "\\", remotefilePath, remoteIP); //invoke messenger file receiving event handler
                    }
                    else
                        Messenger.SendFileNotice(UdpDatagramType.FRefuse, remotefilePath, remoteIP);
                }
                else //user refuses the file
                {
                    Messenger.SendFileNotice(UdpDatagramType.FRefuse, remotefilePath, remoteIP); //send refusal notice to sender
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format(e.Message));
            }
        }
        /// <summary>
        /// receiving file progress window thread entry function 
        /// </summary>
        private void recvFileProgEntry()
        {
            isRecvFileProgReady = true;
            this.receiveFileProg.ShowDialog();  //show a modal dialogue         
        }
        #region Run folder browser dialogue on STA thread
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
        #endregion
        /// <summary>
        /// File ready to receive event handler
        /// </summary>
        /// <param name="remotefilePath">sending file path</param>
        /// <param name="remoteIP">sender IP address</param>
        private void FileReadyRec(string remotefilePath, IPAddress remoteIP)
        {
            Messenger.SendFileNotice(UdpDatagramType.FAccept, remotefilePath, remoteIP);
        }
        /// <summary>
        /// Accept the file sending request event handler
        /// </summary>
        /// <param name="remoteHostname">sender hostname</param>
        /// <param name="filePath">sending file path</param>
        /// <param name="remoteIP">sender IP address</param>
        private void FileAccept(string remoteHostname, string filePath, IPAddress remoteIP)
        {
            string filename = Path.GetFileName(filePath);
            this.sendFileProg = new FileTransWindow("Sending " + filename);
            isSendFileProgReady = false;
            sendFileProgthread = new Thread(new ThreadStart(sendFileProgEntry));
            sendFileProgthread.Start();
            while (!isSendFileProgReady) ;
            Messenger.SendFile(filePath, remoteIP);
        }
        /// <summary>
        ///sending file progress window thread entry function
        /// </summary>
        private void sendFileProgEntry()
        {
            isSendFileProgReady = true;
            this.sendFileProg.ShowDialog();
        }
        /// <summary>
        /// File successfully sent event handler
        /// </summary>
        private void FileSent()
        {
            if (sendFileProg != null)
                this.BeginInvoke(new Action(() => { this.sendFileProg.Close(); this.sendFileProg = null; }));
        }
        /// <summary>
        /// Remote user refuses file event handler
        /// </summary>
        /// <param name="remoteHostname">remote hostname</param>
        /// <param name="remotefilePath">sending file path</param>
        /// <param name="remoteIP">remote IP address</param>
        private void FileRefuse(string remoteHostname, string remotefilePath, IPAddress remoteIP)
        {
            MessageBox.Show(String.Format("{0} refused your file {1}.", remoteHostname, remotefilePath));
        }
        /// <summary>
        /// File successfully received event handler
        /// </summary>
        private void FileReceived()
        {
            if(receiveFileProg!=null)
                this.BeginInvoke(new Action(() => { this.receiveFileProg.Close(); this.receiveFileProg = null; }));
        }
        /// <summary>
        /// File received failure event handler
        /// </summary>
        /// <param name="filename">receiving file name</param>
        /// <param name="err">system exception error message</param>
        private void FileRecFailed(string filename, string err)
        {
            MessageBox.Show(string.Format("Receiving File {0} confront error. Error:{1}", filename, err));
        }

      
    }
}
