using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace WpfImageCutter
{
    public class WpfImageTools
    {
        /// <summary>
        /// Converts a <see cref="BitmapImage"/> to a byte[]
        /// </summary>
        /// <param name="imageBitmap"><see cref="BitmapImage"/> to convert</param>
        /// <returns>Returns a byte[] that contains the BitmapImage</returns>
        public static byte[] GetImageBytes(BitmapImage imageBitmap)
        {
            byte[] data;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(imageBitmap));

            MemoryStream ms = new MemoryStream();
            encoder.Save(ms);
            data = ms.ToArray();

            ms.Dispose();

            return data;
        }

        /// <summary>
        /// Converts a <see cref="ImageBrush"/> to a byte[]
        /// </summary>
        /// <param name="imageBrush"><see cref="ImageBrush"/> to convert</param>
        /// <returns>Returns a byte[] that contains the ImageBrush</returns>
        public static byte[] GetBytesFromBrush(ImageBrush imageBrush)
        {
            try
            {
                byte[] data;
                BitmapImage bitmap = (BitmapImage)imageBrush.ImageSource;
                MemoryStream ms = (MemoryStream)bitmap.StreamSource;
                data = ms.ToArray();

                ms.Dispose();

                return data;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Converts a byte[] to a <see cref="BitmapImage"/>
        /// </summary>
        /// <param name="imageData">byte[] to convert</param>
        /// <returns>Returns a <see cref="BitmapImage"/> from a byte[]</returns>
        public static BitmapImage GetBitmapImage(byte[] imageData)
        {
            try
            {
                MemoryStream ms = new MemoryStream(imageData);

                BitmapImage Bi = new BitmapImage();
                Bi.BeginInit();
                Bi.CacheOption = BitmapCacheOption.OnLoad;
                Bi.StreamSource = ms;
                Bi.EndInit();

                ms.Dispose();

                return Bi;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Converts a byte[] to a <see cref="ImageBrush"/>
        /// </summary>
        /// <param name="imageData">byte[] to convert</param>
        /// <returns>Returns a <see cref="ImageBrush"/> from a byte[]</returns>
        public static ImageBrush GetImageBrush(byte[] imageData)
        {
            try
            {
                ImageBrush image = new ImageBrush
                {
                    ImageSource = GetBitmapImage(imageData),
                    Stretch = Stretch.UniformToFill
                };

                return image;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Opens a window to search pictures
        /// </summary>
        /// <returns>Returns a byte[] that contains the imageData</returns>
        public static byte[] SearchImage()
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog
                {
                    Filter = "JPG (*.jpg)|*.jpg|PNG (*.png)|*.png"
                };
                ofd.ShowDialog();

                FileStream fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read);

                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, Convert.ToInt32(fs.Length));

                fs.Close();

                return data;
            }
            catch
            {
                return null;
            }
        }

        public static CroppedBitmap CutBitmap(BitmapSource source, Int32Rect rect)
        {
            return new CroppedBitmap(source, rect);
        }
    }
}
