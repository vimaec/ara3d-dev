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
	};
}
