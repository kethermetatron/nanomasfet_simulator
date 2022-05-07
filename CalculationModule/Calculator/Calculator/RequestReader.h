#pragma once
#include "MOSFETSimulator.h"
#include "ModelSelection.h"
using json = nlohmann::json;
namespace RequestReader
{
	json RequestJson;
	void ReadRequest(string JsonPath)
	{
		ifstream file(JsonPath);
		if (!file.is_open())return;
		string JsonString((istreambuf_iterator<char>(file)), (istreambuf_iterator<char>()));
		RequestJson = json::parse(JsonString);
		file.close();
	}
	void SetSimulationModel(MOSFETSimulator::MOSFETSimulation &Simulation)
	{
		ModelSelector::ModelSelection(RequestJson.at("Model"));
		Simulation.SourceCalcModel = ModelSelector::SourceCalcModel;
		Simulation.DrainCalcModel = ModelSelector::DrainCalcModel;
		Simulation.SavePath = RequestJson.at("SavePath");
		Simulation.Name = RequestJson.at("SimulationName");
	}
	void SetSimulationParameters(MOSFETSimulator::MOSFETSimulation &Simulation)
	{
		Simulation.ChannelLength = make_pair(RequestJson.at("BasicParameter").at("L_Min"), RequestJson.at("BasicParameter").at("L_Max"));
		Simulation.ChannelLengthSimulationNumber = RequestJson.at("BasicParameter").at("L_Number");
		Simulation.DrainToSourceVoltageRange = make_pair(RequestJson.at("BasicParameter").at("V_DS_Min"), RequestJson.at("BasicParameter").at("V_DS_Max"));
		Simulation.DrainToSourceVoltageSimulationNumber = RequestJson.at("BasicParameter").at("V_DS_Number");
		Simulation.GateVoltageRange = make_pair(RequestJson.at("BasicParameter").at("V_G_Min"), RequestJson.at("BasicParameter").at("V_G_Max"));
		Simulation.GateVoltageSimulationNumber = RequestJson.at("BasicParameter").at("V_G_Number");
		Simulation.Temperature = make_pair(RequestJson.at("BasicParameter").at("T_Min"), RequestJson.at("BasicParameter").at("T_Max"));
		Simulation.TemperatureSimulationNumber = RequestJson.at("BasicParameter").at("T_Number");
		for (int i = 0; i < Simulation.SourceCalcModel.NeededParameters.size(); i++)
		{
			string ParameterName = Simulation.SourceCalcModel.NeededParameters[i].Name;
			Simulation.ExtraSourceParameters.insert(make_pair(ParameterName, RequestJson.at("SourceParameter").at(ParameterName)));
		}
		for (int i = 0; i < Simulation.DrainCalcModel.NeededParameters.size(); i++)
		{
			string ParameterName = Simulation.DrainCalcModel.NeededParameters[i].Name;
			Simulation.ExtraDrainParameters.insert(make_pair(ParameterName, RequestJson.at("DrainParameter").at(ParameterName)));
		}
	}

}
