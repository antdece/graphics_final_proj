using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Production
{
    string invoc;
    int arity = 0;
    public string pred;
    Func<float, bool> cond;
    string succ;
    ArithParser ap;
    public static Dictionary<string, float> constants;

    public string Invocation
    {
        get 
        {
            return invoc;
        }
    }

    private List<char> arguments = new List<char>();
    
    public Production(string pred, Func<float, bool> cond, string succ)
    {
        this.pred = pred;
        this.cond = cond;
        this.succ = succ;
        this.ap = new ArithParser(succ, constants);
    }

    public Production(string pred, string succ)
    {
        this.pred = pred;
        this.cond = x => true;
        this.ap = new ArithParser(succ, constants);
        this.succ = this.ap.Parse();
        ParsePred(pred);
    }

    public string Evaluate(List<float> args)
    {
        if (args.Count != this.arguments.Count) {
            return ""; // FIXME: maybe exception?
        }

        int index = 0;
        Dictionary<string, float> argMap = new Dictionary<string, float>();
        foreach (char name in arguments) {
            argMap.Add(name.ToString(), args[index++]);
        }

        return this.ap.Evaluate(argMap, this.succ);
    }

    private void ParsePred(string pred) 
    {

        if (pred.Length == 0)
            return;

        invoc = pred[0].ToString();
        // Ex: A(l, r)
        for (int i = 1; i < pred.Length; i++) {
            char c = pred[i];
            
            if (c == '(' || c == ',' || c == ')' || c == ' ')
                continue;

            arity++;
            // FIXME: variables may be more than one character long
            arguments.Add(c);
        }
    }

}

public class PL_System : MonoBehaviour
{

    public int iterations = 10;

    public string axiom = "A(1, 10)";

    // contraction ratio for trunk
    public float r1 = 0.9f;

    // contraction ratio for branches
    public float r2 = 0.6f;

    // branching angle from trunk
    public float a0 = 45.0f;

    // branching angle for lateral axis
    public float a2 = 45.0f;

    // branching angle aux.
    public float a1 = 60.0f;

    // divergence angle 
    public float d = 137.5f;

    // width decrease rate
    public float wr = 0.707f;

    // angle for leave direction changes
    public float lt = 22.5f;

    public float startX = 0.0f;
    public float startY = 0.0f;
    public float startZ = 0.0f;
    public List<string> prodPred = new List<string>();
    public List<string> prodSucc = new List<string>();

    private List<Production> productions = new List<Production>();
    private Dictionary<string, float> constants;
    private List<Branch> branches = new List<Branch>();
    private Stack<State> states = new Stack<State>();
    private State state; 
    private int currentBranch = 0;
    private MeshRenderer meshRenderer;
    private LineRenderer lineRenderer;
    private Leaf currentLeaf;
    private List<Leaf> leaves = new List<Leaf>();
    private int leafCount = 0;

    private float radius = 0.1f;

    void Setup()
    {
        state = new State(new Vector3(startX, startY, startZ), Vector3.up, Vector3.left);
        meshRenderer = GetComponent<MeshRenderer>();
        lineRenderer = GetComponent<LineRenderer>(); 

        constants = new Dictionary<string, float> {
            ["r1"] = r1,
            ["r2"] = r2,
            ["a0"] = a0,
            ["a1"] = a1,
            ["a2"] = a2,
            ["d"] = d,
            ["wr"] = wr,
            ["lt"] = lt
        };

        Production.constants = constants;
        // A(1, 10)
        // !(10)F(1)[&(45)B(0,0)]/(137.5)A(0,0)
        LoadProductions();

    }

    private void LoadProductions()
    {
        if (prodPred.Count == 0 || prodSucc.Count == 0 || prodSucc.Count != prodPred.Count) {
                productions.Add(new Production("A(l, w)", "!(w)F(l)L[&(a0)B(l*r2,w*wr)]L/(d)A(l*r1,w*wr)L"));
                productions.Add(new Production("B(l, w)", "!(w)F(l)L[-(a2)$C(l*r2,w*wr)]LC(l*r1,w*wr)L"));
                productions.Add(new Production("C(l, w)", "!(w)F(l)L[+(a2)$B(l*r2,w*wr)]LB(l*r1,w*wr)L"));
                productions.Add(new Production("L", "[’’’'^(lt)'^(lt){-(lt)f+(lt)f+(lt)f-(lt)|-(lt)f+(lt)f+(lt)f}]"));
        } else {
            for (int i = 0; i < prodSucc.Count; i++) {
                Debug.Log($"custom production: {prodPred[i]} -> {prodSucc[i]}");
                productions.Add(new Production(prodPred[i], prodSucc[i]));
            }
        }
    }

    string Generate(int iterations)
    {
        string sentence = axiom;
        for (int n = 0; n < iterations; n++) {
            StringBuilder sb = new StringBuilder();
            StringBuilder curArg = new StringBuilder();
            List<float> arguments = new List<float>();
            bool inFun = false;
            Production current = null;
            foreach (char c in sentence) {

                 if (c == '(') {
                    if (current != null ){
                        inFun = true;
                    } else {
                        sb.Append(c);
                    }
                 } else if (c == ')') {
                    if (current != null) {
                        inFun = false;
                        arguments.Add(Convert(curArg.ToString()));
                        sb.Append(current.Evaluate(arguments));
                        current = null;
                        arguments = new List<float>();
                        curArg.Clear();
                    } else {
                        sb.Append(c);
                    }
                } else if (c == ',') {
                    if (current != null) {
                        arguments.Add(Convert(curArg.ToString()));
                        curArg.Clear();
                    } else {
                        sb.Append(c);
                    }

                } else if (inFun) {
                    curArg.Append(c);
                } else {

                    if (current != null) {
                        sb.Append(current.Evaluate(arguments));
                        current = null;
                    }
                    
                    foreach (Production p in productions) {
                        if (p.Invocation == c.ToString()) {
                            current = p;
                            break;
                        }
                    }

                    if (current == null) 
                        sb.Append(c);
                }
                
            }

            sentence = sb.ToString();
        }

        return sentence;
    }

    void Draw(string sentence)
    {
        Debug.Log($"generated sentence: {sentence}");
        CommandTokenizer ct = new CommandTokenizer(sentence);
        List<float> args;
        while (ct.HasNextCommand()) {
            char nextCommand = ct.NextCommand();
            switch(nextCommand) {
                case 'F':
                    args = ct.GetArguments();
                    DrawBranch(args[0]); // FIXME: check args first
                    break;
                case '!':
                    args = ct.GetArguments();
                    // set line width
                    this.radius = (args[0] / 2) * 0.05f;
                    break;
                case '+':
                    args = ct.GetArguments();
                    state.turn(args[0]);
                    break;
                case '-':
                    args = ct.GetArguments();
                    state.turn(-args[0]);
                    break;
                case '&':
                    args = ct.GetArguments();
                    state.pitch(args[0]);
                    break;
                case '^':
                    args = ct.GetArguments();
                    state.pitch(-args[0]);
                    break;
                case '\\':
                    args = ct.GetArguments();
                    state.roll(-args[0]);
                    break;
                case '/':
                    args = ct.GetArguments();
                    state.roll(args[0]);
                    break;
                case '[':
                    states.Push(state.Clone());
                    break;
                case ']':
                    state = states.Pop();
                    break;
                case '|':
                    state.turn(180.0f);
                break;
                case '$':
                    state.dollarRoll(Vector3.up);
                    break;
                case '{':
                    StartLeaf();
                    break;
                case '}':
                    EndLeaf();
                    break;
                default:
                    
                    State nextState;
                    if (currentLeaf != null && (nextState = currentLeaf.AcceptCommand(nextCommand, state)) != null) {
                        state = nextState;
                    } else {
                        if (ct.HasNextArguments())
                            ct.GetArguments(); // walk through arguments
                    }

                    continue;
            }
        }
    }


    void DrawBranch(float size)
    {
        State nextState = state.NextState(size);
        var mesh = new ConeBranch($"TestMesh_{currentBranch++}", state.GetCurrentPos(), nextState.GetCurrentPos(), this.radius, this.radius * this.wr);
        mesh.position = state.GetCurrentPos();
        mesh.material = meshRenderer.material;
        state = nextState;
        branches.Add(mesh);
    }


    private void StartLeaf()
    {
        currentLeaf = new HexLeaf($"PolygonLeaf_{leafCount++}", state.GetCurrentPos());
        currentLeaf.material = lineRenderer.material;
    }

    private void EndLeaf()
    {
        if (currentLeaf == null)
            return;

        currentLeaf.material = lineRenderer.material;
        currentLeaf.Render();
        leaves.Add(currentLeaf);
        currentLeaf = null;
    }

    private float Convert(string conv)
    {
        double arg = 0;
        int wholeArg = 0;
        if (Double.TryParse(conv, out arg)) {
            return (float)arg;
        } else if (Int32.TryParse(conv, out wholeArg)) {
            return (float)wholeArg;
        // else we gotta problem
        } else
            Debug.Log($"WARNING: failed to convert {conv}");
        return 0;
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
}
