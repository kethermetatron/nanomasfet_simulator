using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Threading;


/*
 * 
 * 坐标轴自变量数据代号：(AxisX,AxisZ,以及其他变量代号）
 * 0 None
 * 1 V_DS
 * 2 V_G
 * 3 L
 * 4 T
 * 5 Position
 * 6 EnergyStates
 * 坐标轴因变量数据代号 (AxisY)
 * 1 LeakageCurrent
 * 2 SourceCurrent
 * 3 DrainCurrent
 * 4 TransmissionCoefficient(source)
 * 5 Potential Barrier (source)
 * 6 SourceDensityofStates
 * 7 SourceElectronDistribution
 * 8 DrainDensityofStates
 * 9 DrainElectronDistribution
 * 
 * Z可以为0
 * X不可以为0
 * 
 *因变量 1-3 适用单位 CurrentUnit ，适用X自变量 1-4 Z 0-4
 * 因变量4,6-9 使用单位：概率，值范围固定为0-1，X固定为6（使用单位 EnergyUnit)，Z：0-4
 * 因变量 5 使用单位:EnergyUnit, x固定为5 （适用单位：PositionUnit）Z：0-4
 * */

public class MOSFET
{
    public double LeakageCurrent;
    public double SourceCurrent;
    public double DrainCurrent;

    public IDictionary<string, double> SourceParameter;
    public IDictionary<string, double> DrainParameter;

    public int ID;
    public int ID_V_DS;
    public int ID_V_G;
    public int ID_T;
    public int ID_L;

    public List<double> DrainEnergyStates;
    public List<double> DrainElectronDistribution;
    public List<double> DrainPotentialBarrier;
    public List<double> DrainPotentialBarrierPosition;
    public List<double> DrainDensityOfStates;
    public List<double> DrainTransmissionCoefficient;
    public List<double> SourceEnergyStates;
    public List<double> SourceElectronDistribution;
    public List<double> SourcePotentialBarrier;
    public List<double> SourcePotentialBarrierPosition;
    public List<double> SourceDensityOfStates;
    public List<double> SourceTransmissionCoefficient;

    public void ReadFromJson(string JsonPath)
    {
        JObject Json = JObject.Parse(File.ReadAllText(JsonPath));

        LeakageCurrent = Json["LeakageCurrent"].ToObject<double>();
        SourceCurrent = Json["SourceCurrent"].ToObject<double>();
        DrainCurrent = Json["DrainCurrent"].ToObject<double>();

        SourceParameter = Json["SourceParameter"].ToObject<IDictionary<string, double>>();
        DrainParameter = Json["DrainParameter"].ToObject<IDictionary<string, double>>();
        DrainEnergyStates = Json["DrainEnergyStates"].ToObject<List<double>>();

        DrainElectronDistribution = Json["DrainElectronDistribution"].ToObject<List<double>>();
        DrainPotentialBarrier = Json["DrainPotentialBarrier"].ToObject<List<double>>();
        DrainPotentialBarrierPosition = Json["DrainPotentialBarrierPosition"].ToObject<List<double>>();
        DrainDensityOfStates = Json["DrainDensityofStates"].ToObject<List<double>>();
        DrainTransmissionCoefficient = Json["DrainTransmissionCoefficient"].ToObject<List<double>>();
        SourceEnergyStates = Json["SourceEnergyStates"].ToObject<List<double>>();
        SourceElectronDistribution = Json["SourceElectronDistribution"].ToObject<List<double>>();
        SourcePotentialBarrier = Json["SourcePotentialBarrier"].ToObject<List<double>>();
        SourcePotentialBarrierPosition = Json["SourcePotentialBarrierPosition"].ToObject<List<double>>();
        SourceDensityOfStates = Json["SourceDensityofStates"].ToObject<List<double>>();
        SourceTransmissionCoefficient = Json["SourceTransmissionCoefficient"].ToObject<List<double>>();
        ID = Json["ID"].ToObject<int>();
        ID_V_DS = Json["ID_V_DS"].ToObject<int>();
        ID_V_G = Json["ID_V_G"].ToObject<int>();
        ID_L = Json["ID_L"].ToObject<int>();
        ID_T = Json["ID_T"].ToObject<int>();

    }
}

public class DisplayRequest
{
    public int AxisYData;
    public int AxisXData;
    public int AxisZData;
    public List<Vector2Int> OtherParameters;

}

public class DoubleVector3
{
    public double x;
    public double y;
    public double z;

    public DoubleVector3(double x, double y,double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

public class DoubleVector2
{
    public double x;
    public double y;

    public DoubleVector2(double x,double y)
    {
        this.x = x;
        this.y = y;
    }
    public Vector2 ToVector2()
    {
        Vector2 newVector2 = new Vector2((float)x, (float)y);
        return newVector2;
    }
}

public class SimulationSettings
{
    public string SimulationName;
    public string ModelName;
    public int SimulationNumber;
    public string EnergyUnit;
    public string CurrentUnit;
    public string PositionUnit;

    public IDictionary<string, double> BasicParameters;
    public IDictionary<string, double> DrainParameters;
    public IDictionary<string, double> SourceParameters;

    public void ReadFromJson(string JsonPath)
    {
        JObject Json = JObject.Parse(File.ReadAllText(JsonPath));

        SimulationName = Json["SimulationName"].ToObject<string>();
        ModelName = Json["ModelName"].ToObject<string>();
        SimulationNumber = Json["SimulationNumber"].ToObject<int>();
        EnergyUnit = Json["EnergyUnit"].ToObject<string>();
        CurrentUnit = Json["CurrentUnit"].ToObject<string>();
        PositionUnit = Json["PositionUnit"].ToObject<string>();
        BasicParameters = Json["BasicParameter"].ToObject<IDictionary<string, double>>();
        SourceParameters = Json["SourceParameter"].ToObject<IDictionary<string, double>>();
        DrainParameters = Json["DrainParameter"].ToObject<IDictionary<string, double>>();
    }
}

public static class DataProcess {
    public const double ELECTRON_CHARGE = 1.6e-19;
    // Start is called before the first frame update
    public static List<MOSFET> MOSFETs = new List<MOSFET>();
    public static SimulationSettings Simulation = new SimulationSettings();
    public static DisplayRequest Request = new DisplayRequest();

    public static string SimulationDataPath;
    public static string ErrorMessage;
    public static string str_AxisX;
    public static bool EnergyUsingEV=true;
    public static string str_AxisY;
    public static string str_AxisZ;
    public static string str_Title;
    public static double AxisXMin;
    public static double AxisXMax;
    public static double AxisYMin;
    public static double AxisYMax;
    public static double AxisZMin;
    public static double AxisZMax;
    public static bool SimulationLoaded = false;
    public static int CurrentDisplay = 0;  //0 No Display, 2 2D Display, 3 3D Display
    public static string AppPath;
    public static int ReadProgress = 0;
    public static bool Reading = false;
    public static TaskDialog taskDialog1 = new TaskDialog();
    public static TaskDialogProgressBar taskDialogProgressBar1 = new TaskDialogProgressBar();

    public static List<DoubleVector3> SimulationData3D = new List<DoubleVector3>();
    public static List<DoubleVector2> SimulationData2D = new List<DoubleVector2>();
    public static List<Vector2> DisplayData2D = new List<Vector2>();
    public static List<Vector3> DisplayData3D = new List<Vector3>();
    public static List<List<Vector3>> DisplayMesh = new List<List<Vector3>>();
    private static bool DialogHasBeenSet = false;

    public static void  Main()
    {
        JObject parsed = JObject.Parse(File.ReadAllText("D:\\QQ\\aa\\OpenMPTest\\OpenMPTest1\\OpenMPTest1\\Data\\MOSFET_1.json"));
        IDictionary<string, double> MOSFETSourceParameters = parsed["SourceParameter"].ToObject<IDictionary<string,double>>();
        string jsonString = JsonConvert.SerializeObject(MOSFETSourceParameters);
        Debug.Log(jsonString);

       // Debug.Log("Main Program Here");
       // Debug.Log(jsonString);
    }

    public static bool ReadSimulationData()
    {
        
        Reading = true;
        MOSFETs.Clear();
        string SettingJsonPath = SimulationDataPath + "\\SimulationSettings.json";

        if(File.Exists(SettingJsonPath))
        {
            Simulation.ReadFromJson(SettingJsonPath);
            ShowProgress1();
            for (int i=1;i<=Simulation.SimulationNumber;i++)
            {
                string MOSFETJsonPath = SimulationDataPath + "\\" + Simulation.SimulationName + i.ToString() + ".json";

                if(File.Exists(MOSFETJsonPath))
                {
                    MOSFET newMOSFET = new MOSFET();
                    newMOSFET.ReadFromJson(MOSFETJsonPath);
                    MOSFETs.Add(newMOSFET);
                }
                else
                {
                    ErrorMessage = "Simulation Data" + i.ToString() + " does not exist";
                    ReadProgress = 0;
                    Reading = false;
                    CloseProgress();
                    return false;
                }
                SetProgress(i);
                ReadProgress = i;
            }
            ReadProgress = 0;
            Reading = false;
            CloseProgress();
            return true;
        }
        ErrorMessage = "Wrong Simulation Path";
        return false;
    }
    public static void SearchSimulationData(ref DisplayRequest request)
    {
        List<int> ChosenMOSFETID = new List<int>();

        for (int i = 0; i < MOSFETs.Count; i++)
        {
            if (CheckingMOSFET(ref request, i))
            {
                ChosenMOSFETID.Add(i);
            }
        }
        Debug.Log("ChosenMOSFETNumber" + ChosenMOSFETID.Count.ToString());

        if (request.AxisZData == 0)
        {
            SimulationData2D.Clear();
            if (request.AxisYData == 1)
            {
                for (int i = 0; i < ChosenMOSFETID.Count; i++)
                {
                    DoubleVector2 DataPoint = new DoubleVector2(GetAxisXZValue(request.AxisXData, ChosenMOSFETID[i]), MOSFETs[ChosenMOSFETID[i]].LeakageCurrent);
                    SimulationData2D.Add(DataPoint);
                }
            } 
            else if (request.AxisYData == 2)
            {
                for (int i = 0; i < ChosenMOSFETID.Count; i++)
                {
                    DoubleVector2 DataPoint = new DoubleVector2(GetAxisXZValue(request.AxisXData, ChosenMOSFETID[i]), MOSFETs[ChosenMOSFETID[i]].SourceCurrent);
                    SimulationData2D.Add(DataPoint);
                }
            }
            else if (request.AxisYData == 3)
            {
                for (int i = 0; i < ChosenMOSFETID.Count; i++)
                {
                    DoubleVector2 DataPoint = new DoubleVector2(GetAxisXZValue(request.AxisXData, ChosenMOSFETID[i]), MOSFETs[ChosenMOSFETID[i]].DrainCurrent);
                    SimulationData2D.Add(DataPoint);
                }
            }
            else if (request.AxisYData == 4)
            {
                for (int i = 0; i < ChosenMOSFETID.Count; i++)
                {
                    for(int j=0;j< MOSFETs[ChosenMOSFETID[i]].SourceTransmissionCoefficient.Count;j++)
                    {
                        DoubleVector2 DataPoint = new DoubleVector2(MOSFETs[ChosenMOSFETID[i]].SourceEnergyStates[j], MOSFETs[ChosenMOSFETID[i]].SourceTransmissionCoefficient[j]);
                        SimulationData2D.Add(DataPoint);
                    }
                    
                }
            }
            else if (request.AxisYData == 5)
            {
                for (int i = 0; i < ChosenMOSFETID.Count; i++)
                {
                    for (int j = 0; j < MOSFETs[ChosenMOSFETID[i]].SourcePotentialBarrier.Count; j++)
                    {
                        DoubleVector2 DataPoint = new DoubleVector2(MOSFETs[ChosenMOSFETID[i]].SourcePotentialBarrierPosition[j], MOSFETs[ChosenMOSFETID[i]].SourcePotentialBarrier[j]);
                        SimulationData2D.Add(DataPoint);
                    }

                }
            }
            else if (request.AxisYData == 6)
            {
                for (int i = 0; i < ChosenMOSFETID.Count; i++)
                {
                    for (int j = 0; j < MOSFETs[ChosenMOSFETID[i]].SourceDensityOfStates.Count; j++)
                    {
                        DoubleVector2 DataPoint = new DoubleVector2(MOSFETs[ChosenMOSFETID[i]].SourceEnergyStates[j], MOSFETs[ChosenMOSFETID[i]].SourceDensityOfStates[j]);
                        SimulationData2D.Add(DataPoint);
                    }

                }
            }
            else if (request.AxisYData == 7)
            {
                for (int i = 0; i < ChosenMOSFETID.Count; i++)
                {
                    for (int j = 0; j < MOSFETs[ChosenMOSFETID[i]].SourceElectronDistribution.Count; j++)
                    {
                        DoubleVector2 DataPoint = new DoubleVector2(MOSFETs[ChosenMOSFETID[i]].SourceEnergyStates[j], MOSFETs[ChosenMOSFETID[i]].SourceElectronDistribution[j]);
                        SimulationData2D.Add(DataPoint);
                    }

                }
            }
            else if (request.AxisYData == 8)
            {
                for (int i = 0; i < ChosenMOSFETID.Count; i++)
                {
                    for (int j = 0; j < MOSFETs[ChosenMOSFETID[i]].DrainDensityOfStates.Count; j++)
                    {
                        DoubleVector2 DataPoint = new DoubleVector2(MOSFETs[ChosenMOSFETID[i]].DrainEnergyStates[j], MOSFETs[ChosenMOSFETID[i]].DrainDensityOfStates[j]);
                        SimulationData2D.Add(DataPoint);
                    }

                }
            }
            else if (request.AxisYData == 9)
            {
                for (int i = 0; i < ChosenMOSFETID.Count; i++)
                {
                    for (int j = 0; j < MOSFETs[ChosenMOSFETID[i]].DrainElectronDistribution.Count; j++)
                    {
                        DoubleVector2 DataPoint = new DoubleVector2(MOSFETs[ChosenMOSFETID[i]].DrainEnergyStates[j], MOSFETs[ChosenMOSFETID[i]].DrainElectronDistribution[j]);
                        SimulationData2D.Add(DataPoint);
                    }

                }
            }


        }
        else
        {
            SimulationData3D.Clear();
            if (request.AxisYData == 1)
            {
                for (int i = 0; i < ChosenMOSFETID.Count; i++)
                {
                    DoubleVector3 DataPoint = new DoubleVector3(GetAxisXZValue(request.AxisXData, ChosenMOSFETID[i]), MOSFETs[ChosenMOSFETID[i]].LeakageCurrent, GetAxisXZValue(request.AxisZData, ChosenMOSFETID[i]));
                    SimulationData3D.Add(DataPoint);
                }
            }
            else if (request.AxisYData == 2)
            {
                for (int i = 0; i < ChosenMOSFETID.Count; i++)
                {
                    DoubleVector3 DataPoint = new DoubleVector3(GetAxisXZValue(request.AxisXData, ChosenMOSFETID[i]), MOSFETs[ChosenMOSFETID[i]].SourceCurrent, GetAxisXZValue(request.AxisZData, ChosenMOSFETID[i]));
                    SimulationData3D.Add(DataPoint);
                }
            }
            else if (request.AxisYData == 3)
            {
                for (int i = 0; i < ChosenMOSFETID.Count; i++)
                {
                    DoubleVector3 DataPoint = new DoubleVector3(GetAxisXZValue(request.AxisXData, ChosenMOSFETID[i]), MOSFETs[ChosenMOSFETID[i]].DrainCurrent, GetAxisXZValue(request.AxisZData, ChosenMOSFETID[i]));
                    SimulationData3D.Add(DataPoint);
                }
            }
            else if (request.AxisYData == 4)
            {
                for (int i = 0; i < ChosenMOSFETID.Count; i++)
                {
                    for (int j = 0; j < MOSFETs[ChosenMOSFETID[i]].SourceTransmissionCoefficient.Count; j++)
                    {
                        DoubleVector3 DataPoint = new DoubleVector3(MOSFETs[ChosenMOSFETID[i]].SourceEnergyStates[j], MOSFETs[ChosenMOSFETID[i]].SourceTransmissionCoefficient[j], GetAxisXZValue(request.AxisZData, ChosenMOSFETID[i]));
                        SimulationData3D.Add(DataPoint);
                    }

                }
            }
            else if (request.AxisYData == 5)
            {
                for (int i = 0; i < ChosenMOSFETID.Count; i++)
                {
                    for (int j = 0; j < MOSFETs[ChosenMOSFETID[i]].SourcePotentialBarrier.Count; j++)
                    {
                        DoubleVector3 DataPoint = new DoubleVector3(MOSFETs[ChosenMOSFETID[i]].SourcePotentialBarrierPosition[j], MOSFETs[ChosenMOSFETID[i]].SourcePotentialBarrier[j], GetAxisXZValue(request.AxisZData, ChosenMOSFETID[i]));
                        SimulationData3D.Add(DataPoint);
                    }

                }
            }
            else if (request.AxisYData == 6)
            {
                for (int i = 0; i < ChosenMOSFETID.Count; i++)
                {
                    for (int j = 0; j < MOSFETs[ChosenMOSFETID[i]].SourceDensityOfStates.Count; j++)
                    {
                        DoubleVector3 DataPoint = new DoubleVector3(MOSFETs[ChosenMOSFETID[i]].SourceEnergyStates[j], MOSFETs[ChosenMOSFETID[i]].SourceDensityOfStates[j], GetAxisXZValue(request.AxisZData, ChosenMOSFETID[i]));
                        SimulationData3D.Add(DataPoint);
                    }

                }
            }
            else if (request.AxisYData == 7)
            {
                for (int i = 0; i < ChosenMOSFETID.Count; i++)
                {
                    for (int j = 0; j < MOSFETs[ChosenMOSFETID[i]].SourceElectronDistribution.Count; j++)
                    {
                        DoubleVector3 DataPoint = new DoubleVector3(MOSFETs[ChosenMOSFETID[i]].SourceEnergyStates[j], MOSFETs[ChosenMOSFETID[i]].SourceElectronDistribution[j], GetAxisXZValue(request.AxisZData, ChosenMOSFETID[i]));
                        SimulationData3D.Add(DataPoint);
                    }

                }
            }
            else if (request.AxisYData == 8)
            {
                for (int i = 0; i < ChosenMOSFETID.Count; i++)
                {
                    for (int j = 0; j < MOSFETs[ChosenMOSFETID[i]].DrainDensityOfStates.Count; j++)
                    {
                        DoubleVector3 DataPoint = new DoubleVector3(MOSFETs[ChosenMOSFETID[i]].DrainEnergyStates[j], MOSFETs[ChosenMOSFETID[i]].DrainDensityOfStates[j], GetAxisXZValue(request.AxisZData, ChosenMOSFETID[i]));
                        SimulationData3D.Add(DataPoint);
                    }

                }
            }
            else if (request.AxisYData == 9)
            {
                for (int i = 0; i < ChosenMOSFETID.Count; i++)
                {
                    for (int j = 0; j < MOSFETs[ChosenMOSFETID[i]].DrainElectronDistribution.Count; j++)
                    {
                        DoubleVector3 DataPoint = new DoubleVector3(MOSFETs[ChosenMOSFETID[i]].DrainEnergyStates[j], MOSFETs[ChosenMOSFETID[i]].DrainElectronDistribution[j], GetAxisXZValue(request.AxisZData, ChosenMOSFETID[i]));
                        SimulationData3D.Add(DataPoint);
                    }

                }
            }
        }


    }

    private static bool CheckingMOSFET(ref DisplayRequest request,int ID)
    {
        for(int i=0;i<request.OtherParameters.Count;i++)
        {
            if(request.OtherParameters[i].x == 1 && MOSFETs[ID].ID_V_DS != request.OtherParameters[i].y)
            {
                return false;
            }
            if (request.OtherParameters[i].x == 2 && MOSFETs[ID].ID_V_G != request.OtherParameters[i].y)
            {
                return false;
            }
            if (request.OtherParameters[i].x == 3 && MOSFETs[ID].ID_L != request.OtherParameters[i].y)
            {
                return false;
            }
            if (request.OtherParameters[i].x == 4 && MOSFETs[ID].ID_T != request.OtherParameters[i].y)
            {
                return false;
            }
        }

        return true;
    }

    private static double GetAxisXZValue(int DataType, int id)
    {
        if (DataType == 1) return MOSFETs[id].SourceParameter["V_DS"];
        if (DataType == 2) return MOSFETs[id].SourceParameter["V_G"];
        if (DataType == 3) return MOSFETs[id].SourceParameter["L"];
        if (DataType == 4) return MOSFETs[id].SourceParameter["T"];

        return 0.0;
    }
    public static void SetUnit(ref DisplayRequest request)
    {
        str_AxisX = GetXZAxisLabel(request.AxisXData);
        str_AxisZ = GetXZAxisLabel(request.AxisZData);
        str_AxisY = GetYAxisLabel(request.AxisYData);
        str_Title = GetYAxisName(request.AxisYData);
        if(request.AxisZData == 0)
        {
            if(EnergyUsingEV)
            {
                if(request.AxisYData == 5 || request.AxisYData == 6 || request.AxisYData == 8)
                {
                    for (int i = 0; i < SimulationData2D.Count; i++)
                    {
                        SimulationData2D[i].y /= ELECTRON_CHARGE;
                    }
                }
                if(request.AxisXData == 6)
                {
                    for (int i = 0; i < SimulationData2D.Count; i++)
                    {
                        SimulationData2D[i].x /= ELECTRON_CHARGE;
                    }
                }
            }
            AxisXMin = SimulationData2D[0].x;
            AxisXMax = SimulationData2D[0].x;
            AxisYMin = SimulationData2D[0].y;
            AxisYMax = SimulationData2D[0].y;
            for (int i=1;i<SimulationData2D.Count;i++)
            {
                AxisXMin = AxisXMin < SimulationData2D[i].x ? AxisXMin : SimulationData2D[i].x;
                AxisXMax = AxisXMax > SimulationData2D[i].x ? AxisXMax : SimulationData2D[i].x;
                AxisYMin = AxisYMin < SimulationData2D[i].y ? AxisYMin : SimulationData2D[i].y;
                AxisYMax = AxisYMax > SimulationData2D[i].y ? AxisYMax : SimulationData2D[i].y;
            }
            if (request.AxisYData == 4 || request.AxisYData == 7 || request.AxisYData == 9)
            {
                AxisYMin = 0;
                AxisYMax = 1;
            }

            DisplayData2D.Clear();
            for(int i=0;i< SimulationData2D.Count; i++)
            {
                DisplayData2D.Add(Scale2D(SimulationData2D[i]));
            }
            DisplayData2D.Sort((a, b) =>
            {
                if (a.x != b.x) return a.x.CompareTo(b.x);
                return a.y.CompareTo(b.y);
            });
            DisplayMesh.Clear();
            DisplayMesh.Add(new List<Vector3>());
            DisplayMesh.Add(new List<Vector3>());
            for (int i = 0; i < DisplayData2D.Count; i++)
            {
                DisplayMesh[0].Add(new Vector3(DisplayData2D[i].x, DisplayData2D[i].y, 0.0f));
                DisplayMesh[1].Add(new Vector3(DisplayData2D[i].x,DisplayData2D[i].y,0.5f));
            }
            CurrentDisplay = 2;
        }
        else
        {
            if (EnergyUsingEV)
            {
                if (request.AxisYData == 5 || request.AxisYData == 6 || request.AxisYData == 8)
                {
                    for (int i = 0; i < SimulationData3D.Count; i++)
                    {
                        SimulationData3D[i].y /= ELECTRON_CHARGE;
                    }
                }
                if (request.AxisXData == 6)
                {
                    for (int i = 0; i < SimulationData3D.Count; i++)
                    {
                        SimulationData3D[i].x /= ELECTRON_CHARGE;
                    }
                }
                if (request.AxisZData == 6)
                {
                    for (int i = 0; i < SimulationData3D.Count; i++)
                    {
                        SimulationData3D[i].z /= ELECTRON_CHARGE;
                    }
                }
            }
            
            AxisXMin = SimulationData3D[0].x;
            AxisXMax = SimulationData3D[0].x;
            AxisYMin = SimulationData3D[0].y;
            AxisYMax = SimulationData3D[0].y;
            AxisZMin = SimulationData3D[0].z;
            AxisZMax = SimulationData3D[0].z;
            for (int i = 1; i < SimulationData3D.Count; i++)
            {
                AxisXMin = AxisXMin < SimulationData3D[i].x ? AxisXMin : SimulationData3D[i].x;
                AxisXMax = AxisXMax > SimulationData3D[i].x ? AxisXMax : SimulationData3D[i].x;
                AxisYMin = AxisYMin < SimulationData3D[i].y ? AxisYMin : SimulationData3D[i].y;
                AxisYMax = AxisYMax > SimulationData3D[i].y ? AxisYMax : SimulationData3D[i].y;
                AxisZMin = AxisZMin < SimulationData3D[i].z ? AxisZMin : SimulationData3D[i].z;
                AxisZMax = AxisZMax > SimulationData3D[i].z ? AxisZMax : SimulationData3D[i].z;
            }
            if(request.AxisYData == 4 || request.AxisYData == 7 || request.AxisYData == 9)
            {
                AxisYMin = 0;
                AxisYMax = 1;
            }
            DisplayData3D.Clear();
            for (int i = 0; i < SimulationData3D.Count; i++)
            {
                DisplayData3D.Add(Scale3D(SimulationData3D[i]));
            }
            DisplayData3D.Sort((a, b) =>
            {
                if (a.z != b.z) return a.z.CompareTo(b.z);
                if (a.x != b.x) return a.x.CompareTo(b.x);
                return a.y.CompareTo(b.y);
            });
            DisplayMesh.Clear();
            DisplayMesh.Add(new List<Vector3>());
            float Pre_Z = DisplayData3D[0].z;
            int ListID = 0;
            for (int i = 0; i < DisplayData3D.Count; i++)
            {
                if (DisplayData3D[i].z != Pre_Z)
                {
                    if(DisplayMesh[ListID].Count<2)
                    {
                        DisplayMesh[ListID].Add(new Vector3(DisplayMesh[ListID][0].x, DisplayMesh[ListID][0].y, DisplayMesh[ListID][0].z));
                    }
                    DisplayMesh.Add(new List<Vector3>());
                    ListID++;
                    Pre_Z = DisplayData3D[i].z;
                }
                DisplayMesh[ListID].Add(DisplayData3D[i]);
            }
            if(DisplayMesh.Count<2)
            {
                DisplayMesh.Add(DisplayMesh[0]);
            }
            CurrentDisplay = 3;
        }

    }
    private static string GetXZAxisLabel(int id)
    {

        if(id == 1) return "V_DS (V)";
        if(id == 2) return "V_G (V)";
        if (id == 3) return "L (m)";
        if (id == 4) return "T (K)";
        if (id == 5) return "x (m)";
        if (id == 6 && !EnergyUsingEV) return "E (J)";
        if (id == 6 && EnergyUsingEV) return "E (eV)";
        return "";
    }
    private static string GetYAxisLabel(int id)
    {
        if (id == 1 || id ==2 || id ==3) return "I (A)";
        if (id == 4) return "Probability";
        if ((id == 5 || id == 6|| id ==8) && !EnergyUsingEV) return "E (J)";
        if (id == 7 || id == 9) return "Probability";
        if ((id == 5 || id == 6 || id == 8) && !EnergyUsingEV) return "E (eV)";
        return "";
    }
    private static string GetYAxisName(int id)
    {
        if (id == 1) return "LeakageCurrent";
        if (id == 2) return "SourceToDrainCurrent";
        if (id == 3) return "DrainToSourceCurrent";
        if (id == 4) return "Transmission Coefficient";
        if (id == 5) return "Energy Band Diagram";
        if (id == 6) return "Density Of States (Source)";
        if (id == 7) return "Electron Distribution (Source)";
        if (id == 8) return "Density Of States (Drain)";
        if (id == 9) return "Electron Distribution (Drain)";
        return "";
    }
    private static Vector2 Scale2D(DoubleVector2 Vector)
    {
        float x = (float)((Vector.x - AxisXMin) / (AxisXMax - AxisXMin));
        float y = (float)((Vector.y - AxisYMin) / (AxisYMax - AxisYMin));
        return new Vector2(x, y);
    }
    private static Vector3 Scale3D(DoubleVector3 Vector)
    {
        float x = (float)((Vector.x - AxisXMin) / (AxisXMax - AxisXMin));
        float y = (float)((Vector.y - AxisYMin) / (AxisYMax - AxisYMin));
        float z = (float)((Vector.z - AxisZMin) / (AxisZMax - AxisZMin));
        return new Vector3(x, y,z);
    }
    public static void ShowProgress1()
    {
        Thread thread1 = new Thread(ShowProgress);
        thread1.Start();
    }
    public static void ShowProgress()
    {
        
        taskDialog1.ProgressBar = new TaskDialogProgressBar();
        taskDialogProgressBar1 = taskDialog1.ProgressBar;
        taskDialog1.Caption = "Simulator";
        taskDialog1.Text = "Reading Simulation Data";
        taskDialogProgressBar1.Maximum = Simulation.SimulationNumber;
        taskDialogProgressBar1.Minimum = 0;
        taskDialogProgressBar1.Value = 0;
        taskDialog1.Show();
        return;
    }
    public static void SetProgress(int value)
    {
        taskDialogProgressBar1.Value = value;
    }
    public static void CloseProgress()
    {
        
        taskDialog1.Close();

    }
}
