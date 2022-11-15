using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
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

namespace PortRoyalist
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FileStructure.Init();

            var preparer = new ScreenShotPreparer();

            var di = preparer.PrepareScreenshot(new FileInfo(FileStructure.MapInputDir("ct.png")));

            var parser = new ScreenShotParser();

            parser.ParseScreenshot(di);

           // ssp.PrepareScreenshots(new FileInfo(MapPath(");

            //var path = ssp.ParseScreenshots();

            //if(path != null)
            //{
            //    //var winFormImg = System.Drawing.Image.FromFile(path.FullName);
            //    //img.Source = ImageUtils.ToImageSource(winFormImg, ImageFormat.Jpeg);
            //    //img.Width = winFormImg.Width;
            //    //img.Height= winFormImg.Height;
            //}
            //else{
            //    this.Close();
            //}


        }
    }


    public class ImageUtils
    {
        public static ImageSource ToImageSource(System.Drawing.Image image, ImageFormat imageFormat)
        {
            BitmapImage bitmap = new BitmapImage();

            using (MemoryStream stream = new MemoryStream())
            {
                // Save to the stream
                image.Save(stream, imageFormat);

                // Rewind the stream
                stream.Seek(0, SeekOrigin.Begin);

                // Tell the WPF BitmapImage to use this stream
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
            }

            return bitmap;
        }
    }
}
