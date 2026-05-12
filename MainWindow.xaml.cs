using Botany.Interfaces;
using Botany.State;
using Botany.Utilities;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Botany;

public partial class MainWindow : Window
{
    private readonly Stopwatch _sw;
    private readonly List<IUpdateable> _updateables;

    private readonly Plant _plant;
    private readonly List<Segment> _segments;

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

        float x = (float)Width / 2f;
        float y = (float)Height / 2f;
        _plant.Position = new(x, y);
        _plant.Rotation = -90f;

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
        float radians = MathX.DegToRad(_plant.Rotation);

        float cos = MathF.Cos(radians);
        float sin = MathF.Sin(radians);

        for (; _index < _plant.Length; _index++)
        {
            var segment = _segments[_index];

            float sx = segment.Start.X * cos - segment.Start.Y * sin;
            float sy = segment.Start.X * sin + segment.Start.Y * cos;

            float ex = segment.End.X * cos - segment.End.Y * sin;
            float ey = segment.End.X * sin + segment.End.Y * cos;

            var line = new Line
            {
                X1 = _plant.Position.X - sx,
                Y1 = _plant.Position.Y + sy,
                X2 = _plant.Position.X - ex,
                Y2 = _plant.Position.Y + ey,
                Stroke = Brushes.ForestGreen,
                StrokeThickness = 1,
            };

            Canvas.Children.Add(line);
        }
    }
}
