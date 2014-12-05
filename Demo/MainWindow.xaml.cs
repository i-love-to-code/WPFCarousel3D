namespace SamNoble.Wpf.Controls.DemoClient
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new Window1().Show();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            new Window2().Show();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }
    }
}
