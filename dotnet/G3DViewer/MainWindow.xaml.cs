using Ara3D;
using Ara3D.DotNetUtilities.Extra;
using Ara3D.Revit.DataModel;
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
        public int NumVertices { get; set; }
        public int NumMaterialIds { get; set; }
        public float LoadTime { get; set; }
        public int FileSize { get; set; }
        public ObservableCollection<AttributeStat> AttributeStats { get; } = new ObservableCollection<AttributeStat>();
    };

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel mainViewModel;

        public static DisplayStats mDisplayStats = new DisplayStats();


        //        string folderName = "E:/VimAecDev/vims/Mechanical_Room-2019";
        string folderName = "E:/VimAecDev/vims/Mountain_House-2019";
//        string folderName = "E:/VimAecDev/vims/Vision_House_Vimaec_2019";

        public MainWindow()
        {
            InitializeComponent();
            mainViewModel = new MainViewModel();
            Closed += (s, e) => {
                if (DataContext is IDisposable)
                {
                    (DataContext as IDisposable).Dispose();
                }
            };

            this.DataContext = mainViewModel;

            mDisplayStats = new DisplayStats();

            
            var logger = new MyLogger();


            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var documentModel = Ara3D.Revit.DataModel.RevitDataModelExtensions.LoadDocumentModelFromFolder(new WrappedJsonSerializer(), folderName, logger);
  //          var sceneNodeList = Ara3D.Revit.DataModel.RevitDataModelExtensions.LoadSceneGraph(folderName, logger);

            var mainGeometryFilePath = System.IO.Path.Combine(folderName, "main.g3d");
            var g3d = G3DExtensions.ReadFromFile(mainGeometryFilePath);

            mDisplayStats.LoadTime = stopwatch.ElapsedMilliseconds / 1000.0f;

            foreach (var attribute in g3d.Attributes)
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

            var groups = Ara3D.Revit.DataModel.RevitDataModelExtensions.SplitByGroup(g3d);

            if (false) // instancing
            {
                foreach (var group in groups)
                {
                    mainViewModel.AddG3DData(group);
                }

                foreach (var node in documentModel.UniqueNodes)
                {
                    if (node.GeometryIndex >= 0)
                    {
                        mainViewModel.AddInstance(node.GeometryIndex, node.Transform);
                    }
                }
            }
            else
            {
                mainViewModel.StartBakingModel();

                foreach (var node in documentModel.UniqueNodes)
                {
                    if (node.GeometryIndex >= 0)
                    {
                        mainViewModel.BakeInstance(groups[node.GeometryIndex], node.Transform);
                    }
                }

                mainViewModel.EndBakingModel();
            }

  //          mainViewModel.AddInstance(0, Matrix4x4.Identity);

            mainViewModel.Title = folderName;
            mainViewModel.UpdateSubTitle();
        }
        public void FileExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BatchedMeshGeometryModel3D_Mouse3DDown(object sender, HelixToolkit.Wpf.SharpDX.MouseDown3DEventArgs e)
        {
    //        viewModel.SelectedGeometry = e.HitTestResult.Geometry;
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S)
            {
                var dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.Filter = "Obj File|*.obj|G3D File|*.g3d|PNG Image|*.png|JPG Image|*.jpg";
                if (dialog.ShowDialog() == true)
                {
                    string fileName = dialog.FileName;
                    string extension = System.IO.Path.GetExtension(fileName).ToLower();
                    if (extension == ".jpg" || extension == ".png")
                    {

                    }
                    else if (extension == ".obj")
                    {

                    }
                    else if (extension == ".g3d")
                    {

                    }
                }
            }
        }
    }
}
