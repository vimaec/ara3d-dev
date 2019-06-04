#include "FbxCliWrapper.h"
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

#ifdef IOS_REF
	#undef  IOS_REF
	#define IOS_REF (*(mSdkManager->GetIOSettings()))
#endif

namespace FbxClrWrapper
{

	void FBXLoader::Initialize()
	{
		// Prepare the FBX SDK.
		InitializeSdkObjects();

		mSceneData = new FBXSceneDataInternal();
	}

	void FBXLoader::ShutDown()
	{
		delete mSceneData;
		mScene->Destroy();
		mSdkManager->Destroy();
		mSceneData_ = nullptr;
	}

	int FBXLoader::LoadFBX(String^ FileName)
	{
		auto fileName = (const char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(FileName);
		bool lResult = LoadScene(fileName);
		if (lResult && TransformDataToCLI())
		{
			return 0;
		}

		return -1;
	}

	 int FBXLoader::SaveFBX(String^ FileName)
	 {
		 if (!TransformDataFromCLI())
		 {
			 return -1;
		 }

		 auto fileName = (const char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(FileName);
		 bool lResult = SaveScene(fileName);
		 if (lResult)
		 {
			 return 0;
		 }

		 return -1;
	 }

	void FBXLoader::InitializeSdkObjects()
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

	bool FBXLoader::SaveScene(const char* pFilename)
	{
		// Create an IOSettings object.
		FbxIOSettings* ios = FbxIOSettings::Create(mSdkManager, IOSROOT);
		mSdkManager->SetIOSettings(ios);

		// ... Configure the FbxIOSettings object here ...

		// Create an exporter.
		FbxExporter* lExporter = FbxExporter::Create(mSdkManager, "");

		// Initialize the exporter.
		bool lExportStatus = lExporter->Initialize(pFilename, -1, mSdkManager->GetIOSettings());


		if (!lExportStatus) 
		{
			printf("Call to FbxExporter::Initialize() failed.\n");
			printf("Error returned: %s\n\n", lExporter->GetStatus().GetErrorString());
			return false;
		}

		// Create a new scene so it can be populated by the CLI Data.
		mScene = FbxScene::Create(mSdkManager, "myScene");

		ExportNodes();

		// Export the scene to the file.
		bool result = lExporter->Export(mScene);

		// Destroy the exporter.
		lExporter->Destroy();

		return result;
	}

	bool FBXLoader::LoadScene(const char* pFilename)
	{
		int lFileMajor, lFileMinor, lFileRevision;
		int lSDKMajor, lSDKMinor, lSDKRevision;
		//int lFileFormat = -1;
		bool lStatus;
		char lPassword[1024];

		// Get the file version number generate by the FBX SDK.
		FbxManager::GetFileFormatVersion(lSDKMajor, lSDKMinor, lSDKRevision);

		// Create an importer.
		FbxImporter* lImporter = FbxImporter::Create(mSdkManager, "");

		// Initialize the importer by providing a filename.
		const bool lImportStatus = lImporter->Initialize(pFilename, -1, mSdkManager->GetIOSettings());
		lImporter->GetFileVersion(lFileMajor, lFileMinor, lFileRevision);

		if (!lImportStatus)
		{
			FbxString error = lImporter->GetStatus().GetErrorString();
			FBXSDK_printf("Call to FbxImporter::Initialize() failed.\n");
			FBXSDK_printf("Error returned: %s\n\n", error.Buffer());

			if (lImporter->GetStatus().GetCode() == FbxStatus::eInvalidFileVersion)
			{
				FBXSDK_printf("FBX file format version for this FBX SDK is %d.%d.%d\n", lSDKMajor, lSDKMinor, lSDKRevision);
				FBXSDK_printf("FBX file format version for file '%s' is %d.%d.%d\n\n", pFilename, lFileMajor, lFileMinor, lFileRevision);
			}

			return false;
		}

		FBXSDK_printf("FBX file format version for this FBX SDK is %d.%d.%d\n", lSDKMajor, lSDKMinor, lSDKRevision);

		if (lImporter->IsFBX())
		{
			FBXSDK_printf("FBX file format version for file '%s' is %d.%d.%d\n\n", pFilename, lFileMajor, lFileMinor, lFileRevision);

			// Set the import states. By default, the import states are always set to 
			// true. The code below shows how to change these states.
			IOS_REF.SetBoolProp(IMP_FBX_MATERIAL, true);
			IOS_REF.SetBoolProp(IMP_FBX_TEXTURE, true);
			IOS_REF.SetBoolProp(IMP_FBX_LINK, true);
			IOS_REF.SetBoolProp(IMP_FBX_SHAPE, true);
			IOS_REF.SetBoolProp(IMP_FBX_GOBO, true);
			IOS_REF.SetBoolProp(IMP_FBX_ANIMATION, true);
			IOS_REF.SetBoolProp(IMP_FBX_GLOBAL_SETTINGS, true);
		}

		// Import the scene.
		lStatus = lImporter->Import(mScene);

		if (lStatus == false && lImporter->GetStatus().GetCode() == FbxStatus::ePasswordError)
		{
			FBXSDK_printf("Please enter password: ");

			lPassword[0] = '\0';

			FBXSDK_CRT_SECURE_NO_WARNING_BEGIN
				scanf("%s", lPassword);
			FBXSDK_CRT_SECURE_NO_WARNING_END

				FbxString lString(lPassword);

			IOS_REF.SetStringProp(IMP_FBX_PASSWORD, lString);
			IOS_REF.SetBoolProp(IMP_FBX_PASSWORD_ENABLE, true);

			lStatus = lImporter->Import(mScene);

			if (lStatus == false && lImporter->GetStatus().GetCode() == FbxStatus::ePasswordError)
			{
				FBXSDK_printf("\nPassword is wrong, import aborted.\n");
			}
		}


		// Convert mesh, NURBS and patch into triangle mesh
	//	FbxGeometryConverter lGeomConverter(mSdkManager);
	//	lGeomConverter.Triangulate(mScene, /*replace*/true);

		// Destroy the importer.
		lImporter->Destroy();

		FbxNode* lRootNode = mScene->GetRootNode();
		if (lRootNode)
		{
			ProcessNode(lRootNode, -1);
		}

		return lStatus;
	}

	void FBXLoader::ExportNodes()
	{
		// Create all the fbx nodes required
		FbxNode* lRootNode = mScene->GetRootNode();
		std::vector<FbxNode*> nodeList;
		for (int nodeIndex = 0; nodeIndex < (int)mSceneData->mNodeNameList.size(); nodeIndex++)
		{
			auto nodeName = mSceneData->mNodeNameList[nodeIndex];

			FbxNode* newNode = FbxNode::Create(mScene, nodeName.c_str());

			newNode->LclTranslation = mSceneData->mNodeTranslationList[nodeIndex];
			newNode->LclRotation = mSceneData->mNodeRotationList[nodeIndex];
			newNode->LclScaling = mSceneData->mNodeScaleList[nodeIndex];

			nodeList.push_back(newNode);
		}

		// use the parent index list to insert children into correct nodes
		for (int nodeIndex = 0; nodeIndex < mSceneData->mNodeParentList.size(); nodeIndex++)
		{
			int32_t nodeParent = mSceneData->mNodeParentList[nodeIndex];

			if (nodeParent == -1)
			{
				lRootNode->AddChild(nodeList[nodeIndex]);
			}
			else
			{
				nodeList[nodeParent]->AddChild(nodeList[nodeIndex]);
			}
		}

		// use the geometry index list to add all of the geometry attributes
		for (int nodeIndex = 0; nodeIndex < mSceneData->mNodeParentList.size(); nodeIndex++)
		{
			uint32_t meshIndex = mSceneData->mNodeMeshIndexList[nodeIndex];

			if (meshIndex == -1)
			{
				continue;
			}

			auto mesh = mSceneData->mMeshList[meshIndex];

			// Create a mesh.
			FbxMesh* lMesh = FbxMesh::Create(mScene, mSceneData->mMeshIdList[meshIndex].c_str());

			// Set the node attribute of the mesh node.
			nodeList[nodeIndex]->SetNodeAttribute(lMesh);

			// Initialize the control point array of the mesh.
			lMesh->InitControlPoints((int)mesh.mVertices.size());
			FbxVector4* lControlPoints = lMesh->GetControlPoints();

			for (int vertexIndex = 0; vertexIndex < mesh.mVertices.size() / 3; vertexIndex++)
			{
				lControlPoints[vertexIndex].mData[0] = mesh.mVertices[vertexIndex * 3 + 0];
				lControlPoints[vertexIndex].mData[1] = mesh.mVertices[vertexIndex * 3 + 1];
				lControlPoints[vertexIndex].mData[2] = mesh.mVertices[vertexIndex * 3 + 2];
			}

			lMesh->ReservePolygonVertexCount((int)mesh.mIndices.size());
			for (int p = 0; p < mesh.mIndices.size(); p++)
			{
				int index = mesh.mIndices[p];
				lMesh->mPolygonVertices[p] = index;
			}

			int globalVertexIndex = 0;
			for (int polygonIndex = 0; polygonIndex < mesh.mFaceSize.size(); polygonIndex++)
			{
				lMesh->BeginPolygon();
				int faceSize = mesh.mFaceSize[polygonIndex];
				for (int vertexIndex = 0; vertexIndex < faceSize; vertexIndex++)
				{
					lMesh->AddPolygon(mesh.mIndices[globalVertexIndex + vertexIndex]);
				}
				lMesh->EndPolygon();

				globalVertexIndex += faceSize;
			}
		}
	}

	void FBXLoader::ProcessNode(FbxNode* pNode, int ParentIndex)
	{
		int nodeIndex = (int)mSceneData->mNodeNameList.size();
		mSceneData->mNodeNameList.push_back(pNode->GetName());
		mSceneData->mNodeParentList.push_back(ParentIndex);

		mSceneData->mNodeTranslationList.push_back(pNode->LclTranslation.Get());
		mSceneData->mNodeRotationList.push_back(pNode->LclRotation.Get());
		mSceneData->mNodeScaleList.push_back(pNode->LclScaling.Get());

		FbxAnimEvaluator* pSceneEvaluator = mScene->GetAnimationEvaluator();

		// Get node's default TRS properties as a transformation matrix
		FbxAMatrix& myNodeDefaultGlobalTransform = pSceneEvaluator->GetNodeGlobalTransform(pNode);
		mSceneData->mNodeTransformList.push_back(myNodeDefaultGlobalTransform);

		int meshIndex = -1;
		if (pNode->GetNodeAttribute())
		{
			FbxNodeAttribute::EType AttributeType = pNode->GetNodeAttribute()->GetAttributeType();
			if (AttributeType == FbxNodeAttribute::eMesh)
			{
				FbxMesh* pMesh = (FbxMesh*)pNode->GetNodeAttribute();

				if (mSceneData->mMeshMap.find(pMesh) != mSceneData->mMeshMap.end())
				{
					meshIndex = mSceneData->mMeshMap[pMesh];
				}
				else
				{
			//		FbxGeometryConverter lGeomConverter(mSdkManager);
			//		lGeomConverter.Triangulate(pMesh, /*replace*/true);

					FbxVector4* pVertices = pMesh->GetControlPoints();
					int iNumVertices = pMesh->GetControlPointsCount();

					FBXMeshDataInternal mesh;

					int maxIndex = 0;

					for (int p = 0; p < pMesh->mPolygonVertices.Size(); p++)
					{
						int index = pMesh->mPolygonVertices[p];
						mesh.mIndices.push_back(index);
						maxIndex = maxIndex > index ? maxIndex : index;
					}

					for (int p = 0; p < pMesh->mPolygons.Size(); p++)
					{
						mesh.mFaceSize.push_back(pMesh->mPolygons[p].mSize);
					}

					assert(pMesh->mPolygonVertices.Size() == mesh.mIndices.size());
					assert(maxIndex < iNumVertices);

					for (int k = 0; k < iNumVertices; k++)
					{
						float x = (float)pVertices[k].mData[0];
						float y = (float)pVertices[k].mData[1];
						float z = (float)pVertices[k].mData[2];
						mesh.mVertices.push_back(x);
						mesh.mVertices.push_back(y);
						mesh.mVertices.push_back(z);
					}

					meshIndex = (int)mSceneData->mMeshList.size();
					mSceneData->mMeshList.push_back(mesh);
					mSceneData->mMeshIdList.push_back(pMesh->GetName());
				}
			}
		}

		mSceneData->mNodeMeshIndexList.push_back(meshIndex);

/*		auto nodeName = pNode->GetName();
		FbxDouble3 translation = pNode->LclTranslation.Get();
		FbxDouble3 rotation = pNode->LclRotation.Get();
		FbxDouble3 scaling = pNode->LclScaling.Get();*/
	
		// Recursively process the children.
		for (int j = 0; j < pNode->GetChildCount(); j++)
		{
			ProcessNode(pNode->GetChild(j), nodeIndex);
		}
	}

};