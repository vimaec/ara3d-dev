#pragma once

#include <vector>

namespace FbxClrWrapper
{
	using namespace System;
	ref class FBXMeshData;

	template <typename tData>
	class InternalMeshAttribute
	{
	public:
		Ara3D::Association mAssociation;
		std::vector<tData> mDataArray;
		std::vector<int> mIndexArray;

		InternalMeshAttribute()
		{}

		// We need a custom move and copy constructor because the compiler
		// doesnt seem to handle Cli enums correctly (Ara3D::Association).
		InternalMeshAttribute(InternalMeshAttribute&& Other) :
			mDataArray(std::move(Other.mDataArray)),
			mIndexArray(std::move(Other.mIndexArray)),
			mAssociation(Other.mAssociation)
		{
		}

		InternalMeshAttribute(const InternalMeshAttribute& Other) = default;
		InternalMeshAttribute& operator = (InternalMeshAttribute&& Other) = default;
		InternalMeshAttribute& operator = (const InternalMeshAttribute& Other) = default;
	};

	class FBXMeshDataInternal
	{
	public:
		std::vector<int32_t> mIndices;
		std::vector<float> mVertices;
		std::vector<int> mFaceSize;

		InternalMeshAttribute<int> mSmoothingGroupAttribute;
		InternalMeshAttribute<float> mUVsAttribute;
		InternalMeshAttribute<float> mNormalsAttribute;
		InternalMeshAttribute<float> mTangentsAttribute;
		InternalMeshAttribute<float> mBinormalsAttribute;

	public:
		FBXMeshDataInternal(FBXMeshData^ SrcData);
		FBXMeshDataInternal()
		{}

		FBXMeshDataInternal(const FBXMeshDataInternal & Other) = default;
		FBXMeshDataInternal(FBXMeshDataInternal && Other) = default;
		FBXMeshDataInternal& operator = (FBXMeshDataInternal && Other) = default;
		FBXMeshDataInternal& operator = (const FBXMeshDataInternal & Other) = default;
	};	
	
	public ref class FBXMeshData
	{
	public:
	//	Ara3D::IArray<int32_t> ^ mIndices;
	//	Ara3D::IArray<float>^ mVertices;
	//	Ara3D::IArray<int>^ mFaceSize;

		Ara3D::IAttribute^ mIndicesAttribute;
		Ara3D::IAttribute^ mVerticesAttribute;
		Ara3D::IAttribute^ mFaceSizeAttribute;

		Ara3D::IAttribute^ mSmoothingGroupAttribute;
		Ara3D::IAttribute^ mUVsAttribute;
		Ara3D::IAttribute^ mNormalsAttribute;
		Ara3D::IAttribute^ mTangentsAttribute;
		Ara3D::IAttribute^ mBinormalsAttribute;

	public:
		FBXMeshData(FBXMeshDataInternal* SrcData);
		FBXMeshData()
		{}
	};

}
