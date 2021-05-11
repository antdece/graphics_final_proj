using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Branch
{
    protected GameObject branchGo;
    protected Vector3 startPos;
    protected Vector3 endPos;
    protected string name;

    public Vector3 position 
    {
        set 
        {
            branchGo.transform.position = value;
        }
    }

    public Branch(string name, Vector3 start, Vector3 end) 
    {
        this.name = name;
        this.branchGo = new GameObject(name);
        this.startPos = start;
        this.endPos = end;
    }
}

public abstract class MeshBranch: Branch 
{
    protected MeshFilter mf;
    protected MeshRenderer mr;
    protected Mesh m;
    protected List<Vector3> _vertices = new List<Vector3>();

    public MeshBranch(string name, Vector3 start, Vector3 end) : base(name, start, end)
    {
        this.mf = branchGo.AddComponent<MeshFilter>();
        this.mr = branchGo.AddComponent<MeshRenderer>();
        this.m = new Mesh();
    }

    public abstract void GenerateMesh();
}

public class LineBranch: Branch
{
    private LineRenderer lineRenderer;
    public static LineRenderer templateLineRenderer;

    public LineBranch(string name, Vector3 start, Vector3 end): base(name, start, end) 
    {
        this.lineRenderer = SetupLineRenderer();
        this.lineRenderer.SetPosition(0, start);
        this.lineRenderer.SetPosition(0, end);
    }

    private LineRenderer SetupLineRenderer() 
    {
        var newLineRenderer = branchGo.AddComponent<LineRenderer>();
        newLineRenderer.useWorldSpace = true;
        newLineRenderer.positionCount = 2;
        newLineRenderer.material = templateLineRenderer.material;
        newLineRenderer.startColor = templateLineRenderer.startColor;
        newLineRenderer.endColor = templateLineRenderer.endColor;
        newLineRenderer.startWidth = templateLineRenderer.startWidth;
        newLineRenderer.endWidth = templateLineRenderer.endWidth;
        newLineRenderer.numCapVertices = 5;
        return newLineRenderer;
    }

}


public class CylinderBranch: MeshBranch
{
    private float radius;
    public Material material 
    {
        set 
        {
            this.mr.material = value;
        }
    }

    public CylinderBranch(string name, Vector3 start, Vector3 end, float radius) : base(name, start, end) 
    {
        Debug.Log($"received radius: {radius}");
        this.radius = radius;
        GenerateMesh();
        Vector3 lookDir = endPos - startPos;
        Quaternion rot = Quaternion.FromToRotation(branchGo.transform.right, lookDir); 
        branchGo.transform.rotation = rot * branchGo.transform.rotation;
    }

    public override void GenerateMesh()
    {
        double circum = Math.PI * radius * 2;
        double length = Vector3.Distance(startPos, endPos);
        const int kCylinderGridSizeY = 12;
        const int kCylinderGridSizeX = 1;
        int gridSize = (kCylinderGridSizeX + 1) * (kCylinderGridSizeY + 1);
        float xStep = (float)(length / kCylinderGridSizeX);
        float yStep = (float)(circum / kCylinderGridSizeY);

        Vector3 rowStart = Vector3.zero;
        Vector2 [] uvs = new Vector2[gridSize];
        for (int i = 0; i <= kCylinderGridSizeY; ++i) {
            Vector3 col = new Vector3(rowStart.x, rowStart.y, rowStart.z);
            for (int j = 0; j <= kCylinderGridSizeX; ++j) {
                double theta = (2 * Math.PI) * (col.y / circum);
                _vertices.Add(new Vector3(col.x, (float)(radius * Math.Sin(theta)), (float)(radius * Math.Cos(theta))));
                uvs[i * j] = new Vector2((float)(col.x / length), (float)(col.y / circum));
                col = new Vector3(col.x + xStep, col.y, col.z);
            }
            rowStart = new Vector3(rowStart.x, col.y + yStep, col.z);
        }

        
        int numSquares = kCylinderGridSizeX * kCylinderGridSizeY;
        int numTriangles = numSquares * 2;
        int [] triangles = new int[numTriangles * 6];

        int index = 0;
        int verticesPerRow = kCylinderGridSizeX + 1;
        int verticesPerCol = kCylinderGridSizeY + 1;
        for (int i = 0; i < gridSize; ++i) {

            // check not last in row of verts, not top row of verts
            if (((i + 1) % verticesPerRow) != 0 && (i / verticesPerRow) != kCylinderGridSizeY) {
                triangles[index] = i;
                triangles[index+2] = i + 1;
                triangles[index+1] = i + verticesPerRow;

                            
                triangles[index+3] = i;
                triangles[index+4] = i + 1;
                triangles[index+5] = i + verticesPerRow;
                index += 6;
            }
            
            if ((i % verticesPerRow) != 0 && i >= verticesPerRow) {
                triangles[index] = i;
                triangles[index+1] = i - verticesPerRow;
                triangles[index+2] = i - 1;

                triangles[index+3] = i;
                triangles[index+4] = i - 1;
                triangles[index+5] = i - verticesPerRow;
                index += 6;
            }

        }


        m.vertices = _vertices.ToArray();
        m.triangles = triangles;
        m.uv = uvs;

        mf.mesh = m;
        mf.mesh.RecalculateNormals();
    }
}

// public class ConeBranch: Branch
// {
//     private float startRad;
//     private float endRad;

//     public ConeBranch(string name, Vector3 start, Vector3 end, float startRad, float endRad) : base(name, start, end)
//     {
//         this.startRad = startRad;
//         this.endRad = endRad;
//     }

//     public override GenerateMesh()
//     {
//         double bottomCircum = Math.PI * 2 * startRad;
//         double topCircum = Math.PI * 2 * endRad;
//         double length = Vector3.Length(startPos, endPos);
//         double sideTriangleLen = (bottomCircum - topCircum) / 2;
//         double xStep = topCircum / 2;
        
//         // left triangle
//         Vector3 vert = Vector3.zero;
//         _vertices.Add(vert);
//         vert = new Vector3(vert.x + sideTriangleLen, vert.y, vert.z);
//         _vertices.Add(vert);

//         _vertices.Add()
//         vert = new Vector3(vert.x, vert.y + length, vert.z);
//         _vertices.Add(vert);


//     }
// }