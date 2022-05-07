using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Windows.Forms;
using UnityEngine.UI;
using TMPro;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using UnityEngine.SceneManagement;
using System.Threading;


public class CommanderUI : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject SimulationNameInputBox;
    public GameObject OpenFolderDialogButton;
    public GameObject SavePathInputBox;
    public GameObject ModelSelectionDropDown;
    public GameObject V_G_Min_InputBox;
    public GameObject V_G_Max_InputBox;
    public GameObject V_G_Number_InputBox;
    public GameObject V_G_Unit_Text;
    public GameObject V_DS_Min_InputBox;
    public GameObject V_DS_Max_InputBox;
    public GameObject V_DS_Number_InputBox;
    public GameObject V_DS_Unit_Text;
    public GameObject L_Min_InputBox;
    public GameObject L_Max_InputBox;
    public GameObject L_Number_InputBox;
    public GameObject L_Unit_Text;
    public GameObject T_Min_InputBox;
    public GameObject T_Max_InputBox;
    public GameObject T_Number_InputBox;
    public GameObject T_Unit_Text;
    public GameObject Prefab_Parameter;
    public GameObject ScrollViewContent;
    public GameObject simulate_button;
    public GameObject BackButton;

    /* the path of models.json, remember to modify */
    public static string model_json_path = "\"" + UnityEngine.Application.streamingAssetsPath +  ""+"\"";
    public static string model_json_path1 = UnityEngine.Application.streamingAssetsPath + "/";
    /* a global variable to store model list */
    public static JObject json_object;
    /* remember to modify */
    public static string request_path = "\"D:\\QQ\\FYP\\CalculationModule\\Calculator\\Calculator\\Request\\request.json\"";
    public static string request_pathl = "D:\\QQ\\FYP\\CalculationModule\\Calculator\\Calculator\\Request\\request.json";
    private int select_index = 0;
    private List<GameObject> lists;
    public static TaskDialog taskDialog1 = new TaskDialog();
    public static TaskDialogProgressBar taskDialogProgressBar1 = new TaskDialogProgressBar();
    public static int TotalSimulationNumber = 0;
    public static string ModelPathName;
    public static string Calculator_Path = UnityEngine.Application.streamingAssetsPath + "/Calculator/";
    void Start()
    {
        lists = new List<GameObject>();
        BackButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { SceneManager.LoadScene(0); });
        OpenFolderDialogButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
        {
            OpenFolderDialogButton_onClick(SavePathInputBox);
        });

        simulate_button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(simulate_data);

        Debug.Log("canvas created");
        /* calling calculator.exe to generate models.json */
        string CalculatorPath = Calculator_Path + "/Calculator.exe";
        string command ="-DisplayModelDescription " + model_json_path.Replace("/", "\\");
        Debug.Log(command);
        System.Diagnostics.Process.Start(CalculatorPath,command);

        /* read json */
        Thread.Sleep(1000);
        Debug.Log(model_json_path1 + "Models.json");
        StreamReader reader = File.OpenText(model_json_path1 + "Models.json");
        JsonTextReader jsonTextReader = new JsonTextReader(reader);
        json_object = (JObject)JToken.ReadFrom(jsonTextReader);

        Debug.Log("Json loaded");

        /* modify the dropdown */
        Dropdown temp_dropdown = ModelSelectionDropDown.GetComponent<Dropdown>();
        var model_list = json_object["Models"];
        List<string> options = new List<string>();

        foreach (JObject model in model_list)
        {
            options.Add(model["Name"].ToString());
        }

        temp_dropdown.AddOptions(options);
        temp_dropdown.onValueChanged.AddListener(model_selection_changed);

        Debug.Log("Dropdown options generated");
    }

    // Update is called once per frame
    void Update()
    {

    }

    void model_selection_changed(int index)
    {
        if (index == 0)
        {
            return;
        }

        select_index = index;

        if (lists.Count != 0)
        {
            for (int i = 0; i < lists.Count; i++)
            {
                Destroy(lists[i]);
            }

            lists.Clear();
        }

        Debug.Log("Model changed: " + index.ToString());
        var model_list = json_object["Models"];
        JObject selected_model = (JObject)model_list[index - 1];

        var source_parameter = selected_model["SourceParameter"];
        int count = 0;

        /* dealing with source parameter */
        foreach (JObject parameter in source_parameter)
        {
            count++;

            string name = parameter["Name"].ToString();
            string max = parameter["Max"].ToString();
            string min = parameter["Min"].ToString();
            string unit = parameter["Unit"].ToString();

            GameObject temp = GenerateModelParameterItem(name, double.Parse(min), double.Parse(max), unit, 300 - count * 40);
            lists.Add(temp);
        }
    }

    void simulate_data()
    {
        var model_list = json_object["Models"];
        JObject selected_model = (JObject)model_list[select_index - 1];
        JObject output = new JObject();

        string L_Max = L_Max_InputBox.GetComponent<TMP_InputField>().text;
        string L_Min = L_Min_InputBox.GetComponent<TMP_InputField>().text;
        string L_Number = L_Number_InputBox.GetComponent<TMP_InputField>().text;
        string T_Max = T_Max_InputBox.GetComponent<TMP_InputField>().text;
        string T_Min = T_Min_InputBox.GetComponent<TMP_InputField>().text;
        string T_Number = T_Number_InputBox.GetComponent<TMP_InputField>().text;
        string V_G_Max = V_G_Max_InputBox.GetComponent<TMP_InputField>().text;
        string V_G_Min = V_G_Min_InputBox.GetComponent<TMP_InputField>().text;
        string V_G_Number = V_G_Number_InputBox.GetComponent<TMP_InputField>().text;
        string V_DS_Max = V_DS_Max_InputBox.GetComponent<TMP_InputField>().text;
        string V_DS_Min = V_DS_Min_InputBox.GetComponent<TMP_InputField>().text;
        string V_DS_Number = V_DS_Number_InputBox.GetComponent<TMP_InputField>().text;

        string model_name = selected_model["Name"].ToString();
        string save_path = SavePathInputBox.GetComponentInParent<TMP_InputField>().text;
        string simulation_name = SimulationNameInputBox.GetComponentInParent<TMP_InputField>().text;

        Dictionary<string, string> source_parameter = new Dictionary<string, string>();
        Dictionary<string, string> drain_parameter = new Dictionary<string, string>();

        if (lists.Count != 0)
        {
            foreach (GameObject temp_object in lists)
            {
                string key = temp_object.name;
                string data = temp_object.transform.Find("SourceValue").gameObject.GetComponent<TMP_InputField>().text;

                source_parameter.Add(key, data);
                drain_parameter.Add(key, data);
            }
        }

        JObject basic_parameter_json = new JObject();
        basic_parameter_json.Add(new JProperty("L_Max", float.Parse(L_Max)));
        basic_parameter_json.Add(new JProperty("L_Min", float.Parse(L_Min)));
        basic_parameter_json.Add(new JProperty("L_Number", float.Parse(L_Number)));
        basic_parameter_json.Add(new JProperty("T_Max", float.Parse(T_Max)));
        basic_parameter_json.Add(new JProperty("T_Min", float.Parse(T_Min)));
        basic_parameter_json.Add(new JProperty("T_Number", float.Parse(T_Number)));
        basic_parameter_json.Add(new JProperty("V_G_Max", float.Parse(V_G_Max)));
        basic_parameter_json.Add(new JProperty("V_G_Min", float.Parse(V_G_Min)));
        basic_parameter_json.Add(new JProperty("V_G_Number", float.Parse(V_G_Number)));
        basic_parameter_json.Add(new JProperty("V_DS_Max", float.Parse(V_DS_Max)));
        basic_parameter_json.Add(new JProperty("V_DS_Min", float.Parse(V_DS_Min)));
        basic_parameter_json.Add(new JProperty("V_DS_Number", float.Parse(V_DS_Number)));

        JObject source_parameter_json = new JObject();
        foreach (var item in source_parameter)
        {
            source_parameter_json.Add(new JProperty(item.Key, float.Parse(item.Value)));
        }

        JObject drain_parameter_json = new JObject();
        foreach (var item in drain_parameter)
        {
            drain_parameter_json.Add(new JProperty(item.Key, float.Parse(item.Value)));
        }

        output.Add(new JProperty("Model", model_name));
        output.Add(new JProperty("SavePath", save_path.Replace("\\", "/") + "/"));
        output.Add(new JProperty("SimulationName", simulation_name));
        output.Add(new JProperty("BasicParameter", basic_parameter_json));
        output.Add(new JProperty("SourceParameter", source_parameter_json));
        output.Add(new JProperty("DrainParameter", drain_parameter_json));

        string converted_string = JsonConvert.SerializeObject(output, Formatting.Indented);
        File.WriteAllText(save_path.Replace("\\", "/")+"/request.json", converted_string);

        Debug.Log("Json saved");
        TotalSimulationNumber = int.Parse(V_DS_Number) * int.Parse(V_G_Number) * int.Parse(T_Number) * int.Parse(L_Number);
        ModelPathName = save_path.Replace("\\", "/") + "/" + simulation_name;
        string CalculatorPath = Calculator_Path + "Calculator.exe";
        string command = "-Simulation " + "\"" + save_path.Replace("\\", "/") + "/request.json\"";
        System.Diagnostics.Process.Start(CalculatorPath, command);
        ShowProgress1();
        
        Debug.Log("Simulation Data Generated");
    }

    void OpenFolderDialogButton_onClick(GameObject InputBox)
    {
        InputBox.GetComponent<TMPro.TMP_InputField>().text = FolderBrowserHelper.GetPathFromWindowsExplorer();
        Debug.Log(SimulationNameInputBox.GetComponent<TMPro.TMP_InputField>().text);
    }

    GameObject GenerateModelParameterItem(string ParameterName, double MinValue, double MaxValue, string Unit, int y_position)
    {
        GameObject newParameter = GameObject.Instantiate(Prefab_Parameter);
        newParameter.name = ParameterName;

        newParameter.transform.SetParent(ScrollViewContent.transform);
        newParameter.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(-120, y_position, 0);
        newParameter.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        newParameter.GetComponent<TMP_Text>().text = ParameterName;
        newParameter.transform.Find("Unit").GetComponent<TMP_Text>().text = Unit;

        GameObject SourceInputBox = newParameter.transform.Find("SourceValue").gameObject;

        SourceInputBox.GetComponent<TMP_InputField>().onDeselect.AddListener(delegate
        {
            InputBoxMinMaxValueLimit(SourceInputBox, MinValue, MaxValue);
        });

        return newParameter;
    }

    void InputBoxMinMaxValueLimit(GameObject InputBox, double MinValue, double MaxValue)
    {
        string s = InputBox.GetComponent<TMP_InputField>().text;
        double value = 0;
        bool correct = double.TryParse(s, out value);

        if (value < MinValue || !correct)
        {
            InputBox.GetComponent<TMP_InputField>().text = MinValue.ToString();
        }

        if (value > MaxValue)
        {
            InputBox.GetComponent<TMP_InputField>().text = MaxValue.ToString();
        }
    }
    public void ShowProgress1()
    {
        Thread thread1 = new Thread(ShowProgress);
        thread1.Start();
        Thread.Sleep(100);
        Thread thread2 = new Thread(SetProgress);
        thread2.Start();
    }
    public void ShowProgress()
    {

        taskDialog1.ProgressBar = new TaskDialogProgressBar();
        taskDialogProgressBar1 = taskDialog1.ProgressBar;
        taskDialog1.Caption = "Simulator";
        taskDialog1.Text = "Simulating";
        taskDialogProgressBar1.Maximum = TotalSimulationNumber;
        taskDialogProgressBar1.Minimum = 0;
        taskDialogProgressBar1.Value = 0;
        taskDialog1.Show();
        return;
    }
    public void SetProgress()
    {
        int id = 1;
        while(id <= TotalSimulationNumber)
        {
            Debug.Log(ModelPathName + id.ToString() + ".json");
            if(File.Exists(ModelPathName + id.ToString() + ".json"))
            {
                taskDialogProgressBar1.Value = id;
                id++;
            }
            Thread.Sleep(1);
        }
        CloseProgress();
        TaskDialog.Show("", "Simulation Data Generated", "Simulator");
    }
    public void CloseProgress()
    {

        taskDialog1.Close();

    }
}


