using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using UnityEngine.SceneManagement;
using System.Threading;


public class PlotterUI : MonoBehaviour
{
    public Dropdown DropDownX;
    public Dropdown DropDownY;
    public Dropdown DropDownZ;

    public GameObject Prefab_Slider;
    public GameObject DisplayButton;
    public GameObject BackButton;
    public GameObject AxisXLabel;
    public GameObject AxisYLabel;
    public GameObject AxisZLabel;
    public GameObject SimulationPathInputBox;
    public GameObject LoadButton;
    public GameObject TitleText;

    private string json_path;
    private List<GameObject> slider_list;
    private static List<string> static_options = new List<string>() { "V_DS", "V_G", "L", "T" };
    private static List<string> xz_axis = new List<string> { "Please Select", "V_DS", "V_G", "L", "T", "Position", "Energy States" };
    private static List<string> y_axis = new List<string> { "", "LeakageCurrent", "SourceCurrent", "DrainCurrent", "TransmissionCoefficient",
    "Potential Barrier", "SourceDensityofStates", "SourceElectronDistribution", "DrainDensityofStates", "DrainElectronDistribution"};

    /* a global variable to store model list */
    public static JObject mosfet_json;
    private GameObject Surface;
    public GameObject Prefab_Point;
    private List<GameObject> Points = new List<GameObject>();
    public GameObject PlotBox;
    public GameObject Prefab_TextX;
    public GameObject Prefab_TextY;
    public GameObject Prefab_TextZ;
    private List<GameObject> AxisXTextList = new List<GameObject>();
    private List<GameObject> AxisYTextList = new List<GameObject>();
    private List<GameObject> AxisZTextList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        slider_list = new List<GameObject>();
        //load_data();
        BackButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { SceneManager.LoadScene(0); });
        LoadButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
        {
            OpenFolderDialogButton_onClick(SimulationPathInputBox);
        });

        DisplayButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(plot);

        DropDownY.GetComponent<Dropdown>().onValueChanged.AddListener(option_y_changed);
        DropDownX.GetComponent<Dropdown>().onValueChanged.AddListener(option_x_changed);
        DropDownZ.GetComponent<Dropdown>().onValueChanged.AddListener(option_z_changed);


        for (int i = 0; i <= 10; i++)
        {
            GameObject TextMeshX = GameObject.Instantiate(Prefab_TextX);
            GameObject TextMeshY = GameObject.Instantiate(Prefab_TextY);
            GameObject TextMeshZ = GameObject.Instantiate(Prefab_TextZ);
            TextMeshX.transform.SetParent(PlotBox.transform);
            TextMeshY.transform.SetParent(PlotBox.transform);
            TextMeshZ.transform.SetParent(PlotBox.transform);
            TextMeshX.transform.localPosition = new Vector3(i * 0.1f - 0.5f, -0.55f, -0.5f);
            TextMeshY.transform.localPosition = new Vector3(-0.55f, i * 0.1f - 0.5f, -0.5f);
            TextMeshZ.transform.localPosition = new Vector3(-0.5f, -0.55f, i * 0.1f - 0.5f);
            TextMeshX.GetComponent<TMP_Text>().text = (i / 10f).ToString();
            TextMeshY.GetComponent<TMP_Text>().text = (i / 10f).ToString();
            TextMeshZ.GetComponent<TMP_Text>().text = (i / 10f).ToString();
            AxisXTextList.Add(TextMeshX);
            AxisYTextList.Add(TextMeshY);
            AxisZTextList.Add(TextMeshZ);

        }
    }

    void option_x_changed(int index)
    {
        int value_x = DropDownX.GetComponent<Dropdown>().value;
        int value_z = DropDownZ.GetComponent<Dropdown>().value;

        string content_x = DropDownX.GetComponent<Dropdown>().options[value_x].text;
        string content_z = DropDownZ.GetComponent<Dropdown>().options[value_z].text;

        List<string> temp = new List<string> { "V_DS", "V_G", "L", "T" };
        temp.Remove(content_z);
        temp.Remove(content_x);

        generate_slider(temp);

        List<string> example = new List<string>() { "Please Select", "V_DS", "V_G", "L", "T", "Position", "Energy States" };
        int y_index = DropDownY.GetComponent<Dropdown>().value;
        List<string> z_options = new List<string>();

        if (y_index > 0 && y_index < 4)
        {
            z_options.Add(example[0]);

            for (int i = 1; i <= 4; i++)
            {
                string parameter_name = example[i];

                if ((int)DataProcess.Simulation.BasicParameters[parameter_name + "_Number"] > 1)
                {
                    z_options.Add(example[i]);
                }
            }
        }
        else if (index >= 4 && index <= 9)
        {
            for (int i = 0; i <= 4; i++)
            {
                string parameter_name = example[i];

                if ((int)DataProcess.Simulation.BasicParameters[parameter_name + "_Number"] > 1 && i > 0)
                {
                    z_options.Add(example[i]);
                }
                else if (i == 0)
                {
                    z_options.Add(example[i]);
                }
            }
        }

        z_options.Remove(content_x);

        DropDownZ.GetComponent<Dropdown>().ClearOptions();
        DropDownZ.GetComponent<Dropdown>().AddOptions(z_options);

        value_x = DropDownX.GetComponent<Dropdown>().value;
        value_z = DropDownZ.GetComponent<Dropdown>().value;

        content_x = DropDownX.GetComponent<Dropdown>().options[value_x].text;
        content_z = DropDownZ.GetComponent<Dropdown>().options[value_z].text;

        temp = new List<string> { "V_DS", "V_G", "L", "T" }; ;
        temp.Remove(content_z);
        temp.Remove(content_x);

        generate_slider(temp);
    }

    void option_z_changed(int index)
    {
        int value_x = DropDownX.GetComponent<Dropdown>().value;
        int value_z = DropDownZ.GetComponent<Dropdown>().value;

        string content_x = DropDownX.GetComponent<Dropdown>().options[value_x].text;
        string content_z = DropDownZ.GetComponent<Dropdown>().options[value_z].text;

        List<string> temp = new List<string> { "V_DS", "V_G", "L", "T" };
        temp.Remove(content_z);
        temp.Remove(content_x);

        generate_slider(temp);

        List<string> example = new List<string>() { "Please Select", "V_DS", "V_G", "L", "T", "Position", "Energy States" };
        List<string> x_options = new List<string>();
        int y_index = DropDownY.GetComponent<Dropdown>().value;

        if (y_index > 0 && y_index < 4)
        {
            for (int i = 1; i <= 4; i++)
            {
                string parameter_name = example[i];

                if ((int)DataProcess.Simulation.BasicParameters[parameter_name + "_Number"] > 1)
                {
                    x_options.Add(example[i]);
                }
            }
        }
        else if (y_index >= 4 && y_index <= 9)
        {
            if (index == 5)
            {
                x_options.Add(example[5]);
            }
            else
            {
                x_options.Add(example[6]);
            }
        }

        x_options.Remove(content_z);

        DropDownX.GetComponent<Dropdown>().ClearOptions();
        DropDownX.GetComponent<Dropdown>().AddOptions(x_options);

        value_x = DropDownX.GetComponent<Dropdown>().value;
        value_z = DropDownZ.GetComponent<Dropdown>().value;

        content_x = DropDownX.GetComponent<Dropdown>().options[value_x].text;
        content_z = DropDownZ.GetComponent<Dropdown>().options[value_z].text;

        temp = new List<string> { "V_DS", "V_G", "L", "T" }; ;
        temp.Remove(content_z);
        temp.Remove(content_x);

        generate_slider(temp);
    }

    void option_y_changed(int index)
    {
        Debug.Log(index);

        List<string> example = new List<string>() { "Please Select", "V_DS", "V_G", "L", "T", "Position", "Energy States" };
        List<string> x_options = new List<string>();
        List<string> z_options = new List<string>();

        if (index > 0 && index < 4)
        {
            z_options.Add(example[0]);

            for (int i = 1; i <= 4; i++)
            {
                string parameter_name = example[i];

                if ((int)DataProcess.Simulation.BasicParameters[parameter_name + "_Number"] > 1)
                {
                    x_options.Add(example[i]);
                    z_options.Add(example[i]);
                }
            }
        }
        else if (index >= 4 && index <= 9)
        {
            if (index == 5)
            {
                x_options.Add(example[5]);
            }
            else
            {
                x_options.Add(example[6]);
            }

            for (int i = 0; i <= 4; i++)
            {
                string parameter_name = example[i];

                if (i > 0 && (int)DataProcess.Simulation.BasicParameters[parameter_name + "_Number"] > 1)
                {
                    z_options.Add(example[i]);
                }
                else if (i == 0)
                {
                    z_options.Add(example[i]);
                }
            }
        }

        DropDownX.GetComponent<Dropdown>().ClearOptions();
        DropDownX.GetComponent<Dropdown>().AddOptions(x_options);
        DropDownZ.GetComponent<Dropdown>().ClearOptions();
        DropDownZ.GetComponent<Dropdown>().AddOptions(z_options);

        int value_x = DropDownX.GetComponent<Dropdown>().value;
        int value_z = DropDownZ.GetComponent<Dropdown>().value;

        string content_x = DropDownX.GetComponent<Dropdown>().options[value_x].text;
        string content_z = DropDownZ.GetComponent<Dropdown>().options[value_z].text;

        List<string> temp = new List<string> { "V_DS", "V_G", "L", "T" }; ;
        temp.Remove(content_z);
        temp.Remove(content_x);

        generate_slider(temp);
    }

    void OpenFolderDialogButton_onClick(GameObject InputBox)
    {
        json_path = FolderBrowserHelper.GetPathFromWindowsExplorer();

        InputBox.GetComponent<TMP_InputField>().text = json_path;
        Debug.Log(InputBox.GetComponent<TMP_InputField>().text);

        DataProcess.SimulationDataPath = json_path;


        bool Success = DataProcess.ReadSimulationData();



        if (Success)
        {
            Debug.Log("settings loading successfully");
            TaskDialog.Show(null, "Simulation Data Load Success", "Simulator");
        }
        else
        {
            Debug.Log(DataProcess.ErrorMessage);
        }
    }

    void generate_slider(List<string> temp_options)
    {
        foreach (GameObject item in slider_list)
        {
            Destroy(item);
        }

        slider_list.Clear();

        int count = 0;

        foreach (string option in temp_options)
        {
            count++;

            GameObject temp_slider = GameObject.Instantiate(Prefab_Slider);
            temp_slider.name = option;
            temp_slider.GetComponent<TMP_Text>().text = option;

            IDictionary<string, double> basic_parameter = DataProcess.Simulation.BasicParameters;
            string number_key = option + "_Number";
            Debug.Log(number_key);
            int number = (int)DataProcess.Simulation.BasicParameters[number_key];
            if (option == "V_DS")
            {
                Debug.Log(number_key + " " + DataProcess.Simulation.BasicParameters["V_DS_Number"].ToString());
            }

            string unit = "";

            if (option.Contains("L"))
            {
                unit = "nm";
            }
            else if (option.Contains("V"))
            {
                unit = "V";
            }
            else
            {
                unit = "K";
            }

            temp_slider.transform.Find("ValueUnit").GetComponent<TMP_Text>().text = "0" + unit;

            temp_slider.transform.SetParent(transform);

            if (number == 1)
            {
                temp_slider.transform.Find("Slider").GetComponent<Slider>().maxValue = 0;
                temp_slider.transform.Find("Slider").GetComponent<Slider>().minValue = 0;
            }
            else
            {
                temp_slider.transform.Find("Slider").GetComponent<Slider>().maxValue = number - 1;
                temp_slider.transform.Find("Slider").GetComponent<Slider>().minValue = 0;
            }

            temp_slider.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(200, count * -50, 0);
            slider_value(temp_slider, unit);
            temp_slider.transform.Find("Slider").GetComponent<Slider>().onValueChanged.AddListener(delegate
            {
                slider_value(temp_slider, unit);
            });

            slider_list.Add(temp_slider);
        }
    }

    public double calculate(double min, double max, int number, int id)
    {
        return number < 2 ? min : min + id * (max - min) / (number - 1);
    }

    public void slider_value(GameObject temp_object, string unit)
    {
        string KeyMax = temp_object.name + "_Max";
        string KeyMin = temp_object.name + "_Min";
        string KeyNumber = temp_object.name + "_Number";

        double MaxValue = DataProcess.Simulation.BasicParameters[KeyMax];
        double MinValue = DataProcess.Simulation.BasicParameters[KeyMin];

        int Number = (int)DataProcess.Simulation.BasicParameters[KeyNumber];
        int id = (int)temp_object.transform.Find("Slider").GetComponent<Slider>().value;
        double value = calculate(MinValue, MaxValue, Number, id);

        if (temp_object.name.Contains("L"))
        {
            temp_object.transform.Find("ValueUnit").GetComponent<TMP_Text>().text = (Mathf.Pow(10, 9) * value).ToString("f3") + unit;
        }
        else
        {
            temp_object.transform.Find("ValueUnit").GetComponent<TMP_Text>().text = value.ToString("f3") + unit;
        }
    }

    /*public void load_data()
    {
        StreamReader reader = File.OpenText(json_path);
        JsonTextReader jsonTextReader = new JsonTextReader(reader);
        mosfet_json = (JObject)JToken.ReadFrom(jsonTextReader);

        Debug.Log("Model loaded");

        JObject basic_parameter_json = (JObject)mosfet_json["BasicParameter"];
        JObject drain_parameter_json = (JObject)mosfet_json["DrainParameter"];
        JObject source_parameter_json = (JObject)mosfet_json["SourceParameter"];
        string current_unit = mosfet_json["CurrentUnit"].ToString();
        string energy_unit = mosfet_json["EnergyUnit"].ToString();
        string model_name = mosfet_json["ModelName"].ToString();
        string simulation_number = mosfet_json["SimulationNumber"].ToString();

        /* find a text to display the information of the model */
    /*string loaded_model_description = "Name: " + model_name + "\n Simulation Number: " + simulation_number + "\n Current Unit: " + current_unit + "\n Energy Unit: " + energy_unit; ;
    //loaded_model.GetComponent<Text>().text = load_model_description;
}*/

    // Update is called once per frame
    void Update()
    {

    }

    public void plot()
    {

        int value_x = DropDownX.GetComponent<Dropdown>().value;
        int value_y = DropDownY.GetComponent<Dropdown>().value;
        int value_z = DropDownZ.GetComponent<Dropdown>().value;

        string content_x = DropDownX.GetComponent<Dropdown>().options[value_x].text;
        string content_y = DropDownY.GetComponent<Dropdown>().options[value_y].text;
        string content_z = DropDownZ.GetComponent<Dropdown>().options[value_z].text;

        DataProcess.Request.AxisXData = xz_axis.IndexOf(content_x);
        DataProcess.Request.AxisYData = y_axis.IndexOf(content_y);
        DataProcess.Request.AxisZData = xz_axis.IndexOf(content_z);

        List<DoubleVector2> temp_list = new List<DoubleVector2>();
        foreach (GameObject temp_object in slider_list)
        {
            string KeyMax = temp_object.name + "_Max";
            string KeyMin = temp_object.name + "_Min";
            string KeyNumber = temp_object.name + "_Number";

            double MaxValue = DataProcess.Simulation.BasicParameters[KeyMax];
            double MinValue = DataProcess.Simulation.BasicParameters[KeyMin];

            int Number = (int)DataProcess.Simulation.BasicParameters[KeyNumber];
            int id = (int)temp_object.transform.Find("Slider").GetComponent<Slider>().value;
            double value = calculate(MinValue, MaxValue, Number, id);

            temp_list.Add(new DoubleVector2(xz_axis.IndexOf(temp_object.name), value));
        }
        DataProcess.Request.OtherParameters = new List<Vector2Int>();

        foreach (GameObject temp_object in slider_list)
        {
            string parameter_name = temp_object.name;
            int parameter_index = xz_axis.IndexOf(parameter_name);
            string KeyMax = temp_object.name + "_Max";
            string KeyMin = temp_object.name + "_Min";
            string KeyNumber = temp_object.name + "_Number";

            double MaxValue = DataProcess.Simulation.BasicParameters[KeyMax];
            double MinValue = DataProcess.Simulation.BasicParameters[KeyMin];

            int Number = (int)DataProcess.Simulation.BasicParameters[KeyNumber];
            int id = (int)temp_object.transform.Find("Slider").GetComponent<Slider>().value + 1;

            DataProcess.Request.OtherParameters.Add(new Vector2Int(parameter_index, id));
        }

        Debug.Log("X:" + DataProcess.Request.AxisXData.ToString() + "   Y:" + DataProcess.Request.AxisYData.ToString() + "   Z:" + DataProcess.Request.AxisZData.ToString());
        for(int i=0;i<DataProcess.Request.OtherParameters.Count;i++)
        {
            Debug.Log("ID:" + DataProcess.Request.OtherParameters[i].x + "  Value:" + DataProcess.Request.OtherParameters[i].y);
        }
        DataProcess.SearchSimulationData(ref DataProcess.Request);
        DataProcess.SetUnit(ref DataProcess.Request);


        //DrawAllPoints();
        Debug.Log("ButtonClick");
        Vector3 Center = new Vector3(0, 0, 0);
        //List<List<Vector3>> MeshVertices = new List<List<Vector3>>();
        Material material = new Material(Shader.Find("Diffuse"));
        Destroy(Surface);
        Surface = new GameObject();
        Surface.transform.SetParent(PlotBox.transform);
        Surface.AddComponent<MeshFilter>();
        Surface.AddComponent<MeshRenderer>();

        /*for (int i = 0; i < 5; i++)
        {
            MeshVertices.Add(new List<Vector3>());
            for (int j = 0; j < 10; j++)
            {
                MeshVertices[i].Add(new Vector3(j / 10f, j * j / 100f, i / 5f));
            }
        }*/
        MeshControl.SetMesh(ref Surface, PlotBox.transform, Center, ref DataProcess.DisplayMesh, material);
        Surface.transform.localPosition = new Vector3(-0.5f, -0.5f, -0.5f);
        Surface.transform.localScale = new Vector3(1, 1, 1);
        Surface.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        //TaskDialog.Show("内容", "标题", "窗体标题");
        SetAxis();
        /* start plotting */
    }
    public void DrawAllPoints()
    {
        for (int i = 0; i < Points.Count; i++)
        {
            Destroy(Points[i]);
        }
        Points.Clear();
        if (DataProcess.CurrentDisplay == 2)
        {
            for (int i = 0; i < DataProcess.DisplayData2D.Count; i++)
            {
                GameObject newPoint = GameObject.Instantiate(Prefab_Point);
                newPoint.transform.SetParent(PlotBox.transform);
                newPoint.transform.localPosition = new Vector3(DataProcess.DisplayData2D[i].x-0.5f, DataProcess.DisplayData2D[i].y -0.5f, -0.5f);
                newPoint.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                Points.Add(newPoint);
            }
        }
        else if (DataProcess.CurrentDisplay == 3)
        {
            for (int i = 0; i < DataProcess.DisplayData3D.Count; i++)
            {
                GameObject newPoint = GameObject.Instantiate(Prefab_Point);
                newPoint.transform.SetParent(PlotBox.transform);
                newPoint.transform.localPosition = new Vector3(DataProcess.DisplayData3D[i].x - 0.5f, DataProcess.DisplayData3D[i].y - 0.5f, DataProcess.DisplayData3D[i].z - 0.5f);
                newPoint.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                Points.Add(newPoint);
            }
        }
    }
    public void SetAxis()
    {
        AxisXLabel.GetComponent<TMP_Text>().text = DataProcess.str_AxisX;
        AxisYLabel.GetComponent<TMP_Text>().text = DataProcess.str_AxisY;
        AxisZLabel.GetComponent<TMP_Text>().text = DataProcess.str_AxisZ;
        TitleText.GetComponent<TMP_Text>().text = DataProcess.str_Title;
        for (int i=0;i<=10;i++)
        {
            if(DataProcess.CurrentDisplay == 3)
            {
                AxisXTextList[i].GetComponent<TMP_Text>().text = (DataProcess.AxisXMin + i * (DataProcess.AxisXMax - DataProcess.AxisXMin)/10).ToString("G2");
                AxisYTextList[i].GetComponent<TMP_Text>().text = (DataProcess.AxisYMin + i * (DataProcess.AxisYMax - DataProcess.AxisYMin)/10).ToString("G2");
                AxisZTextList[i].GetComponent<TMP_Text>().text = (DataProcess.AxisZMin + i * (DataProcess.AxisZMax - DataProcess.AxisZMin)/10).ToString("G2");
            }
            if (DataProcess.CurrentDisplay == 2)
            {
                AxisXTextList[i].GetComponent<TMP_Text>().text = (DataProcess.AxisXMin + i * (DataProcess.AxisXMax - DataProcess.AxisXMin)/10).ToString("G2");
                AxisYTextList[i].GetComponent<TMP_Text>().text = (DataProcess.AxisYMin + i * (DataProcess.AxisYMax - DataProcess.AxisYMin)/10).ToString("G2");
                AxisZTextList[i].GetComponent<TMP_Text>().text = "";
            }
        }

    }

    
}
