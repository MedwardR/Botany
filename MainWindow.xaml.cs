using Botany.Core;
using Botany.Interfaces;
using Botany.Rendering;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Botany;

public partial class MainWindow : Window
{
    private readonly Plant _plant;
    private readonly List<Segment> _segments;

    private readonly Stopwatch _sw;
    private readonly List<IUpdateable> _updateables;

    private readonly Camera _camera;

    private int _index;

    public MainWindow(Plant plant)
    {
        InitializeComponent();
        MouseDown += (s, e) => DragMove();

        _plant = plant;

        var ordered = plant.Segments.OrderBy(segment => segment.Depth);
        _segments = [.. ordered];

        _sw = Stopwatch.StartNew();
        _updateables = [_plant];

        _camera = new(0, 0);

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
        UpdateCanvas();
    }

    private void UpdateCanvas()
    {
        var model = _plant.Transform.ToMatrix();
        var view = _camera.ToMatrix(Width, Height);
        var matrix = model * view;

        for (; _index < _plant.Length; _index++)
        {
            var segment = _segments[_index];

            var start = Vector2.Transform(segment.Start, matrix);
            var end = Vector2.Transform(segment.End, matrix);

            var line = new Line
            {
                X1 = start.X,
                Y1 = start.Y,
                X2 = end.X,
                Y2 = end.Y,
                Stroke = Brushes.ForestGreen,
                StrokeThickness = 1,
            };
            Canvas.Children.Add(line);
        }
    }
}
