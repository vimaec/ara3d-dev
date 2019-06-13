#pragma once

#include <cstdint>
#include <vector>
#include <string>

#include "fbxsdk/include/fbxsdk.h"

#include "FBXMeshData.h"
#include "FBXSceneData.h"

namespace FbxClrWrapper
{
	public ref class FBXLoader
	{
	private:
		static FbxManager* mSdkManager = nullptr;
		static FbxScene* mScene = nullptr;
		static FBXSceneDataInternal *mSceneData = nullptr;
		static FBXSceneData ^mSceneData_ = nullptr;

	public:
		static void Initialize();
		static void ShutDown();
		static int LoadFBX(String^ FileName);
		static int SaveFBX(String^ FileName);
		static FBXSceneData ^ GetSceneData() { return mSceneData_; }
		static void SetSceneData(FBXSceneData^ SceneData) { mSceneData_ = SceneData; }

	private:
		static bool TransformDataToCLI();
		static bool TransformDataFromCLI();

		static void InitializeSdkObjects();
		static bool LoadScene(const char* pFilename);
		static bool SaveScene(const char* pFilename);
		static void ProcessFBXNode(FbxNode* pNode, int ParentIndex);
		static void ExportNodes();
		static int ExtractFBXMeshData(FbxMesh* pMesh);
		static void CreateFBXMeshList(std::vector<FbxMesh*>& MeshList);
		static void CreateFBXNodes(std::vector<FbxMesh*>& MeshList);
	};
}
