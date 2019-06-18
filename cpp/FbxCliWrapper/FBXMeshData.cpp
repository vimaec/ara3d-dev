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

	template <typename tData>
	ref class ArrayDataLookup
	{
	public:
		std::vector<tData>& mArray;

		ArrayDataLookup(std::vector<tData>& Array) :
			mArray(Array)
		{}

		tData Lookup(int n)
		{
			return mArray[n];
		}
	};

	template <typename tData>
	Ara3D::FunctionalArray<tData>^ CreateFunctionalArray(std::vector<tData>& Array)
	{
		auto lookup = gcnew ArrayDataLookup<tData>(Array);
		return gcnew Ara3D::FunctionalArray<tData>((int)Array.size(), gcnew System::Func<int, tData>(lookup, &ArrayDataLookup<tData>::Lookup));
	}

	FBXMeshData::FBXMeshData(FBXMeshDataInternal* SrcData)
	{
		mIndices = 	CreateFunctionalArray(SrcData->mIndices);
		mVertices =	CreateFunctionalArray(SrcData->mVertices);
		mFaceSize =	CreateFunctionalArray(SrcData->mFaceSize);

		auto smoothingGroupArray = CreateFunctionalArray(SrcData->mSmoothingGroupAttribute.mDataArray);
		auto normalsArray = CreateFunctionalArray(SrcData->mNormalsAttribute.mDataArray);
		auto tangentsArray = CreateFunctionalArray(SrcData->mTangentsAttribute.mDataArray);
		auto binormalsArray = CreateFunctionalArray(SrcData->mBinormalsAttribute.mDataArray);

//		mSmoothingGroupAttribute	= Ara3D::G3DExtensions::(normalsArray, 0);
		mNormalsAttribute			= Ara3D::G3DExtensions::ToVertexNormalAttribute(normalsArray, 0);
		mTangentsAttribute			= Ara3D::G3DExtensions::ToVertexNormalAttribute(normalsArray, 0);
		mBinormalsAttribute			= Ara3D::G3DExtensions::ToVertexNormalAttribute(normalsArray, 0);
	}

}