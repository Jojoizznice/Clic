﻿
using System;
using System.Runtime.CompilerServices;
using ConsoleTables;

namespace Clic.Commands.Variables;

class Set : CommandBase
{
    public override string Name => nameof(Set).ToLower();

    public override Task<CommandReturnValue> CommandInvoked(string[] args, CommandArguments ca)
    {
        if (args.Length != 3)
        {
            ca.TextProvider.WriteError("Command must be of form\tset [name] (=/!) [text]");
            return Task.FromResult(new CommandReturnValue(int.MinValue));
        }

        string varName = args[0];
        string op = args[1];
        string varText = args[2];

        bool isImmutable = false;

        switch (op)
        {
            case "=":
                break;
            case "!":
                isImmutable = true;
                break;
            default:
                ca.TextProvider.WriteError($"Invalid operator for {varName} (\"{op}\")");
                return Task.FromResult(new CommandReturnValue(-1));
        }

        Variable v = Variable.CreateVariable(varName, varText, isImmutable);
        ca.VariableHolder.AddVariable(v);

        return Task.FromResult(new CommandReturnValue(0));
    }

    public override string GetHelpText()
    {
        return "";
    }

    public override void Setup()
    {
        
    }
}

class Get : CommandBase
{
    public override string Name => "get";

    public override Task<CommandReturnValue> CommandInvoked(string[] args, CommandArguments ca)
    {
        if (args.Length != 1)
        {
            ca.TextProvider.WriteError("No unique variable name given");
            return Task.FromResult(new CommandReturnValue(-1));
        }

        Variable? v = ca.VariableHolder.FindName(args[0]);
        if (v == null)
        {
            ca.TextProvider.WriteLine($"""{args[0]} existiert nicht""");
            return Task.FromResult(new CommandReturnValue(-1));
        }

        string type = v.GetType().ToString() // man nehme den typ als string
            .Split('.').Last()               // entfernt namespaces
            .Replace("Variable", null)       // nur den tatsächlichen typ
            .ToLower();                      // klein
            
        ca.TextProvider.WriteLine($"""Name: {v.Name}; Typ: {type}; Value: {v.GetValueString()}, {(v.IsImmutable ? "is immutable" : "is mutable")}""");

        return Task.FromResult(new CommandReturnValue(0));
    }

    public override string GetHelpText()
    {
        return "";
    }

    public override void Setup()
    {
        
    }
}

class LsVar : CommandBase
{
    public override string Name => "lsvar";

    public override Task<CommandReturnValue> CommandInvoked(string[] args, CommandArguments ca)
    {
        Variable[] vars = ca.VariableHolder.GetAllVariables();
        ConsoleTable table = new("name", "value", "type", "is immutable");
        ca.TextProvider.WriteLine($"Variable count:  |{vars.Length}|\n");

        foreach (var var in vars)
        {
            table.AddRow(var.Name, var.GetValueString(), var.GetVariableType(), var.IsImmutable.ToString());
        }

        table.Write(Format.Minimal);
        return Task.FromResult(new CommandReturnValue(0));
    }

    public override string GetHelpText()
    {
        return "";
    }

    public override void Setup()
    {
        
    }
}

class Cng : CommandBase
{
    public override string Name => "cng";
    public override string[] Aliases => aliases;
    private readonly string[] aliases = ["mod", "md"];

    public override Task<CommandReturnValue> CommandInvoked(string[] args, CommandArguments ca)
    {
        if (args.Length != 3)
        {
            ca.TextProvider.WriteError("Command must be of form\tcng [name] = [value]");
            return Task.FromResult(new CommandReturnValue(int.MinValue));
        }

        if (args[1] != "=")
        {
            ca.TextProvider.WriteError("Command must be of form\tset [name] = [value]");
            return Task.FromResult(new CommandReturnValue(int.MinValue));
        }

        Variable? v = ca.VariableHolder.FindName(args[0]);
        if (v is null)
        {
            ca.TextProvider.WriteError($"Variable {args[0]} does not exist");
            return Task.FromResult(new CommandReturnValue(-1));
        }

        if (v.IsImmutable)
        {
            ca.TextProvider.WriteError($"Varable {v.Name} is immutable");
            return Task.FromResult(new CommandReturnValue(-1));
        }

        int type = v.GetVariableType();
        bool dSuccess = double.TryParse(args[2], out double d);
        bool bSuccess = bool.TryParse(args[2], out bool b);

        switch (type)
        {
            case 0:
                ((StringVariable)v).Value = args[2];
                break;
            case 1:
                if (!dSuccess)
                {
                    ca.TextProvider.WriteError($"{args[2]} is not a double");
                    return Task.FromResult(new CommandReturnValue(-1));
                }
                ((DoubleVariable)v).Value = d;
                break;
            case 2:
                if (!bSuccess)
                {
                    ca.TextProvider.WriteError($"{args[2]} is not a bool");
                    return Task.FromResult(new CommandReturnValue(-1));
                }
                ((BoolVariable)v).Value = b;
                break;
        }

        return Task.FromResult(new CommandReturnValue(0));
    }

    public override string GetHelpText()
    {
        return "";
    }

    public override void Setup()
    {
        
    }
}

class SetOp : CommandBase
{
    public override string Name => "setop";

    public override Task<CommandReturnValue> CommandInvoked(string[] args, CommandArguments ca)
    {
        if (args.Length != 4)
        {
            ca.TextProvider.WriteError("Command must be of form\tsetop [name] = [variable] [operation]");
            return Task.FromResult(new CommandReturnValue(int.MinValue));
        }

        string varName = args[0];
        string op = args[1];
        string varText = args[2];
        string operation = args[3];

        if (op != "=")
        {
            ca.TextProvider.WriteError($"Invalid operator for {varName} (\"{op}\")");
            return Task.FromResult(new CommandReturnValue(int.MinValue));
        }

        Variable? v = ca.VariableHolder.FindName(varText);
        if (v is null)
        {
            ca.TextProvider.WriteError($"Variable {varText} doesn't exist");
            return Task.FromResult(new CommandReturnValue(-1));
        }
        if (v.GetVariableType() != 1)
        {
            ca.TextProvider.WriteError($"Variable {varText} is not a double variable");
            return Task.FromResult(new CommandReturnValue(-1));
        }

        OperationVariable ov = new(varName, (v as DoubleVariable)!, operation);
        ca.VariableHolder.AddVariable(ov);

        return Task.FromResult(new CommandReturnValue(0));
    }

    public override string GetHelpText()
    {
        throw new NotImplementedException();
    }

    public override void Setup()
    {
        
    }
}

internal class Getop : CommandBase
{
    public override string Name => "getop";

    public override Task<CommandReturnValue> CommandInvoked(string[] args, CommandArguments ca)
    {
        if (args.Length != 1)
        {
            ca.TextProvider.WriteError("No unique variable name given");
            return Task.FromResult(new CommandReturnValue(-1));
        }

        Variable? v = ca.VariableHolder.FindName(args[0]);
        if (v == null)
        {
            ca.TextProvider.WriteError($"""{args[0]} existiert nicht""");
            return Task.FromResult(new CommandReturnValue(-1));
        }

        if (v.GetVariableType() != 3)
        {
            ca.TextProvider.WriteError($"""{args[0]} is no operation variable""");
            return Task.FromResult(new CommandReturnValue(-1));
        }

        string type = v.GetType().ToString() // man nehme den typ als string
            .Split('.').Last()               // entfernt namespaces
            .Replace("Variable", null)       // nur den tatsächlichen typ
            .ToLower();                      // klein

        ca.TextProvider.WriteLine($"""Name: {v.Name}; Typ: {type}; Value: {v.GetValueString()}; is immutable; Operation: "{(v as OperationVariable)!.GetOperation()}" """);

        return Task.FromResult(new CommandReturnValue(0));
    }

    public override string GetHelpText()
    {
        return """
                        
            1. USAGE        

                getop [opvar]

            2.DESCRIPTION

                Prints information about the operation variable.
""";
    }

    public override void Setup()
    {
        
    }
}

/*
 This code be like
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⢿⣿⡿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠿⣿⠟⠛⠃⠈⠉⠉⠉⠉⠉⠀⠀⠀⠀⠀⠀⠀⠀⠉⠉⠛⠻⠿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠿⠟⠛⠉⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠉⠙⠛⠿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠿⠟⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠙⠻⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠛⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡀⣀⢄⡰⣂⠦⡰⢠⡐⡀⡀⢆⡀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠙⠻⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠿⠋⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⠤⣒⠥⣓⠬⣎⠵⣈⠧⣑⢣⢒⡱⣘⢢⠱⢌⡑⢆⠰⣀⠂⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠙⠻⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠟⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⠠⣀⠘⢦⡙⢦⡛⢬⡓⣬⠳⣜⢲⡡⢎⠦⡑⠦⡑⢎⠲⢌⠦⡑⢄⠒⡀⠆⠠⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠻⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡀⠈⣀⡜⡻⢦⣙⢧⢏⡧⡝⢦⡛⢬⢆⡹⢌⡲⣉⠖⡩⢆⡙⢢⠒⡡⢊⠔⢡⠈⠄⡁⠂⢀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠛⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠏⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢰⢡⢿⣥⢿⣵⢯⡾⣽⡎⣷⢹⢧⠿⡼⡎⣵⠏⡶⢡⡾⠱⡎⡼⢡⡎⠱⡌⠈⡆⠈⠰⢠⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⢿⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠐⠸⣻⣮⢷⣏⡾⣱⢯⣞⣧⢻⡜⣧⢞⣣⢗⡹⣔⢫⡔⢫⠔⡳⢌⡱⢢⠌⡱⢈⠒⢠⠉⡐⠂⠄⠁⠠⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣆⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⢞⣽⣶⣽⣾⣳⣯⣶⣝⣞⢫⠟⡾⣹⢮⣏⢷⣊⢧⠚⡥⢋⢖⡡⢎⢅⡚⢄⢃⠚⢄⠂⠡⠈⠄⠠⠐⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⢻⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡟⠉⠇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⠀⢰⢯⣞⣵⣻⣾⣿⣿⣿⣿⣿⣿⣿⣿⢶⣟⣿⡞⣧⢻⡬⢳⣍⠳⣊⠖⣩⠢⠜⣈⠆⣩⠂⠬⣁⠃⠌⠀⠄⠀⠠⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⠎⠀⣯⣿⣾⡽⣯⣷⣿⣿⣿⣿⣿⣿⡻⣽⢯⣿⣷⣿⡼⡶⣭⢳⣬⢳⡐⡎⡔⢣⢩⠐⠬⢠⠉⢆⠠⢁⠂⠌⠀⢀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣧⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡜⣎⠳⣭⣟⣾⣽⣿⣿⣿⣻⣿⣿⣿⣿⡳⢉⡟⣿⣿⣿⣿⣿⣼⣳⢎⣧⠳⡜⡰⢃⢆⡉⢆⠡⢍⠢⡁⠆⡈⢀⠠⠀⠀⠀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⠞⣌⡳⣷⣯⣟⣿⣿⣿⣿⣿⣿⣿⣿⣿⡵⢬⡎⢀⣿⣿⣿⣿⣧⣟⣞⡲⣏⠼⣡⢋⠦⡘⢢⠑⡌⠠⡁⠆⠡⠀⠀⢀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢨⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠀⡀⠀⠀⠀⠀⠀⠀⠀⠀⣸⠀⡭⣟⣷⣟⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠽⣁⠀⢸⣿⣿⣿⣿⣿⡟⣾⢳⣏⢷⢂⠣⡒⢍⢢⠑⠌⠐⠀⡉⢀⠂⢈⠀⠀⠀⠠⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣯⡀⠀⠀⠀⠀⠀⠀⢀⢇⡛⠼⡉⠀⠛⠘⠙⠛⠛⠛⠛⠛⠁⠚⠉⠻⣽⢺⣄⡛⢿⡿⠏⠓⠹⠚⠓⠈⠋⠉⠓⠙⠈⠀⠈⠀⠀⠀⠐⠠⠈⠀⠀⠈⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣧⠀⠀⠀⠀⠀⠀⢨⠞⣌⠰⡡⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢩⢳⡎⠟⣮⠕⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠐⠠⠀⠀⠀⠀⠀⠀⠂⢀⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢰⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⡀⠀⠀⠀⠀⢮⡘⠄⠁⡄⣀⣀⣀⣀⡄⡀⠀⠀⠀⢀⠀⠀⠀⠠⣟⡼⠂⠀⠃⠀⠀⠀⠀⠀⠀⢀⣀⠀⣠⢄⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣰⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡷⠀⠀⠀⢸⡡⢞⣤⣲⡽⣾⣿⡿⠟⠊⠁⠀⠈⠀⠉⠢⠔⢢⢾⣷⣾⡴⣀⠀⠀⠀⠀⢠⢾⡙⠞⠋⠁⠀⠉⠘⠉⠃⠌⠀⠀⠄⠀⠀⠀⠀⠀⠀⠀⠠⠀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣼⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠂⠀⠀⣎⢽⣛⠾⢹⡛⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⡃⠻⣿⣿⢦⡙⠀⠀⠀⠈⠀⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠠⠀⢀⠀⠀⠀⠀⠀⢀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣼⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣶⠀⢠⠀⣨⢿⣳⠀⠀⠀⠀⠀⠠⠀⠀⠀⠀⠀⠀⠀⠀⠀⡅⢰⣿⣿⢯⡑⠀⠀⠀⠀⠀⠀⠀⢀⣴⠆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡀⠄⠈⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠀⠀⣼⣻⣏⠉⠁⣤⣶⣿⣷⣶⠖⠂⠀⠀⠀⠀⠀⢀⡞⠁⣸⣿⡿⣄⠫⡄⠀⠀⠈⢷⠆⠈⠛⠊⢧⡄⣀⢀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⠠⠀⠀⠀⠀⠀⠀⠀⠀⠀⢼⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⢹⠀⢀⣿⢹⣿⣶⣾⠛⠻⠟⠉⠀⠀⠀⠀⠀⠀⢀⣴⣿⡽⢆⣳⣿⢿⣬⢓⡄⠀⠀⠀⠀⠛⣶⣤⣀⠀⠀⠀⠉⠀⠀⠀⠀⣀⠤⣙⢒⠲⡰⠀⠀⠀⠀⠀⠠⠀⠀⠀⠀⠠⠄⠀⠀⠘⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣟⣸⠀⢀⠹⢺⣿⣿⣿⣦⡤⠀⠀⠔⠀⠀⠀⣀⣴⣿⢛⡿⣟⢯⡿⣯⠷⣎⢧⠀⠀⠈⠰⠀⠀⠈⠐⠭⠚⠰⠉⠀⠀⢀⣤⡶⢯⡳⢌⠎⡡⢡⠁⠀⠀⠀⠈⢀⠀⠀⠀⠀⣁⠀⠀⠀⠀⢸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⢹⠀⢸⣿⣤⣿⣿⣿⡀⠃⣄⣠⡄⣤⣶⣾⣿⡿⢋⢺⣿⣙⣦⢟⡽⣻⡜⠦⠀⠀⠀⠀⠭⣌⢓⡖⣢⠤⡔⢦⡲⣽⣻⣞⡿⢳⡝⢎⡜⢡⠂⠀⢀⠀⠀⠀⢀⠀⠀⠀⠀⠄⡃⠀⢀⠀⢰⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡟⢸⠀⢸⣯⠟⣿⣿⣿⣿⣦⣜⣿⣤⣿⣿⣿⡿⡴⠂⠈⡿⣱⣯⣿⡞⡵⢊⠅⠀⠀⠀⠀⠀⠺⣝⣞⣧⢻⣙⠮⡱⢧⢳⢎⡜⡳⣜⠣⡜⠠⠁⠀⢀⠂⠀⠀⠀⠀⠀⠀⠀⠄⡁⠀⠀⠀⢸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⢾⠀⢸⡳⣿⣿⢿⣿⣿⣿⣿⣿⣿⣿⣿⠟⠉⠁⠀⣰⡀⣼⣿⣿⣝⢦⢣⠀⠀⠀⠀⠀⠀⠀⠙⣾⣵⢢⣏⢶⡱⣎⠳⡞⣼⠱⣊⠱⢠⠁⢀⠐⠠⠀⠂⠀⠀⠀⠀⠀⠀⠘⠴⡀⠀⠀⠐⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣾⠀⠈⣷⢻⡾⣟⠹⣿⣿⣿⣿⣿⡿⠊⠀⠀⠀⡴⢋⠁⠙⠛⠉⠙⠚⠇⠂⠀⠀⠐⡀⠄⠀⠀⠀⢿⣏⡞⣣⢝⣮⡛⡵⢊⡕⠢⢑⠢⠈⠄⡈⠄⠂⠀⠀⠀⠀⠀⠀⠀⠠⠈⡵⠀⠀⠀⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡆⠀⣭⢳⡿⣽⣺⢿⣿⡿⣟⠡⠀⠀⠀⠀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠐⠈⠀⠀⠀⠈⢳⡝⣆⢫⢶⡹⢜⡡⠜⡀⠢⠁⠌⠐⢀⠠⠀⠂⠀⠀⠀⠀⠀⠀⠁⠆⠘⡄⠀⠀⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡇⠀⢎⡳⣝⢧⣏⠿⡼⣙⠄⠀⠀⠀⢠⣾⣷⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠄⡀⠀⢻⣎⡳⢎⠖⡡⢎⠰⢀⠡⠁⠌⠀⠄⠀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⠀⠸⡅⢎⡸⣜⢫⠄⠁⠀⠀⠀⣠⢾⣿⣿⣿⠀⠐⣶⣶⡒⠀⢤⣀⠀⠀⠂⠀⠐⠀⠄⡘⠐⠠⠀⠀⠺⣱⢋⠬⡑⢌⠒⡀⢂⠁⠂⢈⠀⠄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠀⠘⢨⢇⠳⣌⢗⡂⠀⠀⢀⠲⣥⣻⣽⣳⣯⣿⣻⣟⣿⠉⠀⢸⣿⡗⢦⢤⢠⢀⠀⠀⠀⠁⠃⠀⠀⠀⠱⣋⢄⠃⠆⡈⠐⠠⠈⠐⠀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠠⠀⠀⠀⣼⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡆⠀⢘⢮⡡⢘⠦⠀⠀⠀⢆⠙⣶⢋⠻⢿⣽⣳⡟⣿⠿⠀⠀⣾⣿⡉⢈⢎⡆⢫⠜⡡⢂⠀⠀⠀⡀⠀⠀⠜⢢⠉⠄⠠⠁⠂⠁⠈⠀⠀⡀⠀⠀⠀⠀⠀⠀⠀⠘⠄⠀⢀⣼⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⠀⠘⣬⢣⣚⢤⠄⠀⠀⠺⡴⣹⢞⡄⠺⢭⠋⠗⠉⠀⠀⠀⠙⠉⠁⠈⠈⠑⠊⠔⡁⠊⠀⠀⠀⢡⠀⠀⠒⡌⠰⢀⠀⠀⠂⠐⠀⠀⠀⠁⡀⠄⠀⠀⠀⠀⠀⣀⢀⣀⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡄⠀⣎⢧⡹⢎⢣⣀⠀⠘⠱⠋⠊⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠀⠀⠀⠸⡄⢃⠌⡄⠀⠀⠀⠀⠀⠀⠄⠀⠄⠈⠀⠄⠀⢀⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣧⠀⠸⣆⠙⡈⡱⣎⢷⠀⠀⢀⢠⡄⠒⢂⣈⠈⠛⣿⣗⡒⠖⡰⢂⠓⠰⠀⠀⠀⠀⠀⠀⠀⠄⠀⠀⠀⠱⡜⢂⠌⠀⠀⠀⢀⠐⡈⠄⠂⢈⠀⡐⠀⠀⠀⣸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡄⠀⠎⣜⡱⢳⡹⢮⠁⠀⢸⣟⡿⡥⠀⠈⠑⠊⢁⠀⡀⠀⠑⠋⠀⠀⠀⠀⠀⠀⠀⢀⠠⢀⠄⡀⠀⢸⡱⠃⠀⠀⠀⠠⢀⠂⡐⠠⠁⠂⠀⠀⠀⠀⢀⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡀⠐⠨⣕⢣⢟⡡⠀⠀⢸⣎⢷⡹⢧⣄⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠐⡈⠄⢢⢁⠂⠀⠀⣣⠇⠁⠀⠀⠄⡁⠂⡐⠠⠁⠠⠁⠀⠐⠀⠀⢸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⡄⠀⣙⠎⡞⠀⠀⢠⣛⡾⣧⣿⣳⡼⣘⢆⡀⢤⡀⢠⣤⣤⡀⠄⠀⣀⠀⠀⠐⠈⡄⠂⠄⠀⠐⢣⢆⠀⠠⢁⠂⡐⠠⠐⠀⢁⠀⠂⠀⠀⠀⠀⠀⠹⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⡀⠈⢝⠲⠀⢸⣣⢿⣽⣣⣿⢿⣷⣻⢾⣽⠲⡥⠄⠘⠁⠀⠀⢦⡑⣎⠧⢓⠈⠠⡑⢈⠄⠀⠈⠂⠀⠀⠂⠐⠀⡐⠀⠈⠀⠀⠀⠀⠀⠀⠀⠀⠀⠹⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⡄⠈⠃⠀⠰⡩⢞⡳⣿⡽⣯⢿⡽⣏⡞⡧⣝⠲⣆⠤⣄⢀⡼⡱⢎⠌⠀⢀⠒⠠⠁⠀⠀⠀⠀⠀⠄⠀⠈⠀⠀⠀⠄⠀⠀⠀⠀⠀⣸⠀⠀⠀⠀⠸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡦⠀⠀⠀⠀⠹⢼⡡⢟⡼⢣⠳⡌⢎⡑⡈⠑⠈⠲⣌⠳⣘⠱⠀⠀⠀⠀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⡏⠀⠀⠀⠀⠀⢹⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠁⠀⠀⠀⠀⠀⠀⠉⠘⠌⢣⠓⠜⢢⠱⠌⠱⠂⢁⠠⠃⠌⠁⠀⠀⠀⠀⠀⠀⠀⠀⡀⠀⠀⠀⠄⠀⠐⠀⠀⠀⠀⡀⠂⠁⢀⡾⠁⠀⠀⠀⠀⠀⠀⠹⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡟⠀⠠⡄⠀⠀⠀⢀⠠⠀⠀⠀⠀⠁⠀⠀⠀⠀⠀⠀⠀⠁⠀⠀⠀⠀⠀⠀⠀⢀⠠⠐⠀⠀⠀⠂⠁⠀⡀⠀⠀⠄⡀⠐⠁⠀⢠⡾⠁⠀⠀⠀⠀⠀⠀⠀⠀⠙⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠿⠟⠛⠉⠀⣠⢣⠱⠀⢰⠀⠠⢁⠂⠄⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⠀⠁⠀⠀⢀⠀⡀⠁⡀⠂⠁⡀⠄⢁⠂⠀⠀⠀⣠⠞⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠛⠿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠿⠟⠛⠉⢀⡀⢠⢄⡒⡒⠈⢄⢣⢏⠀⢸⡄⠀⠂⠉⠰⢀⠄⡀⠀⡀⢀⠀⠀⠀⠀⠀⠀⠀⠀⡀⠀⠀⡀⢀⠈⠀⢀⠀⠄⢀⠐⠠⠐⡈⠀⠀⠀⣠⡼⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠙⠿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠟⠛⠋⠉⠁⢀⠀⡤⢀⡔⢌⠲⣈⠱⢊⡔⠁⠀⠌⣆⢫⠀⠀⣿⡀⠀⠠⣁⠊⠄⡁⢂⠁⠂⠄⠀⠀⠀⠀⠀⠂⠀⠀⠄⠂⠀⡀⠠⠈⠀⠠⢈⠀⠌⡐⠀⠀⠀⣀⡾⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠉⠻⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⠿⡿⣛⠻⡍⠭⢠⢄⠲⡐⣂⠆⡉⢆⠣⡐⢣⠜⠢⢅⠢⡑⠨⣐⠁⢠⠘⡄⠎⠶⠀⠸⣧⡀⠀⠦⡑⢢⠐⡠⢈⠀⠀⠀⠀⠀⠈⠀⠀⠁⠠⠀⠐⠀⡀⠄⠀⡁⠐⠠⠈⠐⠀⠀⢠⡾⠋⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠛⠿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⡿⠿⢟⠻⣍⠡⡔⢬⠳⡱⢌⠱⣈⠱⢂⠌⢢⠁⠆⡘⠡⡈⢆⠡⡈⠌⠑⢌⠒⡀⢠⠃⠆⡁⢎⠰⡉⢎⢣⠄⠘⢷⣄⠀⠙⢤⠃⡔⢂⡐⠀⠀⠀⠀⠁⠀⡁⠠⠀⠐⠀⠐⠀⡀⠂⠄⠁⠀⠀⢀⣠⡾⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠉⠙⠿⢿⣿⣿⣿⣿⣿⣿⣿
⡥⢶⣉⢯⠳⣌⠳⢌⢣⠣⡑⠌⠂⣄⠣⢌⡘⠤⠑⡰⢈⠱⡈⢆⠣⠘⠌⡱⢈⠒⡀⠠⡙⠤⠐⡂⠔⡐⠨⢌⠳⣄⠈⠻⣧⡀⠈⠑⡌⠒⠤⢁⠂⡀⠀⠀⠁⠀⠄⠂⠠⢀⠈⡐⠀⠌⠀⢀⣠⠴⠟⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠉⠛⠻⢿⣿⣿
⣛⢦⡝⡬⢣⢌⡓⡌⢂⠱⡘⢬⠱⢠⢃⠢⡘⢄⠃⡔⢁⠂⡐⠄⠢⢉⠔⡠⠃⡜⠀⠀⡓⢌⡑⢈⠐⠤⣁⠢⠘⢤⠓⡄⠈⠿⣦⡀⠈⠉⠐⠂⡐⠠⠀⠀⠠⠀⡀⢀⠁⡄⠂⠀⢀⣠⡴⠟⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠙
⣜⢢⡱⢠⢃⠎⠴⡈⢆⠢⣍⢢⡑⢢⠌⢣⡑⢊⠐⡠⢂⡒⢄⡈⣁⠂⡌⢠⢁⠒⡀⠀⣑⠢⠔⡨⢘⠰⡀⠆⢉⠢⡉⠜⢤⡀⠉⢻⣶⣀⠀⠂⠐⠀⠀⠡⠀⠄⠐⠀⠀⢀⣠⠶⠛⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠠⠐⠀⠀⠠⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⢮⡱⣄⠣⢎⡄⢣⠑⡌⢓⡌⢆⣉⠒⡌⡰⡘⢦⠱⢌⡡⢘⠠⠒⡄⢊⠔⡡⢊⠰⠀⠀⢈⠆⠱⣀⠁⢆⠱⠈⠄⠂⠌⢡⠒⠠⠑⡀⠈⠻⣶⣀⠀⠐⠈⠑⠈⣀⣠⣴⠾⠋⠁⢀⠤⡐⠤⠀⠀⠀⠀⠀⠀⠀⠐⠀⠠⠀⠐⠈⠀⠀⠠⠀⠀⠀⠄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡀⢀⠀⠄⡐⡀⠆⠰⡐⢠⠃⠜⡠
⢣⢓⠬⣙⠲⣘⢣⠙⡜⢢⡘⠆⠦⡑⡄⢣⠑⣎⠱⡀⢆⠣⢌⡑⢌⠢⡘⠄⡡⢊⠅⠀⠈⡜⠰⣀⠩⢀⢂⠉⠤⢉⠐⢠⠈⠄⡑⠨⢄⡄⠀⠙⠿⠆⢠⡴⠟⠉⠁⠀⠀⠀⠈⠄⠂⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠄⠀⡐⠀⠀⢀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠠⢀⠂⠡⠐⠠⣈⠰⢀⠡⢊⠥⡘⠤⡉⢆⡱
⡣⢞⡰⣌⡱⢆⠧⡚⡔⢣⠜⣌⠰⠱⡈⢄⠩⢆⡑⢌⠢⡑⠂⢌⠢⡑⢌⢂⠱⡈⢌⡀⠀⢌⠱⡀⢆⠠⢊⠐⢂⠂⡉⠄⢌⡐⢠⠑⢢⢘⡱⢢⠄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⠀⠌⡐⢀⠂⡐⠠⢀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠠⢁⠒⠠⠘⠠⢁⠒⡀⢆⠡⢊⠄⢢⠁⢆⠱⢨⠐
⡝⢮⡱⢲⡹⢌⠶⡱⢌⢣⠚⡌⢡⠃⡜⢤⢋⡔⡀⠎⡐⢈⡘⢄⠃⢌⠢⢌⢢⢁⠂⡄⠀⢀⠣⡐⢌⠢⢁⠊⡄⠣⡐⠌⢢⠐⠢⠉⢄⠎⢤⠃⠀⠀⠀⡁⠄⡀⠂⠄⠁⠂⠐⠀⡐⠠⠂⠄⡁⠂⢌⡐⠠⠌⡐⢡⠑⠂⢀⢠⡀⠄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠠⢀⠃⠆⠡⡈⠔⡁⢂⠅⣂⠱⡈⠜⡠⢉⠆⡘⢤⣉
⡙⢦⡱⢡⡘⣌⠲⡑⢊⠦⡙⢌⠥⡚⢌⠦⠈⠔⣁⠊⠰⡁⢌⠢⡘⢄⠣⢌⠢⢌⠂⡔⠀⠀⢢⠑⠂⠱⡈⠰⢀⠃⡔⠡⠂⢌⠡⠡⠈⢤⠋⠀⠀⠀⠀⠁⠀⠀⠐⠈⠄⡐⠠⠁⠄⠡⠘⠠⢌⠡⢂⠔⠡⠘⡀⠃⠈⢀⡌⠃⠀⠀⠀⠀⢀⠀⠠⢀⠠⠀⠄⠠⢀⠂⠁⠤⠁⢆⠡⠊⢌⡑⠄⢣⠘⠄⢊⠄⢣⠑⠬⡐⢡⠊⡜⡰⢂
⡙⢦⣙⢦⠱⢌⡱⢌⠡⢆⡱⢊⠒⣍⠲⡌⠜⡰⢈⡄⠃⡜⢠⠒⢌⠢⡑⢌⠒⡌⠒⠌⡄⠀⠠⡉⠤⡑⠤⡁⠌⡐⠠⠁⡜⣀⠂⡡⢉⠆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠀⠄⠠⠈⠄⡀⢈⠁⢢⠐⡁⢎⠰⠁⠂⢀⠴⠃⠀⠀⠀⠀⢀⠂⢄⡈⠐⡀⢂⠡⠈⠔⡠⠌⡐⠄⠃⡄⠂⢅⠂⠐⡈⠤⢘⠈⡔⢈⠆⡘⠤⡑⢢⠑⡄⢣⡉
⠻⣆⡛⡆⢇⠶⡰⢞⡸⢆⢳⢘⡸⣀⠷⡘⢷⡀⠇⡀⢇⠸⣀⠛⡆⢳⠸⡘⢇⡰⠛⠶⣰⠀⠀⢷⢀⠳⠆⠳⢆⠸⣀⠳⠀⠆⠃⠶⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡘⠀⠀⠃⢀⠰⢀⠘⡀⠇⠸⢀⠃⠀⢰⠛⠀⠀⠀⠀⠀⠀⠆⡘⢀⢀⠃⡰⠆⢃⠛⠰⡀⢇⠰⠘⢃⠀⠛⡀⠸⢰⠀⠆⡀⢃⠆⡘⢰⠃⢆⠃⡆⢳⠘⡆⡸
⡹⣜⡱⢈⠎⢒⠉⢆⠱⢊⠦⡁⢆⡱⢢⢍⠒⡌⢢⠑⣈⠒⡄⢣⢀⠃⢆⡑⠢⡔⢩⠐⡤⠂⠀⢘⠢⡁⢎⠡⢊⠔⠰⢠⠉⠤⣉⠂⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠠⠀⠀⠌⠀⠄⢂⠐⠠⢂⠐⠈⠁⢀⠠⠌⠁⠀⠀⠀⠀⠀⠀⠌⠀⡐⢀⠂⡐⠠⢈⠄⡌⠡⡐⢌⠂⡅⢂⡘⠤⢐⡁⢂⠘⠤⡑⡀⠊⢄⠡⢊⡐⠌⡰⢁⠊⡔⣡
⡱⢎⡔⢡⢊⠤⢋⠤⡉⡌⢒⠡⢎⠴⡃⢎⠒⡌⡡⠊⡄⢣⠘⡰⢈⠜⡠⢌⠱⡐⢢⠑⠤⢃⠀⠀⠣⡘⢄⠣⢌⠰⡈⢄⠈⣒⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠐⠀⠄⠁⢂⠡⠈⠄⡈⠐⡀⠀⢀⠡⡀⠊⠀⠀⠀⠀⠀⠀⠀⡈⠄⠂⠐⡀⠌⠠⠑⡈⠐⠄⡃⠌⡄⠡⠘⡀⠆⡘⢠⠐⡡⢊⠐⠤⢡⠉⢆⡘⠄⡐⢨⠐⡌⢢⠱⢄
⢱⢊⡜⢢⢃⢎⡱⢢⠱⠌⣡⢋⡜⠢⢑⣊⠱⢠⠑⢢⠘⢠⠑⡄⠣⡘⠰⡈⢆⡉⢆⡉⢆⠃⢆⠀⠀⠱⢊⠰⠈⢄⠱⡈⡰⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⠂⠄⢂⠁⠂⢀⠁⠠⢈⡐⠂⠁⠀⠀⠀⠀⠀⠀⠀⡀⠄⠐⠈⠠⢀⠂⡁⠂⠄⡡⠂⠌⡐⠠⠑⢢⠐⡐⠈⠄⠃⡔⠡⠈⢆⠡⠊⠤⠘⢠⠑⡂⠱⠈⠆⡱⢈
⣂⠦⡰⢁⠎⡰⢄⠃⣘⠰⢄⠣⡌⣅⠒⡄⡃⢆⠩⢀⠂⠄⠃⠤⢑⠨⢡⠘⠤⡘⠤⡘⠠⡉⠄⢂⠀⠀⢣⠊⠜⡠⢂⡑⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠐⠀⠀⠀⠀⠀⠌⡀⠂⠌⠀⠀⠀⡐⠂⠀⠀⠀⠀⠀⠀⠀⠀⠀⠠⢀⠐⠠⠈⠄⠠⠐⡀⠁⠐⡀⢁⠂⠄⡁⠃⠄⠡⡀⠡⢊⠐⢨⠐⡡⢈⡐⢉⠤⠉⡄⢂⠌⡄⡁⢂⠅⢢
⡜⢢⠱⡉⠦⡑⠢⡘⢄⠣⢎⠢⡑⠄⢣⡘⡱⣈⠆⣡⠊⢄⡑⠌⠢⡑⠢⢉⠖⠡⢂⠱⢡⠘⡄⢂⠐⡀⠀⠉⢆⠱⠈⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡀⠆⠀⠈⠰⠁⠌⠀⢠⠐⠂⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡐⠀⠠⠀⠡⠈⠀⢁⠐⠠⢀⠐⡀⠂⡐⠀⠂⠌⡐⠠⢁⠂⠌⠠⡑⠄⠃⠄⠈⠠⢁⠐⠌⡒⠤⡑⢨⠘⠤
⡜⣡⠣⠜⡰⢁⠣⡐⢌⠱⡈⠔⡡⢊⠤⡑⢡⠂⢌⠠⢁⠂⠰⣈⠐⠤⣁⠢⢌⠑⡈⢆⠃⢆⠘⡀⢆⠰⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⡀⠔⡠⢀⠤⡐⡠⢁⠌⡀⠀⠑⠈⠂⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠠⠐⡀⠀⠄⡁⢂⠐⠠⢁⠀⠂⡐⠠⢀⠀⡁⠠⠈⠀⠐⠠⢁⠂⠌⠠⢁⠰⢈⠡⠈⠄⡀⠂⡌⠰⢠⣁⠒⡠⠉⡔
⡜⣡⢋⠆⣁⠂⢅⠒⠌⢂⠔⢡⠂⡅⢢⠑⢢⠉⡄⠒⡄⣈⠐⡠⠌⡐⢠⠐⡈⠐⠠⡀⠎⢂⡁⢀⠂⠒⡄⢂⠐⡠⠀⠀⠀⠀⠀⢀⠂⠄⣃⠐⡈⢆⠐⡁⠂⢆⠰⡀⠀⢀⡀⠄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠌⠐⠠⢀⠁⢂⠐⡀⠂⡁⠀⠄⡀⠀⠄⠀⡀⠀⠐⠀⠡⢈⠐⠠⠈⡀⠁⠎⡐⢂⠀⡁⠂⠐⠀⡀⢃⠆⠠⢓⡐⢣⠘
⠜⡤⢣⠘⠠⠌⡀⢊⠰⢁⠎⠤⠑⡈⠤⢉⠂⡜⢀⠣⠐⡀⢂⠁⢂⠑⢢⠁⠘⣈⠡⠐⢨⠐⡀⠀⠌⠡⡐⢀⠂⡑⠀⢂⡐⠀⣁⠂⠌⠰⡀⠅⠒⡀⢂⠐⡉⠄⠣⠌⠑⠂⢌⠘⠄⠀⠀⠀⠀⠀⠀⠀⠄⡁⠂⠈⠠⢀⠈⡀⠠⠀⠡⠀⠡⠀⢀⠠⠀⠂⢀⠐⠈⠀⠁⠀⠈⠄⠡⢀⠁⢂⠐⠈⠄⠀⠀⠡⠐⠠⠡⠌⡑⢢⠘⣄⠣
⠚⡴⢡⠘⡄⢢⠐⠡⢂⠌⡘⡄⢃⠰⡈⢆⠡⡐⢌⠰⠁⡄⠠⠌⡀⠎⡰⢈⠰⠀⢂⠈⠔⠂⠄⡐⠈⡐⠄⢃⠐⡈⠔⠠⢀⠣⠀⠎⡀⣁⠐⣈⠡⠐⡀⠂⢌⠰⡁⢎⠠⠉⡄⠌⡰⠁⠄⡀⠀⠀⠀⡈⠐⠀⡀⠁⠄⠂⠠⠐⡀⠂⠀⡐⠀⠀⠀⠠⠀⠐⠀⠠⠀⠁⡈⠄⠀⢂⠐⠂⢌⠀⠂⠄⠂⠀⠐⠀⠠⠁⠒⢠⠁⡆⢡⢂⠡
⡹⣐⠣⡑⢌⠰⡈⢆⠡⢊⠔⡈⢂⠡⠌⢠⢃⠜⡀⢂⠅⡈⢁⢂⠐⠠⠁⠂⡄⠡⢀⡐⢈⠆⡐⠀⠀⠰⠈⡄⠂⢄⠨⠑⢢⠐⣡⢂⠱⢀⠃⠄⠢⢁⠄⠡⢊⠄⡱⢈⠆⡱⠐⡈⠄⡡⢈⠁⠠⠀⡐⠀⠠⠁⠀⡀⠀⠀⠀⠀⠀⠄⠁⠀⠈⢀⠠⠐⠀⠄⠀⢂⠀⡐⠀⠄⡈⠄⡈⠐⠀⠂⠈⠀⠀⠀⠀⠀⠀⠠⠉⢄⠘⠄⠣⢌⠒
⡱⢌⠓⡈⠆⡑⠰⠈⠆⢃⠂⠌⢢⢁⠢⡐⠌⡒⢠⠈⡐⠄⡂⠌⠠⢁⠂⠡⠄⡁⠂⠄⠃⡄⠠⢁⠀⢡⠂⡘⡁⢂⠨⡐⠄⢢⠀⠂⠄⠃⡌⠰⢁⠊⢄⡑⠌⢢⠐⣁⠊⡀⠑⠌⡐⠐⡠⠈⠄⠁⠠⠈⠀⠀⠁⢀⠠⠁⠀⠂⠀⠀⠐⠀⠀⠠⠀⠠⠁⠀⠂⢀⠐⡀⠂⠄⠐⠠⠐⡀⠌⡐⠀⢂⠀⠁⠀⠀⠀⠁⠌⠠⢈⢈⠱⠈⠆
⢱⢋⡔⠰⠠⠄⠂⠤⡐⢀⠢⠈⠔⢂⠡⡘⢠⠘⡀⠆⠡⠘⡀⠄⠡⠂⠌⡐⠄⠠⠡⠌⠒⠠⢁⠂⠈⠄⠒⠠⡈⠄⠒⠠⠘⠠⠈⠡⢈⠡⠄⣁⠊⡐⢠⠂⢌⠢⡑⠠⠒⡈⡁⠆⠠⠑⢠⠁⢀⠡⠀⠠⠁⢀⠈⠀⡀⠀⠁⠠⠁⠀⠂⠀⠀⠀⢀⠀⠐⠀⠡⠀⠀⠄⠡⠐⡈⠄⠡⠐⠠⠀⠐⠀⠀⠀⠀⠀⠀⠀⢈⠰⡈⠆⠨⢑⡈
mao */
