using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FFT_image_filtering
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void bOpen_Click(object sender, EventArgs e)
        {
            var f = new OpenFileDialog();

            if (f.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                var filePath = f.FileName;

                using (var b = new Bitmap(filePath))
                {
                    
                }
            }

        }
    }
}
