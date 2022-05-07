#pragma once
#include "MOSFETSimulator.h"
using json = nlohmann::json;
namespace ModelSelector 
{
	class CalcModel
	{
	public:
		string Name;
		MOSFETSimulator::CalculationModel SourceCalcModel;
		MOSFETSimulator::CalculationModel DrainCalcModel;
		virtual void Initialize() = 0;
		virtual void SetModelDescription() = 0;
		json ToJson()
		{
			return MOSFETSimulator::GetCalcModelJson(SourceCalcModel, DrainCalcModel);
		}
	};
	
	vector<CalcModel*> Models = {};
	MOSFETSimulator::CalculationModel SourceCalcModel;
	MOSFETSimulator::CalculationModel DrainCalcModel;
	json ModelDisplayJson;
	void ModelSelection(string ModelName)
	{
		for (int i = 0; i < Models.size(); i++)
		{
			(*Models[i]).SetModelDescription();
			if ((*Models[i]).Name == ModelName)
			{
				(*Models[i]).Initialize();
				SourceCalcModel = (*Models[i]).SourceCalcModel;
				DrainCalcModel = (*Models[i]).DrainCalcModel;
				return;
			}
		}
	}
	void DisplayAllModels(string OutputPath)
	{
		json DisplayJson;
		string path;
		string JsonString;
		stringstream ss;
		ofstream file;
		for (int i = 0; i < Models.size(); i++)
		{
			(*Models[i]).SetModelDescription();
			DisplayJson["Models"].push_back((*Models[i]).ToJson());
		}
		ss << OutputPath << "\\Models.json";
		ss >> path;
		file.open(path, ios::out);
		JsonString = DisplayJson.dump();
		file << JsonString;
		file.close();
	}
	
	
}

