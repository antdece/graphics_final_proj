﻿using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ParseException: Exception
{
    public ParseException(string message) : base(message) {}
}

interface Exp
{
    float Eval(Dictionary<string, float> vars);
}

enum Ops
{
    plus,
    minus,
    mult,
    div,
    invalid
}

struct VarExp: Exp 
{
    string varName;

    public VarExp(string name)
    {
        this.varName = name;
    }

    public float Eval(Dictionary<string, float> env)
    {   
        return env[varName];
    }
}

struct ConstExp: Exp
{
    float val;

    public ConstExp(float val)
    {
        this.val = val;
    }

    public float Eval(Dictionary<string, float> env)
    {
        return this.val;
    }
}

struct BinExp: Exp
{
    Exp l;
    Ops op;
    Exp r;

    public BinExp(Exp l, Ops op, Exp r) 
    {
        this.l = l;
        this.op = op;
        this.r = r;
    }

    public float Eval(Dictionary<string, float> env)
    {
        switch(op) {
            case Ops.plus:
                return l.Eval(env) + r.Eval(env);
            case Ops.minus:
                return l.Eval(env) - r.Eval(env);
            case Ops.mult:
                return l.Eval(env) * r.Eval(env);
            case Ops.div: 
                return l.Eval(env) / r.Eval(env);
        }

        return 0.0f; // never
    }
}


public class ArithParser
{
    private Dictionary<string, float> constants;
    private Dictionary<string, Exp> expr = new Dictionary<string, Exp>();
    private Tokenizer tok;
    private int varCount = 0;
    private string input;

    public ArithParser(string input, Dictionary<string, float> constants)
    {
        this.input = input;
        this.constants = constants;
        this.tok = new Tokenizer(input);
    }

    public string Parse()
    {
        List<Token> toks = this.tok.Tokenize();
        StringBuilder sb = new StringBuilder();        
        foreach(Token tok in toks) {
            if (tok.tt == TokenType.PARAM) {
                string tempVar = $"V{varCount++}";
                expr.Add(tempVar, ParseParam(tok.arithTokens));
                sb.Append(tempVar);
            } else {
                sb.Append(tok.lexeme);
            }
        }
        
        return sb.ToString(); // FIXME: may want to save state instead for evaluation
    }

    public string Evaluate(Dictionary<string, float> args, string succ) 
    {
        StringBuilder evalSucc = new StringBuilder(succ);
        foreach (KeyValuePair<string, Exp> pair in expr) {
            evalSucc.Replace(pair.Key, pair.Value.Eval(args).ToString());
        }

        return evalSucc.ToString();
    }

    private Exp ParseParam(Queue<ArithToken> param) 
    {
        return ParseAddition(param);
    }

    private Exp ParseAddition(Queue<ArithToken> param)
    {
        
        Exp lhs = ParseMult(param);
        if (param.Count == 0)
            return lhs;
        if (param.Count != 0 && param.Peek().att != ArithTokenType.PLUS && param.Peek().att != ArithTokenType.MINUS)
            throw new ParseException("Expect PLUS/MINUS operator");

        Ops op = OpsFromTok(param.Dequeue());
        Exp rhs = ParseMult(param);
        return new BinExp(lhs, op, rhs);
    }

    private Exp ParseMult(Queue<ArithToken> param)
    {
        // expect Variable for lhs
        // FIXME: could easily out index if not careful
        if (param.Count != 0 && param.Peek().att != ArithTokenType.VAR)
            throw new ParseException("Expect Variable");

        ArithToken lhsTok = param.Dequeue();
        Exp lhs = GetVarExp(lhsTok);
        if (param.Count == 0) {
            return lhs;
        }

        if (param.Count != 0 && param.Peek().att != ArithTokenType.MULT && param.Peek().att != ArithTokenType.DIV)
            throw new ParseException("Expected multiplication operator");

        Ops op = OpsFromTok(param.Dequeue());

        if (param.Count != 0 && param.Peek().att != ArithTokenType.VAR)
            throw new ParseException("Expected variable");

        Exp rhs = GetVarExp(param.Dequeue());
        return new BinExp(lhs, op, rhs);
    }

    private Exp GetVarExp(ArithToken tok)
    {
        string varName = tok.lexeme;
        Exp lhs;
        float val;

        if (this.constants.TryGetValue(varName, out val)) {

            lhs = new ConstExp(val);
        } else {
            Debug.Log($"is variable: {varName}");
            lhs = new VarExp(varName);
        }
        return lhs;
    }

    private static Ops OpsFromTok(ArithToken tok)
    {
        switch (tok.att) {
            case ArithTokenType.PLUS: return Ops.plus;
            case ArithTokenType.MINUS: return Ops.minus;
            case ArithTokenType.DIV: return Ops.div;
            case ArithTokenType.MULT: return Ops.mult;
            // TODO: this is a bad solution to this failure mode
            default: 
                Debug.Log("WARNING: INVALID OP CODE");
                return Ops.invalid;
        }
    }

}