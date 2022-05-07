#pragma once
#include "MOSFETSimulator.h"
#include "ModelSelection.h"
namespace RectangularPotentialBarrierModel
{
	double GetTransmissionCoefficient(double dwElectronEnergy, double dwBarrierEnergy, double dwDrainEnergy, double dwChannelLength)//unit:eV
	{
		if (dwDrainEnergy < 0 || dwBarrierEnergy < 0)return -1.00;
		if (dwElectronEnergy > dwBarrierEnergy)
		{
			double dwWaveNumber = sqrt(2.0 * ELECTRON_MASS * (dwElectronEnergy - dwBarrierEnergy)) / H_BAR;
			double dwDenominatorPart1 = dwDrainEnergy / dwElectronEnergy / 4.0;
			double dwDenominatorPart2 = 0.5 * sqrt(1 + dwDrainEnergy / dwElectronEnergy);
			double dwDenominatorPart3 = (dwBarrierEnergy / dwElectronEnergy) * ((dwBarrierEnergy + dwDrainEnergy) / (dwElectronEnergy - dwBarrierEnergy)) / 4.0;
			double dwDenominatorPart4 = pow(sin(dwWaveNumber * dwChannelLength), 2);
			double dwTransmissionCoefficient = 1.0 / (0.5 + dwDenominatorPart1 + dwDenominatorPart2 + dwDenominatorPart3 * dwDenominatorPart4);
			return dwTransmissionCoefficient;
		}
		if (dwElectronEnergy < dwBarrierEnergy)
		{
			double dwWaveNumber = sqrt(2.0 * ELECTRON_MASS * (dwBarrierEnergy - dwElectronEnergy)) / H_BAR;
			double dwDenominatorPart1 = dwDrainEnergy / (4.0 * dwElectronEnergy);
			double dwDenominatorPart2 = 0.5 * sqrt(1 + dwDrainEnergy / dwElectronEnergy);
			double dwDenominatorPart3 = (dwBarrierEnergy / dwElectronEnergy) * ((dwBarrierEnergy + dwDrainEnergy) / (dwBarrierEnergy - dwElectronEnergy)) / 4.0;
			double dwDenominatorPart4 = pow(sinh(dwWaveNumber * dwChannelLength), 2);
			double dwTransmissionCoefficient = 1.0 / (0.5 + dwDenominatorPart1 + dwDenominatorPart2 + dwDenominatorPart3 * dwDenominatorPart4);
			return dwTransmissionCoefficient;
		}
		if (dwElectronEnergy == dwBarrierEnergy)
		{
			double dwDenominatorPart1 = dwDrainEnergy / dwElectronEnergy / 4.0;
			double dwDenominatorPart2 = 0.5 * sqrt(1 + dwDrainEnergy / dwElectronEnergy);
			double dwDenominatorPart3 = ELECTRON_MASS * pow(dwChannelLength, 2) / (2 * pow(H_BAR, 2)) * (dwElectronEnergy + dwDrainEnergy);
			double dwTransmissionCoefficient = 1.0 / (0.5 + dwDenominatorPart1 + dwDenominatorPart2 + dwDenominatorPart3);
			return dwTransmissionCoefficient;
		}

	}
	double GetFermiDistribution(double dwElectronEnergy, double dwFermiEnergy, double dwTemperature)
	{
		if (dwTemperature > 0.0)
		{
			return 1.0 / (exp((dwElectronEnergy - dwFermiEnergy) / (BOLTZMANN_CONSTANT * dwTemperature)) + 1.0);
		}
		if (dwTemperature == 0.0)
		{
			return dwElectronEnergy > dwFermiEnergy ? 1.0 : 0.0;
		}
		if (dwTemperature < 0.0)
		{
			return -1.0;
		}
	}
	long CalculateNumberOfStates_FixRange(unordered_map<string, double>* pParameters)
	{
		double MinEnergy = (*pParameters)["E_Min"] * ELECTRON_CHARGE;
		double MaxEnergy = (*pParameters)["E_Max"] * ELECTRON_CHARGE;
		double EnergyGap = (*pParameters)["Delta_E"] * ELECTRON_CHARGE;
		return (long)ceil((MaxEnergy - MinEnergy) / EnergyGap);
	}
	pair<double, double> CalculateStates_FixRange(long EnergyStateID, unordered_map<string, double>* pParameters)
	{
		double MinEnergy = (*pParameters)["E_Min"] * ELECTRON_CHARGE;
		double MaxEnergy = (*pParameters)["E_Max"] * ELECTRON_CHARGE;
		double EnergyGap = (*pParameters)["Delta_E"] * ELECTRON_CHARGE;
		return make_pair(MinEnergy + EnergyStateID * EnergyGap, EnergyGap);
	}
	double CalculateFirmiDistribution(double ElectronEnergy, unordered_map<string, double>* pParameters)
	{
		double DrainOrSource = (*pParameters)["D/S"];
		double DrainToSourceVoltage = (*pParameters)["V_DS"];
		double FermiEnergy = DrainOrSource < 0 ? (*pParameters)["E_f"] * ELECTRON_CHARGE : (*pParameters)["E_f"] * ELECTRON_CHARGE - ELECTRON_CHARGE * DrainToSourceVoltage;
		double Temperature = (*pParameters)["T"];
		return GetFermiDistribution(ElectronEnergy, FermiEnergy, Temperature);
	}
	double CalculateTransmissionDistribution(double ElectronEnergy, unordered_map<string, double>* pParameters)
	{
		double BasePotentialBarrier = (*pParameters)["E_B"] * ELECTRON_CHARGE;
		double DrainToSouceVoltage = (*pParameters)["V_DS"];
		double GateVoltage = (*pParameters)["V_G"];
		double ChannelLength = (*pParameters)["L"];
		double BarrierEnergy = BasePotentialBarrier - ELECTRON_CHARGE * GateVoltage;
		double DrainEnergy = DrainToSouceVoltage * ELECTRON_CHARGE;
		return GetTransmissionCoefficient(ElectronEnergy, BarrierEnergy, DrainEnergy, ChannelLength);
	}
	long CalculateNumberOfPositionPoint(unordered_map<string, double>* pParameters)
	{
		return 21;
	}
	double CalculatePosition(long ID, unordered_map<string, double>* pParameters)
	{
		double ChannelLength = (*pParameters)["L"];
		double PositionGap = ChannelLength / 6;
		if (ID <= 7)return -PositionGap * (7 - ID);
		if (ID > 7 && ID <= 14) return PositionGap * (ID - 8 + 0.01);
		if (ID > 14) return PositionGap * (ID - 9 + 0.01);
	}
	double CalculatePotentialBarrier(double Position, unordered_map<string, double>* pParameters)
	{
		double BarrierEnergy = ((*pParameters)["E_B"] - (*pParameters)["V_G"])*ELECTRON_CHARGE;
		double DrainEnergy = (*pParameters)["V_DS"] * ELECTRON_CHARGE * -1.0;
		double ChannelLength = (*pParameters)["L"];
		if (Position <= 0)return 0.0;
		if (Position > 0 && Position <= ChannelLength) return BarrierEnergy;
		if (Position > ChannelLength) return DrainEnergy;
	}

	class RectangularPotentialBarrierModel : public ModelSelector::CalcModel
	{
	public:
		virtual void SetModelDescription()
		{
			Name = "RectangularPotentialBarrier";
			MOSFETSimulator::Parameter E_B("E_B", 10.0, 0.0, "eV", "Height of Rectangular Potential Barrier");
			MOSFETSimulator::Parameter E_f("E_f", 10.0, 0.0, "eV", "Fermi Energy");
			MOSFETSimulator::Parameter E_Min("E_Min", 10.0, 0.0, "eV", "Max Electron Energy");
			MOSFETSimulator::Parameter E_Max("E_Max", 10.0, 0.0, "eV", "Min Electron Energy");
			MOSFETSimulator::Parameter Delta_E("Delta_E", 1.0, 0.0, "eV", "Energy Gap of Simulation");
			SourceCalcModel.Name = "RectangularPotentialBarrier";
			SourceCalcModel.Description = "aaaaaaaaa";
			DrainCalcModel.Name = "RectangularPotentialBarrier";
			DrainCalcModel.Description = "123455a";
			SourceCalcModel.NeededParameters = { E_B,E_f,E_Min,E_Max,Delta_E };
			DrainCalcModel.NeededParameters = { E_B,E_f,E_Min,E_Max,Delta_E };
			SourceCalcModel.ResultEnergyUnit = "J";
			DrainCalcModel.ResultEnergyUnit = "J";
			SourceCalcModel.ResultCurrentUnit = "A";
			DrainCalcModel.ResultCurrentUnit = "A";
			SourceCalcModel.ResultPositionUnit = "m";
			SourceCalcModel.ResultPositionUnit = "m";

		}
		virtual void Initialize()
		{
			SourceCalcModel.FuncCalNumberOfStates = CalculateNumberOfStates_FixRange;
			SourceCalcModel.FuncCalStates = CalculateStates_FixRange;
			SourceCalcModel.FuncCalElectronDistribution = CalculateFirmiDistribution;
			SourceCalcModel.FuncCalTransmissionCoefficients = CalculateTransmissionDistribution;
			SourceCalcModel.FuncCalNumberOfPositionPoints = CalculateNumberOfPositionPoint;
			SourceCalcModel.FuncCalPositionPoints = CalculatePosition;
			SourceCalcModel.FuncCalPotentialBarrier = CalculatePotentialBarrier;
			DrainCalcModel.FuncCalNumberOfStates = CalculateNumberOfStates_FixRange;
			DrainCalcModel.FuncCalStates = CalculateStates_FixRange;
			DrainCalcModel.FuncCalElectronDistribution = CalculateFirmiDistribution;
			DrainCalcModel.FuncCalTransmissionCoefficients = CalculateTransmissionDistribution;
			DrainCalcModel.FuncCalNumberOfPositionPoints = CalculateNumberOfPositionPoint;
			DrainCalcModel.FuncCalPositionPoints = CalculatePosition;
			DrainCalcModel.FuncCalPotentialBarrier = CalculatePotentialBarrier;

		}
	};
	
	
}

