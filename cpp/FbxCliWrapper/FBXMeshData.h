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
		Ara3D::IArray<int32_t> ^ mIndices;
		Ara3D::IArray<float>^ mVertices;
		Ara3D::IArray<int>^ mFaceSize;

	public:
		FBXMeshData(FBXMeshDataInternal& SrcData);
		FBXMeshData()
		{}
	};

}
