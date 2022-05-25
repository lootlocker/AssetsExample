using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VertexColorer : MonoBehaviour
{
    public static VertexColorer instance;

    [SerializeField]
    private Color characterColor;
    private void Awake()
    {
        instance = this;
    }
    public void ChangeColor(Color newColor)
    {
        Mesh mesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;
        
        Vector3[] vertices = mesh.vertices;
        Color[] colors = new Color[vertices.Length];
        characterColor = newColor;
        for (int i = 0; i < vertices.Length; i++)
        {
            colors[i] = characterColor;
        }

        mesh.colors = colors;
        mesh.RecalculateNormals();
        Debug.Log("Changed color");
    }

    [ContextMenu("ChangeColor")]
    public void ChangeColor()
    {
        Mesh mesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;

        Vector3[] vertices = mesh.vertices;
        Color[] colors = new Color[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            colors[i] = characterColor;
        }

        mesh.colors = colors;
        mesh.RecalculateNormals();
        Debug.Log("Changed color");
    }
    [ContextMenu("ChangeRegularMeshColor")]
    public void ChangeColorRegularMesh()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        Vector3[] vertices = mesh.vertices;
        Color[] colors = new Color[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            colors[i] = characterColor;
        }

        mesh.colors = colors;
        mesh.RecalculateNormals();
        Debug.Log("Changed color");
    }
}
