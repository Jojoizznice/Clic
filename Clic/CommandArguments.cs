using Clic.Commands.Variables;

namespace Clic;

public class CommandArguments
{
    public required WorkingFolder WorkingFolder { get; init; }
    public required VariableHolder VariableHolder { get; init; }
    public required TextProvider TextProvider { get; init; }
}

public class CommandReturnValue
{
    private readonly double? _valueDouble;
    private readonly string? _valueString;
    private readonly bool _isString;

    public CommandReturnValue(double value)
    {
        _valueDouble = value;
        _isString = false;
    }

    public CommandReturnValue(string value)
    {
        _valueString = value;
        _isString = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="valueDouble"></param>
    /// <param name="valueString"></param>
    /// <returns>isString</returns>
    public bool GetValue(out double valueDouble, out string valueString)
    {
        valueDouble = _valueDouble ?? 0;
        valueString = _valueString ?? null!;
        return _isString;
    }

    public Variable AsVariable(string varName)
    {
        return Variable.CreateVariable(varName, _isString ? _valueString! : _valueDouble!.Value.ToString(), true);
    }
}