using FbxClrWrapper;
using System;
using System.Collections.Generic;

namespace Ara3D
{
    public static class Program
    {
        class TestObject
        {
            int x;
            int y;
        }
        public static void Main(string[] args)
        {
            {

                int numItems = 10000000;
                var data = new TestObject[numItems];
                for (int i = 0; i < numItems; i++)
                {
                    data[i] = new TestObject();
                }

                var dict = new Dictionary<TestObject, int>();


                Console.WriteLine("Inserting " + numItems + " items into Dictionary...");

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                for (int i = 0; i < numItems; i++)
                {
                    dict[data[i]] = i;
                }

                var time = stopwatch.ElapsedMilliseconds;

                var res = true;
                for (int i = 0; i < numItems; i++)
                {
                    res = res && (dict[data[i]] == i);
                }

                var time2 = stopwatch.ElapsedMilliseconds - time;

                Console.WriteLine("Write Time: " + time + "ms Read Time: " + time2 + "ms");
                var t = time;
            }

            {

                int numItems = 10000000;
                var data = new string[numItems];
                for (int i = 0; i < numItems; i++)
                {
                    data[i] = "Data: " + i;
                }

                var dict = new int[numItems];

                Console.WriteLine("Inserting " + numItems + " items into array...");

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                for (int i = 0; i < numItems; i++)
                {
                    dict[i] = i;
                }

                var time = stopwatch.ElapsedMilliseconds;

                var res = true;
                for (int i = 0; i < numItems; i++)
                {
                    res = res && (dict[i] == i);
                }

                var time2 = stopwatch.ElapsedMilliseconds - time;

                Console.WriteLine("Array test:");
                Console.WriteLine("Write Time: " + time + "ms Read Time: " + time2 + "ms");
                var t = time;
            }

            //   var filePath = @"E:\VimAecDev\vims\Models\CDiggins_313401_S_v19.fbx";
            //    var filePath = @"E:\VimAecDev\vims\Models\10042-MDL-C134003-BM-000001.fbx";
            var filePath = @"E:\VimAecDev\vims\Models\MobilityPavilion_mdl.fbx";
            //var filePath = @"E:\VimAecDev\vims\Models\pentagon_prism.FBX";

            var scene = FbxImporter.LoadFBX(filePath);
            var outputFilePath = @"d:\test.obj";
            scene.ToIGeometry().WriteObj(outputFilePath);

            FbxImporter.SaveFBX(scene, @"E:\VimAecDev\vims\Models\test.fbx");
        }
    }
}
