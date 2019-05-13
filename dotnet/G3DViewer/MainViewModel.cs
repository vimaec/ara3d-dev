namespace G3DViewer
{
    using System.Collections.Generic;

    using HelixToolkit.Wpf.SharpDX;
    using SharpDX;
    using Media3D = System.Windows.Media.Media3D;
    using Point3D = System.Windows.Media.Media3D.Point3D;
    using Vector3D = System.Windows.Media.Media3D.Vector3D;
    using Transform3D = System.Windows.Media.Media3D.Transform3D;
    using TranslateTransform3D = System.Windows.Media.Media3D.TranslateTransform3D;
    using Color = System.Windows.Media.Color;
    using Plane = SharpDX.Plane;
    using Vector3 = SharpDX.Vector3;
    using Colors = System.Windows.Media.Colors;
    using Color4 = SharpDX.Color4;
    using System;
    using System.IO;
    using System.Windows.Threading;
    using System.Diagnostics;
    using System.Linq;
    using System.Collections.ObjectModel;

    public struct sG3DObject
    {
        int mStartIndex;
        int mCount;
    }

    public class MainViewModelMesh : ObservableObject
    {
        public MeshGeometry3D Model { get; set; }
        public ObservableCollection<Matrix> ModelInstances { get; } = new ObservableCollection<Matrix>();
        public ObservableCollection<InstanceParameter> InstanceParams { get; } = new ObservableCollection<InstanceParameter>();
        public PhongMaterial ModelMaterial { get; set; }
        public Transform3D ModelTransform { get; set; }

        public void AddInstance(Matrix Mat, InstanceParameter InstanceParam)
        {
            ModelInstances.Add(Mat);
            InstanceParams.Add(InstanceParam);
            OnPropertyChanged(nameof(InstanceParams));
            OnPropertyChanged(nameof(ModelInstances));
        }
    }

    public class MainViewModel : BaseViewModel
    {
        public ObservableCollection<MainViewModelMesh> Models { get; set; } = new ObservableCollection<MainViewModelMesh>();
        public Matrix ModelsTransform = Matrix.Identity;
        public LineGeometry3D Lines { get; private set; }
        public LineGeometry3D Grid { get; private set; }

        public Vector3D DirectionalLightDirection { get; private set; }
        public Color DirectionalLightColor { get; private set; }
        public Color AmbientLightColor { get; private set; }
        public Stream Texture { private set; get; }

        public DisplayStats displayStats
        {
            get
            {
                return MainWindow.mDisplayStats;
            }
        }

        private int InstanceCount = 0;
        private int ModelCount = 0;
        private int TriangleCount = 0;

        public MainViewModel()
        {
            Title = "G3D Viewer";
            EffectsManager = new DefaultEffectsManager();
            // camera setup
            Camera = new PerspectiveCamera { Position = new Point3D(40, 40, 40), LookDirection = new Vector3D(-40, -40, -40), UpDirection = new Vector3D(0, 1, 0), FarPlaneDistance = 10000.0, NearPlaneDistance = 5.0 };

            // setup lighting            
            this.AmbientLightColor = Colors.DarkGray;
            this.DirectionalLightColor = Colors.White;
            this.DirectionalLightDirection = new Vector3D(-2, -5, -2);

            
            // Texture = LoadFileToMemory("E:/VimAecDev/Cubemap_Grandcanyon.dds");
        }

        public void UpdateSubTitle()
        {
            SubTitle = "Models: " + ModelCount + "\nInstances: " + InstanceCount + "\nTriangleCount: " + TriangleCount;
        }

        public int AddModel(MeshGeometry3D Model)
        {
            MainViewModelMesh newViewModelMesh = new MainViewModelMesh();

            newViewModelMesh.Model = Model;
            newViewModelMesh.ModelTransform = Media3D.Transform3D.Identity;

            // model material
            newViewModelMesh.ModelMaterial = PhongMaterials.White;
            newViewModelMesh.ModelMaterial.DiffuseMap = LoadFileToMemory("E:/VimAecDev/floor_d.png");
            newViewModelMesh.ModelMaterial.NormalMap = LoadFileToMemory("E:/VimAecDev/floor_n.png");

            Models.Add(newViewModelMesh);
            OnPropertyChanged(nameof(Models));

            ModelCount++;

            return Models.Count - 1;
        }

        public void AddInstance(int ModelIndex, Ara3D.Matrix4x4 Transform)
        {
            var model = Models[ModelIndex];

            var color = new Color4(1, 1, 1, 1);
         //   model.AddInstance(Ara3DToSharpDX(Transform), new InstanceParameter() { DiffuseColor = color, TexCoordOffset = new Vector2(0, 0) });
            model.AddInstance(SharpDX.Matrix.Identity, new InstanceParameter() { DiffuseColor = color, TexCoordOffset = new Vector2(0, 0) });

            TriangleCount += model.Model.Triangles.Count();

            InstanceCount++;
        }
   /*        
        const int num = 1;
        List<Matrix> instances = new List<Matrix>(num * 2);
        List<InstanceParameter> parameters = new List<InstanceParameter>(num * 2);
     
        private void CreateModels()
        {


            var matrix = Matrix.Identity;
                    var color = new Color4(1, 1, 1, 1);//new Color4((float)Math.Abs(i) / num, (float)Math.Abs(j) / num, (float)Math.Abs(i + j) / (2 * num), 1);
                    //  var emissiveColor = new Color4( rnd.NextFloat(0,1) , rnd.NextFloat(0, 1), rnd.NextFloat(0, 1), rnd.NextFloat(0, 0.2f));
                

                    parameters.Add(new InstanceParameter() { DiffuseColor = color, TexCoordOffset = new Vector2(0,0) });
                    instances.Add(matrix);

            InstanceParam = parameters.ToArray();
            ModelInstances = instances.ToArray();
        }
        */
        public void OnMouseLeftButtonDownHandler(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var viewport = sender as Viewport3DX;
            if (viewport == null) { return; }
            var point = e.GetPosition(viewport);
            var hitTests = viewport.FindHits(point);
            if (hitTests.Count > 0)
            {
                foreach(var hit in hitTests)
                {                  
                 /*   if (hit.ModelHit is InstancingMeshGeometryModel3D)
                    {
                        var index = (int)hit.Tag;
                        InstanceParam[index].EmissiveColor = InstanceParam[index].EmissiveColor != Colors.Yellow.ToColor4()? Colors.Yellow.ToColor4() : Colors.Black.ToColor4();
                        InstanceParam = (InstanceParameter[])InstanceParam.Clone();
                        break;
                    }
                    else if(hit.ModelHit is LineGeometryModel3D)
                    {
                        var index = (int)hit.Tag;
                        SelectedLineInstances = new Matrix[] { ModelInstances[index] };
                        break;
                    }*/
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        static SharpDX.Vector3 Ara3DToSharpDX(Ara3D.Vector3 input)
        {
            return new SharpDX.Vector3(input.X, input.Z, input.Y);
        }
        static SharpDX.Matrix Ara3DToSharpDX(Ara3D.Matrix4x4 input)
        {
            return new SharpDX.Matrix(
                input.M11, input.M12, input.M13, input.M14,
                input.M21, input.M22, input.M23, input.M24,
                input.M31, input.M32, input.M33, input.M34,
                input.M41, input.M42, input.M43, input.M44
                );
        }

        MeshBuilder bakedMeshBuilder = null;

        public void StartBakingModel()
        {
            Debug.Assert(bakedMeshBuilder == null);
            bakedMeshBuilder = new MeshBuilder(true, true, true);
        }
        public void EndBakingModel()
        {
            Debug.Assert(bakedMeshBuilder != null);
            int index = AddModel(bakedMeshBuilder.ToMeshGeometry3D());
            bakedMeshBuilder = null;

            AddInstance(index, Ara3D.Matrix4x4.Identity);
        }

        Ara3D.Vector3 mul(Ara3D.Matrix4x4 mat, Ara3D.Vector3 vec)
        {
            return new Ara3D.Vector3(
                mat.M11 * vec.X + mat.M12 * vec.Y + mat.M13 * vec.Z,
                mat.M21 * vec.X + mat.M22 * vec.Y + mat.M23 * vec.Z,
                mat.M31 * vec.X + mat.M32 * vec.Y + mat.M33 * vec.Z
                ) + new Ara3D.Vector3(mat.M41, mat.M42, mat.M43);
        }

        internal void BakeInstance(Ara3D.IG3D g3dFile, Ara3D.Matrix4x4 Transform)
        {
            bool hasFixedFaceSize = Ara3D.G3DExtensions.HasFixedFaceSize(g3dFile);
            int faceCount = Ara3D.G3DExtensions.FaceCount(g3dFile);
            Ara3D.IArray<int> faceSizes = Ara3D.G3DExtensions.FaceSizes(g3dFile);

            var vertexData = g3dFile.VertexAttribute.ToVector3s();
            var indexData = g3dFile.IndexAttribute.ToInts();

            int globalVectorIndex = 0;
            for (int face = 0; face < faceCount; face++)
            {
                int faceSize = faceSizes[face];

                List<SharpDX.Vector3> vectors = new List<Vector3>(faceSize);
                for (int i = 0; i < faceSize; i++)
                {
                    int vectorIndex = indexData[globalVectorIndex + faceSize - 1 - i];
                    vectors.Add(Ara3DToSharpDX(mul(Transform, vertexData[vectorIndex])));
                }

                globalVectorIndex += faceSize;

                bakedMeshBuilder.AddPolygon(vectors);
            }

            displayStats.NumVertices += vertexData.Count;
            displayStats.NumFaces += faceCount;
        }

        internal int AddG3DData(Ara3D.IG3D g3dFile)
        {
            bool hasFixedFaceSize = Ara3D.G3DExtensions.HasFixedFaceSize(g3dFile);
            int faceCount = Ara3D.G3DExtensions.FaceCount(g3dFile);
            Ara3D.IArray<int> faceSizes = Ara3D.G3DExtensions.FaceSizes(g3dFile);

            var vertexData = g3dFile.VertexAttribute.ToVector3s();
            var indexData = g3dFile.IndexAttribute.ToInts();


            var b1 = new MeshBuilder(true, true, true);
            var l1 = new LineBuilder();

            int globalVectorIndex = 0;
            for (int face = 0; face < faceCount; face++)
            {
                int faceSize = faceSizes[face];

                List<SharpDX.Vector3> vectors = new List<Vector3>(faceSize);
                for (int i = 0; i < faceSize; i++)
                {
                    int vectorIndex = indexData[globalVectorIndex + faceSize - 1 - i];
                    vectors.Add(Ara3DToSharpDX(vertexData[vectorIndex]));
                }

                globalVectorIndex += faceSize;

                b1.AddPolygon(vectors);

                for (int vectorIndex = 0; vectorIndex < faceSize; vectorIndex++ )
                {
                    l1.AddLine(vectors[vectorIndex], vectors[(vectorIndex + 1) % faceSize]);
                }
            }

            return AddModel(b1.ToMeshGeometry3D());
/*            for (int i = 0; i < Model.TextureCoordinates.Count; ++i)
            {
                var tex = Model.TextureCoordinates[i];
                Model.TextureCoordinates[i] = new Vector2(tex.X * 0.5f, tex.Y * 0.5f);
            }

            l1.AddBox(new Vector3(0, 0, 0), 1.1, 1.1, 1.1);
            Lines = l1.ToLineGeometry3D();
            Lines.Colors = new Color4Collection(Enumerable.Repeat(Colors.White.ToColor4(), Lines.Positions.Count));*/
        }
    }
}