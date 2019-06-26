﻿using Ara3D;
using UnityEditor.Experimental.AssetImporters;

[ScriptedImporter(1, "g3d")]
// ReSharper disable once CheckNamespace
public class ImportG3D : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        ctx.ImportG3D();
    }
}