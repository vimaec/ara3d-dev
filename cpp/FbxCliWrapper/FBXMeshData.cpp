#include "FBXMeshData.h"


namespace FbxClrWrapper
{
	FBXMeshDataInternal::FBXMeshDataInternal(FBXMeshData^ SrcData)
	{
		mIndices.resize(SrcData->mInds->Count);
		mVertices.resize(SrcData->mVerts->Count);
		mFaceSize.resize(SrcData->mFcSz->Count);

		for (int i = 0; i < SrcData->mInds->Count; i++)
		{
			mIndices[i] = SrcData->mInds[i];
		}
		for (int i = 0; i < SrcData->mVerts->Count; i++)
		{
			mVertices[i] = SrcData->mVerts[i];
		}
		for (int i = 0; i < SrcData->mFcSz->Count; i++)
		{
			mFaceSize[i] = SrcData->mFcSz[i];
		}
	}

	FBXMeshData::FBXMeshData(FBXMeshDataInternal& SrcData)
	{
		mIndices = gcnew array<int32_t>(SrcData.mIndices.size());
		mVertices = gcnew array<float>(SrcData.mVertices.size());
		mFaceSize = gcnew array<int>(SrcData.mFaceSize.size());
		for (size_t i = 0; i < SrcData.mIndices.size(); i++)
		{
			mIndices[i] = SrcData.mIndices[i];
		}
		for (size_t i = 0; i < SrcData.mVertices.size(); i++)
		{
			mVertices[i] = SrcData.mVertices[i];
		}
		for (size_t i = 0; i < SrcData.mFaceSize.size(); i++)
		{
			mFaceSize[i] = SrcData.mFaceSize[i];
		}
	}

}