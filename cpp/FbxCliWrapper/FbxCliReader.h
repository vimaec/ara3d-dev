#pragma once

#include <cstdint>
#include <vector>
#include <string>

#include "fbxsdk/include/fbxsdk.h"

#include "FBXMeshData.h"
#include "FBXSceneData.h"
#include "FbxCliBase.h"

namespace FbxClrWrapper
{
	public ref class FbxCliReader : public FbxCliBase
	{
	public:
		int LoadFBX(String^ FileName);

	private:
		bool LoadScene(const char* pFilename);
		void ProcessFBXNode(FbxNode* pNode, int ParentIndex);
		int ExtractFBXMeshData(FbxMesh* pMesh);

		template <typename tElement, typename tData>
		void GetElementInfo(tElement* Element, InternalMeshAttribute<tData>& Attribute);
	};


	template <typename tData>
	void PushFbxData(std::vector<tData>& DataArray, FbxVector4& Data)
	{
		DataArray.push_back(Data[0]);
		DataArray.push_back(Data[1]);
		DataArray.push_back(Data[2]);
//		DataArray.push_back(Data[3]); TODO: Check if we need proper vec4 support - need to change the arity calculation
	}

	template <typename tData>
	void PushFbxData(std::vector<tData>& DataArray, FbxVector2& Data)
	{
		DataArray.push_back(Data[0]);
		DataArray.push_back(Data[1]);
	}	
	
	template <typename tData>
	void PushFbxData(std::vector<tData>& DataArray, int Data)
	{
		DataArray.push_back(Data);
	}

	template <typename tElement, typename tData>
	void FbxCliReader::GetElementInfo(tElement* Element, InternalMeshAttribute<tData>& Attribute)
	{
		if (Element)
		{
			Attribute.mAssociation = FbxMappingModeToAra3DAssociation(Element->GetMappingMode());

			auto& indexArray = Element->GetIndexArray();
			auto& dataArray = Element->GetDirectArray();

			for (int i = 0; i < indexArray.GetCount(); i++)
			{
				PushFbxData(Attribute.mIndexArray, indexArray[i]);
			}

			for (int i = 0; i < dataArray.GetCount(); i++)
			{
				PushFbxData(Attribute.mDataArray, dataArray[i]);
			}
		}
	}
}
