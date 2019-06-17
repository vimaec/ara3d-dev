#include "FbxCliReader.h"
#include <assert.h>

namespace FbxClrWrapper
{
	int FbxCliReader::LoadFBX(String^ FileName)
	{
		auto fileName = (const char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(FileName);
		bool lResult = LoadScene(fileName);
		if (lResult && TransformDataToCLI())
		{
			return 0;
		}

		return -1;
	}

	bool FbxCliReader::LoadScene(const char* pFilename)
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
			mSdkManager->GetIOSettings()->SetBoolProp(IMP_FBX_MATERIAL, true);
			mSdkManager->GetIOSettings()->SetBoolProp(IMP_FBX_TEXTURE, true);
			mSdkManager->GetIOSettings()->SetBoolProp(IMP_FBX_LINK, true);
			mSdkManager->GetIOSettings()->SetBoolProp(IMP_FBX_SHAPE, true);
			mSdkManager->GetIOSettings()->SetBoolProp(IMP_FBX_GOBO, true);
			mSdkManager->GetIOSettings()->SetBoolProp(IMP_FBX_ANIMATION, true);
			mSdkManager->GetIOSettings()->SetBoolProp(IMP_FBX_GLOBAL_SETTINGS, true);
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

			mSdkManager->GetIOSettings()->SetStringProp(IMP_FBX_PASSWORD, lString);
			mSdkManager->GetIOSettings()->SetBoolProp(IMP_FBX_PASSWORD_ENABLE, true);

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

		auto axisSytem = mScene->GetGlobalSettings().GetAxisSystem();

		FbxAxisSystem newAxisSystem(FbxAxisSystem::EUpVector::eYAxis,
			FbxAxisSystem::EFrontVector::eParityOdd,
			FbxAxisSystem::ECoordSystem::eRightHanded);

		newAxisSystem.ConvertScene(mScene);

		FbxNode* lRootNode = mScene->GetRootNode();
		if (lRootNode)
		{
			ProcessFBXNode(lRootNode, -1);
		}

		return lStatus;
	}

	void FbxCliReader::ProcessFBXNode(FbxNode* pNode, int ParentIndex)
	{
		int nodeIndex = (int)mSceneData->mNodeNameList.size();
		mSceneData->mNodeNameList.push_back(pNode->GetName());
		mSceneData->mNodeParentList.push_back(ParentIndex);

		// Get node's default TRS properties as a transformation matrix
		FbxAMatrix& myNodeDefaultGlobalTransform = pNode->EvaluateGlobalTransform();
		mSceneData->mNodeTransformList.push_back(myNodeDefaultGlobalTransform);

		FbxDouble3 pTranslation	= pNode->LclTranslation;
		FbxDouble3 pRotation	= pNode->LclRotation;
		FbxDouble3 pScaling		= pNode->LclScaling;

		int meshIndex = -1;
		if (pNode->GetNodeAttribute())
		{
			FbxNodeAttribute::EType AttributeType = pNode->GetNodeAttribute()->GetAttributeType();
			if (AttributeType == FbxNodeAttribute::eMesh)
			{
				auto pMesh = (FbxMesh*)pNode->GetNodeAttribute();

				if (mSceneData->mMeshMap.find(pMesh) != mSceneData->mMeshMap.end())
				{
					meshIndex = mSceneData->mMeshMap[pMesh];
				}
				else
				{
					meshIndex = ExtractFBXMeshData(pMesh);
				}
			}
		}

		mSceneData->mNodeMeshIndexList.push_back(meshIndex);

		// Recursively process the children.
		for (int j = 0; j < pNode->GetChildCount(); j++)
		{
			ProcessFBXNode(pNode->GetChild(j), nodeIndex);
		}
	}

	int FbxCliReader::ExtractFBXMeshData(FbxMesh* pMesh)
	{
		//FbxGeometryConverter lGeomConverter(mSdkManager);
		//pMesh = (FbxMesh*)lGeomConverter.Triangulate(pMesh, false);

		FbxVector4* pVertices = pMesh->GetControlPoints();
		int iNumVertices = pMesh->GetControlPointsCount();

		FBXMeshDataInternal* internalMesh = new FBXMeshDataInternal();

		int maxIndex = 0;

		for (int p = 0; p < pMesh->mPolygonVertices.Size(); p++)
		{
			int index = pMesh->mPolygonVertices[p];
			internalMesh->mIndices.push_back(index);
			maxIndex = maxIndex > index ? maxIndex : index;
		}

		for (int p = 0; p < pMesh->mPolygons.Size(); p++)
		{
			internalMesh->mFaceSize.push_back(pMesh->mPolygons[p].mSize);
		}

		assert(pMesh->mPolygonVertices.Size() == internalMesh->mIndices.size());
		assert(maxIndex < iNumVertices);

		for (int k = 0; k < iNumVertices; k++)
		{
			float x = pVertices[k].mData[0];
			float y = pVertices[k].mData[1];
			float z = pVertices[k].mData[2];
			internalMesh->mVertices.push_back(x);
			internalMesh->mVertices.push_back(y);
			internalMesh->mVertices.push_back(z);
		}

	/*	GetElementInfo(pMesh->GetElementSmoothing(), internalMesh->mSmoothingGroupAttribute);
		GetElementInfo(pMesh->GetElementNormal(), internalMesh->mNormalsAttribute);
		GetElementInfo(pMesh->GetElementBinormal(), internalMesh->mBinormalsAttribute);
		GetElementInfo(pMesh->GetElementTangent(), internalMesh->mTangentsAttribute);*/

		int meshIndex = (int)mSceneData->mMeshList.size();
		mSceneData->mMeshList.push_back(internalMesh);
		mSceneData->mMeshIdList.push_back(pMesh->GetName());

		mSceneData->mMeshMap[pMesh] = meshIndex;

		return meshIndex;
	}
};