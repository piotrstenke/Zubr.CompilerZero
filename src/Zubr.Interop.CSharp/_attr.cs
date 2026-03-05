using System;
using System.ComponentModel;

namespace zubr.interop.csharp;

[Obsolete(Constants.ObsoleteMessage, DiagnosticId = Constants.ObsoleteDiagnosticId)]
[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class InternalInheritAttribute : Attribute
{
}

[Obsolete(Constants.ObsoleteMessage, DiagnosticId = Constants.ObsoleteDiagnosticId)]
[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class MustOverrideAttribute : Attribute
{
}

[Obsolete(Constants.ObsoleteMessage, DiagnosticId = Constants.ObsoleteDiagnosticId)]
[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class InvokerAttribute : Attribute
{
}

[Obsolete(Constants.ObsoleteMessage, DiagnosticId = Constants.ObsoleteDiagnosticId)]
[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = true)]
public sealed class DefaultTypeParameterAttribute(Type type) : Attribute
{
	public Type Type { get; } = type;
}

[Obsolete(Constants.ObsoleteMessage, DiagnosticId = Constants.ObsoleteDiagnosticId)]
[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = true)]
public sealed class ImplDeclarationAttribute(Type type) : Attribute
{
	public Type Type { get; } = type;
}
