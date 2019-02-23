// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainViewModel.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace Ara3D
{
    public class ViewerTestModel : INotifyPropertyChanged
    {
        public ViewerTestModel()
        {
            CreateModel();
        }

        void CreateModel()
        {
            var cube = new CubeVisual3D
            {
                Center = new Point3D(3, 0, 0),
                Fill = new SolidColorBrush(Colors.Yellow),
                SideLength = 0.9f
            };
            Model = cube;
        }

        private MeshElement3D model;
        public MeshElement3D Model
        {
            get { return model; }
            set { model = value; RaisePropertyChanged("Model"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}