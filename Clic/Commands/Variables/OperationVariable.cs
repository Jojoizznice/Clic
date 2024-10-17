using org.mariuszgromada.math.mxparser;

namespace Clic.Commands.Variables;

internal class OperationVariable : Variable
{
    public override string Name { get; }

    public double Value {
        get
        {
            if (_value.HasValue && _value.Value.oldHash == _hash)
            {
                return _value.Value.cache;
            }

            return RecalculateValue();
        } 
    }

    private double RecalculateValue()
    {
        if (oldValue == _var.Value)
        {
            return oldValue;
        }

        Console.WriteLine("calculated " + _operation.Replace("$", _var.Value.ToString()));
        Expression ex = new(_operation.Replace("$", _var.Value.ToString()));
        _hash++;
        _value = (_hash, ex.calculate());
        oldValue = _var.Value;
        return _value.Value.cache;
    }

    private readonly string _operation;
    private readonly DoubleVariable _var;

    private ulong _hash;
    private (ulong oldHash, double cache)? _value;
    private double oldValue;

    public OperationVariable(string name, DoubleVariable var, string operation) : base(true)
    {
        Name = name;
        _operation = operation;
        _var = var;
        _hash = 0;
    }

    public override string GetValueString()
    {
        return Value.ToString();
    }

    public override int GetVariableType() => 3;
}
