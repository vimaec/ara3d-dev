#include "FbxCliBase.h"
#include <assert.h>

#ifdef _WIN64
	#if _DEBUG
		#pragma comment(lib, "fbxsdk/lib/vs2015/x64/debug/libfbxsdk-md.lib")
	#else
		#pragma comment(lib, "fbxsdk/lib/vs2015/x64/release/libfbxsdk-md.lib")
	#endif
#else
	#if _DEBUG
		#pragma comment(lib, "fbxsdk/lib/vs2015/x86/debug/libfbxsdk-md.lib")
	#else
		#pragma comment(lib, "fbxsdk/lib/vs2015/x86/release/libfbxsdk-md.lib")
	#endif
#endif

namespace FbxClrWrapper
{

	void FbxCliBase::Initialize()
	{
		// Prepare the FBX SDK.
		InitializeSdkObjects();

		mSceneData = new FBXSceneDataInternal();
	}

	void FbxCliBase::ShutDownAPI()
	{
		if (mScene != nullptr)
		{
			mScene->Destroy();
		}

		if (mSdkManager != nullptr)
		{
			mSdkManager->Destroy();
		}

		mScene = nullptr;
		mSdkManager = nullptr;
	}

	void FbxCliBase::DestroyData()
	{
		delete mSceneData;
		mSceneData = nullptr;
		mSceneData_ = nullptr;
	}

	void FbxCliBase::InitializeSdkObjects()
	{
		//The first thing to do is to create the FBX Manager which is the object allocator for almost all the classes in the SDK
		mSdkManager = FbxManager::Create();
		if (!mSdkManager)
		{
			FBXSDK_printf("Error: Unable to create FBX Manager!\n");
			exit(1);
		}
		else FBXSDK_printf("Autodesk FBX SDK version %s\n", mSdkManager->GetVersion());

		//Create an IOSettings object. This object holds all import/export settings.
		FbxIOSettings* ios = FbxIOSettings::Create(mSdkManager, IOSROOT);
		mSdkManager->SetIOSettings(ios);

		//Load plugins from the executable directory (optional)
		FbxString lPath = FbxGetApplicationDirectory();
		mSdkManager->LoadPluginsDirectory(lPath.Buffer());

		//Create an FBX scene. This object holds most objects imported/exported from/to files.
		mScene = FbxScene::Create(mSdkManager, "My Scene");
		if (!mScene)
		{
			FBXSDK_printf("Error: Unable to create FBX scene!\n");
			exit(1);
		}
	}

	bool FbxCliBase::TransformDataToCLI()
	{
		if (mSceneData != nullptr)
		{
			mSceneData_ = gcnew FBXSceneData(*mSceneData);
			return true;
		}

		return false;
	}
	bool FbxCliBase::TransformDataFromCLI()
	{
		if (mSceneData != nullptr)
		{
			mSceneData = new FBXSceneDataInternal(mSceneData_);
			return true;
		}

		return false;
	}
};