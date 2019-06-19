#pragma once

#include <cstdint>
#include <vector>
#include <string>

#include "fbxsdk/include/fbxsdk.h"

#include "FBXMeshData.h"
#include "FBXSceneData.h"

namespace FbxClrWrapper
{
	public ref class FbxCliBase
	{
	protected:
		FbxManager* mSdkManager = nullptr;
		FbxScene* mScene = nullptr;
		FBXSceneDataInternal *mSceneData = nullptr;
		FBXSceneData ^mSceneData_ = nullptr;

	public:
		~FbxCliBase()
		{
			ShutDownAPI();
			DestroyData();
		}

	public:
		void Initialize();
		void ShutDownAPI();
		void DestroyData();
	
		FBXSceneData ^ GetSceneData() { return mSceneData_; }
		void SetSceneData(FBXSceneData^ SceneData) { mSceneData_ = SceneData; }

	protected:
		bool TransformDataToCLI();
		bool TransformDataFromCLI();
		void InitializeSdkObjects();

		Ara3D::Association FbxMappingModeToAra3DAssociation(FbxLayerElement::EMappingMode MappingMode)
		{
			Ara3D::Association associationTable[] = {
				Ara3D::Association::assoc_none,		// eNone,
				Ara3D::Association::assoc_vertex,	// eByControlPoint,
				Ara3D::Association::assoc_corner,	// eByPolygonVertex,
				Ara3D::Association::assoc_face,		// eByPolygon,
				Ara3D::Association::assoc_edge,		// eByEdge,
				Ara3D::Association::assoc_object,	// eAllSame
			};

			return associationTable[MappingMode];
		}
	};
}
