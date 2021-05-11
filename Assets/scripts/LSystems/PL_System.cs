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
            Debug.Log("uneven args");
            foreach (char arg in this.arguments) {
                Debug.Log($"prod arg: {arg}");
            }
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

    public string axiom = "A(10, 10)";

    // contraction ratio for trunk
    public float r1 = 0.9f;

    // contraction ratio for branches
    public float r2 = 0.6f;

    // branching angle from trunk
    public float a0 = 45.0f;

    // branching angle for lateral axis
    public float a2 = 45.0f;

    // divergence angle 
    public float d = 137.5f;

    // width decrease rate
    public float wr = 0.707f;

    public float startX = 0;
    public float startY = 0;
    public float startZ = 0;

    private List<Production> productions = new List<Production>();
    private Dictionary<string, float> constants;
    private List<Branch> branches = new List<Branch>();
    private Stack<State> states = new Stack<State>();
    private State state; 
    private int currentBranch = 0;
    private MeshRenderer meshRenderer;

    private float radius;

    void Setup()
    {
        state = new State(new Vector3(startX, startY, startZ), Vector3.up, Vector3.left);
        meshRenderer = GetComponent<MeshRenderer>();

        constants = new Dictionary<string, float> {
            ["r1"] = r1,
            ["r2"] = r2,
            ["a0"] = a0,
            ["a2"] = a2,
            ["d"] = d,
            ["wr"] = wr
        };

        Production.constants = constants;
        // A(1, 10)
        // !(10)F(1)[&(45)B(0,0)]/(137.5)A(0,0)
        productions.Add(new Production("A(l, w)", "!(w)F(l)[&(a0)B(l*r2,w*wr)]/(d)A(l*r1,w*wr)"));
        productions.Add(new Production("B(l, w)", "!(w)F(l)[-(a2)$C(l*r2,w*wr)]C(l*r1,w*wr)"));
        productions.Add(new Production("C(l, w)", "!(w)F(l)[+(a2)$B(l*r2,w*wr)]B(l*r1,w*wr)"));
        // productions.Add(new Production(""))
    }

    string Generate(int iterations)
    {
        // Ex:
        // start: A(1, 10)
        // Gen 1: !(w)F(l)[&(a0)B(l*r2,w*wr)]/(d)A(l*r1,w*wr)
        // find 'A' as an invocation for a production
        // replace 'A' with production specified by A(l, w), with l = 1, w = 10 and constants

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

                    continue;
                 }

                if (c == ')') {
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
                    continue;
                }

                if (c == ',') {
                    if (current != null) {
                        arguments.Add(Convert(curArg.ToString()));
                        curArg.Clear();
                    } else {
                        sb.Append(c);
                    }
                        
                    continue;
                }

                if (inFun) {
                    curArg.Append(c);
                    continue;
                }

                if (current == null) {
                    foreach (Production p in productions) {
                        if (p.Invocation == c.ToString()) {
                            current = p;
                            break;
                        }
                    }

                    if (current == null) 
                        sb.Append(c);
                } else {
                    sb.Append(current.Evaluate(arguments));
                    current = null;
                }
            }

            Debug.Log($"new sentence: {sb.ToString()}");
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
                    Debug.Log("about to draw forward");
                    args = ct.GetArguments();
                    Debug.Log($"received arguments: {args.Count}");
                    DrawMesh(args[0]); // FIXME: check args first

                    break;
                case '!':
                    Debug.Log("about to set line width");
                    args = ct.GetArguments();
                    // set line width
                    this.radius = (args[0] / 2) * 0.05f;
                    Debug.Log("successfully set line width");
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
                    Debug.Log("executing pitch");
                    args = ct.GetArguments();
                    Debug.Log($"received pitch args: {args.Count}");
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
                    Debug.Log("executing roll");
                    args = ct.GetArguments();
                    Debug.Log($"received roll arguments: {args.Count}");
                    state.roll(args[0]);
                    break;
                case '[':
                    Debug.Log("saving state");
                    states.Push(state.Clone());
                    break;
                case ']':
                    Debug.Log("restoring state");
                    state = states.Pop();
                    break;
                case '$':
                    Debug.Log("executing dollar");
                    state.dollarRoll(Vector3.up);
                    break;
                default:
                    Debug.Log($"no action for invocation: {nextCommand}");
                    if (ct.HasNextArguments())
                        ct.GetArguments(); // walk through arguments
                    break;
            }
        }
    }


    void DrawMesh(float size)
    {
        State nextState = state.NextState(size);
        var mesh = new CylinderBranch($"TestMesh_{currentBranch++}", state.GetCurrentPos(), nextState.GetCurrentPos(), this.radius);
        Debug.Log($"Drawing forward: {currentBranch}");
        mesh.position = state.GetCurrentPos();
        mesh.material = meshRenderer.material;
        state = nextState;
        branches.Add(mesh);
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
        Debug.Log("IN START");
        Setup();
        string sentence = Generate(iterations);
        Draw(sentence);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
