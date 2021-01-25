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

        //Cutter bounds
        private double MinLeft, MinTop, MaxRight, MaxBottom;
        //Active handle
        private Rectangle ActiveHandler;        

        #region SourceProperty
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
                    UpdateLayout();
                    UpdateBounds();
                    UpdateHandlersPosition();                   
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
            LeftHandler.Margin = new Thickness(Clamp(position, MinLeft, RightHandler.Margin.Left), LeftHandler.Margin.Top, 0, 0);
            UpdateTopBottomMiddlePosition();
        }

        private void MoveRightHandler(double position)
        {
            RightHandler.Margin = new Thickness(Clamp(position, LeftHandler.Margin.Left, MaxRight), RightHandler.Margin.Top, 0, 0);
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
        #endregion

        private double Clamp(double value, double min, double max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
    }
}
