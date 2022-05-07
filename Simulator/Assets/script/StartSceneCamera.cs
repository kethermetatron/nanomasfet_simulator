using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class StartSceneCamera : MonoBehaviour
{
    public GameObject Button1;
    // Start is called before the first frame update
    void Start()
    {
        Button btn = Button1.GetComponent<Button>();
        btn.onClick.AddListener(Button1_onClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void Button1_onClick()
    {
        SceneManager.LoadScene("Plotter");
    }
}
