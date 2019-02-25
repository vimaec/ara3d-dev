using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using WPFBridge;

namespace Ara3D
{
    public static class Helix
    {
        public static Application CurrentOrNewApplication
            => Application.Current ?? new Application();

        public static void ApplicationDispatch(Action action)
            => CurrentOrNewApplication.Dispatcher.Invoke(action);

        public static void ApplicationRun(Func<Window> func)
            => CurrentOrNewApplication.Dispatcher.Invoke(() => CurrentOrNewApplication.Run(func()));

        public static void ShowModal()
        {
            var newWindowThread = new Thread(() =>
            {
                var app = CurrentOrNewApplication;
                var model = new ViewerTestModel();
                var w = new ViewerTestWindow
                {
                    DataContext = model,
                };
                w.Show();
                app.Run();
            });

            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.Start();
            newWindowThread.Join();
        }

        public static Model3DGroup LoadFileModel3DGroup(string filePath) 
            => new ModelImporter().Load(filePath);

        public static IGeometry LoadGeometry(string filePath)
             => LoadFileModel3DGroup(filePath).ToIGeometry();

        public static void RenderToPng(Viewport3D view, string fileName, Brush background = null, int overSamplingMultiplier = 1) 
            => view.SaveBitmap(fileName, background, overSamplingMultiplier, BitmapExporter.OutputFormat.Png);

        public static void RenderToJpg(Viewport3D view, string fileName, Brush background = null, int overSamplingMultiplier = 1) 
            => view.SaveBitmap(fileName, background, overSamplingMultiplier, BitmapExporter.OutputFormat.Jpg);

        public static void RenderToBmp(Viewport3D view, string fileName, Brush background = null, int overSamplingMultiplier = 1)
            => view.SaveBitmap(fileName, background, overSamplingMultiplier, BitmapExporter.OutputFormat.Bmp);        

        public static string[] ValidExportExtensions = {".obj", ".stl", ".dae", ".xaml", ".xml", ".x3d", ".jpg", ".png" };

        public static string ValidateExportExtension(string filePath)
            => ValidExportExtensions.Contains(Path.GetExtension(filePath ?? "").ToLowerInvariant())
                ? filePath
                : throw new Exception(
                    $"Target export file {filePath} does not have one of the exported extensions {string.Join(", ", ValidExportExtensions)}"); 
            
        public static void Export(Viewport3D view, string fileName, Brush background = null)
            => view.Export(ValidateExportExtension(fileName), background);        
    }
}
