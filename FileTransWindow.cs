using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace Chatime
{
    public partial class FileTransWindow : Form
    {
        private string filename;
        public FileTransWindow(string filename)
        {
            InitializeComponent();
            this.filename = filename;
        }

        private void FileTransWindow_Load(object sender, EventArgs e)
        {
            label1.Text = filename;
            label1.Location = new Point(this.Width / 2 - label1.Width / 2,label1.Location.Y); //put the label text in the middle of form body
        }
    }
}
