#include "FBXMeshData.h"


namespace FbxClrWrapper
{
	FBXMeshDataInternal::FBXMeshDataInternal(FBXMeshData^ SrcData)
	{
		mIndices.resize(SrcData->mIndices->Length);
		mVertices.resize(SrcData->mVertices->Length);
		mFaceSize.resize(SrcData->mFaceSize->Length);

		for (size_t i = 0; i < SrcData->mIndices->Length; i++)
		{
			mIndices[i] = SrcData->mIndices[i];
		}
		for (size_t i = 0; i < SrcData->mVertices->Length; i++)
		{
			mVertices[i] = SrcData->mVertices[i];
		}
		for (size_t i = 0; i < SrcData->mFaceSize->Length; i++)
		{
			mFaceSize[i] = SrcData->mFaceSize[i];
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