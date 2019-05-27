using FbxClrWrapper;
using System.Collections.Generic;

namespace Ara3D
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var filePath = @"E:\VimAecDev\vims\Models\MobilityPavilion_mdl.fbx";

            Ara3D.FbxImporter.CreateScene("");
            var scene = FbxImporter.CreateScene(filePath);
            var outputFilePath = @"d:\test.obj";
            scene.ToIGeometry().WriteObj(outputFilePath);
        }
    }
}
