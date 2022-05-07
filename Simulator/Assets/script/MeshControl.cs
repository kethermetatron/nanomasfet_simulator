using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.WindowsAPICodePack.Dialogs;

public static class MeshControl
{
    /*public GameObject ReferenceCube;
    private GameObject Surface;
    public GameObject DisplayButton;
    public GameObject Prefab_Point;
    private List<GameObject> Points = new List<GameObject>();*/
    static void Start()
    {
        /*Button btn = DisplayButton.GetComponent<Button>();
        btn.onClick.AddListener(OnClickTest1);*/
    }
    /*public void OnClickTest1()
    {
        Debug.Log("ButtonClick");
        Vector3 Center = new Vector3(0, 0, 0);
        List<List<Vector3>> MeshVertices = new List<List<Vector3>>();
        Material material = new Material(Shader.Find("Diffuse"));
        Destroy(Surface);
        Surface = new GameObject();
        Surface.transform.SetParent(transform);
        Surface.AddComponent<MeshFilter>();
        Surface.AddComponent<MeshRenderer>();
        
        for (int i = 0; i < 5; i++)
        {
            MeshVertices.Add(new List<Vector3>());
            for (int j = 0; j < 10; j++)
            {
                MeshVertices[i].Add(new Vector3(j / 10f, j * j / 100f, i / 5f));
            }
        }
        SetMesh(ref Surface, transform, Center, ref MeshVertices, material);
        Surface.transform.localPosition = new Vector3(-0.5f, -0.5f, -0.5f);
        Surface.transform.localScale = new Vector3(1, 1, 1);
        Surface.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        TaskDialog.Show("内容","标题","窗体标题");
    }*/
    // Start is called before the first frame update
    public static void SetMesh(ref GameObject Surface,Transform ParentTransform, Vector3 Center, ref List<List<Vector3>> MeshVertices, Material material)
    {
        List<List<int>> VerticesID = new List<List<int>>();
        Vector3[] Vertices = ConvertToMeshArray(ref MeshVertices, ref VerticesID);
        AddCenter(Center, ref Vertices);
        //ConvertToWorldPosition(ref Vertices, ParentTransform);
        int[] Triangles = GetTriangles(ref MeshVertices, ref VerticesID);
        Surface.GetComponent<MeshRenderer>().material = material;
        Mesh mesh = Surface.GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.vertices = Vertices;
        mesh.triangles = Triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
   

    /*public static GameObject CreateSurfaceMesh(Vector3 Center,ref List<List<Vector3>> MeshVertices,Material material, string Name)
    {
        List<List<int>> VerticesID = new List<List<int>>();
        Vector3[] Vertices = ConvertToMeshArray(ref MeshVertices, ref VerticesID);
        AddCenter(Center, ref Vertices);
        int[] Triangles = GetTriangles(ref MeshVertices, ref VerticesID);
        GameObject NewSurface = new GameObject();
        Mesh mesh = NewSurface.AddComponent<MeshFilter>().mesh;
        NewSurface.AddComponent<MeshRenderer>();
        NewSurface.GetComponent<Renderer>().material = material;
        mesh.Clear();
        mesh.vertices = Vertices;
        mesh.triangles = Triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return NewSurface;
    }*/
    public static void ConvertToWorldPosition(ref Vector3[] Vertices, Transform ParentTransform)
    {
        for (int i = 0; i < Vertices.Length; i++)
        {
            Vertices[i] = ParentTransform.TransformPoint(Vertices[i]);
        }
    }
    public static Vector3[] ConvertToMeshArray(ref List<List<Vector3>> MeshVertices, ref List<List<int>> VerticesID)
    {
        int VerticesSum = 0;
        int index = 0;
        VerticesID.Clear();
        for (int i = 0;i < MeshVertices.Count; i++)
        {
            VerticesSum += MeshVertices[i].Count;
        }
        Vector3[] VerticesArray = new Vector3[VerticesSum * 2];
        for (int i = 0; i < MeshVertices.Count; i++) 
        {
            VerticesID.Add(new List<int>());
            for (int j = 0; j < MeshVertices[i].Count; j++)
            {
                VerticesID[i].Add(index);
                VerticesArray[index] = MeshVertices[i][j];
                VerticesArray[index + VerticesSum] = MeshVertices[i][j];
                index++;
            }
        }
        return VerticesArray;
    }
    public static void AddCenter(Vector3 Center, ref Vector3[] VerticesArray)
    {
        for (int i = 0; i < VerticesArray.Length; i++)
        {
            VerticesArray[i] += Center;
        }
    }
    public static int[] GetTriangles(ref List<List<Vector3>> MeshVertices, ref List<List<int>> VerticesID)
    {
        int VerticesSum = 0;
        for (int i = 0; i < MeshVertices.Count; i++)
        {
            VerticesSum += MeshVertices[i].Count;
        }
        List<int> Triangles = new List<int>();
        for (int i = 0; i < MeshVertices.Count - 1; i++)
        {
            for (int j = 0; j < MeshVertices[i].Count - 1; j++)
            {
                if(MeshVertices[i+1].Count > j)
                {
                    Triangles.Add(VerticesID[i][j]);
                    Triangles.Add(VerticesID[i+1][j]);
                    Triangles.Add(VerticesID[i][j + 1]);

                    Triangles.Add(VerticesID[i][j + 1] + VerticesSum);
                    Triangles.Add(VerticesID[i + 1][j] + VerticesSum);
                    Triangles.Add(VerticesID[i][j] + VerticesSum);

                    if (MeshVertices[i + 1].Count > j + 1)
                    {
                        Triangles.Add(VerticesID[i + 1][j]);
                        Triangles.Add(VerticesID[i + 1][j + 1]);
                        Triangles.Add(VerticesID[i][j + 1]);

                        Triangles.Add(VerticesID[i][j + 1] + VerticesSum);
                        Triangles.Add(VerticesID[i + 1][j + 1] + VerticesSum);
                        Triangles.Add(VerticesID[i + 1][j] + VerticesSum);

                        if (j == MeshVertices[i].Count - 2)
                        {
                            for (int k = j + 2; k < MeshVertices[i + 1].Count; k++) 
                            {
                                Triangles.Add(VerticesID[i][j+1]);
                                Triangles.Add(VerticesID[i + 1][k - 1]);
                                Triangles.Add(VerticesID[i + 1][k]);

                                Triangles.Add(VerticesID[i + 1][k] + VerticesSum);
                                Triangles.Add(VerticesID[i + 1][k - 1] + VerticesSum);
                                Triangles.Add(VerticesID[i][j + 1] + VerticesSum);

                            }
                        }
                    }
                }
                else
                {
                    Triangles.Add(VerticesID[i][j]);
                    Triangles.Add(VerticesID[i + 1][MeshVertices[i + 1].Count - 1]);
                    Triangles.Add(VerticesID[i][j + 1]);

                    Triangles.Add(VerticesID[i][j + 1] + VerticesSum);
                    Triangles.Add(VerticesID[i + 1][MeshVertices[i + 1].Count - 1] + VerticesSum);
                    Triangles.Add(VerticesID[i][j] + VerticesSum);

                }
            }
        }
        return Triangles.ToArray();
    }
}
