using System.Windows;

namespace Ara3D
{
    public partial class ViewerTestWindow : Window
    {
        public ViewerTestWindow()
        {
            InitializeComponent();
        }

        public void FileExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
