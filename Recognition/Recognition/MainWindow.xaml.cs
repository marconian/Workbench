using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;


namespace Recognition
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Vision vision = new Vision();
            vision.OpenEyes(@"C:\SD\mlf.werkmap\Recognition\Recognition\images\jungle.jpeg");
            //vision.OpenEyes(@"C:\SD\mlf.werkmap\Recognition\Recognition\images\Tauw_freestanding_rgb.png");

            InitializeComponent();
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            MemoryStream ms = new MemoryStream();
            //using (System.Drawing.Bitmap bitmap = vision.ShowVision(15))
            using (System.Drawing.Bitmap bitmap = vision.ShowVision())
            {
                ms.Seek(0, SeekOrigin.Begin);
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            }
            bmp.StreamSource = ms;
            bmp.EndInit();
            conception.Source = bmp;
            groupCount.Text = vision._groups.Count.ToString();
        }

    }
}
