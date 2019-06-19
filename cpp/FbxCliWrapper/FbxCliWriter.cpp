#include "FbxCliWriter.h"
#include <assert.h>


namespace FbxClrWrapper
{
	int FbxCliWriter::SaveFBX(String^ FileName)
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

	bool FbxCliWriter::SaveScene(const char* pFilename)
	{
		// Create an IOSettings object.
		FbxIOSettings* ios = FbxIOSettings::Create(mSdkManager, IOSROOT);
		mSdkManager->SetIOSettings(ios);

		// ... Configure the FbxIOSettings object here ...

		ios->SetBoolProp(IMP_FBX_GLOBAL_SETTINGS, true);

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

		FbxAxisSystem newAxisSystem(FbxAxisSystem::EUpVector::eYAxis,
			FbxAxisSystem::EFrontVector::eParityOdd,
			FbxAxisSystem::ECoordSystem::eRightHanded);

		mScene->GetGlobalSettings().SetAxisSystem(newAxisSystem);

		ExportNodes();

		// Export the scene to the file.
		bool result = lExporter->Export(mScene);

		if (!result)
		{
			auto status = lExporter->GetStatus();
			printf("Error: %s", status.GetErrorString());
		}

		// Destroy the exporter.
		lExporter->Destroy();

		return result;
	}

	void FbxCliWriter::ExportNodes()
	{
		// Create all the FBX meshes and nodes
		std::vector<FbxMesh*> meshList;
		CreateFBXMeshList(meshList);
		CreateFBXNodes(meshList);
	}

	void FbxCliWriter::CreateFBXNodes(std::vector<FbxMesh*>& MeshList)
	{
		// Create all the fbx nodes required
		// Skip the IScene's root node
		FbxNode* lRootNode = mScene->GetRootNode();
		std::vector<FbxNode*> nodeList;
		for (int nodeIndex = 1; nodeIndex < (int)mSceneData->mNodeNameList.size(); nodeIndex++)
		{
			auto nodeName = mSceneData->mNodeNameList[nodeIndex];

			FbxNode* newNode = FbxNode::Create(mScene, nodeName.c_str());
			nodeList.push_back(newNode);
		}

		// use the parent index list to insert children into correct nodes
		for (int nodeIndex = 0; nodeIndex < mSceneData->mNodeParentList.size(); nodeIndex++)
		{
			int32_t meshIndex = mSceneData->mNodeMeshIndexList[nodeIndex];
			int32_t nodeParentIndex = mSceneData->mNodeParentList[nodeIndex];
			FbxMatrix nodeLocalTransform;
			auto node = nodeIndex == 0 ? lRootNode : nodeList[nodeIndex - 1];

			if (nodeParentIndex == -1)
			{
				nodeLocalTransform = *reinterpret_cast<FbxMatrix*>(&mSceneData->mNodeTransformList[nodeIndex]);
			}
			else if (nodeParentIndex == 0)
			{
				auto nodeGlobalTransform = *reinterpret_cast<FbxMatrix*>(&mSceneData->mNodeTransformList[nodeIndex]);
				auto parentGlobalTransform = *reinterpret_cast<FbxMatrix*>(&mSceneData->mNodeTransformList[nodeParentIndex]);
				nodeLocalTransform = parentGlobalTransform.Inverse() * nodeGlobalTransform;
				//nodeLocalTransform = *reinterpret_cast<FbxMatrix*>(&mSceneData->mNodeTransformList[nodeIndex]);
				bool res = lRootNode->AddChild(node);
			}
			else
			{
				auto nodeGlobalTransform = *reinterpret_cast<FbxMatrix*>(&mSceneData->mNodeTransformList[nodeIndex]);
				auto parentGlobalTransform = *reinterpret_cast<FbxMatrix*>(&mSceneData->mNodeTransformList[nodeParentIndex]);
				nodeLocalTransform = parentGlobalTransform.Inverse() * nodeGlobalTransform;

				bool res = nodeList[nodeParentIndex - 1]->AddChild(node);
			}

			if (meshIndex != -1)
			{
				// Set the node attribute of the mesh node.
				node->SetNodeAttribute(MeshList[meshIndex]);
			}

			FbxVector4 pTranslation;
			FbxVector4 pRotation;
			FbxVector4 pShearing;
			FbxVector4 pScaling;
			double pSign;
			nodeLocalTransform.GetElements(pTranslation, pRotation, pShearing, pScaling, pSign);

			node->LclTranslation = pTranslation;
			node->LclRotation = pRotation;
			node->LclScaling = pScaling;
		}
	}

	void FbxCliWriter::CreateFBXMeshList(std::vector<FbxMesh*> & MeshList)
	{
		// Create all the FBX meshes
		for (int meshIndex = 0; meshIndex < (int)mSceneData->mMeshList.size(); meshIndex++)
		{
			// Create a mesh.
			FbxMesh* lMesh = FbxMesh::Create(mScene, mSceneData->mMeshIdList[meshIndex].c_str());
			auto internalMesh = mSceneData->mMeshList[meshIndex];

			// Initialize the control point array of the mesh.
			lMesh->InitControlPoints((int)internalMesh->mVertices.size());
			FbxVector4* lControlPoints = lMesh->GetControlPoints();

			for (int vertexIndex = 0; vertexIndex < internalMesh->mVertices.size() / 3; vertexIndex++)
			{
				lControlPoints[vertexIndex].mData[0] = internalMesh->mVertices[vertexIndex * 3 + 0];
				lControlPoints[vertexIndex].mData[1] = internalMesh->mVertices[vertexIndex * 3 + 1];
				lControlPoints[vertexIndex].mData[2] = internalMesh->mVertices[vertexIndex * 3 + 2];
			}

			lMesh->ReservePolygonVertexCount((int)internalMesh->mIndices.size());
			for (int p = 0; p < internalMesh->mIndices.size(); p++)
			{
				int index = internalMesh->mIndices[p];
				lMesh->mPolygonVertices[p] = index;
			}

			int globalVertexIndex = 0;
			for (int polygonIndex = 0; polygonIndex < internalMesh->mFaceSize.size(); polygonIndex++)
			{
				lMesh->BeginPolygon();
				int faceSize = internalMesh->mFaceSize[polygonIndex];
				for (int vertexIndex = 0; vertexIndex < faceSize; vertexIndex++)
				{
					lMesh->AddPolygon(internalMesh->mIndices[globalVertexIndex + vertexIndex]);
				}
				lMesh->EndPolygon();

				globalVertexIndex += faceSize;
			}

			MeshList.push_back(lMesh);
		}
	}
};