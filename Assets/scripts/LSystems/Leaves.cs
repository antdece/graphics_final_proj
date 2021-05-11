using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Leaf 
{
    protected GameObject leafGo;
    protected MeshFilter mf;
    protected MeshRenderer mr;
    protected Mesh m;
    protected List<Vector3> _vertices = new List<Vector3>();
    protected string _name;
    
    public string name 
    {
        get { return _name; }
    }

    public Material material 
    {
        get {
            return mr.material;
        }

        set {
            mr.material = value;
        }
    }

    public Vector3[] vertices 
    {
        get {
            return _vertices.ToArray();
        }
    }

    public Leaf(string name, Vector3 start) 
    {
        this._name = name;
        leafGo = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        mf = leafGo.GetComponent<MeshFilter>();
        mr = leafGo.GetComponent<MeshRenderer>();
        m = new Mesh();
        _vertices.Add(start);
    }

    public abstract State AcceptCommand(char command, State currentState);
    public abstract void Render();
}

class HexLeaf: Leaf 
{
    public HexLeaf(string name, Vector3 start) : base(name, start) {}

    public override State AcceptCommand(char command, State currentState) 
    {
        if (command == 'f') {
            State nextState = currentState.NextState(0.15f);
            _vertices.Add(nextState.GetCurrentPos());
            return nextState;
        }

        return null;
    }

    public override void Render()
    {
        m.vertices = vertices;
        m.triangles = new int[] {
                2, 4, 3,
                5, 2, 1,
                0, 5, 1, 
                5, 4, 2,
                2, 3, 4,
                5, 1, 2,
                0, 1, 5, 
                5, 2, 4
        };

        m.uv = new Vector2[] {
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(0, 0)
        };

        mf.mesh = m;
        mf.mesh.RecalculateNormals();        
    }
}

class PolygonLeaf: Leaf 
{
    public PolygonLeaf(string name, Vector3 start) : base(name, start) {}

    public override State AcceptCommand(char command, State currentState) 
    {
        if (command == 'G') {
            return currentState.NextState(0.1f);
        } else if (command == '.') {
            _vertices.Add(currentState.GetCurrentPos());
            return currentState;
        }
            
        return null;
    }

    public override void Render() 
    {
        m.vertices = vertices;
        m.triangles = new int[] {
              0, 1, 7,
              0, 1, 2,
              0, 2, 3,
              0, 3, 4,
              0, 4, 5,
              0, 5, 6,
              0, 6, 7,
              0, 7, 1,
              0, 2, 1,
              0, 3, 2,
              0, 4, 3,
              0, 5, 4,
              0, 6, 5,
              0, 7, 6
        };

        m.uv = new Vector2[] {
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(0, 0)
        };

        mf.mesh = m;
        mf.mesh.RecalculateNormals();
    }
}




