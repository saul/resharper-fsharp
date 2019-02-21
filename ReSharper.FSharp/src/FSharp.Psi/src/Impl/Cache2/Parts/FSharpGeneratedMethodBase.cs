using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.DeclaredElement.CompilerGenerated;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Caches2;
using JetBrains.ReSharper.Psi.ExtensionsAPI.DeclaredElements;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.Util;

namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Cache2.Parts
{
  public abstract class FSharpGeneratedMethodBase : FSharpGeneratedFunctionBase, IMethod
  {
    [NotNull] protected readonly TypeElement ContainingType;

    protected FSharpGeneratedMethodBase([NotNull] ITypeElement containingType) =>
      ContainingType = (TypeElement) containingType;

    protected override IClrDeclaredElement ContainingElement => ContainingType;

    protected IType ContainingTypeType =>
      ContainingType.WithIdSubstitution();

    public override ITypeElement GetContainingType() => ContainingType;
    public override ITypeMember GetContainingTypeMember() => ContainingType;

    public override DeclaredElementType GetElementType() =>
      CLRDeclaredElementType.METHOD;

    public IList<ITypeParameter> TypeParameters => EmptyList<ITypeParameter>.Instance;
    public override IList<IParameter> Parameters => EmptyList<IParameter>.Instance;

    public override ISubstitution IdSubstitution =>
      MethodIdSubstitution.Create(this);

    public bool IsXamlImplicitMethod => false;
    public bool IsExtensionMethod => false;
    public bool IsVarArg => false;
    public bool IsAsync => false;
  }
}
