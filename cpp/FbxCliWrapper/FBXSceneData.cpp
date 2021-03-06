#include "FBXSceneData.h"


namespace FbxClrWrapper
{
	FBXSceneDataInternal::FBXSceneDataInternal(FBXSceneData^ SrcData)
	{
		mNodeNameList.resize(SrcData->mNodeNameList->Length);
		mNodeParentList.resize(SrcData->mNodeParentList->Length);
		mNodeTransformList.resize(SrcData->mNodeTransformList->Length >> 4);
		mNodeMeshIndexList.resize(SrcData->mNodeMeshIndexList->Length);
		mMeshList.resize(SrcData->mMeshList->Length);
		mMeshIdList.resize(SrcData->mMeshIdList->Length);

		for (int i = 0; i < SrcData->mNodeNameList->Length; i++)
		{
			auto str = (const char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(SrcData->mNodeNameList[i]);
			mNodeNameList[i] = str;
		}
		for (int i = 0; i < SrcData->mNodeParentList->Length; i++)
		{
			mNodeParentList[i] = SrcData->mNodeParentList[i];
		}
		for (int i = 0; i < SrcData->mNodeTransformList->Length / 16; i++)
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
		for (int i = 0; i < SrcData->mNodeMeshIndexList->Length; i++)
		{
			mNodeMeshIndexList[i] = SrcData->mNodeMeshIndexList[i];
		}
		for (int i = 0; i < SrcData->mMeshList->Length; i++)
		{
			mMeshList[i] = new FBXMeshDataInternal(SrcData->mMeshList[i]);
		}
		for (int i = 0; i < SrcData->mMeshIdList->Length; i++)
		{
			auto str = (const char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(SrcData->mMeshIdList[i]);
			mMeshIdList[i] = str;
		}
	}

	FBXSceneData::FBXSceneData(FBXSceneDataInternal& SrcData)
	{
		mNodeNameList = gcnew array<String^>((int)SrcData.mNodeNameList.size());
		mNodeParentList = gcnew array<int32_t>((int)SrcData.mNodeParentList.size());
		mNodeTransformList = gcnew array<float>((int)SrcData.mNodeTransformList.size() * 4 * 4);
		mNodeMeshIndexList = gcnew array<int>((int)SrcData.mNodeMeshIndexList.size());
		mMeshList = gcnew array<FBXMeshData^>((int)SrcData.mMeshList.size());
		mMeshIdList = gcnew array<String^>((int)SrcData.mMeshIdList.size());

		for (int i = 0; i < (int)SrcData.mNodeNameList.size(); i++)
		{
			mNodeNameList[i] = gcnew String(SrcData.mNodeNameList[i].c_str());
		}
		for (int i = 0; i < (int)SrcData.mNodeParentList.size(); i++)
		{
			mNodeParentList[i] = SrcData.mNodeParentList[i];
		}
		for (int i = 0; i < (int)SrcData.mNodeTransformList.size(); i++)
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
		for (int i = 0; i < (int)SrcData.mNodeMeshIndexList.size(); i++)
		{
			mNodeMeshIndexList[i] = SrcData.mNodeMeshIndexList[i];
		}
		for (int i = 0; i < (int)SrcData.mMeshList.size(); i++)
		{
			mMeshList[i] = gcnew FBXMeshData(SrcData.mMeshList[i]);
		}
		for (int i = 0; i < (int)SrcData.mMeshIdList.size(); i++)
		{
			mMeshIdList[i] = gcnew String(SrcData.mMeshIdList[i].c_str());
		}
	}

}