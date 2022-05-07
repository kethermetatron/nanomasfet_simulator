using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using Button = UnityEngine.UI.Button;
using System.IO;

public class ExportUI : MonoBehaviour
{
    /*
     *  str_AxisX Y Z 是指X,Y,Z坐标上显示的说明
        str_Title 图的标题
        二维数据是SimulationData2D List<DoubleVector2> 折线 需要网格，xylabel，标题
        三维数据是SimulationData3D  散点图
        CurrentDisplay = 0;  //0 No Display, 2 2D Display, 3 3D Display
     */

    private static string matplotlib_exe_path = UnityEngine.Application.streamingAssetsPath + "/pyplotter/" +"test_plot.exe";
    //private static string matplotlib_exe_path = "D:/QQ/FYP/test_plot.py";

    public GameObject ExportButton;
    // Start is called before the first frame update

    void Start()
    {
        ExportButton.GetComponent<Button>().onClick.AddListener(export_json);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void export_json()
    {
        if (DataProcess.CurrentDisplay != 0)
        {
            int display_mode = DataProcess.CurrentDisplay;

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Portable Network Graphics|*.png;|Joint Photographic Experts Group|*.jpg,*.jpeg;|Portable Document Format|*.pdf;|Scalable Vector Graphics|*.svg,*.svgz";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string file_path = dialog.FileName;
                string data = "[";

                JObject output_json = new JObject();

                output_json.Add(new JProperty("mode", display_mode));
                output_json.Add(new JProperty("x_label", DataProcess.str_AxisX));
                output_json.Add(new JProperty("y_label", DataProcess.str_AxisY));
                output_json.Add(new JProperty("title", DataProcess.str_Title));
                output_json.Add(new JProperty("file_name", file_path));

                if (display_mode == 2)
                {
                    foreach (DoubleVector2 vector in DataProcess.SimulationData2D)
                    {
                        data += "[" + vector.x.ToString() + "," + vector.y.ToString() + "],";
                    }
                }
                else
                {
                    foreach (DoubleVector3 vector in DataProcess.SimulationData3D)
                    {
                        data += "[" + vector.x.ToString() + "," + vector.y.ToString() + "," + vector.z.ToString() + "],";
                    }

                    output_json.Add(new JProperty("z_label", DataProcess.str_AxisZ));
                }

                data = data.Substring(0, data.Length - 1) + "]";
                output_json.Add(new JProperty("data", data));

                string converted_string = JsonConvert.SerializeObject(output_json, Formatting.Indented);
                string saved_json_path = file_path.Substring(0, file_path.LastIndexOf('\\')) + "test.json";
                File.WriteAllText(saved_json_path, converted_string);

                Debug.Log("Json saved");

                System.Diagnostics.Process.Start( matplotlib_exe_path ," -json " + saved_json_path);
            }
        }
    }
}
