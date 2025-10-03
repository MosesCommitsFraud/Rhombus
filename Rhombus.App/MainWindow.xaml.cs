using System.Windows;
using Rhombus.App.ViewModels;

namespace Rhombus.App;

public partial class MainWindow : Window
{
    public MainWindow(SoundboardViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
