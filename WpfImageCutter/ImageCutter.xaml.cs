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

            for (int i = 0; i < 10; i++)
            {
                backgroundPoints.Add(new Point());
            }
        }

        #region PrivateVariables
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
        //Allows the movement of all handlers
        private bool MoveAllHandlers = false;
        //Allows the use of the source as blur background
        private bool useSourceAsBackground = false;
        //Min and Max value to move a handler
        private double min, max, alignPosition;
        //Precalculated values used when all handlers are moved at once
        private double PreviewMaxRight, PreviewMaxBottom, PreviewHalfWidth, PreviewHalfHeight, PreviewLeft, PreviewTop;
        //BackgroundDim points
        PointCollection backgroundPoints = new PointCollection();        
        #endregion

        #region SourceProperty
        [Category("ImageCutter Properties")]
        [Description("Gets or sets the image to cut")]
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

                    if(useSourceAsBackground)
                    {
                        BackgroundImage.Source = value;
                    }

                    PixelWidth = ((BitmapSource)value).PixelWidth;
                    PixelHeight = ((BitmapSource)value).PixelHeight;

                    UpdateLayout();
                    UpdateBounds();                    
                }
            }
        }
        #endregion

        #region BorderSizeProperty
        [Category("ImageCutter Properties")]
        [Description("Gets or sets the size of the border")]
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
        [Description("Gets or sets the length of all handlers")]
        public double HandlerLength
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
        [Category("ImageCutter Properties")]
        [Description("Gets or sets the min width in pixels to cut")]
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
        [Category("ImageCutter Properties")]
        [Description("Gets or sets the min height in pixels to cut")]
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
        [Category("ImageCutter Properties")]
        [Description("Gets or sets the max width in pixels to cut")]
        public int MaxPixelWidth
        {
            get
            {
                return maxPixelWidth;
            }
            set
            {
                if (value >= 0 && value >= minPixelWidth)
                {
                    maxPixelWidth = value;
                    UpdateSizeRestriction();
                }
            }
        }
        #endregion

        #region MaxPixelHeightProperty
        [Category("ImageCutter Properties")]
        [Description("Gets or sets the max height in pixels to cut")]
        public int MaxPixelHeight
        {
            get
            {
                return maxPixelHeight;
            }
            set
            {
                if (value >= 0 && value >= minPixelHeight)
                {
                    maxPixelHeight = value;
                    UpdateSizeRestriction();
                }
            }
        }
        #endregion

        #region HandlerBrushProperty
        [Category("ImageCutter Properties")]
        [Description("Gets or sets the brush that paints the handlers")]
        public Brush HandlerBrush
        {
            get { return (Brush)GetValue(HandlerBrushProperty); }
            set { SetValue(HandlerBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HandlersBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HandlerBrushProperty =
            DependencyProperty.Register("HandlerBrush", typeof(Brush), typeof(ImageCutter), new PropertyMetadata(Brushes.Black));
        #endregion

        #region BorderColorProperty
        [Category("ImageCutter Properties")]
        [Description("Gets or sets the brush that paints the border")]
        public Brush BorderColor
        {
            get { return (Brush)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BorderColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BorderColorProperty =
            DependencyProperty.Register("BorderColor", typeof(Brush), typeof(ImageCutter), new PropertyMetadata(Brushes.White));
        #endregion

        #region BackgroundDimColorProperty
        [Category("ImageCutter Properties")]
        [Description("Gets or sets the brush that paints the area outside the cutter")]
        public Brush BackgroundDimColor
        {
            get { return (Brush)GetValue(BackgroundDimColorProperty); }
            set { SetValue(BackgroundDimColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundDimColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundDimColorProperty =
            DependencyProperty.Register("BackgroundDimColor", typeof(Brush), typeof(ImageCutter), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(102, 0, 0, 0))));
        #endregion

        #region UseSourceAsBackgroundProperty
        [Category("ImageCutter Properties")]
        public bool UseSourceAsBackground
        {
            get
            {
                return useSourceAsBackground;
            }
            set
            {
                useSourceAsBackground = value;

                if (useSourceAsBackground)
                {
                    UpdateBackgroundImage();
                }
                else
                {
                    BackgroundImage.Source = null;
                }
            }
        }
        #endregion

        #region UpdateEvents
        /// <summary>
        /// Updates the bounds in which the handlers can move
        /// </summary>
        private void UpdateBounds()
        {
            ControlImage.Margin = new Thickness(BorderSize);
            MinLeft = (ActualWidth - ControlImage.ActualWidth) / 2 - LeftHandler.ActualWidth;
            MaxRight = ActualWidth - MinLeft - RightHandler.ActualWidth;
            MinTop = (ActualHeight - ControlImage.ActualHeight) / 2 - TopHandler.ActualHeight;
            MaxBottom = ActualHeight - MinTop - BottomHandler.ActualHeight;
            UpdateSizeRestriction();
        }

        /// <summary>
        /// Moves the handlers position to its max
        /// </summary>
        private void UpdateHandlersPosition()
        {            
            LeftHandler.Margin = new Thickness(MinLeft, MinTop, 0, 0);
            TopHandler.Margin = new Thickness(MinLeft, MinTop, 0, 0);

            MoveRightHandler(ActualWidth, false);
            MoveBottomHandler(ActualHeight, false);

            UpdatePreview();

            UpdateTopBottomMiddlePosition();
            UpdateLeftRightMiddlePosition();
        }

        /// <summary>
        /// Keeps the top and bottom handlers aligned at the center
        /// </summary>
        private void UpdateTopBottomMiddlePosition(bool alignPositions = true)
        {
            if (!alignPositions)
            {
                return;
            }            

            alignPosition = LeftHandler.Margin.Left + PreviewRect.ActualWidth / 2 - TopHandler.ActualWidth / 2;

            TopHandler.Margin = new Thickness(alignPosition, TopHandler.Margin.Top, 0, 0);
            BottomHandler.Margin = new Thickness(alignPosition, BottomHandler.Margin.Top, 0, 0);
        }

        /// <summary>
        /// Keeps the left and right handlers aligned at the center
        /// </summary>
        private void UpdateLeftRightMiddlePosition(bool alignPositions = true)
        {            
            if(!alignPositions)
            {
                return;
            }            

            alignPosition = TopHandler.Margin.Top + PreviewRect.ActualHeight / 2 - LeftHandler.ActualHeight / 2;

            LeftHandler.Margin = new Thickness(LeftHandler.Margin.Left, alignPosition, 0, 0);
            RightHandler.Margin = new Thickness(RightHandler.Margin.Left, alignPosition, 0, 0);
        }

        /// <summary>
        /// Updates the <see cref="PreviewRect"/> size
        /// </summary>
        private void UpdatePreview()
        {
            PreviewRect.Margin = new Thickness(LeftHandler.Margin.Left, TopHandler.Margin.Top, 0, 0);

            PreviewRect.Width = RightHandler.Margin.Left - LeftHandler.Margin.Left + RightHandler.ActualWidth;
            PreviewRect.Height = BottomHandler.Margin.Top - TopHandler.Margin.Top + BottomHandler.ActualHeight;
            PreviewRect.UpdateLayout();

            PrecalculatePreviewRectBounds();

            UpdateBackgroundDim();
        }

        /// <summary>
        /// Updates the <see cref="PointCollection"/> of <see cref="BackgroundDim"/>
        /// </summary>
        private void UpdateBackgroundDim()
        {            
            //bounds
            backgroundPoints[0] = UpdatePoint(backgroundPoints[0], MinLeft, MinTop);            
            backgroundPoints[1] = UpdatePoint(backgroundPoints[1], MaxRight + RightHandler.ActualWidth, MinTop);            
            backgroundPoints[2] = UpdatePoint(backgroundPoints[2], MaxRight + RightHandler.ActualWidth, MaxBottom + BottomHandler.ActualHeight);            
            backgroundPoints[3] = UpdatePoint(backgroundPoints[3], MinLeft, MaxBottom + BottomHandler.ActualHeight);
            
            //cutter
            backgroundPoints[4] = UpdatePoint(backgroundPoints[4], LeftHandler.Margin.Left, BottomHandler.Margin.Top);            
            backgroundPoints[5] = UpdatePoint(backgroundPoints[5], RightHandler.Margin.Left, BottomHandler.Margin.Top);            
            backgroundPoints[6] = UpdatePoint(backgroundPoints[6], RightHandler.Margin.Left, TopHandler.Margin.Top);            
            backgroundPoints[7] = UpdatePoint(backgroundPoints[7], LeftHandler.Margin.Left, TopHandler.Margin.Top);
            
            //join
            backgroundPoints[8] = UpdatePoint(backgroundPoints[8], LeftHandler.Margin.Left, BottomHandler.Margin.Top);
            backgroundPoints[9] = UpdatePoint(backgroundPoints[9], MinLeft, MaxBottom + BottomHandler.ActualHeight);

            BackgroundDim.Points = backgroundPoints;
        }

        /// <summary>
        /// Updates the restrictions of the cutter and the handlers positions
        /// </summary>
        private void UpdateSizeRestriction()
        {
            minWidth = minPixelWidth * ControlImage.ActualWidth / PixelWidth;
            minHeight = minPixelHeight * ControlImage.ActualHeight / PixelHeight;

            maxWidth = maxPixelWidth * ControlImage.ActualWidth / PixelWidth;
            maxHeight = maxPixelHeight * ControlImage.ActualHeight / PixelHeight;

            UpdateHandlersPosition();
        }

        private void UpdateBackgroundImage()
        {
            BackgroundImage.Source = Source;
        }

        private Point UpdatePoint(Point p, double x, double y)
        {
            p.X = x;
            p.Y = y;

            return p;
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

            PrecalculatePreviewRectBounds();
        }

        private void PrecalculatePreviewRectBounds()
        {
            PreviewMaxRight = MaxRight - PreviewRect.ActualWidth + RightHandler.ActualWidth;
            PreviewMaxBottom = MaxBottom - PreviewRect.ActualHeight + BottomHandler.ActualHeight;

            PreviewHalfWidth = PreviewRect.ActualWidth / 2;
            PreviewHalfHeight = PreviewRect.ActualHeight / 2;
        }
        #endregion

        #region HandleMovement
        /// <summary>
        /// Moves the <see cref="LeftHandler"/> to a given position
        /// </summary>
        /// <param name="position">Position from left to right</param>        
        private void MoveLeftHandler(double position, bool alignPositions = true)
        {
            position = Clamp(position, MinLeft, RightHandler.Margin.Left - RightHandler.ActualWidth);

            min = (maxWidth <= 0) ? MinLeft : RightHandler.Margin.Left - BorderSize - maxWidth;
            position = Clamp(position, min, RightHandler.Margin.Left - BorderSize - minWidth);

            LeftHandler.Margin = new Thickness(position, LeftHandler.Margin.Top, 0, 0);
            UpdateTopBottomMiddlePosition(alignPositions);
        }

        /// <summary>
        /// Moves the <see cref="RightHandler"/> to a given position
        /// </summary>
        /// <param name="position">Position from left to right</param>        
        private void MoveRightHandler(double position, bool alignPositions = true)
        {
            position = Clamp(position, LeftHandler.Margin.Left + LeftHandler.ActualWidth, MaxRight);

            min = LeftHandler.Margin.Left + BorderSize + minWidth;
            max = (maxWidth <= 0) ? MaxRight : LeftHandler.Margin.Left + BorderSize + maxWidth;
            position = Clamp(position, min, max);

            RightHandler.Margin = new Thickness(position, RightHandler.Margin.Top, 0, 0);
            UpdateTopBottomMiddlePosition(alignPositions);
        }

        /// <summary>
        /// Moves the <see cref="TopHandler"/> to a given position
        /// </summary>
        /// <param name="position">Position from top to bottom</param>        
        private void MoveTopHandler(double position, bool alignPositions = true)
        {
            position = Clamp(position, MinTop, BottomHandler.Margin.Top - BottomHandler.ActualHeight);

            min = (maxHeight <= 0) ? MinTop : BottomHandler.Margin.Top - BorderSize - maxHeight;
            max = BottomHandler.Margin.Top - BorderSize - minHeight;
            position = Clamp(position, min, max);

            TopHandler.Margin = new Thickness(TopHandler.Margin.Left, position, 0, 0);
            UpdateLeftRightMiddlePosition(alignPositions);
        }

        /// <summary>
        /// Moves the <see cref="BottomHandler"/> to a given position
        /// </summary>
        /// <param name="position">Position from top to bottom</param>        
        private void MoveBottomHandler(double position, bool alignPositions = true)
        {
            position = Clamp(position, TopHandler.Margin.Top + TopHandler.ActualHeight, MaxBottom);

            min = TopHandler.Margin.Top + BorderSize + minHeight;
            max = (maxHeight <= 0) ? MaxBottom : TopHandler.Margin.Top + BorderSize + maxHeight;
            position = Clamp(position, min, max);

            BottomHandler.Margin = new Thickness(BottomHandler.Margin.Left, position, 0, 0);
            UpdateLeftRightMiddlePosition(alignPositions);
        }

        /// <summary>
        /// Moves all handlers to a given <see cref="Point"/>
        /// </summary>
        /// <param name="position">Central <see cref="Point"/></param>
        private void MoveHandlers(Point position)
        {            
            PreviewLeft = Clamp(position.X - PreviewHalfWidth, MinLeft, PreviewMaxRight);
            PreviewTop = Clamp(position.Y - PreviewHalfHeight, MinTop, PreviewMaxBottom);

            PreviewRect.Margin = new Thickness(PreviewLeft, PreviewTop, 0, 0);

            MoveLeftHandler(PreviewRect.Margin.Left, false);
            MoveTopHandler(PreviewRect.Margin.Top, false);
            MoveRightHandler(PreviewRect.Margin.Left + PreviewRect.ActualWidth - RightHandler.ActualWidth, false);
            MoveBottomHandler(PreviewRect.Margin.Top + PreviewRect.ActualHeight - BottomHandler.ActualHeight, false);

            UpdateTopBottomMiddlePosition();
            UpdateLeftRightMiddlePosition();
            
            UpdateBackgroundDim();
        }

        /// <summary>
        /// Decides which handler moves depending on its name
        /// </summary>
        /// <param name="sender"><see cref="Rectangle"/> Handler</param>        
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
        //Updates the bounds when the size of the control is changed
        private void MainControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateBounds();
        }

        //Updates the bounds when the control is loaded
        private void MainControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateBounds();
        }

        //Sets the ActiveHandler to null
        private void MainGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ActiveHandler = null;
        }

        //Sets the ActiveHandler to null
        private void MainControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ActiveHandler = null;
        }

        //Sets the ActiveHandler to null
        private void MainControl_MouseLeave(object sender, MouseEventArgs e)
        {
            ActiveHandler = null;
        }

        //Enables the movement of all handlers
        private void PreviewRect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MoveAllHandlers = true;
        }

        //Disables the movement of all handlers
        private void PreviewRect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MoveAllHandlers = false;
        }

        //Disables the movement of all handlers
        private void PreviewRect_MouseLeave(object sender, MouseEventArgs e)
        {
            MoveAllHandlers = false;
        }
        #endregion

        #region Clamp
        private double Clamp(double value, double min, double max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
        #endregion

        #region ImageCropping
        /// <summary>
        /// Cuts the area of the image inside the <see cref="PreviewRect"/>
        /// </summary>
        /// <returns>Null if source is null or if width or height are equal to 0, else returns a <see cref="CroppedBitmap"/></returns>
        public CroppedBitmap CutImage()
        {
            if (Source != null)
            {                
                int left = (int)(PixelWidth * (LeftHandler.Margin.Left - MinLeft) / ControlImage.ActualWidth);
                int top = (int)(PixelHeight * (TopHandler.Margin.Top - MinTop)/ ControlImage.ActualHeight);
                int width = (int)(PixelWidth * (PreviewRect.ActualWidth - (BorderSize * 2)) / ControlImage.ActualWidth);
                int height = (int)(PixelHeight * (PreviewRect.ActualHeight - (BorderSize * 2)) / ControlImage.ActualHeight);

                width = (int)Clamp(width, minPixelWidth, (maxPixelWidth > 0) ? maxPixelWidth : PixelWidth);
                height = (int)Clamp(height, minPixelHeight, (maxPixelHeight > 0) ? maxPixelHeight : PixelHeight);

                if (width <= 0 || height <= 0)
                {
                    return null;
                }

                return new CroppedBitmap((BitmapImage)Source, new Int32Rect(left, top, width, height));
            }
            else
            {
                return null;
            }            
        }
        #endregion
    }
}
