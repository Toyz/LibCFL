using LibCFL;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using static System.Text.Encoding;

namespace IMVUCflViewer
{
    /// <summary>
    /// Interaction logic for Viewer.xaml
    /// </summary>
    public partial class Viewer : Window
    {
        private readonly CFLEntry _item;

        public Viewer(LibCFL.CFLEntry item)
        {
            _item = item;
            InitializeComponent();

            using (var ms = new MemoryStream(item.FileContents))
            {
                if(ms.IsImage())
                {
                    ms.Seek(0, 0);

                    PreviewImage.Visibility = Visibility.Visible;
                    PreviewImage.Stretch = System.Windows.Media.Stretch.None;
                    
                    PreviewImage.Source = BitmapFrame.Create(ms, BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad); ;
                } else
                {
                    ms.Seek(0, 0);

                    sc.Visibility = Visibility.Visible;
                    TextBoxPreview.Text = UTF8.GetString(ms.ToArray());
                }
            }

        }
    }
}
