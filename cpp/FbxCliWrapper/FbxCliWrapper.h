#pragma once

#include <cstdint>
#include <vector>
#include <string>

#include "fbxsdk/include/fbxsdk.h"

#include "FBXMeshData.h"
#include "FBXSceneData.h"

namespace FbxClrWrapper
{
	#define _CPP_ true
	#include "../../dotnet/G3D/Constants.cs"

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

	private:
		static void TransformDataToCLI()
		{
			mSceneData_ = gcnew FBXSceneData(*mSceneData);
		}
		static void TransformDataFromCLI()
		{
			mSceneData = new FBXSceneDataInternal(mSceneData_);
		}

		static void InitializeSdkObjects();
		static bool LoadScene(const char* pFilename);
		static bool SaveScene(const char* pFilename);
		static void ProcessNode(FbxNode* pNode, int ParentIndex);
		static void ExportNodes();

	};
}
