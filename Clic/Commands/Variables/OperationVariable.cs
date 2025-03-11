using org.mariuszgromada.math.mxparser;

namespace Clic.Commands.Variables;

internal class OperationVariable : Variable
{
    public override string Name { get; }

    private readonly string _operation;
    private readonly DoubleVariable _var;

    private ulong _hash;
    private (ulong oldHash, double cache)? _value;
    private double oldValue;

    bool ValueIsValid()
    {
        return
            _value.HasValue &&
            _value.Value.oldHash == _hash &&
            oldValue == _var.Value;
    }
   
    public double Value {
        get
        {
            if (ValueIsValid())
            {
                return _value! /* Checked in ValueIsValid() */ .Value.cache;
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

        Expression ex = new(_operation.Replace("$", _var.Value.ToString()));
        _hash++;
        _value = (_hash, ex.calculate());
        oldValue = _var.Value;
        return _value.Value.cache;
    }

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

    public string GetOperation() => _operation;
}
