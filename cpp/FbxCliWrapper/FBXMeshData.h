#pragma once

#include <vector>

namespace FbxClrWrapper
{
	using namespace System;
	ref class FBXMeshData;

	class FBXMeshDataInternal
	{
	public:
		std::vector<int32_t> mIndices;
		std::vector<float> mVertices;
		std::vector<int> mFaceSize;

	public:
		FBXMeshDataInternal(FBXMeshData^ SrcData);
		FBXMeshDataInternal()
		{}
	};	
	
	public ref class FBXMeshData
	{
	public:

		// TODO: try change everything over to IArray
		array<int32_t>^ mIndices;
		array<float>^ mVertices;
		array<int>^ mFaceSize;

		Ara3D::IArray<int32_t> ^ mInds;
		Ara3D::IArray<float>^ mVerts;
		Ara3D::IArray<int>^ mFcSz;

	public:
		FBXMeshData(FBXMeshDataInternal& SrcData);
		FBXMeshData()
		{}
	};

}
