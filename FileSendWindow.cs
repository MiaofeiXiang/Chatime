using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace Chatime
{
    public delegate void FileReadyToSendEventHandler(string filepath);
    public partial class FileSendWindow : Form
    {
        public string filepath = null;
        public event FileReadyToSendEventHandler fileReadyToSend;
        public FileSendWindow()
        {
            InitializeComponent();
        }

        private void button_Browse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog OpenFD = new OpenFileDialog())     //实例化一个 OpenFileDialog 的对象
            {

                //定义打开的默认文件夹位置
                OpenFD.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);


                OpenFD.ShowDialog();                  //显示打开本地文件的窗体
                filepath = OpenFD.FileName;
                textBox_filepath.Text = OpenFD.FileName;             //将 路径名称 显示在 textBox 控件上
            }
        }

        private void button_Send_Click(object sender, EventArgs e)
        {
            if (File.Exists(filepath))
            {
                fileReadyToSend(filepath);
                this.Close();
            }
            else
                MessageBox.Show("Please select a valid file!");
        }

        private void FileSendWindow_Load(object sender, EventArgs e)
        {

        }
    }
}
