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

using WpfImageTools;

namespace TestWindow
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadImageBtn_Click(object sender, RoutedEventArgs e)
        {
            Cutter.Source = ImageTools.GetBitmapImage(ImageTools.SearchImage());            
        }

        private void CutImageBtn_Click(object sender, RoutedEventArgs e)
        {
            ImageResult.Source = Cutter.CutImage();           
        }

        private void BlurBcgCb_Checked(object sender, RoutedEventArgs e)
        {
            Cutter.UseSourceAsBackground = true;
        }

        private void BlurBcgCb_Unchecked(object sender, RoutedEventArgs e)
        {
            Cutter.UseSourceAsBackground = false;
        }
    }
}
