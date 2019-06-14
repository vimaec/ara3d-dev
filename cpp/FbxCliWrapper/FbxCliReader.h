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
	public ref class FbxCliReader : public FbxCliBase
	{
	public:
		int LoadFBX(String^ FileName);

	private:
		bool LoadScene(const char* pFilename);
		void ProcessFBXNode(FbxNode* pNode, int ParentIndex);
		int ExtractFBXMeshData(FbxMesh* pMesh);
	};
}
