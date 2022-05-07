using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StarterUI : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject ToCommanderButton;
    public GameObject ToPlotterButton;
    public GameObject QuitButton;
    void Start()
    {
        ToCommanderButton.GetComponent<Button>().onClick.AddListener(ToCommander);
        ToPlotterButton.GetComponent<Button>().onClick.AddListener(ToPlotter);
        QuitButton.GetComponent<Button>().onClick.AddListener(QuitApplication);
        DataProcess.AppPath = Application.absoluteURL;
        Debug.Log(Application.streamingAssetsPath);
        Screen.SetResolution(1191,636,false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ToCommander()
    {
        SceneManager.LoadScene(1);
    }
    public void ToPlotter()
    {
        SceneManager.LoadScene(2);
    }
    public void QuitApplication()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}
