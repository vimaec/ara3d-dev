using FbxClrWrapper;
using System;
using System.Collections.Generic;

namespace Ara3D
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            FbxImporterTests.TestFbxImporter("", "");

            //var filePath = @"E:\VimAecDev\vims\Models\sketchup-automation\bob.fbx";
            //var filePath = @"E:\VimAecDev\vims\Models\sketchup-automation\R.Tamu.Baru.fbx";
            var filePath = @"E:\VimAecDev\vims\Models\CDiggins_313401_S_v19.fbx";
            //var filePath = @"E:\VimAecDev\vims\Models\10042-MDL-C134003-BM-000001.fbx";
            //var filePath = @"E:\VimAecDev\vims\Models\MobilityPavilion_mdl.fbx";
            //var filePath = @"E:\VimAecDev\vims\Models\pentagon_prism.FBX";
            //var filePath = @"E:\VimAecDev\vims\Models\axis_tripod.FBX";

            var scene = FbxImporter.LoadFBX(filePath);
   //         var outputFilePath = @"d:\test.obj";
   //         scene.ToIGeometry().WriteObj(outputFilePath);

            FbxImporter.SaveFBX(scene, @"E:\VimAecDev\vims\Models\test.fbx");
        }
    }
}
