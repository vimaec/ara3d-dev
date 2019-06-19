#include "FBXMeshData.h"


namespace FbxClrWrapper
{
	FBXMeshDataInternal::FBXMeshDataInternal(FBXMeshData^ SrcData)
	{
		auto indicesArray = SrcData->mIndicesAttribute->ToInts();
		auto verticesArray = SrcData->mVerticesAttribute->ToFloats();
		auto faceSizeArray = SrcData->mFaceSizeAttribute->ToInts();

		mIndices.resize(indicesArray->Count);
		mVertices.resize(verticesArray->Count);
		mFaceSize.resize(faceSizeArray->Count);

		for (int i = 0; i < indicesArray->Count; i++)
		{
			mIndices[i] = indicesArray[i];
		}
		for (int i = 0; i < verticesArray->Count; i++)
		{
			mVertices[i] = verticesArray[i];
		}
		for (int i = 0; i < faceSizeArray->Count; i++)
		{
			mFaceSize[i] = faceSizeArray[i];
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
		auto indicesArray = 	CreateFunctionalArray(SrcData->mIndices);
		auto verticesArray =	CreateFunctionalArray(SrcData->mVertices);
		auto faceSizeArray =	CreateFunctionalArray(SrcData->mFaceSize);

		auto smoothingGroupArray = CreateFunctionalArray(SrcData->mSmoothingGroupAttribute.mDataArray);
		auto normalsArray = CreateFunctionalArray(SrcData->mNormalsAttribute.mDataArray);
		auto tangentsArray = CreateFunctionalArray(SrcData->mTangentsAttribute.mDataArray);
		auto binormalsArray = CreateFunctionalArray(SrcData->mBinormalsAttribute.mDataArray);

		// TODO: map smoothing groups
//		mSmoothingGroupAttribute	= Ara3D::G3DExtensions::(normalsArray, 0);
		mIndicesAttribute			= Ara3D::G3DExtensions::ToIndexAttribute(indicesArray);
		mVerticesAttribute			= Ara3D::G3DExtensions::ToVertexAttribute(verticesArray);
		mFaceSizeAttribute			= Ara3D::G3DExtensions::ToFaceSizeAttribute(faceSizeArray, Ara3D::Association::assoc_face);

		mNormalsAttribute			= Ara3D::G3DExtensions::ToAttribute(normalsArray, Ara3D::Association::assoc_vertex, Ara3D::AttributeType::attr_normal, 0, 3);
		mTangentsAttribute			= Ara3D::G3DExtensions::ToAttribute(tangentsArray, Ara3D::Association::assoc_vertex, Ara3D::AttributeType::attr_tangent, 0, 3);
		mBinormalsAttribute			= Ara3D::G3DExtensions::ToAttribute(binormalsArray, Ara3D::Association::assoc_vertex, Ara3D::AttributeType::attr_binormal, 0, 3);
	}

}