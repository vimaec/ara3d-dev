using Ara3D;
using Ara3D.DotNetUtilities.Extra;
//using Ara3D.Revit.DataModel;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace G3DViewer
{
    public class MyLogger : StdLogger
    {
        public delegate void LogCallbackDelegate(LogEvent e);
        public LogCallbackDelegate OnLogCallback;

        public new ILogger Log(string message = "", LogLevel level = LogLevel.None, int eventId = 0)
        {
            var ret = base.Log(message, level, eventId);

            if (OnLogCallback != null)
            {
                OnLogCallback(Events.Last());
            }

            return ret;
        }
    };

    public class AttributeStat : ObservableObject
    {
        public string Association { get; set; }
        public int ArrayLength { get; set; }
        public string AttributeType { get; set; }
        public int AttributeTypeIndex { get; set; }
        public int DataArity { get; set; }
        public string DataType { get; set; }
    }

    public class DisplayStats : ObservableObject
    {
        public int NumFaces { get; set; }
        public int NumTriangles { get; set; }
        public int NumDegenerateFaces { get; set; }
        public int NumDegenerateTriangles { get; set; }
        public int NumSmallFaces { get; set; }
        public int NumSmallTriangles { get; set; }
        public int NumVertices { get; set; }
        public int NumMaterialIds { get; set; }
        public int NumObjectIds { get; set; }
        public float LoadTime { get; set; }
        public float VertexBufferGenerationTime { get; set; }
        public int FileSize { get; set; }
        public AABox AABB { get; set; } = new AABox();
        public ObservableCollection<AttributeStat> AttributeStats { get; } = new ObservableCollection<AttributeStat>();

        public const int NumHistogramDivisions = 16;
        public const float SmallTriangleSize = 0.000001f;
        public float MinTriangleArea = float.MaxValue;
        public float MaxTriangleArea = 0.0f;
        public int[] AreaHistogramArray = new int[NumHistogramDivisions];
        public Dictionary<string, float> AreaHistogramLog { get; set; } = new Dictionary<string, float>();
        public Dictionary<string, int> AreaHistogram { get; set; } = new Dictionary<string, int>();
    };

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel mainViewModel;
        IG3D mG3D = null;

        public static DisplayStats mDisplayStats = new DisplayStats();

        public MainWindow()
        {
            InitializeComponent();
            Closed += (s, e) => {
                if (DataContext is IDisposable)
                {
                    (DataContext as IDisposable).Dispose();
                }
            };
        }

        public void OpenG3D(string FileName)
        {
            mainViewModel = new MainViewModel();
            this.DataContext = mainViewModel;

            mDisplayStats = new DisplayStats();
            mainViewModel.displayStats = mDisplayStats;

            var logger = new MyLogger();

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var G3D = G3DExtensions.ReadFromFile(FileName);

            mDisplayStats.LoadTime = stopwatch.ElapsedMilliseconds / 1000.0f;

            OpenIG3D(G3D);
        }

        public void OpenFBX(string FileName)
        {
            mainViewModel = new MainViewModel();
            this.DataContext = mainViewModel;

            mDisplayStats = new DisplayStats();
            mainViewModel.displayStats = mDisplayStats;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var scene = FbxImporter.LoadFBX(FileName);
            mDisplayStats.LoadTime = stopwatch.ElapsedMilliseconds / 1000.0f;

            var geometry = scene.ToIGeometry();
            OpenIG3D(geometry);
            /*OpenIG3D(scene.TransformedGeometries()[2]);
            return;            
            foreach (var geom in scene.TransformedGeometries().ToArray())
            {
                if (geom != null)
                {
                    OpenIG3D(geom);
                }
            }*/
        }

        public void OpenIG3D(IG3D G3D)
        {
            mG3D = G3D;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            int index = mainViewModel.AddG3DData(mG3D);

            mDisplayStats.VertexBufferGenerationTime = stopwatch.ElapsedMilliseconds / 1000.0f - mDisplayStats.LoadTime;

            foreach (var attribute in mG3D.Attributes)
            {
                var attributeStat = new AttributeStat();

                attributeStat.Association = ((Association)attribute.Descriptor._association).ToString();
                attributeStat.ArrayLength = attribute.ElementCount();
                attributeStat.AttributeType = ((AttributeType)attribute.Descriptor._attribute_type).ToString();
                attributeStat.AttributeTypeIndex = attribute.Descriptor._attribute_type_index;
                attributeStat.DataArity = attribute.Descriptor._data_arity;
                attributeStat.DataType = ((DataType)attribute.Descriptor._data_type).ToString();

                mDisplayStats.AttributeStats.Add(attributeStat);
            }

            var materialIds = G3DExtensions.MaterialIds(mG3D);
            if (materialIds != null)
            {
                var materialIdMap = new Dictionary<int, int>();
                for (int materialIdIndex = 0; materialIdIndex < materialIds.Count; materialIdIndex++)
                {
                    int materialId = materialIds[materialIdIndex];
                    if (!materialIdMap.ContainsKey(materialId))
                    {
                        materialIdMap[materialId] = materialId;
                    }
                }
                mDisplayStats.NumMaterialIds = materialIdMap.Count;
            }

            var objectIds = G3DExtensions.ObjectIds(mG3D);
            if (objectIds != null)
            {
                var objectIdMap = new Dictionary<int, int>();
                for (int objectIdIndex = 0; objectIdIndex < objectIds.Count; objectIdIndex++)
                {
                    int objectId = objectIds[objectIdIndex];
                    if (!objectIdMap.ContainsKey(objectId))
                    {
                        objectIdMap[objectId] = objectId;
                    }
                }
                mDisplayStats.NumObjectIds = objectIdMap.Count;
            }

            mainViewModel.Title = "";
            mainViewModel.UpdateSubTitle();
            mainViewModel.displayStats = mDisplayStats;
            Chart.ItemsSource = mDisplayStats.AreaHistogramLog;
        }

        public void FileExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                var dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.Filter = "Obj File|*.obj|G3D File|*.g3d|PNG Image|*.png|JPG Image|*.jpg";
                if (dialog.ShowDialog() == true)
                {
                    string extension = System.IO.Path.GetExtension(dialog.FileName).ToLower();
                    if (extension == ".jpg" || extension == ".png")
                    {
                        HelixToolkit.Wpf.SharpDX.Direct2DImageFormat format = (extension == ".jpg" ? HelixToolkit.Wpf.SharpDX.Direct2DImageFormat.Jpeg : HelixToolkit.Wpf.SharpDX.Direct2DImageFormat.Png);
                        HelixToolkit.Wpf.SharpDX.ViewportExtensions.SaveScreen(view1, dialog.FileName, format);
                    }
                    else if (extension == ".obj")
                    {
                        Ara3D.Geometry.ToIGeometry(mG3D).WriteObj(dialog.FileName);
                    }
                    else if (extension == ".g3d")
                    {
                        mG3D.WriteG3D(dialog.FileName);
                    }
                }
            }
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {

        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }

        private void Window_DragLeave(object sender, DragEventArgs e)
        {

        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                string fileName = files[0];
                OpenFile(fileName);
            }
        }

        public void OpenFile(string FileName)
        {
            if (System.IO.Path.GetExtension(FileName).ToLower() == ".g3d")
            {
                OpenG3D(FileName);
            }
            else if (System.IO.Path.GetExtension(FileName).ToLower() == ".fbx")
            {
                OpenFBX(FileName);
            }
        }

    }
}
