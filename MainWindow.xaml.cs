using Botany.Interfaces;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace Botany;

public partial class MainWindow : Window
{
    private const float FPS = 30;

    private readonly Stopwatch _sw;
    private readonly List<IUpdateable> _updateables;

    public MainWindow()
    {
        InitializeComponent();
        MouseDown += (_, _) => DragMove();

        _sw = Stopwatch.StartNew();
        _updateables = [];

        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1000f / FPS),
        };
        timer.Tick += Timer_Tick;
        timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        float deltaTime = _sw.ElapsedMilliseconds * 1000f;
        _sw.Restart();

        foreach (var u in _updateables)
        {
            u.Update(deltaTime);
        }
    }
}
