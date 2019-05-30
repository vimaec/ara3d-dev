#include "FbxCliWrapper.h"

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
	}

	int FBXLoader::LoadFBX(String^ FileName)
	{
		const char* fileName = (const char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(FileName);
		bool lResult = LoadScene(fileName);
		if (lResult)
		{
			TransformData();
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

	bool FBXLoader::LoadScene(const char* pFilename)
	{
		int lFileMajor, lFileMinor, lFileRevision;
		int lSDKMajor, lSDKMinor, lSDKRevision;
		//int lFileFormat = -1;
		int i, lAnimStackCount;
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

			// From this point, it is possible to access animation stack information without
			// the expense of loading the entire file.

			FBXSDK_printf("Animation Stack Information\n");

			lAnimStackCount = lImporter->GetAnimStackCount();

			FBXSDK_printf("    Number of Animation Stacks: %d\n", lAnimStackCount);
			FBXSDK_printf("    Current Animation Stack: \"%s\"\n", lImporter->GetActiveAnimStackName().Buffer());
			FBXSDK_printf("\n");

			for (i = 0; i < lAnimStackCount; i++)
			{
				FbxTakeInfo* lTakeInfo = lImporter->GetTakeInfo(i);

				FBXSDK_printf("    Animation Stack %d\n", i);
				FBXSDK_printf("         Name: \"%s\"\n", lTakeInfo->mName.Buffer());
				FBXSDK_printf("         Description: \"%s\"\n", lTakeInfo->mDescription.Buffer());

				// Change the value of the import name if the animation stack should be imported 
				// under a different name.
				FBXSDK_printf("         Import Name: \"%s\"\n", lTakeInfo->mImportName.Buffer());

				// Set the value of the import state to false if the animation stack should be not
				// be imported. 
				FBXSDK_printf("         Import State: %s\n", lTakeInfo->mSelect ? "true" : "false");
				FBXSDK_printf("\n");
			}

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

		// Print the nodes of the scene and their attributes recursively.
		// Note that we are not printing the root node because it should
		// not contain any attributes.
		FbxNode* lRootNode = mScene->GetRootNode();
		if (lRootNode)
		{
			PrintNode(lRootNode, -1);
			//			for (int i = 0; i < lRootNode->GetChildCount(); i++)
			//			{
			//				PrintNode(lRootNode->GetChild(i));
			//			}
		}

	
		FBXSDK_printf("\nPress any key to continue.\n");

		FBXSDK_CRT_SECURE_NO_WARNING_BEGIN
			getchar();
		FBXSDK_CRT_SECURE_NO_WARNING_END

		return lStatus;
	}


	int numTabs = 0;
	// Print the required number of tabs.
	void FBXLoader::PrintTabs()
	{
		for (int i = 0; i < numTabs; i++)
		{
			printf("\t");
		}
	}

	// Print a node, its attributes, and all its children recursively.
	void FBXLoader::PrintNode(FbxNode* pNode, int ParentIndex)
	{
		int nodeIndex = mSceneData->mNodeNameList.size();
		mSceneData->mNodeNameList.push_back(pNode->GetName());
		mSceneData->mNodeParentList.push_back(ParentIndex);

		mSceneData->mNodeTranslationList.push_back(pNode->LclTranslation.Get());
		mSceneData->mNodeRotationList.push_back(pNode->LclRotation.Get());
		mSceneData->mNodeScaleList.push_back(pNode->LclScaling.Get());

		FbxAnimEvaluator* pSceneEvaluator = mScene->GetAnimationEvaluator();

		// Get node’s default TRS properties as a transformation matrix
		FbxAMatrix& myNodeDefaultGlobalTransform = pSceneEvaluator->GetNodeGlobalTransform(pNode);
		mSceneData->mNodeTransformList.push_back(myNodeDefaultGlobalTransform);

		int meshIndex = -1;
		if (pNode->GetNodeAttribute())
		{
			FbxNodeAttribute::EType AttributeType = pNode->GetNodeAttribute()->GetAttributeType();
			if (AttributeType == FbxNodeAttribute::eMesh)
			{
				FbxMesh* pMesh = (FbxMesh*)pNode->GetNodeAttribute();
				FbxVector4* pVertices = pMesh->GetControlPoints();
				int iNumVertices = pMesh->GetControlPointsCount();

				PrintTabs();
				printf("Mesh Node, PolyCount: %d\n", pMesh->GetPolygonCount());

				FBXMeshDataInternal mesh;

				for (int j = 0; j < pMesh->GetPolygonCount(); j++)
				{
					int iNumIndices = pMesh->GetPolygonSize(j);
					mesh.mFaceSize.push_back(iNumIndices);

					for (int k = 0; k < iNumIndices; k++)
					{
						int iControlPointIndex = pMesh->GetPolygonVertex(j, k);
						mesh.mIndices.push_back(iControlPointIndex);
					}
				}

				for (int k = 0; k < iNumVertices; k++)
				{
					float x = (float)pVertices[k].mData[0];
					float y = (float)pVertices[k].mData[1];
					float z = (float)pVertices[k].mData[2];
					mesh.mVertices.push_back(x);
					mesh.mVertices.push_back(y);
					mesh.mVertices.push_back(z);
				}

				meshIndex = mSceneData->mMeshList.size();
				mSceneData->mMeshList.push_back(mesh);
				mSceneData->mMeshIdList.push_back(pNode->GetName());
			}
		}

		mSceneData->mNodeMeshIndexList.push_back(meshIndex);


		//////////////////////////////////////////////////////////////////////////

		PrintTabs();
		const char* nodeName = pNode->GetName();
		FbxDouble3 translation = pNode->LclTranslation.Get();
		FbxDouble3 rotation = pNode->LclRotation.Get();
		FbxDouble3 scaling = pNode->LclScaling.Get();

		// Print the contents of the node.
		printf("<node name='%s' translation='(%f, %f, %f)' rotation='(%f, %f, %f)' scaling='(%f, %f, %f)'>\n",
			nodeName,
			translation[0], translation[1], translation[2],
			rotation[0], rotation[1], rotation[2],
			scaling[0], scaling[1], scaling[2]
		);
		numTabs++;

		// Print the node's attributes.
		for (int i = 0; i < pNode->GetNodeAttributeCount(); i++)
			PrintAttribute(pNode->GetNodeAttributeByIndex(i));

		// Recursively print the children.
		for (int j = 0; j < pNode->GetChildCount(); j++)
			PrintNode(pNode->GetChild(j), nodeIndex);

		numTabs--;
		PrintTabs();
		printf("</node>\n");
	}
	/**
 * Print an attribute.
 */
	void FBXLoader::PrintAttribute(FbxNodeAttribute* pAttribute) {
		if (!pAttribute) return;

		FbxString typeName = GetAttributeTypeName(pAttribute->GetAttributeType());
		FbxString attrName = pAttribute->GetName();
		PrintTabs();
		// Note: to retrieve the character array of a FbxString, use its Buffer() method.
		printf("<attribute type='%s' name='%s'/>\n", typeName.Buffer(), attrName.Buffer());
	}

	/**
 * Return a string-based representation based on the attribute type.
 */
	FbxString FBXLoader::GetAttributeTypeName(FbxNodeAttribute::EType type) {
		switch (type) {
		case FbxNodeAttribute::eUnknown: return "unidentified";
		case FbxNodeAttribute::eNull: return "null";
		case FbxNodeAttribute::eMarker: return "marker";
		case FbxNodeAttribute::eSkeleton: return "skeleton";
		case FbxNodeAttribute::eMesh: return "mesh";
		case FbxNodeAttribute::eNurbs: return "nurbs";
		case FbxNodeAttribute::ePatch: return "patch";
		case FbxNodeAttribute::eCamera: return "camera";
		case FbxNodeAttribute::eCameraStereo: return "stereo";
		case FbxNodeAttribute::eCameraSwitcher: return "camera switcher";
		case FbxNodeAttribute::eLight: return "light";
		case FbxNodeAttribute::eOpticalReference: return "optical reference";
		case FbxNodeAttribute::eOpticalMarker: return "marker";
		case FbxNodeAttribute::eNurbsCurve: return "nurbs curve";
		case FbxNodeAttribute::eTrimNurbsSurface: return "trim nurbs surface";
		case FbxNodeAttribute::eBoundary: return "boundary";
		case FbxNodeAttribute::eNurbsSurface: return "nurbs surface";
		case FbxNodeAttribute::eShape: return "shape";
		case FbxNodeAttribute::eLODGroup: return "lodgroup";
		case FbxNodeAttribute::eSubDiv: return "subdiv";
		default: return "unknown";
		}
	}
};