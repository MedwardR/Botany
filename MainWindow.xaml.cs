using System.Windows;

namespace Botany;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        MouseDown += (_, _) => DragMove();
    }
}
