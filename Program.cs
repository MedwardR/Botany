using Botany.State;
using System.IO;

namespace Botany;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            var app = new App();

            foreach (string path in args)
            {
                if (File.Exists(path))
                {
                    string ini = File.ReadAllText(path);
                    var plant = Plant.Deserialize(ini);

                    plant.Speed = 86400f * 200;

                    var window = new MainWindow(plant);
                    window.Show();
                }
                else throw new FileNotFoundException($"File not found: '{path}'", path);
            }
            app.Run();
        }
    }
}
