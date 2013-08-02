namespace Chatime
{
    partial class ChatForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.list_User = new System.Windows.Forms.ListBox();
            this.groupUser = new System.Windows.Forms.GroupBox();
            this.textMessage = new System.Windows.Forms.TextBox();
            this.buttonSend = new System.Windows.Forms.Button();
            this.buttonFile = new System.Windows.Forms.Button();
            this.listMessage = new System.Windows.Forms.ListBox();
            this.groupUser.SuspendLayout();
            this.SuspendLayout();
            // 
            // list_User
            // 
            this.list_User.FormattingEnabled = true;
            this.list_User.Location = new System.Drawing.Point(15, 19);
            this.list_User.Name = "list_User";
            this.list_User.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.list_User.Size = new System.Drawing.Size(137, 433);
            this.list_User.TabIndex = 0;
            // 
            // groupUser
            // 
            this.groupUser.Controls.Add(this.list_User);
            this.groupUser.Location = new System.Drawing.Point(12, 12);
            this.groupUser.Name = "groupUser";
            this.groupUser.Size = new System.Drawing.Size(167, 476);
            this.groupUser.TabIndex = 1;
            this.groupUser.TabStop = false;
            this.groupUser.Text = "User List";
            // 
            // textMessage
            // 
            this.textMessage.Location = new System.Drawing.Point(195, 353);
            this.textMessage.Multiline = true;
            this.textMessage.Name = "textMessage";
            this.textMessage.Size = new System.Drawing.Size(443, 124);
            this.textMessage.TabIndex = 3;
            // 
            // buttonSend
            // 
            this.buttonSend.Location = new System.Drawing.Point(644, 397);
            this.buttonSend.Name = "buttonSend";
            this.buttonSend.Size = new System.Drawing.Size(75, 23);
            this.buttonSend.TabIndex = 4;
            this.buttonSend.Text = "Send";
            this.buttonSend.UseVisualStyleBackColor = true;
            this.buttonSend.Click += new System.EventHandler(this.buttonSend_Click);
            // 
            // buttonFile
            // 
            this.buttonFile.Location = new System.Drawing.Point(644, 441);
            this.buttonFile.Name = "buttonFile";
            this.buttonFile.Size = new System.Drawing.Size(75, 23);
            this.buttonFile.TabIndex = 1;
            this.buttonFile.Text = "File";
            this.buttonFile.UseVisualStyleBackColor = true;
            this.buttonFile.Click += new System.EventHandler(this.buttonFile_Click);
            // 
            // listMessage
            // 
            this.listMessage.FormattingEnabled = true;
            this.listMessage.Location = new System.Drawing.Point(195, 22);
            this.listMessage.Name = "listMessage";
            this.listMessage.Size = new System.Drawing.Size(520, 316);
            this.listMessage.TabIndex = 5;
            // 
            // ChatForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(727, 500);
            this.Controls.Add(this.listMessage);
            this.Controls.Add(this.buttonFile);
            this.Controls.Add(this.buttonSend);
            this.Controls.Add(this.textMessage);
            this.Controls.Add(this.groupUser);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(743, 538);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(743, 538);
            this.Name = "ChatForm";
            this.Text = "Chatime";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ChatForm_FormClosing);
            this.Load += new System.EventHandler(this.ChatForm_Load);
            this.groupUser.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox list_User;
        private System.Windows.Forms.GroupBox groupUser;
        private System.Windows.Forms.TextBox textMessage;
        private System.Windows.Forms.Button buttonSend;
        private System.Windows.Forms.Button buttonFile;
        private System.Windows.Forms.ListBox listMessage;
    }
}

