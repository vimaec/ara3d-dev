#pragma once

#include <cstdint>
#include <vector>
#include <string>

#include "fbxsdk/include/fbxsdk.h"

namespace FbxClrWrapper
{
	#define _CPP_ true
	#include "../../dotnet/G3D/Constants.cs"


	using namespace System;
	class FBXMeshDataInternal
	{
	public:
		std::vector<int32_t> mIndices;
		std::vector<float> mVertices;
		std::vector<int> mFaceSize;
	};	
	
	public ref class FBXMeshData
	{
	public:
		array<int32_t>^ mIndices;
		array<float>^ mVertices;
		array<int>^ mFaceSize;

	public:
		FBXMeshData(FBXMeshDataInternal & Src)
		{
			mIndices = gcnew array<int32_t>(Src.mIndices.size());
			mVertices = gcnew array<float>(Src.mVertices.size());
			mFaceSize = gcnew array<int>(Src.mFaceSize.size());
			for (size_t i = 0; i < Src.mIndices.size(); i++)
			{
				mIndices[i] = Src.mIndices[i];
			}
			for (size_t i = 0; i < Src.mVertices.size(); i++)
			{
				mVertices[i] = Src.mVertices[i];
			}
			for (size_t i = 0; i < Src.mFaceSize.size(); i++)
			{
				mFaceSize[i] = Src.mFaceSize[i];
			}
		}
	};

	class FBXSceneDataInternal
	{
	public:
		std::vector<std::string> mNodeNameList;
		std::vector<int32_t> mNodeParentList;
		std::vector<FbxDouble3> mNodeTranslationList;
		std::vector<FbxDouble3> mNodeRotationList;
		std::vector<FbxDouble3> mNodeScaleList;
		std::vector<FbxDouble4x4> mNodeTransformList;
		std::vector<int> mNodeMeshIndexList;

		std::vector<FBXMeshDataInternal> mMeshList;
		std::vector<std::string> mMeshIdList;
	};

	public ref class FBXSceneData
	{
	public:
		array<String^>^ mNodeNameList;
		array<int32_t>^ mNodeParentList;
		array<float>^ mNodeTranslationList;
		array<float>^ mNodeRotationList;
		array<float>^ mNodeScaleList;
		array<float>^ mNodeTransformList;
		array<int>^ mNodeMeshIndexList;

		array<FBXMeshData^>^ mMeshList;
		array<String^>^ mMeshIdList;

	public:
		FBXSceneData(FBXSceneDataInternal &SrcData)
		{
			mNodeNameList = gcnew array<String ^>(SrcData.mNodeNameList.size());
			mNodeParentList = gcnew array<int32_t>(SrcData.mNodeParentList.size());
			mNodeTranslationList = gcnew array<float>(SrcData.mNodeTranslationList.size() * 3);
			mNodeRotationList = gcnew array<float>(SrcData.mNodeRotationList.size() * 3);
			mNodeScaleList = gcnew array<float>(SrcData.mNodeScaleList.size() * 3);
			mNodeTransformList = gcnew array<float>(SrcData.mNodeTransformList.size() * 4*4);
			mNodeMeshIndexList = gcnew array<int>(SrcData.mNodeMeshIndexList.size());
			mMeshList = gcnew array<FBXMeshData ^>(SrcData.mMeshList.size());
			mMeshIdList = gcnew array<String ^>(SrcData.mMeshIdList.size());

			for (size_t i = 0; i < SrcData.mNodeNameList.size(); i++)
			{
				mNodeNameList[i] = gcnew String(SrcData.mNodeNameList[i].c_str());
			}
			for (size_t i = 0; i < SrcData.mNodeParentList.size(); i++)
			{
				mNodeParentList[i] = SrcData.mNodeParentList[i];
			}
			for (size_t i = 0; i < SrcData.mNodeTranslationList.size(); i++)
			{
				mNodeTranslationList[i * 3 + 0] = (float)SrcData.mNodeTranslationList[i].mData[0];
				mNodeTranslationList[i * 3 + 1] = (float)SrcData.mNodeTranslationList[i].mData[1];
				mNodeTranslationList[i * 3 + 2] = (float)SrcData.mNodeTranslationList[i].mData[2];
			}
			for (size_t i = 0; i < SrcData.mNodeRotationList.size(); i++)
			{
				mNodeRotationList[i * 3 + 0] = (float)SrcData.mNodeRotationList[i].mData[0];
				mNodeRotationList[i * 3 + 1] = (float)SrcData.mNodeRotationList[i].mData[1];
				mNodeRotationList[i * 3 + 2] = (float)SrcData.mNodeRotationList[i].mData[2];
			}
			for (size_t i = 0; i < SrcData.mNodeScaleList.size(); i++)
			{
				mNodeScaleList[i * 3 + 0] = (float)SrcData.mNodeScaleList[i].mData[0];
				mNodeScaleList[i * 3 + 1] = (float)SrcData.mNodeScaleList[i].mData[1];
				mNodeScaleList[i * 3 + 2] = (float)SrcData.mNodeScaleList[i].mData[2];
			}
			for (size_t i = 0; i < SrcData.mNodeTransformList.size(); i++)
			{
				mNodeTransformList[i * 16 + 0 + 0] = (float)SrcData.mNodeTransformList[i].mData[0].mData[0];
				mNodeTransformList[i * 16 + 1 + 0] = (float)SrcData.mNodeTransformList[i].mData[0].mData[1];
				mNodeTransformList[i * 16 + 2 + 0] = (float)SrcData.mNodeTransformList[i].mData[0].mData[2];
				mNodeTransformList[i * 16 + 3 + 0] = (float)SrcData.mNodeTransformList[i].mData[0].mData[3];
				mNodeTransformList[i * 16 + 0 + 4] = (float)SrcData.mNodeTransformList[i].mData[1].mData[0];
				mNodeTransformList[i * 16 + 1 + 4] = (float)SrcData.mNodeTransformList[i].mData[1].mData[1];
				mNodeTransformList[i * 16 + 2 + 4] = (float)SrcData.mNodeTransformList[i].mData[1].mData[2];
				mNodeTransformList[i * 16 + 3 + 4] = (float)SrcData.mNodeTransformList[i].mData[1].mData[3];
				mNodeTransformList[i * 16 + 0 + 8] = (float)SrcData.mNodeTransformList[i].mData[2].mData[0];
				mNodeTransformList[i * 16 + 1 + 8] = (float)SrcData.mNodeTransformList[i].mData[2].mData[1];
				mNodeTransformList[i * 16 + 2 + 8] = (float)SrcData.mNodeTransformList[i].mData[2].mData[2];
				mNodeTransformList[i * 16 + 3 + 8] = (float)SrcData.mNodeTransformList[i].mData[2].mData[3];
				mNodeTransformList[i * 16 + 0 + 12] = (float)SrcData.mNodeTransformList[i].mData[3].mData[0];
				mNodeTransformList[i * 16 + 1 + 12] = (float)SrcData.mNodeTransformList[i].mData[3].mData[1];
				mNodeTransformList[i * 16 + 2 + 12] = (float)SrcData.mNodeTransformList[i].mData[3].mData[2];
				mNodeTransformList[i * 16 + 3 + 12] = (float)SrcData.mNodeTransformList[i].mData[3].mData[3];
			}
			for (size_t i = 0; i < SrcData.mNodeMeshIndexList.size(); i++)
			{
				mNodeMeshIndexList[i] = SrcData.mNodeMeshIndexList[i];
			}
			for (size_t i = 0; i < SrcData.mMeshList.size(); i++)
			{
				mMeshList[i] = gcnew FBXMeshData(SrcData.mMeshList[i]);
			}
			for (size_t i = 0; i < SrcData.mMeshIdList.size(); i++)
			{
				mMeshIdList[i] = gcnew String(SrcData.mMeshIdList[i].c_str());
			}
		}
	};

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
		static FBXSceneData ^ GetSceneData() { return mSceneData_; }

	private:
		static void TransformData()
		{
			mSceneData_ = gcnew FBXSceneData(*mSceneData);
		}

		static void InitializeSdkObjects();
		static bool LoadScene(const char* pFilename);
		static void PrintTabs();
		static void PrintNode(FbxNode* pNode, int ParentIndex);
		static void PrintAttribute(FbxNodeAttribute* pAttribute);
		static FbxString GetAttributeTypeName(FbxNodeAttribute::EType type);

	};
}
