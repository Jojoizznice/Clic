using System.Diagnostics;

namespace Clic.Commands.Variables;

public class VariableHolder
{
    readonly List<DoubleVariable> doubleVariables = [];
    readonly List<BoolVariable> boolVariables = [];
    readonly List<StringVariable> stringVariables = [];
    readonly List<OperationVariable> operationVariables = [];

    public void Clear()
    {
        doubleVariables.Clear();
        boolVariables.Clear();
        stringVariables.Clear();
    }

    public void AddVariable(Variable var)
    {
        if (FindName(var.Name) != null)
        {
            throw new Exception("already exists");
        }

        int varType = var.GetVariableType();

        switch (varType)
        {
            case 0:
                stringVariables.Add((var as StringVariable)!);
                break;
            case 1:
                doubleVariables.Add((var as DoubleVariable)!);
                break;
            case 2:
                boolVariables.Add((var as BoolVariable)!);
                break;
            case 3:
                operationVariables.Add((var as OperationVariable)!);
                break;
            default:
                throw new UnreachableException("No type was found, i " + varType);
        }
    }

    private (int list, int index) FindVariable(Variable var)
    {
        int varType = var.GetVariableType();

        switch (varType)
        {
            case 0:
                int i = stringVariables.IndexOf((var as StringVariable)!);
                return (varType, i);
            case 1:
                int j = doubleVariables.IndexOf((var as DoubleVariable)!);
                return (varType, j);
            case 2:
                int k = boolVariables.IndexOf((var as BoolVariable)!);
                return (varType, k);
            case 3:
                int l = operationVariables.IndexOf((var as OperationVariable)!);
                return (varType, l);
            default:
                throw new UnreachableException("No type was found, i " + varType);
        }
    }

    public Variable? FindName(string name)
    {
        foreach (Variable va in doubleVariables)
        {
            if (va.Name == name) return va;
        }

        foreach (Variable va in boolVariables)
        {
            if (va.Name == name) return va;
        }

        foreach (Variable va in stringVariables)
        {
            if (va.Name == name) return va;
        }

        foreach (Variable va in operationVariables)
        {
            if (va.Name == name) return va;
        }

        return null;
    }

    public Variable? FindName(ReadOnlySpan<char> name)
    {
        foreach (Variable va in doubleVariables)
        {
            if (va.Name == name) return va;
        }

        foreach (Variable va in boolVariables)
        {
            if (va.Name == name) return va;
        }

        foreach (Variable va in stringVariables)
        {
            if (va.Name == name) return va;
        }

        foreach (Variable va in operationVariables)
        {
            if (va.Name == name) return va;
        }

        return null;
    }

    public void RemoveVariable(Variable var)
    {
        var (list, index) = FindVariable(var);
        switch (list)
        {
            case 0:
                stringVariables.RemoveAt(index);
                return;
            case 1:
                doubleVariables.RemoveAt(index);
                return;
            case 2:
                boolVariables.RemoveAt(index);
                return;
            case 3:
                operationVariables.RemoveAt(index);
                return;
        }
    }

    public Variable[] GetAllVariables()
    {
        List<Variable> list = [];

        list.AddRange(doubleVariables.Cast<Variable>());
        list.AddRange(boolVariables.Cast<Variable>());
        list.AddRange(stringVariables.Cast<Variable>());
        list.AddRange(operationVariables.Cast<Variable>());

        return [.. list];
    }
}
