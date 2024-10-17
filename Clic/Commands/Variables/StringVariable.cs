namespace Clic.Commands.Variables;

public class StringVariable : Variable
{
    public override string Name { get; }
    public string Value
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
    private string _value;

    public StringVariable(string name, string value, bool isImmutable) : base(isImmutable)
    {
        Name = name;
        _value = value;
    }

    public override string GetValueString()
    {
        return Value;
    }

    public static bool TryCreate(string name, string value, bool isImmutable, out Variable var)
    {
        var = new StringVariable(name, value, isImmutable);
        return true;
    }

    public override int GetVariableType() => 0;
}
