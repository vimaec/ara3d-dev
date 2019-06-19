using FbxClrWrapper;
using System;
using System.Collections.Generic;

namespace Ara3D
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            //FbxImporterTests.TestFbxImporter("", "");

            //var filePath = @"E:\VimAecDev\vims\Models\sketchup-automation\bob.fbx";
            //var filePath = @"E:\VimAecDev\vims\Models\sketchup-automation\R.Tamu.Baru.fbx";
            //var filePath = @"E:\VimAecDev\vims\Models\CDiggins_313401_S_v19.fbx";
            //var filePath = @"E:\VimAecDev\vims\Models\10042-MDL-C134003-BM-000001.fbx";
            //var filePath = @"E:\VimAecDev\vims\Models\MobilityPavilion_mdl.fbx";
            //var filePath = @"E:\VimAecDev\vims\Models\pentagon_prism.FBX";
            var filePath = @"E:\VimAecDev\vims\Models\axis_tripod.FBX";
            //var filePath = @"E:\VimAecDev\vims\Models\HI1901-B-SBR.FBX";

            FbxImporter importer = new FbxImporter();
            FbxExporter exporter = new FbxExporter();


            var scene = importer.LoadFBX(filePath);
            exporter.SaveFBX(scene, @"E:\VimAecDev\vims\Models\test.fbx");

            importer.DestroyData();
            exporter.DestroyData();
        }
    }
}
