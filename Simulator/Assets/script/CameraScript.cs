using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class CameraScript : MonoBehaviour
{

    private float XRotationSpeed = 10f;
    private float YRotationSpeed = 10f;

    public Camera MainCamera;
    private Ray MouseRay;
    private RaycastHit MouseRayHit;
    public GameObject cam;
    public GameObject CameraBall;
    public GameObject ReferenceCube;
    public GameObject Prefab_GridCube;
    public GameObject Prefab_TextX;
    public GameObject Prefab_TextY;
    public GameObject Prefab_TextZ;
    
    private int MouseDragMode; //0 is rotate, 1 is drag
    private GameObject DraggingObject;
    private Vector3 LastMousePosition;
    private List<GameObject> GridList = new List<GameObject>();
    private List<GameObject> AxisXTextList = new List<GameObject>();
    private List<GameObject> AxisYTextList = new List<GameObject>();
    private List<GameObject> AxisZTextList = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        GenerateGridCubes();
    }

    // Update is called once per frame
    void Update()
    {
        
        Vector3 fwd = cam.transform.forward;
        fwd.Normalize();
        if(Input.GetMouseButtonDown(0))
        {
            MouseDragMode = 0;
            MouseRay = MainCamera.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(MouseRay,out MouseRayHit))
            {
                
                DraggingObject = MouseRayHit.collider.gameObject;
                
                if (DraggingObject.name == "ArrowXCube" || DraggingObject.name == "ArrowYCube" || DraggingObject.name == "ArrowZCube")
                {
                    Debug.Log("Hit" + DraggingObject.name);
                    MouseDragMode = 1;
                }
            }
        }
        if (Input.GetMouseButton(1))
        {
            if (MouseDragMode == 0)
            {
                float x = Input.GetAxis("Mouse X");
                float y = Input.GetAxis("Mouse Y");
                if (Mathf.Abs(x) > Mathf.Abs(y))
                {
                    Vector3 vaxis = Vector3.Cross(fwd, Vector3.right);
                    ReferenceCube.transform.Rotate(vaxis, -x * XRotationSpeed, Space.World);
                }
                else if (Mathf.Abs(y) > Mathf.Abs(x))
                {
                    Vector3 haxis = Vector3.Cross(fwd, Vector3.up);
                    ReferenceCube.transform.Rotate(haxis, -y * YRotationSpeed, Space.World);

                }
            }
        }
        if (Input.GetMouseButton(0))
        {
            if (MouseDragMode == 1)
            {
                if(DraggingObject.name == "ArrowXCube")
                {
                    Transform CenterTransform = ReferenceCube.transform.Find("CoordinateCenter");
                    Vector3 CenterScreenPosition = MainCamera.WorldToScreenPoint(CenterTransform.transform.position);
                    Vector3 DraggingObjectScreenPosition = MainCamera.WorldToScreenPoint(transform.TransformPoint(DraggingObject.transform.position));
                    Vector3 DraggingObjectLocalPosition = DraggingObject.transform.localPosition;
                    Vector3 AxisVector = DraggingObjectScreenPosition - CenterScreenPosition;
                    Vector3 MouseVector = Input.mousePosition - LastMousePosition;
                    float CosValue = Mathf.Cos(Mathf.PI / 180 * Vector3.Angle(MouseVector, AxisVector));
                    float DistanceRatio = MouseVector.magnitude / AxisVector.magnitude * CosValue;
                    DraggingObjectLocalPosition.x *= 1 + DistanceRatio;
                    if (DraggingObjectLocalPosition.x>5 && DraggingObjectLocalPosition.x <35)
                    {
                        DraggingObject.transform.localPosition =DraggingObjectLocalPosition;
                        DraggingObjectLocalPosition = DraggingObject.transform.localPosition;
                        Vector3 AxisXScale = CenterTransform.Find("CoordinateAxisX").transform.localScale;
                        Vector3 AxisXPosition = CenterTransform.Find("CoordinateAxisX").transform.localPosition;
                        AxisXScale.y = DraggingObjectLocalPosition.x / 2;
                        AxisXPosition.x = DraggingObjectLocalPosition.x / 2;
                        CenterTransform.Find("CoordinateAxisX").transform.localScale = AxisXScale;
                        CenterTransform.Find("CoordinateAxisX").transform.localPosition = AxisXPosition;
                        Vector3 BoxScale = CenterTransform.Find("CoordinateBox").transform.localScale;
                        Vector3 BoxPosition = CenterTransform.Find("CoordinateBox").transform.localPosition;
                        BoxScale.x = DraggingObjectLocalPosition.x;
                        BoxPosition.x = DraggingObjectLocalPosition.x / 2;
                        CenterTransform.Find("CoordinateBox").transform.localScale = BoxScale;
                        CenterTransform.Find("CoordinateBox").transform.localPosition = BoxPosition;

                    }
                }
                if (DraggingObject.name == "ArrowYCube")
                {
                    Transform CenterTransform = ReferenceCube.transform.Find("CoordinateCenter");
                    Vector3 CenterScreenPosition = MainCamera.WorldToScreenPoint(CenterTransform.transform.position);
                    Vector3 DraggingObjectScreenPosition = MainCamera.WorldToScreenPoint(transform.TransformPoint(DraggingObject.transform.position));
                    Vector3 DraggingObjectLocalPosition = DraggingObject.transform.localPosition;
                    Vector3 AxisVector = DraggingObjectScreenPosition - CenterScreenPosition;
                    Vector3 MouseVector = Input.mousePosition - LastMousePosition;
                    float CosValue = Mathf.Cos(Mathf.PI / 180 * Vector3.Angle(MouseVector, AxisVector));
                    float DistanceRatio = MouseVector.magnitude / AxisVector.magnitude * CosValue;
                    DraggingObjectLocalPosition.y *= 1 + DistanceRatio;
                    if (DraggingObjectLocalPosition.y > 5 && DraggingObjectLocalPosition.y < 35)
                    {
                        DraggingObject.transform.localPosition = DraggingObjectLocalPosition;
                        DraggingObjectLocalPosition = DraggingObject.transform.localPosition;
                        Vector3 AxisYScale = CenterTransform.Find("CoordinateAxisY").transform.localScale;
                        Vector3 AxisYPosition = CenterTransform.Find("CoordinateAxisY").transform.localPosition;
                        AxisYScale.y = DraggingObjectLocalPosition.y / 2;
                        AxisYPosition.y = DraggingObjectLocalPosition.y / 2;
                        CenterTransform.Find("CoordinateAxisY").transform.localScale = AxisYScale;
                        CenterTransform.Find("CoordinateAxisY").transform.localPosition = AxisYPosition;
                        Vector3 BoxScale = CenterTransform.Find("CoordinateBox").transform.localScale;
                        Vector3 BoxPosition = CenterTransform.Find("CoordinateBox").transform.localPosition;
                        BoxScale.y = DraggingObjectLocalPosition.y;
                        BoxPosition.y = DraggingObjectLocalPosition.y / 2;
                        CenterTransform.Find("CoordinateBox").transform.localScale = BoxScale;
                        CenterTransform.Find("CoordinateBox").transform.localPosition = BoxPosition;

                    }
                }
                if (DraggingObject.name == "ArrowZCube")
                {
                    Transform CenterTransform = ReferenceCube.transform.Find("CoordinateCenter");
                    Vector3 CenterScreenPosition = MainCamera.WorldToScreenPoint(CenterTransform.transform.position);
                    Vector3 DraggingObjectScreenPosition = MainCamera.WorldToScreenPoint(transform.TransformPoint(DraggingObject.transform.position));
                    Vector3 DraggingObjectLocalPosition = DraggingObject.transform.localPosition;
                    Vector3 AxisVector = DraggingObjectScreenPosition - CenterScreenPosition;
                    Vector3 MouseVector = Input.mousePosition - LastMousePosition;
                    float CosValue = Mathf.Cos(Mathf.PI / 180 * Vector3.Angle(MouseVector, AxisVector));
                    float DistanceRatio = MouseVector.magnitude / AxisVector.magnitude * CosValue;
                    DraggingObjectLocalPosition.z *= 1 + DistanceRatio;
                    if (DraggingObjectLocalPosition.z > 5 && DraggingObjectLocalPosition.z < 35)
                    {
                        DraggingObject.transform.localPosition = DraggingObjectLocalPosition;
                        DraggingObjectLocalPosition = DraggingObject.transform.localPosition;
                        Vector3 AxisZScale = CenterTransform.Find("CoordinateAxisZ").transform.localScale;
                        Vector3 AxisZPosition = CenterTransform.Find("CoordinateAxisZ").transform.localPosition;
                        AxisZScale.y = DraggingObjectLocalPosition.z / 2;
                        AxisZPosition.z = DraggingObjectLocalPosition.z / 2;
                        CenterTransform.Find("CoordinateAxisZ").transform.localScale = AxisZScale;
                        CenterTransform.Find("CoordinateAxisZ").transform.localPosition = AxisZPosition;
                        Vector3 BoxScale = CenterTransform.Find("CoordinateBox").transform.localScale;
                        Vector3 BoxPosition = CenterTransform.Find("CoordinateBox").transform.localPosition;
                        BoxScale.z = DraggingObjectLocalPosition.z;
                        BoxPosition.z = DraggingObjectLocalPosition.z / 2;
                        CenterTransform.Find("CoordinateBox").transform.localScale = BoxScale;
                        CenterTransform.Find("CoordinateBox").transform.localPosition = BoxPosition;

                    }
                }
            }
        }
        if(Input.GetMouseButtonUp(0))
        {
            MouseDragMode = 0;
        }
        LastMousePosition = Input.mousePosition;
    }

    
    public void GenerateGridCubes()
    {
        Transform AxisCenter = ReferenceCube.transform.Find("CoordinateCenter");
        Transform CoordinateBox = AxisCenter.transform.Find("CoordinateBox");
        Transform PlotBox = CoordinateBox.transform.Find("PlotBox");
        for(int i=0;i<10;i++)
        {
            for(int j=0;j<10;j++)
            {
                for(int k=0;k<10;k++)
                {
                    GameObject grid = GameObject.Instantiate(Prefab_GridCube);
                    grid.transform.SetParent(PlotBox);
                    grid.transform.localPosition = new Vector3(0.05f + i * 0.1f-0.5f, 0.05f + j * 0.1f - 0.5f, 0.05f + k * 0.1f - 0.5f);
                    grid.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    GridList.Add(grid);
                }
            }
        }
        /*for(int i=0;i<=10;i++)
        {
            GameObject TextMeshX = GameObject.Instantiate(Prefab_TextX);
            GameObject TextMeshY = GameObject.Instantiate(Prefab_TextY);
            GameObject TextMeshZ = GameObject.Instantiate(Prefab_TextZ);
            TextMeshX.transform.SetParent(PlotBox);
            TextMeshY.transform.SetParent(PlotBox);
            TextMeshZ.transform.SetParent(PlotBox);
            TextMeshX.transform.localPosition = new Vector3(i * 0.1f - 0.5f, -0.55f, -0.5f);
            TextMeshY.transform.localPosition = new Vector3(-0.55f, i * 0.1f - 0.5f, -0.5f);
            TextMeshZ.transform.localPosition = new Vector3(-0.5f, -0.55f, i * 0.1f - 0.5f);
            TextMeshX.GetComponent<TMP_Text>().text = (i / 10f).ToString();
            TextMeshY.GetComponent<TMP_Text>().text = (i / 10f).ToString();
            TextMeshZ.GetComponent<TMP_Text>().text = (i / 10f).ToString();
            AxisXTextList.Add(TextMeshX);
            AxisYTextList.Add(TextMeshY);
            AxisZTextList.Add(TextMeshZ);

        }*/
    }
    static float ClampAngle(float Angle,float Min,float Max)
    {
        if (Angle < -360) Angle += 360;
        if (Angle > 360) Angle -= 360;
        return Mathf.Clamp(Angle, Min, Max);
    }
    public void SetAxisXLabelText(double MinValue,double MaxValue)
    {

    }
    public void SetAxisYLabelText(double MinValue, double MaxValue)
    {

    }
    public void SetAxisZLabelText(double MinValue, double MaxValue)
    {

    }


}
