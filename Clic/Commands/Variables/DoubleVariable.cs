using System.Globalization;

namespace Clic.Commands.Variables;

public class DoubleVariable : Variable
{
    public override string Name { get; }
    public double Value
    {
        get
        {
            return _value;
        }
        set
        {
            if (IsImmutable)
            {
                throw new InvalidOperationException($"Variable {Name} is immutable");
            }
            _value = value;
        }
    }
    private double _value;

    public DoubleVariable(string name, double value, bool isImmutable) : base(isImmutable)
    {
        Name = name;
        _value = value;
    }

    public override string GetValueString()
    {
        return Value.ToString();
    }

    public static bool TryCreate(string name, string value, bool isImmutable, out Variable var)
    {
        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out double parsed))
        {
            var = null!;
            return false;
        }

        var = new DoubleVariable(name, parsed, isImmutable);
        return true;
    }

    public override int GetVariableType() => 1;
}
