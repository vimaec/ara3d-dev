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
		FbxManager* mSdkManager = nullptr;
		FbxScene* mScene = nullptr;
		FBXSceneDataInternal *mSceneData = nullptr;
		FBXSceneData ^mSceneData_ = nullptr;

	public:
		~FBXLoader()
		{
			ShutDownAPI();
			DestroyData();
		}

	public:
		void Initialize();
		void ShutDownAPI();
		void DestroyData();
		int LoadFBX(String^ FileName);
		int SaveFBX(String^ FileName);
		FBXSceneData ^ GetSceneData() { return mSceneData_; }
		void SetSceneData(FBXSceneData^ SceneData) { mSceneData_ = SceneData; }

	private:
		bool TransformDataToCLI();
		bool TransformDataFromCLI();

		void InitializeSdkObjects();
		bool LoadScene(const char* pFilename);
		bool SaveScene(const char* pFilename);
		void ProcessFBXNode(FbxNode* pNode, int ParentIndex);
		void ExportNodes();
		int ExtractFBXMeshData(FbxMesh* pMesh);
		void CreateFBXMeshList(std::vector<FbxMesh*>& MeshList);
		void CreateFBXNodes(std::vector<FbxMesh*>& MeshList);
	};
}
