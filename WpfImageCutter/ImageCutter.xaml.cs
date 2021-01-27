using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace WpfImageCutter
{
    /// <summary>
    /// Lógica de interacción para ImageCutter.xaml
    /// </summary>
    public partial class ImageCutter : UserControl
    {
        public ImageCutter()
        {
            InitializeComponent();
        }

        //Image size in pixels
        private int PixelWidth = 1, PixelHeight = 1;
        //Min and max size on pixels to cut
        private int minPixelWidth = 0, minPixelHeight = 0, maxPixelWidth = 0, maxPixelHeight = 0;
        //Scalable min and max size of the PreviewRect
        private double minWidth, minHeight, maxWidth, maxHeight;
        //Cutter bounds
        private double MinLeft, MinTop, MaxRight, MaxBottom;
        //Active handle
        private Rectangle ActiveHandler;
        private bool MoveAllHandlers = false;

        #region SourceProperty
        [Category("ImageCutter Properties")]
        public ImageSource Source
        {
            get
            {
                return ControlImage.Source;
            }
            set
            {
                if (value != null)
                {
                    ControlImage.Source = value;

                    PixelWidth = ((BitmapSource)value).PixelWidth;
                    PixelHeight = ((BitmapSource)value).PixelHeight;

                    UpdateLayout();
                    UpdateBounds();
                    UpdateHandlersPosition();
                }
            }
        }
        #endregion

        #region BorderSizeProperty
        [Category("ImageCutter Properties")]
        public int BorderSize
        {
            get
            {
                return (int)PreviewRect.BorderThickness.Left;
            }
            set
            {
                if (value >= 1)
                {
                    PreviewRect.BorderThickness = new Thickness(value);
                    LeftHandler.Width = value;
                    TopHandler.Height = value;
                    RightHandler.Width = value;
                    BottomHandler.Height = value;

                    UpdateLayout();
                    UpdateBounds();
                    UpdateHandlersPosition();
                }
            }
        }
        #endregion

        #region HandlerLenghtProperty
        [Category("ImageCutter Properties")]
        public double HandlerLenght
        {
            get
            {
                return LeftHandler.ActualHeight;
            }
            set
            {
                if (value >= 1)
                {
                    LeftHandler.Height = value;
                    RightHandler.Height = value;
                    TopHandler.Width = value;
                    BottomHandler.Width = value;

                    UpdateLayout();
                    UpdateLeftRightMiddlePosition();
                    UpdateTopBottomMiddlePosition();
                }
            }
        }
        #endregion

        #region MinPixelWidthProperty
        public int MinPixelWidth
        {
            get
            {
                return minPixelWidth;
            }
            set
            {
                if (value >= 0)
                {
                    minPixelWidth = value;
                    UpdateSizeRestriction();
                }
            }
        }
        #endregion

        #region MinPixelHeightProperty
        public int MinPixelHeight
        {
            get
            {
                return minPixelHeight;
            }
            set
            {
                if (value >= 0)
                {
                    minPixelHeight = value;
                    UpdateSizeRestriction();
                }
            }
        }
        #endregion

        #region MaxPixelWidthProperty
        public int MaxPixelWidth
        {
            get
            {
                return maxPixelWidth;
            }
            set
            {
                if (value >= 0 && value > minPixelWidth)
                {
                    maxPixelWidth = value;
                    UpdateSizeRestriction();
                }
            }
        }
        #endregion

        #region MaxPixelHeightProperty
        public int MaxPixelHeight
        {
            get
            {
                return maxPixelHeight;
            }
            set
            {
                if (value >= 0 && value > minPixelHeight)
                {
                    maxPixelHeight = value;
                    UpdateSizeRestriction();
                }
            }
        }
        #endregion

        #region UpdateEvents
        private void UpdateBounds()
        {
            MinLeft = (ActualWidth - ControlImage.ActualWidth) / 2;
            MaxRight = ActualWidth - MinLeft - RightHandler.ActualWidth;
            MinTop = (ActualHeight - ControlImage.ActualHeight) / 2;
            MaxBottom = ActualHeight - MinTop - BottomHandler.ActualHeight;
            UpdateSizeRestriction();
        }

        private void UpdateHandlersPosition()
        {
            MoveLeftHandler(0);
            MoveRightHandler(ActualWidth);
            MoveTopHandler(0);
            MoveBottomHandler(ActualHeight);

            UpdatePreview();
        }

        private void UpdateTopBottomMiddlePosition()
        {
            double position = LeftHandler.Margin.Left + (((RightHandler.Margin.Left + RightHandler.ActualWidth - LeftHandler.Margin.Left) / 2) - (TopHandler.ActualWidth / 2));

            TopHandler.Margin = new Thickness(position, TopHandler.Margin.Top, 0, 0);
            BottomHandler.Margin = new Thickness(position, BottomHandler.Margin.Top, 0, 0);
        }

        private void UpdateLeftRightMiddlePosition()
        {
            double position = TopHandler.Margin.Top + (((BottomHandler.Margin.Top + BottomHandler.ActualHeight - TopHandler.Margin.Top) / 2) - (LeftHandler.ActualHeight / 2));

            LeftHandler.Margin = new Thickness(LeftHandler.Margin.Left, position, 0, 0);
            RightHandler.Margin = new Thickness(RightHandler.Margin.Left, position, 0, 0);
        }

        private void UpdatePreview()
        {
            PreviewRect.Margin = new Thickness(LeftHandler.Margin.Left, TopHandler.Margin.Top, 0, 0);
            PreviewRect.Width = RightHandler.Margin.Left - LeftHandler.Margin.Left + RightHandler.ActualWidth;
            PreviewRect.Height = BottomHandler.Margin.Top - TopHandler.Margin.Top + BottomHandler.ActualHeight;

            UpdateBackgroundDim();
        }

        private void UpdateBackgroundDim()
        {
            BackgroundDim.Points.Clear();

            BackgroundDim.Points.Add(new Point(MinLeft, MinTop));
            BackgroundDim.Points.Add(new Point(MaxRight + RightHandler.ActualWidth, MinTop));
            BackgroundDim.Points.Add(new Point(MaxRight + RightHandler.ActualWidth, MaxBottom + BottomHandler.ActualHeight));
            BackgroundDim.Points.Add(new Point(MinLeft, MaxBottom + BottomHandler.ActualHeight));

            BackgroundDim.Points.Add(new Point(LeftHandler.Margin.Left, BottomHandler.Margin.Top));
            BackgroundDim.Points.Add(new Point(RightHandler.Margin.Left, BottomHandler.Margin.Top));
            BackgroundDim.Points.Add(new Point(RightHandler.Margin.Left, TopHandler.Margin.Top));
            BackgroundDim.Points.Add(new Point(LeftHandler.Margin.Left, TopHandler.Margin.Top));

            BackgroundDim.Points.Add(new Point(LeftHandler.Margin.Left, BottomHandler.Margin.Top));
            BackgroundDim.Points.Add(new Point(MinLeft, MaxBottom + BottomHandler.ActualHeight));
        }

        private void UpdateSizeRestriction()
        {
            minWidth = minPixelWidth * ControlImage.ActualWidth / PixelWidth;
            minHeight = minPixelHeight * ControlImage.ActualHeight / PixelHeight;

            maxWidth = maxPixelWidth * ControlImage.ActualWidth / PixelWidth;
            maxHeight = maxPixelHeight * ControlImage.ActualHeight / PixelHeight;

            UpdateHandlersPosition();
        }
        #endregion

        #region Grab/Release
        private void GrabHandler(object sender, MouseButtonEventArgs e)
        {
            ActiveHandler = (Rectangle)sender;
        }

        private void ReleaseHandler(object sender, MouseButtonEventArgs e)
        {
            ActiveHandler = null;
        }
        #endregion

        #region HandleMovement
        private void MoveLeftHandler(double position)
        {
            position = Clamp(position, MinLeft, RightHandler.Margin.Left - RightHandler.ActualWidth);

            double min = RightHandler.Margin.Left - maxWidth;
            double RestrictedPosition = Clamp(position, (maxWidth <= 0) ? 0 : min, RightHandler.Margin.Left - minWidth);

            LeftHandler.Margin = new Thickness(RestrictedPosition, LeftHandler.Margin.Top, 0, 0);
            UpdateTopBottomMiddlePosition();
        }

        private void MoveRightHandler(double position)
        {
            position = Clamp(position, LeftHandler.Margin.Left, MaxRight);

            double min = LeftHandler.Margin.Left + minWidth;
            double max = (maxWidth <= 0)? MaxRight : LeftHandler.Margin.Left + maxWidth;
            double RestrictedPosition = Clamp(position, min, max);

            RightHandler.Margin = new Thickness(RestrictedPosition, RightHandler.Margin.Top, 0, 0);
            UpdateTopBottomMiddlePosition();
        }

        private void MoveTopHandler(double position)
        {
            TopHandler.Margin = new Thickness(TopHandler.Margin.Left, Clamp(position, MinTop, BottomHandler.Margin.Top), 0, 0);
            UpdateLeftRightMiddlePosition();
        }

        private void MoveBottomHandler(double position)
        {
            BottomHandler.Margin = new Thickness(BottomHandler.Margin.Left, Clamp(position, TopHandler.Margin.Top, MaxBottom), 0, 0);
            UpdateLeftRightMiddlePosition();
        }

        private void MoveHandlers(Point position)
        {
            double maxright, maxbottom;

            maxright = Clamp(position.X - PreviewRect.ActualWidth / 2, MinLeft, MaxRight - PreviewRect.ActualWidth + RightHandler.ActualWidth);
            maxbottom = Clamp(position.Y - PreviewRect.ActualHeight / 2, MinTop, MaxBottom - PreviewRect.ActualHeight + BottomHandler.ActualHeight);

            PreviewRect.Margin = new Thickness(maxright, maxbottom, 0, 0);

            MoveLeftHandler(PreviewRect.Margin.Left);
            MoveTopHandler(PreviewRect.Margin.Top);
            MoveRightHandler(PreviewRect.Margin.Left + PreviewRect.ActualWidth - RightHandler.ActualWidth);
            MoveBottomHandler(PreviewRect.Margin.Top + PreviewRect.ActualHeight - BottomHandler.ActualHeight);

            UpdatePreview();
        }

        private void MainGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (ActiveHandler != null)
            {
                switch (ActiveHandler.Name)
                {
                    case "LeftHandler":
                        MoveLeftHandler(e.GetPosition(MainGrid).X);
                        break;
                    case "TopHandler":
                        MoveTopHandler(e.GetPosition(MainGrid).Y);
                        break;
                    case "RightHandler":
                        MoveRightHandler(e.GetPosition(MainGrid).X);
                        break;
                    case "BottomHandler":
                        MoveBottomHandler(e.GetPosition(MainGrid).Y);
                        break;
                }
                UpdatePreview();
            }
            else if (MoveAllHandlers)
            {
                MoveHandlers(e.GetPosition(MainGrid));
            }
        }
        #endregion                       

        #region LocalEvents
        private void MainControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateBounds();
            UpdateHandlersPosition();
            UpdateTopBottomMiddlePosition();
            UpdateLeftRightMiddlePosition();
        }

        private void MainControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateBounds();                       
            UpdateTopBottomMiddlePosition();
            UpdateLeftRightMiddlePosition();
        }

        private void MainGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ActiveHandler = null;
        }

        private void MainControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ActiveHandler = null;
        }

        private void MainControl_MouseLeave(object sender, MouseEventArgs e)
        {
            ActiveHandler = null;
        }

        private void PreviewRect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MoveAllHandlers = true;
        }

        private void PreviewRect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MoveAllHandlers = false;
        }

        private void PreviewRect_MouseLeave(object sender, MouseEventArgs e)
        {
            MoveAllHandlers = false;
        }
        #endregion                

        private double Clamp(double value, double min, double max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public CroppedBitmap CutImage()
        {
            if (Source != null)
            {                                
                int left = (int)(PixelWidth * (LeftHandler.Margin.Left - MinLeft) / ControlImage.ActualWidth);
                int top = (int)(PixelHeight * (TopHandler.Margin.Top - MinTop)/ ControlImage.ActualHeight);
                int width = (int)(PixelWidth * PreviewRect.ActualWidth / ControlImage.ActualWidth);
                int height = (int)(PixelHeight * PreviewRect.ActualHeight / ControlImage.ActualHeight);

                return new CroppedBitmap((BitmapImage)Source, new Int32Rect(left, top, width, height));
            }
            else
            {
                return null;
            }            
        }        
    }
}
