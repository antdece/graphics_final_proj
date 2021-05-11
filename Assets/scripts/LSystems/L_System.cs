using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public class State 
{
    public float x;
    public float y;
    public float z;
    public Vector3 pos;
    public Vector3 h;
    public Vector3 l;
    public Vector3 u;


    public State(Vector3 pos, Vector3 h, Vector3 l) 
    {
        this.pos = pos;
        this.h = h;
        this.l = l;
        this.u = Vector3.Cross(h, l);
    }

    public Vector3 GetCurrentPos()
    {
        return pos;
    }

    private void rotateAxis(float deg, Vector3 axis) 
    {
        h = Quaternion.AngleAxis(deg, axis) * h;
        l = Quaternion.AngleAxis(deg, axis) * l;
        u = Quaternion.AngleAxis(deg, axis) * u;
    }

    public void roll(float deg) => rotateAxis(deg, h);

    public void pitch(float deg) => rotateAxis(deg, l);

    public void turn(float deg) => rotateAxis(deg, u);

    public void dollarRoll(Vector3 v) 
    {
        var VcrossH = Vector3.Cross(v, h);
        l = VcrossH.normalized;
    }

    public State NextState(float size) 
    {
        Vector3 nextPos =  GetCurrentPos() + h * size;
        return new State(nextPos, h, l);
    }

    public State Clone() 
    {
        return new State(pos, h, l);
    }
}


public class L_System : MonoBehaviour
{

    private LineRenderer lineRenderer;
    private State state;
    private Stack<State> states = new Stack<State>();

    private MeshRenderer meshRenderer;
    private Leaf currentLeaf;
    private ArrayList leaves = new ArrayList();
    private int leafCount = 0;

    public float theta = 22.5f;
    public float size = 0.25f;
    public int iterations = 5;
    public string axiom = "A";
    public Dictionary<char, string> rules = new Dictionary<char, string>();

    private int currentLine = 0;
    private ArrayList lines = new ArrayList();
    private ArrayList branches = new ArrayList();

    
    void Setup() 
    {
        lineRenderer = GetComponent<LineRenderer>();
        meshRenderer = GetComponent<MeshRenderer>();
        LineBranch.templateLineRenderer = lineRenderer;
        state = new State(Vector3.zero, Vector3.up, Vector3.left);

        rules.Add('A', "[&FL!A]/////’[&FL!A]///////’[&FL!A]");
        rules.Add('F', "S/////F");
        rules.Add('S', "F L");
        // rules.Add('L', "[’’’∧∧{-f+f+f-|-f+f+f}]");
        rules.Add('L', "{[++++G.][++GG.][+GGG.][GGGGG.][-GGG.][--GG.][----G.]}");
    }

    // Start is called before the first frame update
    void Start()
    {
        Setup();
        string sentence = Generate(iterations);
        Draw(sentence);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // F: draw forward
    // X: nothing
    // +: turn left by theta degrees
    // -: turn right by theta degrees
    // [: push state onto stack
    // ]: pop state off of stack
    void Draw(string sentence) 
    {
        Debug.Log($"drawing sentence: {sentence}");
        foreach (char a in sentence) {
            switch (a) {
                case '+':
                    state.turn(theta);
                    break;
                case '-':
                    state.turn(-theta);
                    break;
                case '&':
                    state.pitch(theta);
                    break;
                case '^':
                    state.pitch(-theta);
                    break;
                case '\\':
                    state.roll(theta);
                    break;
                case '/':
                    state.roll(theta);
                    break;
                case '[':
                    states.Push(state.Clone());
                    break;
                case ']':
                    state = states.Pop();
                    break;
                case '|':
                    state.turn(180);
                    break;
                case 'X':
                    continue;
                case 'F':
                    // DrawLine();
                    DrawMesh();
                    break;
                case '{':
                    StartLeaf();
                    break;
                case '}':
                    EndLeaf();
                    break;
                default:
                    // State nextState;
                    // if (currentLeaf != null && (nextState = currentLeaf.AcceptCommand(a, state)) != null)
                    //     state = nextState;
                    continue;
            }
        }
    }

    string Generate(int n) 
    {   
        // string current = "[’’’∧∧{-f+f+f-|-f+f+f}]";
        string current = axiom;
        Debug.Log("# generations: {n}");
        for (int i = 0; i < n; ++i) {
            StringBuilder sb = new StringBuilder();
            foreach (char c in current) {
                if (rules.ContainsKey(c)) {
                    sb.Append(rules[c]);
                } else {
                    sb.Append(c);
                }
            }

            current = sb.ToString();
        }
        
        return current;
    }

    void DrawMesh()
    {
        State nextState = state.NextState(size);
        var mesh = new CylinderBranch($"TestMesh_{currentLine++}", state.GetCurrentPos(), nextState.GetCurrentPos(), 0.05f);
        mesh.position = state.GetCurrentPos();
        mesh.material = lineRenderer.material;
        state = nextState;
        branches.Add(mesh);
    }

    void DrawLine() 
    {
        var lineGo = new GameObject($"Line_{currentLine}");
        lineGo.transform.position = Vector3.zero;
        

        LineRenderer newLine = SetupLineRenderer(lineGo);
        newLine.tag = $"Line_{currentLine}";

        Vector3 startPos = state.GetCurrentPos();

        newLine.SetPosition(0, startPos);
        
        Vector3 endPos = NextPoint(size);
        State nextState = state.NextState(size);

        newLine.SetPosition(1, endPos);

        state = nextState;
        currentLine++;
    }

    void StartLeaf()
    {
        // currentLeaf = new HexLeaf($"Leaf_{leafCount++}", state.GetCurrentPos());
        currentLeaf = new PolygonLeaf($"PolygonLeaf", state.GetCurrentPos());
        currentLeaf.material = meshRenderer.material;
    }

    void EndLeaf()
    {
        if (currentLeaf == null)
            return;

        Debug.Log($"Leaf points: {currentLeaf.name}");
        foreach (Vector3 vertex in currentLeaf.vertices) {
            Debug.Log(vertex);
        }

        currentLeaf.Render();
        currentLeaf.material = meshRenderer.material;
        leaves.Add(currentLeaf);
    }

    private Vector3 NextPoint(float size)
    {
        Vector3 curPos = state.GetCurrentPos();
        return curPos + state.h * size;
    }

    private LineRenderer SetupLineRenderer(GameObject go)
    {
        var newLineRenderer = go.AddComponent<LineRenderer>();
        newLineRenderer.useWorldSpace = true;
        newLineRenderer.positionCount = 2;
        newLineRenderer.material = lineRenderer.material;
        newLineRenderer.startColor = lineRenderer.startColor;
        newLineRenderer.endColor = lineRenderer.endColor;
        newLineRenderer.startWidth = lineRenderer.startWidth;
        newLineRenderer.endWidth = lineRenderer.endWidth;
        newLineRenderer.numCapVertices = 5;
        return newLineRenderer;        
    }
}
