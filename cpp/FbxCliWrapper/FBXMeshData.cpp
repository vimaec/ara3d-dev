#include "FBXMeshData.h"


namespace FbxClrWrapper
{
	FBXMeshDataInternal::FBXMeshDataInternal(FBXMeshData^ SrcData)
	{
		mIndices.resize(SrcData->mIndices->Count);
		mVertices.resize(SrcData->mVertices->Count);
		mFaceSize.resize(SrcData->mFaceSize->Count);

		for (int i = 0; i < SrcData->mIndices->Count; i++)
		{
			mIndices[i] = SrcData->mIndices[i];
		}
		for (int i = 0; i < SrcData->mVertices->Count; i++)
		{
			mVertices[i] = SrcData->mVertices[i];
		}
		for (int i = 0; i < SrcData->mFaceSize->Count; i++)
		{
			mFaceSize[i] = SrcData->mFaceSize[i];
		}
	}

	ref class cLookup
	{
	public:
		FBXMeshDataInternal* mSrcData;
		cLookup(FBXMeshDataInternal& SrcData) :
			mSrcData (&SrcData)
		{}

		int32_t IndFunc(int n)
		{
			return mSrcData->mIndices[n];
		}
		float VertFunc(int n)
		{
			return mSrcData->mVertices[n];
		}
		int32_t FaceSizeFunc(int n)
		{
			return mSrcData->mFaceSize[n];
		}
	};

	FBXMeshData::FBXMeshData(FBXMeshDataInternal& SrcData)
	{
		auto lookup = gcnew cLookup(SrcData);

		mIndices = gcnew Ara3D::FunctionalArray<int32_t>(SrcData.mIndices.size(), gcnew System::Func<int, int32_t>(lookup, &cLookup::IndFunc));
		mVertices = gcnew Ara3D::FunctionalArray<float>(SrcData.mVertices.size(), gcnew System::Func<int, float>(lookup, &cLookup::VertFunc));
		mFaceSize = gcnew Ara3D::FunctionalArray<int32_t>(SrcData.mFaceSize.size(), gcnew System::Func<int, int32_t>(lookup, &cLookup::FaceSizeFunc));
	}

}