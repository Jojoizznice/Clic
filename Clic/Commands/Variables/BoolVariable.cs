namespace Clic.Commands.Variables;

public class BoolVariable : Variable
{
    public override string Name { get; }
    public bool Value
    {
        get 
        {
            return _value;
        }
        set
        {
            if (IsImmutable)
            {
                throw new InvalidOperationException($"Variable {Name} is invalid");
            }
            _value = value;
        }
    }
    private bool _value;

    public BoolVariable(string name, bool value, bool isImmutable) : base(isImmutable)
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
        if (!bool.TryParse(value, out bool parsed))
        {
            var = null!;
            return false;
        }

        var = new BoolVariable(name, parsed, isImmutable);
        return true;
    }

    public override int GetVariableType() => 2;
}
