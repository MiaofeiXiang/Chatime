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
            using (OpenFileDialog OpenFD = new OpenFileDialog())
            {

                OpenFD.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);


                OpenFD.ShowDialog();                  //show a modal dialogue
                filepath = OpenFD.FileName;
                textBox_filepath.Text = OpenFD.FileName;             //show the selected file path in textBox
            }
        }

        private void button_Send_Click(object sender, EventArgs e)
        {
            if (File.Exists(filepath)) //check if the selected file exists
            {
                fileReadyToSend(filepath); //raise file ready to send event
                this.Close();
            }
            else
                MessageBox.Show("Please select a valid file!");
        }
    }
}
