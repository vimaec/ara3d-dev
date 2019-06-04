#pragma once

#include <vector>
#include <string>
#include <map>

#include "fbxsdk/include/fbxsdk.h"
#include "FBXMeshData.h"

namespace FbxClrWrapper
{
	using namespace System;
	ref class FBXSceneData;

	class FBXSceneDataInternal
	{
	public:
		std::vector<std::string> mNodeNameList;
		std::vector<int32_t> mNodeParentList;
		std::vector<FbxDouble3> mNodeTranslationList;
		std::vector<FbxDouble3> mNodeRotationList;
		std::vector<FbxDouble3> mNodeScaleList;
		std::vector<FbxDouble4x4> mNodeTransformList;
		std::vector<int> mNodeMeshIndexList;

		std::vector<FBXMeshDataInternal> mMeshList;
		std::vector<std::string> mMeshIdList;

		std::map<FbxMesh*, int> mMeshMap; // Used to gather information about instancing in the fbx file

	public:
		FBXSceneDataInternal(FBXSceneData^ SrcData);
		FBXSceneDataInternal()
		{}
	};

	public ref class FBXSceneData
	{
	public:
		array<String^>^ mNodeNameList;
		array<int32_t>^ mNodeParentList;
		array<float>^ mNodeTranslationList;
		array<float>^ mNodeRotationList;
		array<float>^ mNodeScaleList;
		array<float>^ mNodeTransformList;
		array<int>^ mNodeMeshIndexList;

		array<FBXMeshData^>^ mMeshList;
		array<String^>^ mMeshIdList;

	public:
		FBXSceneData(FBXSceneDataInternal& SrcData);
		FBXSceneData()
		{}
	};
}
