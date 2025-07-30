namespace Renova.Autofac;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public abstract class AopAttribute : Attribute
{
    public int Order { get; set; } = 0; // 控制执行顺序
}
