#pragma once
#include <cstdint>
#include <vector>
#include <string>

#include "fbxsdk/include/fbxsdk.h"

#define _CPP_ true
#include "../../dotnet/G3D/Constants.cs"

using namespace System;
namespace FbxClrWrapper
{
	class FBXMeshData
	{
	public:
		std::vector<int32_t> mIndices;
		std::vector<float> mVertices;
		std::vector<int> mFaceSize;
	};	
	
	public ref class FBXMeshData_
	{
	public:
		array<int32_t>^ mIndices;
		array<float>^ mVertices;
		array<int>^ mFaceSize;

	public:
		FBXMeshData_(FBXMeshData & Src)
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

	class FBXSceneData
	{
	public:
		std::vector<std::string> mNodeNameList;
		std::vector<int32_t> mNodeParentList;
		std::vector<FbxDouble3> mNodeTranslationList;
		std::vector<FbxDouble3> mNodeRotationList;
		std::vector<FbxDouble3> mNodeScaleList;
		std::vector<int> mNodeMeshIndexList;

		std::vector<FBXMeshData> mMeshList;
	};

	public ref class FBXSceneData_
	{
	public:
		array<String^>^ mNodeNameList;
		array<int32_t>^ mNodeParentList;
		array<float>^ mNodeTranslationList;
		array<float>^ mNodeRotationList;
		array<float>^ mNodeScaleList;
		array<int>^ mNodeMeshIndexList;

		array<FBXMeshData_^>^ mMeshList;

	public:
		FBXSceneData_(FBXSceneData &SrcData)
		{
			mNodeNameList = gcnew array<String ^>(SrcData.mNodeNameList.size());
			mNodeParentList = gcnew array<int32_t>(SrcData.mNodeParentList.size());
			mNodeTranslationList = gcnew array<float>(SrcData.mNodeTranslationList.size() * 3);
			mNodeRotationList = gcnew array<float>(SrcData.mNodeRotationList.size() * 3);
			mNodeScaleList = gcnew array<float>(SrcData.mNodeScaleList.size() * 3);
			mNodeMeshIndexList = gcnew array<int>(SrcData.mNodeMeshIndexList.size());
			mMeshList = gcnew array<FBXMeshData_ ^>(SrcData.mMeshList.size());

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
			for (size_t i = 0; i < SrcData.mNodeMeshIndexList.size(); i++)
			{
				mNodeMeshIndexList[i] = SrcData.mNodeMeshIndexList[i];
			}
			for (size_t i = 0; i < SrcData.mMeshList.size(); i++)
			{
				mMeshList[i] = gcnew FBXMeshData_(SrcData.mMeshList[i]);
			}
		}
	};

	public ref class FBXLoader
	{
	private:
		static FbxManager* mSdkManager = nullptr;
		static FbxScene* mScene = nullptr;
		static FBXSceneData *mSceneData = nullptr;
		static FBXSceneData_ ^mSceneData_ = nullptr;

	public:
		static void Initialize();
		static void ShutDown();
		static int LoadFBX(String^ FileName);
		static FBXSceneData_ ^ GetSceneData() { return mSceneData_; }

	private:
		static void TransformData()
		{
			mSceneData_ = gcnew FBXSceneData_(*mSceneData);
		}

		static void InitializeSdkObjects();
		static bool LoadScene(const char* pFilename);
		static void PrintTabs();
		static void PrintNode(FbxNode* pNode, int ParentIndex);
		static void PrintAttribute(FbxNodeAttribute* pAttribute);
		static FbxString GetAttributeTypeName(FbxNodeAttribute::EType type);

	};
}
