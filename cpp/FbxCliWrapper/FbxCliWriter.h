#pragma once

#include <cstdint>
#include <vector>
#include <string>

#include "fbxsdk/include/fbxsdk.h"

#include "FBXMeshData.h"
#include "FBXSceneData.h"
#include "FbxCliWrapper.h"

namespace FbxClrWrapper
{
	public ref class FbxCliWriter : public FbxCliBase
	{
	public:
		int SaveFBX(String^ FileName);

	private:
		bool SaveScene(const char* pFilename);
		void ExportNodes();
		void CreateFBXMeshList(std::vector<FbxMesh*>& MeshList);
		void CreateFBXNodes(std::vector<FbxMesh*>& MeshList);
	};
}
