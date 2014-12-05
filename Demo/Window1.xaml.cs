namespace SamNoble.Wpf.Controls.DemoClient
{
    using SamNoble.Wpf.Controls.Carousel3D;
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private Storyboard xRotationAnimation;
        private Storyboard yRotationAnimation;
        private Storyboard zRotationAnimation;

        private Stopwatch stopwatch = new Stopwatch();
        private int frameCounter = 0;
        private TimeSpan lastUpdate = new TimeSpan();

        public Window1()
        {
            InitializeComponent();

            CompositionTarget.Rendering += this.CalculateFramerate;

            for (var i = 0; i < 16; i++)
            {
                this.btnAddItem_Click(this, new RoutedEventArgs());
            }
        }

        private void cbxAnimateXChecked(object sender, RoutedEventArgs e)
        {
            if (this.xRotationAnimation == null)
            {
                this.xRotationAnimation = this.CreateStoryboard(Carousel3DPanel.EllipseRotationXProperty);
                this.xRotationAnimation.Begin();
            }
            else
            {
                var newValue = this.ellipsePanel3D.EllipseRotationX;

                this.xRotationAnimation.Stop();
                this.xRotationAnimation.Remove();
                this.xRotationAnimation = null;

                this.ellipsePanel3D.EllipseRotationX = newValue;
            }
        }

        private void cbxAnimateYChecked(object sender, RoutedEventArgs e)
        {
            if (this.yRotationAnimation == null)
            {
                this.yRotationAnimation = this.CreateStoryboard(Carousel3DPanel.EllipseRotationYProperty);
                this.yRotationAnimation.Begin();
            }
            else
            {
                var newValue = this.ellipsePanel3D.EllipseRotationY;

                this.yRotationAnimation.Stop();
                this.yRotationAnimation.Remove();
                this.yRotationAnimation = null;

                this.ellipsePanel3D.EllipseRotationY = newValue;
            }
        }

        private void cbxAnimateZChecked(object sender, RoutedEventArgs e)
        {
            // To animate this thing, we want to animate it from its current position to 360, then loop indefinitely.
            // One solution is to have an animation to take it from its current to 360, then one from 
            if (this.zRotationAnimation == null)
            {
                this.zRotationAnimation = this.CreateStoryboard(Carousel3DPanel.EllipseRotationZProperty);
                this.zRotationAnimation.Begin();
            }
            else
            {
                var newValue = this.ellipsePanel3D.EllipseRotationZ;

                this.zRotationAnimation.Stop();
                this.zRotationAnimation.Remove();
                this.zRotationAnimation = null;

                this.ellipsePanel3D.EllipseRotationZ = newValue;
            }
        }

        private Storyboard CreateStoryboard(DependencyProperty targetProperty)
        {
            var animation = new Storyboard();

            Storyboard.SetTarget(animation, this.ellipsePanel3D);
            Storyboard.SetTargetProperty(animation, new PropertyPath(targetProperty));

            animation.Children.Add(new DoubleAnimation()
            {
                To = 360,
                Duration = TimeSpan.FromSeconds((360 - (double)this.ellipsePanel3D.GetValue(targetProperty)) * 0.0278),
                AutoReverse = false
            });

            animation.Children.Add(new DoubleAnimation()
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(10),
                AutoReverse = false,
                RepeatBehavior = RepeatBehavior.Forever
            });

            return animation;
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e)
        {
            ellipsePanel3D.Children.Add(new Button() { Content = string.Format("Button {0}", ellipsePanel3D.Children.Count + 1) });
        }

        private void btnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (ellipsePanel3D.Children.Count > 0)
            {
                ellipsePanel3D.Children.RemoveAt(ellipsePanel3D.Children.Count - 1);
            }
        }

        private void btnMoveNext_Click(object sender, RoutedEventArgs e)
        {
            this.ellipsePanel3D.RotateRight(true);
        }

        private void btnMovePrev_Click(object sender, RoutedEventArgs e)
        {
            this.ellipsePanel3D.RotateLeft(true);
        }

        private void ItemClick(object sender, RoutedEventArgs e)
        {
            if (this.cbxAnimateToSelected.IsChecked == true)
            {
                this.ellipsePanel3D.AnimateIntoView(sender as UIElement, true);
            }
            else
            {
                MessageBox.Show(string.Format("You clicked on '{0}'", (sender as Button).Content));
            }
        }

        private Point lastMousePosition;

        private void CarouselMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.lastMousePosition = e.GetPosition(this.ellipsePanel3D);

            this.ellipsePanel3D.MouseMove += this.CarouselMouseMove;
            this.ellipsePanel3D.PreviewMouseLeftButtonUp += this.CarouselMouseButtonUp;
        }

        private void CarouselMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.ellipsePanel3D.MouseMove -= this.CarouselMouseMove;
            this.ellipsePanel3D.PreviewMouseLeftButtonUp -= this.CarouselMouseButtonUp;
        }

        private void CarouselMouseMove(object sender, MouseEventArgs e)
        {
            var mousePosition = e.GetPosition(this.ellipsePanel3D);

            var delta = mousePosition.X - this.lastMousePosition.X;

            this.ellipsePanel3D.EllipseRotationY += delta;

            this.lastMousePosition = mousePosition;
        }

        private void CalculateFramerate(object sender, EventArgs e)
        {
            if (frameCounter++ == 0)
            {
                stopwatch.Start();
            }

            if (this.stopwatch.Elapsed - this.lastUpdate > TimeSpan.FromSeconds(1))
            {
                this.lastUpdate = this.stopwatch.Elapsed;

                this.Title = string.Format("FPS: {0}", frameCounter);

                frameCounter = 0;
            }
        }
    }
}
