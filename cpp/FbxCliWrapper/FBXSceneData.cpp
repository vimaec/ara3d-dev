#include "FBXSceneData.h"


namespace FbxClrWrapper
{
	FBXSceneDataInternal::FBXSceneDataInternal(FBXSceneData^ SrcData)
	{
		mNodeNameList.resize(SrcData->mNodeNameList->Length);
		mNodeParentList.resize(SrcData->mNodeParentList->Length);
		mNodeTranslationList.resize(SrcData->mNodeTranslationList->Length * 3);
		mNodeRotationList.resize(SrcData->mNodeRotationList->Length * 3);
		mNodeScaleList.resize(SrcData->mNodeScaleList->Length * 3);
		mNodeTransformList.resize(SrcData->mNodeTransformList->Length * 4 * 4);
		mNodeMeshIndexList.resize(SrcData->mNodeMeshIndexList->Length);
		mMeshList.resize(SrcData->mMeshList->Length);
		mMeshIdList.resize(SrcData->mMeshIdList->Length);

		for (size_t i = 0; i < SrcData->mNodeNameList->Length; i++)
		{
			auto str = (const char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(SrcData->mNodeNameList[i]);
			mNodeNameList[i] = str;
		}
		for (size_t i = 0; i < SrcData->mNodeParentList->Length; i++)
		{
			mNodeParentList[i] = SrcData->mNodeParentList[i];
		}
		for (size_t i = 0; i < SrcData->mNodeTranslationList->Length / 3; i++)
		{
			mNodeTranslationList[i].mData[0] = (float)SrcData->mNodeTranslationList[i * 3 + 0];
			mNodeTranslationList[i].mData[1] = (float)SrcData->mNodeTranslationList[i * 3 + 1];
			mNodeTranslationList[i].mData[2] = (float)SrcData->mNodeTranslationList[i * 3 + 2];
		}
		for (size_t i = 0; i < SrcData->mNodeRotationList->Length / 3; i++)
		{
			mNodeRotationList[i].mData[0] = (float)SrcData->mNodeRotationList[i * 3 + 0];
			mNodeRotationList[i].mData[1] = (float)SrcData->mNodeRotationList[i * 3 + 1];
			mNodeRotationList[i].mData[2] = (float)SrcData->mNodeRotationList[i * 3 + 2];
		}
		for (size_t i = 0; i < SrcData->mNodeScaleList->Length / 3; i++)
		{
			mNodeScaleList[i].mData[0] = (float)SrcData->mNodeScaleList[i * 3 + 0];
			mNodeScaleList[i].mData[1] = (float)SrcData->mNodeScaleList[i * 3 + 1];
			mNodeScaleList[i].mData[2] = (float)SrcData->mNodeScaleList[i * 3 + 2];
		}
		for (size_t i = 0; i < SrcData->mNodeTransformList->Length / 16; i++)
		{
			mNodeTransformList[i].mData[0].mData[0] = (float)SrcData->mNodeTransformList[i * 16 + 0 + 0];
			mNodeTransformList[i].mData[0].mData[1] = (float)SrcData->mNodeTransformList[i * 16 + 1 + 0];
			mNodeTransformList[i].mData[0].mData[2] = (float)SrcData->mNodeTransformList[i * 16 + 2 + 0];
			mNodeTransformList[i].mData[0].mData[3] = (float)SrcData->mNodeTransformList[i * 16 + 3 + 0];
			mNodeTransformList[i].mData[1].mData[0] = (float)SrcData->mNodeTransformList[i * 16 + 0 + 4];
			mNodeTransformList[i].mData[1].mData[1] = (float)SrcData->mNodeTransformList[i * 16 + 1 + 4];
			mNodeTransformList[i].mData[1].mData[2] = (float)SrcData->mNodeTransformList[i * 16 + 2 + 4];
			mNodeTransformList[i].mData[1].mData[3] = (float)SrcData->mNodeTransformList[i * 16 + 3 + 4];
			mNodeTransformList[i].mData[2].mData[0] = (float)SrcData->mNodeTransformList[i * 16 + 0 + 8];
			mNodeTransformList[i].mData[2].mData[1] = (float)SrcData->mNodeTransformList[i * 16 + 1 + 8];
			mNodeTransformList[i].mData[2].mData[2] = (float)SrcData->mNodeTransformList[i * 16 + 2 + 8];
			mNodeTransformList[i].mData[2].mData[3] = (float)SrcData->mNodeTransformList[i * 16 + 3 + 8];
			mNodeTransformList[i].mData[3].mData[0] = (float)SrcData->mNodeTransformList[i * 16 + 0 + 12];
			mNodeTransformList[i].mData[3].mData[1] = (float)SrcData->mNodeTransformList[i * 16 + 1 + 12];
			mNodeTransformList[i].mData[3].mData[2] = (float)SrcData->mNodeTransformList[i * 16 + 2 + 12];
			mNodeTransformList[i].mData[3].mData[3] = (float)SrcData->mNodeTransformList[i * 16 + 3 + 12];
		}
		for (size_t i = 0; i < SrcData->mNodeMeshIndexList->Length; i++)
		{
			mNodeMeshIndexList[i] = SrcData->mNodeMeshIndexList[i];
		}
		for (size_t i = 0; i < SrcData->mMeshList->Length; i++)
		{
			mMeshList[i] = FBXMeshDataInternal(SrcData->mMeshList[i]);
		}
		for (size_t i = 0; i < SrcData->mMeshIdList->Length; i++)
		{
			auto str = (const char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(SrcData->mMeshIdList[i]);
			mMeshIdList[i] = str;
		}
	}

	FBXSceneData::FBXSceneData(FBXSceneDataInternal& SrcData)
	{
		mNodeNameList = gcnew array<String^>(SrcData.mNodeNameList.size());
		mNodeParentList = gcnew array<int32_t>(SrcData.mNodeParentList.size());
		mNodeTranslationList = gcnew array<float>(SrcData.mNodeTranslationList.size() * 3);
		mNodeRotationList = gcnew array<float>(SrcData.mNodeRotationList.size() * 3);
		mNodeScaleList = gcnew array<float>(SrcData.mNodeScaleList.size() * 3);
		mNodeTransformList = gcnew array<float>(SrcData.mNodeTransformList.size() * 4 * 4);
		mNodeMeshIndexList = gcnew array<int>(SrcData.mNodeMeshIndexList.size());
		mMeshList = gcnew array<FBXMeshData^>(SrcData.mMeshList.size());
		mMeshIdList = gcnew array<String^>(SrcData.mMeshIdList.size());

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

}