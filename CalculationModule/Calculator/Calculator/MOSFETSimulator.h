#pragma once
#include "Constants.h"
#include <iostream>
#include <math.h>
#include <vector>
#include <omp.h>
#include <map>
#include <unordered_map>
#include <string>
#include <utility>
#include <nlohmann/json.hpp>
#include <sstream>
#include <fstream>
using namespace std;
using json = nlohmann::json;

namespace MOSFETSimulator
{
	class EnergyBandModel
	{
	public:
		/* Vector "NeededParameters" should include all the names of parameters that are needed in your model. The program would find and copy these parameters from MOSFET*/
		vector<string> NeededParameters;
		/* Vector "Parameters" saves all the parameters and their names that are specified by "NeededParameters" */
		unordered_map<string, double> *pParameters = NULL;

		vector<double> EnergyStates;
		vector<double> DensityOfStates;
		long(*FuncCalNumberOfStates)(unordered_map<string, double>* pParameters) = NULL;
		/* The returned value of function "*FuncCalStates" should be a pair of double data which are <EnergyState, Density Of States> */
		pair<double, double>(*FuncCalStates)(long EnergyStateID, unordered_map<string, double>* pParameters) = NULL;
		void SetParameters(unordered_map<string, double>* pAllParameters);
		void CalculateStates();
		bool OpenMPOn = false;
		json GetEnergyStatesJson();
		json GetDensityOfStatesJson();

	};
	void EnergyBandModel::CalculateStates()
	{
		long nStatesNumber = (*FuncCalNumberOfStates)(pParameters);
		EnergyStates.insert(EnergyStates.end(), nStatesNumber, 0.0);
		DensityOfStates.insert(DensityOfStates.end(), nStatesNumber, 0.0);
		if (OpenMPOn)
		{
#pragma omp parallel for
			for (long i = 0; i < nStatesNumber; i++)
			{
				pair<double, double> EnergyState;
				EnergyState = (*FuncCalStates)(i, pParameters);
				EnergyStates[i] = EnergyState.first;
				DensityOfStates[i] = EnergyState.second;
			}
		}
		else
		{
			for (long i = 0; i < nStatesNumber; i++)
			{
				pair<double, double> EnergyState;
				EnergyState = (*FuncCalStates)(i, pParameters);
				EnergyStates[i] = EnergyState.first;
				DensityOfStates[i] = EnergyState.second;
			}
		}
	}
	void EnergyBandModel::SetParameters(unordered_map<string, double>* pAllParameters)
	{
		pParameters = pAllParameters;
	}
	json EnergyBandModel::GetEnergyStatesJson()
	{
		json ModelJson(EnergyStates);
		return ModelJson;
	}
	json EnergyBandModel::GetDensityOfStatesJson()
	{
		json ModelJson(DensityOfStates);
		return ModelJson;
	}

	class ElectronDistributionModel
	{
	public:
		vector<string> NeededParameters;
		unordered_map<string, double> *pParameters = NULL;
		vector<pair<double, double>> ElectronDistribution;
		double(*FuncCalElectronDistribution)(double ElectronEnergy, unordered_map<string, double>* pParameters) = NULL;
		void SetParameters(unordered_map<string, double>* pAllParameters);
		void CalculateElectronDistribution(vector<double>* pEnergyStates);
		bool OpenMPOn = false;
		json GetJson();
	};
	void ElectronDistributionModel::CalculateElectronDistribution(vector<double>* pEnergyStates)
	{
		long nStatesNumber = (*pEnergyStates).size();
		ElectronDistribution.insert(ElectronDistribution.end(), nStatesNumber, make_pair(0.0, 0.0));
		if (OpenMPOn)
		{
#pragma omp parallel for
			for (long i = 0; i < nStatesNumber; i++)
			{
				ElectronDistribution[i] = make_pair((*pEnergyStates)[i], (*FuncCalElectronDistribution)((*pEnergyStates)[i], pParameters));
			}
		}
		else
		{
			for (long i = 0; i < nStatesNumber; i++)
			{
				ElectronDistribution[i] = make_pair((*pEnergyStates)[i], (*FuncCalElectronDistribution)((*pEnergyStates)[i], pParameters));
			}
		}
	}
	void ElectronDistributionModel::SetParameters(unordered_map<string, double>* pAllParameters)
	{
		pParameters = pAllParameters;
	}
	json ElectronDistributionModel::GetJson()
	{
		json ModelJson;
		for (unsigned int i = 0; i < ElectronDistribution.size(); i++)
		{
			ModelJson.push_back(ElectronDistribution[i].second);
		}
		return ModelJson;
	}

	class PotentialBarrierModel
	{
	public:
		vector<string> NeededParameters;
		unordered_map<string, double> *pParameters = NULL;
		vector<pair<double, double>> TransmisionCoeffients;
		vector<double> PositionPoints;
		vector<double> PotentialBarriers;
		double(*FuncCalTransmissionCoefficients)(double ElectronEnergy, unordered_map<string, double>* pParameters) = NULL;
		long(*FuncCalNumberOfPositionPoints)(unordered_map<string, double>* pParameters) = NULL;
		double(*FuncCalPositionPoints)(long PointID, unordered_map<string, double>* pParameters) = NULL;
		double(*FuncCalPotentialBarrier)(double Position, unordered_map<string, double>* pParameters) = NULL;

		void SetParameters(unordered_map<string, double>* pAllParameters);
		void CalculateTransmissionCoefficients(vector<double>* EnergyStates);
		bool OpenMPOn = false;
		json GetJson();
		json GetPositionJson();
		json GetPotentialBarrierJson();
	};
	void PotentialBarrierModel::CalculateTransmissionCoefficients(vector<double>* pEnergyStates)
	{
		long nStatesNumber = (*pEnergyStates).size();
		TransmisionCoeffients.insert(TransmisionCoeffients.end(), nStatesNumber, make_pair(0.0, 0.0));
		if (OpenMPOn)
		{
#pragma omp parallel for
			for (long i = 0; i < nStatesNumber; i++)
			{
				TransmisionCoeffients[i] = make_pair((*pEnergyStates)[i], (*FuncCalTransmissionCoefficients)((*pEnergyStates)[i], pParameters));
			}
		}
		else
		{
			for (long i = 0; i < nStatesNumber; i++)
			{
				TransmisionCoeffients[i] = make_pair((*pEnergyStates)[i], (*FuncCalTransmissionCoefficients)((*pEnergyStates)[i], pParameters));

			}
		}
		long nPointNumber = (*FuncCalNumberOfPositionPoints)(pParameters);
		PositionPoints.insert(PositionPoints.end(), nPointNumber, 0.0);
		PotentialBarriers.insert(PotentialBarriers.end(), nPointNumber, 0.0);
		for (long i = 0; i < nPointNumber; i++)
		{
			PositionPoints[i] = (*FuncCalPositionPoints)(i, pParameters);
			PotentialBarriers[i] = (*FuncCalPotentialBarrier)(PositionPoints[i], pParameters);
		}
	}
	void PotentialBarrierModel::SetParameters(unordered_map<string, double>* pAllParameters)
	{
		pParameters = pAllParameters;
	}
	json PotentialBarrierModel::GetJson()
	{
		json ModelJson;
		for (unsigned int i = 0; i < TransmisionCoeffients.size(); i++)
		{
			ModelJson.push_back(TransmisionCoeffients[i].second);
		}
		return ModelJson;
	}
	json PotentialBarrierModel::GetPositionJson()
	{
		json ModelJson;
		for (unsigned int i = 0; i < PositionPoints.size(); i++)
		{
			ModelJson.push_back(PositionPoints[i]);
		}
		return ModelJson;
	}
	json PotentialBarrierModel::GetPotentialBarrierJson()
	{
		json ModelJson;
		for (unsigned int i = 0; i < PotentialBarriers.size(); i++)
		{
			ModelJson.push_back(PotentialBarriers[i]);
		}
		return ModelJson;
	}


	class MOSFET
	{
	private:
		EnergyBandModel SourceEnergyBand;
		ElectronDistributionModel SourceElectronDistribution;
		PotentialBarrierModel SourcePotentialBarrier;
		EnergyBandModel DrainEnergyBand;
		ElectronDistributionModel DrainElectronDistribution;
		PotentialBarrierModel DrainPotentialBarrier;
		double SourceToDrainCurrent;
		double DrainToSourceCurrent;
		void SaveFile();
		json SaveJson;
		void GetJson();
	public:
		string SavePath;
		string Name;
		int ID;
		string EnergyUnit;
		string CurrentUnit;
		int ID_V_DS;
		int ID_V_G;
		int ID_T;
		int ID_L;

		unordered_map<string, double> SourceParameters;
		unordered_map<string, double> DrainParameters;
		double LeakageCurrent;
		void SetSourceParameters(unordered_map<string, double>* pParameters);
		void SetDrainParameters(unordered_map<string, double>* pParameters);

		void SetSourceEnergyBandModelFunc_StatesNumber(long(*FuncCalNumberOfStates)(unordered_map<string, double>* pParameters));
		void SetSourceEnergyBandModelFunc_States(pair<double, double>(*FuncCalStates)(long EnergyStateID, unordered_map<string, double>* pParameters));
		void SetSourceElectronDistributionModelFunc(double(*FuncCalElectronDistribution)(double ElectronEnergy, unordered_map<string, double>* pParameters));
		void SetSourcePotentialBarrierModelFunc(double(*FuncCalTransmissionCoefficients)(double ElectronEnergy, unordered_map<string, double>* pParameters));
		void SetSourcePotentialBarrierModelFunc_PositionNumber(long(*FuncCalNumberOfPositionPoints)(unordered_map<string, double>* pParameters));
		void SetSourcePotentialBarrierModelFunc_Position(double(*FuncCalPositionPoints)(long PointID, unordered_map<string, double>* pParameters));
		void SetSourcePotentialBarrierModelFunc_Barrier(double(*FuncCalPotentialBarrier)(double Position, unordered_map<string, double>* pParameters));
		void SetDrainEnergyBandModelFunc_StatesNumber(long(*FuncCalNumberOfStates)(unordered_map<string, double>* pParameters));
		void SetDrainEnergyBandModelFunc_States(pair<double, double>(*FuncCalStates)(long EnergyStateID, unordered_map<string, double>* pParameters));
		void SetDrainElectronDistributionModelFunc(double(*FuncCalElectronDistribution)(double ElectronEnergy, unordered_map<string, double>* pParameters));
		void SetDrainPotentialBarrierModelFunc(double(*FuncCalTransmissionCoefficients)(double ElectronEnergy, unordered_map<string, double>* pParameters));
		void SetDrainPotentialBarrierModelFunc_PositionNumber(long(*FuncCalNumberOfPositionPoints)(unordered_map<string, double>* pParameters));
		void SetDrainPotentialBarrierModelFunc_Position(double(*FuncCalPositionPoints)(long PointID, unordered_map<string, double>* pParameters));
		void SetDrainPotentialBarrierModelFunc_Barrier(double(*FuncCalPotentialBarrier)(double Position, unordered_map<string, double>* pParameters));

		void Calculate();
		void Save();
	};
	void MOSFET::SetSourceParameters(unordered_map<string, double>* pParameters)
	{
		SourceParameters = (*pParameters);
	}
	void MOSFET::SetDrainParameters(unordered_map<string, double>* pParameters)
	{
		DrainParameters = (*pParameters);
	}
	void MOSFET::SetSourceEnergyBandModelFunc_StatesNumber(long(*FuncCalNumberOfStates)(unordered_map<string, double>* pParameters))
	{
		SourceEnergyBand.FuncCalNumberOfStates = FuncCalNumberOfStates;
	}
	void MOSFET::SetSourceEnergyBandModelFunc_States(pair<double, double>(*FuncCalStates)(long EnergyStateID, unordered_map<string, double>* pParameters))
	{
		SourceEnergyBand.FuncCalStates = FuncCalStates;
	}
	void MOSFET::SetSourceElectronDistributionModelFunc(double(*FuncCalElectronDistribution)(double ElectronEnergy, unordered_map<string, double>* pParameters))
	{
		SourceElectronDistribution.FuncCalElectronDistribution = FuncCalElectronDistribution;
	}
	void MOSFET::SetSourcePotentialBarrierModelFunc(double(*FuncCalTransmissionCoefficients)(double ElectronEnergy, unordered_map<string, double>* pParameters))
	{
		SourcePotentialBarrier.FuncCalTransmissionCoefficients = FuncCalTransmissionCoefficients;

	}
	void MOSFET::SetDrainEnergyBandModelFunc_StatesNumber(long(*FuncCalNumberOfStates)(unordered_map<string, double>* pParameters))
	{
		DrainEnergyBand.FuncCalNumberOfStates = FuncCalNumberOfStates;
	}
	void MOSFET::SetDrainEnergyBandModelFunc_States(pair<double, double>(*FuncCalStates)(long EnergyStateID, unordered_map<string, double>* pParameters))
	{
		DrainEnergyBand.FuncCalStates = FuncCalStates;
	}
	void MOSFET::SetDrainElectronDistributionModelFunc(double(*FuncCalElectronDistribution)(double ElectronEnergy, unordered_map<string, double>* pParameters))
	{
		DrainElectronDistribution.FuncCalElectronDistribution = FuncCalElectronDistribution;
	}
	void MOSFET::SetDrainPotentialBarrierModelFunc(double(*FuncCalTransmissionCoefficients)(double ElectronEnergy, unordered_map<string, double>* pParameters))
	{
		DrainPotentialBarrier.FuncCalTransmissionCoefficients = FuncCalTransmissionCoefficients;
	}
	void MOSFET::SetSourcePotentialBarrierModelFunc_PositionNumber(long(*FuncCalNumberOfPositionPoints)(unordered_map<string, double>* pParameters))
	{
		SourcePotentialBarrier.FuncCalNumberOfPositionPoints = FuncCalNumberOfPositionPoints;
	}
	void MOSFET::SetSourcePotentialBarrierModelFunc_Position(double(*FuncCalPositionPoints)(long PointID, unordered_map<string, double>* pParameters))
	{
		SourcePotentialBarrier.FuncCalPositionPoints = FuncCalPositionPoints;
	}
	void MOSFET::SetSourcePotentialBarrierModelFunc_Barrier(double(*FuncCalPotentialBarrier)(double Position, unordered_map<string, double>* pParameters))
	{
		SourcePotentialBarrier.FuncCalPotentialBarrier = FuncCalPotentialBarrier;
	}
	void MOSFET::SetDrainPotentialBarrierModelFunc_PositionNumber(long(*FuncCalNumberOfPositionPoints)(unordered_map<string, double>* pParameters))
	{
		DrainPotentialBarrier.FuncCalNumberOfPositionPoints = FuncCalNumberOfPositionPoints;
	}
	void MOSFET::SetDrainPotentialBarrierModelFunc_Position(double(*FuncCalPositionPoints)(long PointID, unordered_map<string, double>* pParameters))
	{
		DrainPotentialBarrier.FuncCalPositionPoints = FuncCalPositionPoints;
	}
	void MOSFET::SetDrainPotentialBarrierModelFunc_Barrier(double(*FuncCalPotentialBarrier)(double Position, unordered_map<string, double>* pParameters))
	{
		DrainPotentialBarrier.FuncCalPotentialBarrier = FuncCalPotentialBarrier;
	}
	void MOSFET::Calculate()
	{
		SourceEnergyBand.SetParameters(&SourceParameters);
		SourceElectronDistribution.SetParameters(&SourceParameters);
		SourcePotentialBarrier.SetParameters(&SourceParameters);
		DrainEnergyBand.SetParameters(&DrainParameters);
		DrainElectronDistribution.SetParameters(&DrainParameters);
		DrainPotentialBarrier.SetParameters(&DrainParameters);
		SourceEnergyBand.CalculateStates();
		SourceElectronDistribution.CalculateElectronDistribution(&SourceEnergyBand.EnergyStates);
		SourcePotentialBarrier.CalculateTransmissionCoefficients(&SourceEnergyBand.EnergyStates);
		DrainEnergyBand.CalculateStates();
		DrainElectronDistribution.CalculateElectronDistribution(&DrainEnergyBand.EnergyStates);
		DrainPotentialBarrier.CalculateTransmissionCoefficients(&DrainEnergyBand.EnergyStates);
		SourceToDrainCurrent = 0.0;
		DrainToSourceCurrent = 0.0;
		for (unsigned int i = 0; i < SourceEnergyBand.EnergyStates.size(); i++)
		{
			SourceToDrainCurrent += SourceElectronDistribution.ElectronDistribution[i].second * SourcePotentialBarrier.TransmisionCoeffients[i].second * SourceEnergyBand.DensityOfStates[i];
		}
		SourceToDrainCurrent *= 2.0 * ELECTRON_CHARGE / H_BAR;
		for (unsigned int i = 0; i < DrainEnergyBand.EnergyStates.size(); i++)
		{
			DrainToSourceCurrent += DrainElectronDistribution.ElectronDistribution[i].second * DrainPotentialBarrier.TransmisionCoeffients[i].second * DrainEnergyBand.DensityOfStates[i];
		}
		DrainToSourceCurrent *= 2.0 * ELECTRON_CHARGE / H_BAR;
		LeakageCurrent = SourceToDrainCurrent - DrainToSourceCurrent;

	}
	void MOSFET::SaveFile()
	{
		string path;
		string JsonString;
		stringstream ss;
		ofstream file;
		ss << SavePath << Name << ID << ".json";
		ss >> path;
		file.open(path, ios::out);
		JsonString = SaveJson.dump();
		file << JsonString;
		file.close();
	}
	void MOSFET::GetJson()
	{
		json SourceParameterJson(SourceParameters);
		json DrainParameterJson(DrainParameters);
		json SourceEnergyStatesJson = SourceEnergyBand.GetEnergyStatesJson();
		json SourceDensityofStatesJson = SourceEnergyBand.GetDensityOfStatesJson();
		json SourceElectronDistributionJson = SourceElectronDistribution.GetJson();
		json SourcePotentialBarrierPositionJson = SourcePotentialBarrier.GetPositionJson();
		json SourcePotentialBarrierJson = SourcePotentialBarrier.GetPotentialBarrierJson();
		json SourceTransmissionCoefficient = SourcePotentialBarrier.GetJson();
		json DrainEnergyStatesJson = DrainEnergyBand.GetEnergyStatesJson();
		json DrainDensityofStatesJson = DrainEnergyBand.GetDensityOfStatesJson();
		json DrainElectronDistributionJson = DrainElectronDistribution.GetJson();
		json DrainPotentialBarrierPositionJson = DrainPotentialBarrier.GetPositionJson();
		json DrainPotentialBarrierJson = DrainPotentialBarrier.GetPotentialBarrierJson();
		json DrainTransmissionCoefficient = DrainPotentialBarrier.GetJson();

		SaveJson["SourceParameter"] = SourceParameterJson;
		SaveJson["SourceEnergyStates"] = SourceEnergyStatesJson;
		SaveJson["SourceDensityofStates"] = SourceDensityofStatesJson;
		SaveJson["SourceElectronDistribution"] = SourceElectronDistributionJson;
		SaveJson["SourcePotentialBarrier"] = SourcePotentialBarrierJson;
		SaveJson["SourcePotentialBarrierPosition"] = SourcePotentialBarrierPositionJson;
		SaveJson["SourceTransmissionCoefficient"] = SourceTransmissionCoefficient;
		SaveJson["DrainParameter"] = DrainParameterJson;
		SaveJson["DrainEnergyStates"] = DrainEnergyStatesJson;
		SaveJson["DrainDensityofStates"] = DrainDensityofStatesJson;
		SaveJson["DrainElectronDistribution"] = DrainElectronDistributionJson;
		SaveJson["DrainPotentialBarrier"] = DrainPotentialBarrierJson;
		SaveJson["DrainPotentialBarrierPosition"] = DrainPotentialBarrierPositionJson;
		SaveJson["DrainTransmissionCoefficient"] = DrainTransmissionCoefficient;
		SaveJson["LeakageCurrent"] = LeakageCurrent;
		SaveJson["DrainCurrent"] = DrainToSourceCurrent;
		SaveJson["SourceCurrent"] = SourceToDrainCurrent;
		SaveJson["EnergyUnit"] = EnergyUnit;
		SaveJson["CurrentUnit"] = CurrentUnit;
		SaveJson["ID"] = ID;
		SaveJson["ID_V_DS"] = ID_V_DS;
		SaveJson["ID_V_G"] = ID_V_G;
		SaveJson["ID_T"] = ID_T;
		SaveJson["ID_L"] = ID_L;
	}
	void MOSFET::Save()
	{
		GetJson();
		SaveFile();
	}
	class Parameter
	{
	public:
		string Name;
		double MaxValue;
		double MinValue;
		string Unit;
		string Description;
		json ToJson()
		{
			json ParameterJson;
			ParameterJson["Name"] = Name;
			ParameterJson["Max"] = MaxValue;
			ParameterJson["Min"] = MinValue;
			ParameterJson["Unit"] = Unit;
			ParameterJson["Description"] = Description;
			return ParameterJson;
		}
		Parameter(string ParameterName, double ParameterMaxValue, double ParameterMinValue, string ParameterUnit, string ParameterDescription)
		{
			Name = ParameterName;
			MaxValue = ParameterMaxValue;
			MinValue = ParameterMinValue;
			Unit = ParameterUnit;
			Description = ParameterDescription;
		}
	};
	class CalculationModel
	{
	public:
		string Name;
		string Description;
		vector<Parameter> NeededParameters;
		string ResultEnergyUnit;
		string ResultCurrentUnit;
		string ResultPositionUnit;
		long(*FuncCalNumberOfStates)(unordered_map<string, double>* pParameters);
		pair<double, double>(*FuncCalStates)(long EnergyStateID, unordered_map<string, double>* pParameters);
		double(*FuncCalElectronDistribution)(double ElectronEnergy, unordered_map<string, double>* pParameters);
		double(*FuncCalTransmissionCoefficients)(double ElectronEnergy, unordered_map<string, double>* pParameters);
		long(*FuncCalNumberOfPositionPoints)(unordered_map<string, double>* pParameters);
		double(*FuncCalPositionPoints)(long PointID, unordered_map<string, double>* pParameters);
		double(*FuncCalPotentialBarrier)(double Position, unordered_map<string, double>* pParameters);
	};
	
	json GetCalcModelJson(CalculationModel SourceCalcModel,CalculationModel DrainCalcModel)
	{
		json ModelJson;
		json SourceParameterJson;
		json DrainParameterJson;
		for (int i = 0; i < SourceCalcModel.NeededParameters.size(); i++)
		{
			SourceParameterJson.push_back(SourceCalcModel.NeededParameters[i].ToJson());
		}
		for (int i = 0; i < DrainCalcModel.NeededParameters.size(); i++)
		{
			DrainParameterJson.push_back(DrainCalcModel.NeededParameters[i].ToJson());
		}
		ModelJson["Name"] = SourceCalcModel.Name;
		ModelJson["Description"] = SourceCalcModel.Description;
		ModelJson["SourceParameter"] = SourceParameterJson;
		ModelJson["DrainParameter"] = DrainParameterJson;
		ModelJson["ResultEnergyUnit"] = SourceCalcModel.ResultEnergyUnit;
		ModelJson["ResultCurrentUnit"] = SourceCalcModel.ResultCurrentUnit;
		ModelJson["ResultPositionUnit"] = SourceCalcModel.ResultPositionUnit;
		return ModelJson;
	}
	class MOSFETSimulation
	{
	public:
		/*V_DS Range, Format: pair<V_DS_MIN, V_DS_MAX>  map_name: V_DS*/
		pair<double, double> DrainToSourceVoltageRange;
		int DrainToSourceVoltageSimulationNumber;
		/*V_G Range, Format: pair<V_G_MIN, V_G_MAX> map_name: V_GS*/
		pair<double, double> GateVoltageRange;
		int GateVoltageSimulationNumber;
		/*Temperature Range, Format: pair<T_MIN,T_MAX> map_name: T*/
		pair<double, double> Temperature;
		int TemperatureSimulationNumber;
		/*ChannelLength Range, Format: pair<L_MIN,L_MAX>, map_name: L*/
		pair<double, double> ChannelLength;
		int ChannelLengthSimulationNumber;
		unordered_map<string, double> ExtraSourceParameters;
		unordered_map<string, double> ExtraDrainParameters;
		CalculationModel SourceCalcModel;
		CalculationModel DrainCalcModel;
		void Simulate();
		void ShowLeakageCurrent();
		string SavePath;
		string Name;
		void Save();
	private:
		long SimulationNumber;
		vector<MOSFET> MOSFETs;
		void SetModel();
		void TaskAllocation();
		void Calculate();
		void SaveSimulationSetting();

	};
	void MOSFETSimulation::SetModel()
	{
#pragma omp parallel for
		for (int i = 0; i < MOSFETs.size(); i++)
		{
			MOSFETs[i].SetSourceEnergyBandModelFunc_States(SourceCalcModel.FuncCalStates);
			MOSFETs[i].SetSourceEnergyBandModelFunc_StatesNumber(SourceCalcModel.FuncCalNumberOfStates);
			MOSFETs[i].SetSourceElectronDistributionModelFunc(SourceCalcModel.FuncCalElectronDistribution);
			MOSFETs[i].SetSourcePotentialBarrierModelFunc(SourceCalcModel.FuncCalTransmissionCoefficients);
			MOSFETs[i].SetSourcePotentialBarrierModelFunc_PositionNumber(SourceCalcModel.FuncCalNumberOfPositionPoints);
			MOSFETs[i].SetSourcePotentialBarrierModelFunc_Position(SourceCalcModel.FuncCalPositionPoints);
			MOSFETs[i].SetSourcePotentialBarrierModelFunc_Barrier(SourceCalcModel.FuncCalPotentialBarrier);
			MOSFETs[i].SetDrainEnergyBandModelFunc_States(DrainCalcModel.FuncCalStates);
			MOSFETs[i].SetDrainEnergyBandModelFunc_StatesNumber(DrainCalcModel.FuncCalNumberOfStates);
			MOSFETs[i].SetDrainElectronDistributionModelFunc(DrainCalcModel.FuncCalElectronDistribution);
			MOSFETs[i].SetDrainPotentialBarrierModelFunc(DrainCalcModel.FuncCalTransmissionCoefficients);
			MOSFETs[i].SetDrainPotentialBarrierModelFunc_PositionNumber(DrainCalcModel.FuncCalNumberOfPositionPoints);
			MOSFETs[i].SetDrainPotentialBarrierModelFunc_Position(DrainCalcModel.FuncCalPositionPoints);
			MOSFETs[i].SetDrainPotentialBarrierModelFunc_Barrier(DrainCalcModel.FuncCalPotentialBarrier);
			MOSFETs[i].CurrentUnit = SourceCalcModel.ResultCurrentUnit;
			MOSFETs[i].EnergyUnit = SourceCalcModel.ResultEnergyUnit;
		}
	}
	void MOSFETSimulation::TaskAllocation()
	{
		SimulationNumber = DrainToSourceVoltageSimulationNumber * GateVoltageSimulationNumber * TemperatureSimulationNumber * ChannelLengthSimulationNumber;
		MOSFETs.insert(MOSFETs.end(), SimulationNumber, MOSFET());
		unordered_map<string, double> BasicSourceParameters(ExtraSourceParameters);
		unordered_map<string, double> BasicDrainParameters(ExtraDrainParameters);
		BasicSourceParameters.insert(make_pair("V_DS", 0.0));
		BasicSourceParameters.insert(make_pair("V_G", 0.0));
		BasicSourceParameters.insert(make_pair("T", 0.0));
		BasicSourceParameters.insert(make_pair("L", 0.0));
		BasicSourceParameters.insert(make_pair("D/S", -1.0));
		BasicDrainParameters.insert(make_pair("V_DS", 0.0));
		BasicDrainParameters.insert(make_pair("V_G", 0.0));
		BasicDrainParameters.insert(make_pair("T", 0.0));
		BasicDrainParameters.insert(make_pair("L", 0.0));
		BasicDrainParameters.insert(make_pair("D/S", 1.0));
		int index = 0;
		for (int i = 0; i < DrainToSourceVoltageSimulationNumber; i++)
		{
			double V_DS = DrainToSourceVoltageSimulationNumber < 2 ? DrainToSourceVoltageRange.first : DrainToSourceVoltageRange.first + i * ((DrainToSourceVoltageRange.second - DrainToSourceVoltageRange.first) / (DrainToSourceVoltageSimulationNumber - 1.0));
			BasicSourceParameters["V_DS"] = V_DS;
			BasicDrainParameters["V_DS"] = V_DS;
			for (int j = 0; j < GateVoltageSimulationNumber; j++)
			{
				double V_G = GateVoltageSimulationNumber < 2 ? GateVoltageRange.first : GateVoltageRange.first + j * ((GateVoltageRange.second - GateVoltageRange.first) / (GateVoltageSimulationNumber - 1.0));
				BasicSourceParameters["V_G"] = V_G;
				BasicDrainParameters["V_G"] = V_G;
				for (int k = 0; k < TemperatureSimulationNumber; k++)
				{
					double T = TemperatureSimulationNumber < 2 ? Temperature.first : Temperature.first + k * ((Temperature.second - Temperature.first) / (TemperatureSimulationNumber - 1.0));
					BasicSourceParameters["T"] = T;
					BasicDrainParameters["T"] = T;
					for (int l = 0; l < ChannelLengthSimulationNumber; l++)
					{
						double L = ChannelLengthSimulationNumber < 2 ? ChannelLength.first : ChannelLength.first + l * ((ChannelLength.second - ChannelLength.first) / (ChannelLengthSimulationNumber - 1.0));
						BasicSourceParameters["L"] = L;
						BasicDrainParameters["L"] = L;
						MOSFETs[index].SetSourceParameters(&BasicSourceParameters);
						MOSFETs[index].SetDrainParameters(&BasicDrainParameters);
						MOSFETs[index].SavePath = SavePath;
						MOSFETs[index].Name = Name;
						MOSFETs[index].ID = index + 1;
						MOSFETs[index].ID_V_DS = i + 1;
						MOSFETs[index].ID_V_G = j + 1;
						MOSFETs[index].ID_T = k + 1;
						MOSFETs[index].ID_L = l + 1;
						index++;
					}
				}
			}
		}
	}
	void MOSFETSimulation::Calculate()
	{
#pragma omp parallel for
		for (int i = 0; i < MOSFETs.size(); i++)
		{
			MOSFETs[i].Calculate();
		}
	}
	void MOSFETSimulation::Simulate()
	{
		TaskAllocation();
		SetModel();
		Calculate();
	}
	void MOSFETSimulation::ShowLeakageCurrent()
	{
		for (unsigned int i = 0; i < MOSFETs.size(); i++)
		{
			cout << "MOSFET:" << i << " V_DS:" << MOSFETs[i].SourceParameters["V_DS"] << " Leakage:" << MOSFETs[i].LeakageCurrent << endl;
		}
	}
	void MOSFETSimulation::Save()
	{
#pragma omp parallel for
		for (int i = 0; i < MOSFETs.size(); i++)
		{
			MOSFETs[i].Save();
		}
		SaveSimulationSetting();
	}
	void MOSFETSimulation::SaveSimulationSetting()
	{
		json SimulationJson;
		json SourceParameterJson(ExtraSourceParameters);
		json DrainParameterJson(ExtraDrainParameters);
		json BasicParameterJson;
		SimulationJson["SimulationName"] = Name;
		SimulationJson["ModelName"] = SourceCalcModel.Name;
		SimulationJson["SourceParameter"] = SourceParameterJson;
		SimulationJson["DrainParameter"] = DrainParameterJson;
		BasicParameterJson["V_DS_Min"] = DrainToSourceVoltageRange.first;
		BasicParameterJson["V_DS_Max"] = DrainToSourceVoltageRange.second;
		BasicParameterJson["V_DS_Number"] = DrainToSourceVoltageSimulationNumber;
		BasicParameterJson["V_G_Min"] = GateVoltageRange.first;
		BasicParameterJson["V_G_Max"] = GateVoltageRange.second;
		BasicParameterJson["V_G_Number"] = GateVoltageSimulationNumber;
		BasicParameterJson["L_Min"] = ChannelLength.first;
		BasicParameterJson["L_Max"] = ChannelLength.second;
		BasicParameterJson["L_Number"] = ChannelLengthSimulationNumber;
		BasicParameterJson["T_Min"] = Temperature.first;
		BasicParameterJson["T_Max"] = Temperature.second;
		BasicParameterJson["T_Number"] = TemperatureSimulationNumber;
		SimulationJson["BasicParameter"] = BasicParameterJson;
		SimulationJson["SimulationNumber"] = MOSFETs.size();
		SimulationJson["EnergyUnit"] = SourceCalcModel.ResultEnergyUnit;
		SimulationJson["CurrentUnit"] = SourceCalcModel.ResultCurrentUnit;
		SimulationJson["PositionUnit"] = SourceCalcModel.ResultPositionUnit;
		string path;
		string JsonString;
		stringstream ss;
		ofstream file;
		ss << SavePath << "SimulationSettings" << ".json";
		ss >> path;
		file.open(path, ios::out);
		JsonString = SimulationJson.dump();
		file << JsonString;
		file.close();
	}

	
}


