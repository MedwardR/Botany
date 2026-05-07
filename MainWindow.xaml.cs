using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Botany.Interfaces;
using Botany.State;

namespace Botany;

public partial class MainWindow : Window
{
    private readonly Stopwatch _sw;
    private readonly List<IUpdateable> _updateables;

    private readonly Plant _plant;

    private int _index;

    public MainWindow()
    {
        InitializeComponent();
        MouseDown += (_, _) => DragMove();

        _sw = Stopwatch.StartNew();
        _plant = new();

        _updateables = [_plant];

        CompositionTarget.Rendering += OnRendering;
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        float deltaTime = (float)_sw.Elapsed.TotalSeconds;
        _sw.Restart();

        foreach (var u in _updateables)
        {
            u.Update(deltaTime);
        }
        Render();
    }

    private void Render()
    {
        while (_index < _plant.Length)
        {
            var line = _plant.Lines[_index];

            var shape = new Line
            {
                X1 = line.Start.X,
                Y1 = line.Start.Y,
                X2 = line.End.X,
                Y2 = line.End.Y,
                Stroke = Brushes.ForestGreen,
                StrokeThickness = 1,
            };
            Canvas.Children.Add(shape);

            _index++;
        }
    }
}
