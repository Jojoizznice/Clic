using System.Diagnostics;

namespace Clic.Commands.Variables;

public abstract class Variable
{
    public bool IsImmutable { get; } = false;
    
    public abstract string Name { get; }

    public abstract string GetValueString();

    private protected Variable(bool isImmutable)
    {
        IsImmutable = isImmutable;
    } 

    public static Variable CreateVariable(string name, string value, bool isImmutable)
    {
        bool bResult = BoolVariable.TryCreate(name, value, isImmutable, out Variable var);
        if (bResult)
            return var;

        bool dResult = DoubleVariable.TryCreate(name, value, isImmutable, out var);
        if (dResult) 
            return var;

        bool sResult = StringVariable.TryCreate(name, value, isImmutable, out var);
        if (sResult)
            return var;

        throw new UnreachableException($"""no var type (name: "{name}"; text: "{value}"), stringVar is {var}""");
    }

    public static Variable GetDefault()
    {
        return new DefaultVariableProvider();
    }

    /// <summary>
    /// Gets the variable type
    /// </summary>
    /// <returns>0 => <see cref="StringVariable"/>;<br></br>
    /// 1 => <see cref="DoubleVariable"/>;<br></br>
    /// 2 => <see cref="BoolVariable"/>;<br></br>
    /// 3 => <see cref="OperationVariable"/>;<br></br>
    /// -2 => <see cref="DefaultVariableProvider"/>;</returns>
    public abstract int GetVariableType();

    private class DefaultVariableProvider : Variable
    {
        public DefaultVariableProvider() : base(true) { }

        public override string Name => throw new NotImplementedException();

        public override string GetValueString()
        {
            throw new NotImplementedException();
        }

        public override int GetVariableType()
        {
            return -2;
        }
    }
}
