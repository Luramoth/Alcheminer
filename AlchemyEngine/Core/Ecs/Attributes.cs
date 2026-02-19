namespace AlchemyEngine.Core.Ecs;

[AttributeUsage(AttributeTargets.Struct)]
public sealed class ComponentAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ReadAttribute<T> : Attribute where T : struct;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class WriteAttribute<T> : Attribute where T : struct;
