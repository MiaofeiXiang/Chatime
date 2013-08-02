using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Chatime
{
    public partial class FileReceiveWindow : Form
    {
        public FileReceiveWindow()
        {
            InitializeComponent();
        }

        public void updateProgressBar(int progressBarportion)
        {
            progressBar1.Value = progressBarportion;
            if (progressBar1.Value == progressBar1.Maximum)
                this.Close();
        }

        private void FileReceiveWindow_Load(object sender, EventArgs e)
        {

        }
    }
}
