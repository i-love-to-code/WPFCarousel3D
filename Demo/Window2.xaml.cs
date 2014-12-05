namespace SamNoble.Wpf.Controls.DemoClient
{
    using SamNoble.Wpf.Controls.Carousel3D;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        private Carousel3DPanel ellipsePanel3D;

        public Window2()
        {
            InitializeComponent();

            this.DataContext = new List<string>(new [] 
            {
                "Item 1",
                "Item 2",
                "Item 3",
                "Item 4",
                "Item 5",
                "Item 6",
                "Item 7",
                "Item 8"
            });
        }

        public void ItemClick(object sender, EventArgs e)
        {
            if (animateToFront.IsChecked == true)
            {
                this.ellipsePanel3D.AnimateIntoView(sender as UIElement, true);
            }
            else
            {
                MessageBox.Show(string.Format("You clicked on {0}", (sender as Button).Content));
            }
        }

        private void Carousel3DPanel_Loaded(object sender, RoutedEventArgs e)
        {
            this.ellipsePanel3D = sender as Carousel3DPanel;
        }
    }
}
