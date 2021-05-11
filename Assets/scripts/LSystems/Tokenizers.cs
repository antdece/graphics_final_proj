using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandTokenizerException: Exception
{
    public CommandTokenizerException(string message) : base(message) {}
}


public enum TokenType
{
    COMMAND,
    PARAM,
}

public enum ArithTokenType
{
    VAR,
    PLUS,
    MINUS,
    MULT,
    DIV,
}


public struct ArithToken 
{
    public ArithTokenType att;
    public string lexeme;

    public ArithToken(string lexeme, ArithTokenType att)
    {
        this.lexeme = lexeme;
        this.att = att;
    }
}

public struct Token
{
    public TokenType tt;
    public string lexeme; 
    public Queue<ArithToken> arithTokens;

    public Token(string c, TokenType tt)
    {
        this.lexeme = c;
        this.tt = tt;
        this.arithTokens = new Queue<ArithToken>();
    }

    public Token(string c, TokenType tt, Queue<ArithToken> at) 
    {
        this.lexeme = c;
        this.tt = tt;
        this.arithTokens = at;
    }
}

public class Tokenizer
{
    string succ;
    int index = 0;

    public Tokenizer(string sent)
    {
        this.succ = sent;
    }

    public List<Token> Tokenize()
    {
        List<Token> tokens = new List<Token>();
        StringBuilder currentLex = new StringBuilder();
        StringBuilder currentParam = new StringBuilder();
        bool inInvocation = false;
        for (int i = 0; i < succ.Length; i++) {
            char curr = succ[i];
            if (curr == ')') {
                inInvocation = false;
                string paramStr = currentParam.ToString();
                List<Queue<ArithToken>> paramToks = TokenizeParam(paramStr);
                int index = 0;
                foreach (Queue<ArithToken> arithExp in paramToks) {
                    Token paramToken = new Token(paramStr, TokenType.PARAM, arithExp);
                    tokens.Add(paramToken);
                    // reinsert comma
                    if (++index != paramToks.Count)
                        tokens.Add(new Token(",", TokenType.COMMAND));
                }
                
                currentParam.Clear();
            }

            if (inInvocation) {
                currentParam.Append(curr);
            } else {
                currentLex.Append(curr);
            }

            if (curr == '(') {
                inInvocation = true;
                tokens.Add(new Token(currentLex.ToString(), TokenType.COMMAND));
                currentLex.Clear();
            }
        }
        tokens.Add(new Token(currentLex.ToString(), TokenType.COMMAND));
        
        return tokens;
    }

    private List<Queue<ArithToken>> TokenizeParam(string param)
    {
        string currentVar = "";
        List<Queue<ArithToken>> parameters = new List<Queue<ArithToken>>();
        Queue<ArithToken> arithTokens = new Queue<ArithToken>();
        bool isVar = false;
        foreach (char c in param) {
            if (c == '*' || c == '/' || c == '+' || c == '-' || c == ',') {
                arithTokens.Enqueue(new ArithToken(currentVar, ArithTokenType.VAR));
                currentVar = "";

                switch(c) {
                    case '*':
                        arithTokens.Enqueue(new ArithToken(c.ToString(), ArithTokenType.MULT));
                        break;
                    case '/':
                        arithTokens.Enqueue(new ArithToken(c.ToString(), ArithTokenType.DIV));
                        break;
                    case '+':
                        arithTokens.Enqueue(new ArithToken(c.ToString(), ArithTokenType.PLUS));
                        break;
                    case '-':
                        arithTokens.Enqueue(new ArithToken(c.ToString(), ArithTokenType.MINUS));
                        break;
                    case ',':
                        parameters.Add(arithTokens);
                        arithTokens = new Queue<ArithToken>();
                        break;
                }
            } else {
                currentVar += c;
            }

        }

        if (currentVar.Length != 0) {
            arithTokens.Enqueue(new ArithToken(currentVar, ArithTokenType.VAR));
            parameters.Add(arithTokens);
        }

        foreach(Queue<ArithToken> parameter in parameters) {
            Debug.Log($"===paramters for {param}===");
            foreach(ArithToken at in parameter) {
                Debug.Log($"at: {at.lexeme}");
            }
        }

        return parameters;
    }

}

public class CommandTokenizer
{
    private string commands;
    private int index = 0;

    public CommandTokenizer(string commands)
    {
        this.commands = commands;
    }

    public char NextCommand() 
    {
        if (!HasNextCommand())
            throw new CommandTokenizerException("No remaining commands");

        return ConsumeNext();
    }

    public char Peek()
    {
        if (!HasNext())
            throw new CommandTokenizerException("No remaining characters");

        return this.commands[index];
    }

    public List<float> GetArguments()
    {
        if (!HasNext())
            throw new CommandTokenizerException("Reached end of command string");

        if (Peek() != '(') // no arguments
            throw new CommandTokenizerException($"Requested arguments for command that has no arguments: {Peek()}");

        ConsumeNext(); // consume '('
        List<float> arguments = new List<float>();        
        string currentArg = "";
        while (Peek() != ')') {
            Debug.Log($"{Peek()}");
            if (Peek() == ',') {
                ConsumeNext();
                double arg = 0.0;
                if (Double.TryParse(currentArg, out arg))
                    arguments.Add((float)arg);
                else
                    throw new CommandTokenizerException($"Failed to convert argument to floating point value: {currentArg}");
                currentArg = "";
            } else {
                Debug.Log("about to grab next part of current arg");
                currentArg += ConsumeNext(); // FIXME: have better named private call perhaps?
            }
        }
        Debug.Log("about to consume end paren");
        ConsumeNext(); // consume ')'

        double darg = 0.0;
        if (Double.TryParse(currentArg, out darg))
            arguments.Add((float)darg);
        else
            throw new CommandTokenizerException($"Failed to convert argument to floating point value: {currentArg}");

        if (arguments.Count == 0)
            throw new CommandTokenizerException("Failed to extract arguments");

        return arguments;
    }

    public bool HasNextCommand()
    {
        return HasNext() && Peek() != '(';
    }

    public bool HasNextArguments()
    {
        return Peek() == '(';
    }

    public bool HasNext()
    {
        return index < commands.Length;
    }

    private char ConsumeNext()
    {
        if (!HasNext())
            throw new CommandTokenizerException("Reached end of command string prematurely");
        return this.commands[index++];
    }
}