using System;
using System.Collections.Generic;
using Autodesk.Max.MaxPlus;

namespace Ara3D
{
    public class UtilityRenameAndSetLayer : IUtilityPlugin
    {
        public void Evaluate()
        {
            var folder = @"C:\Users\ara3d\AppData\Local\Ara3D\RevitDevPlugin\2019-03-10_12-52-27-rac_basic_sample_project";
            var document = API.LoadDocument(folder);

            var layers = new Dictionary<string, Layer>();

            foreach (var n in Core.GetRootNode().GetDescendants())
            {
                var e = document.GetElement(n.Name);
                if (e == null)
                    continue;
                
                n.Name = e.Model.Name;

                var cat = e.Category;
                if (cat == null)
                    continue;

                var catName = cat.Model.Name;
        
                if (!layers.ContainsKey(catName))
                {
                    // Note: will fail if a layer with that name already exists 
                    layers.Add(catName, API.CreateLayer(catName));
                }

                // Unfortunate API name I know
                layers[catName].AddToLayer(n);
            } 
        }
    }
}