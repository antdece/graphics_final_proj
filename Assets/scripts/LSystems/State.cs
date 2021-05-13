using UnityEngine;

public class State 
{
    private Vector3 pos;
    private Vector3 h;
    private Vector3 l;
    private Vector3 u;


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