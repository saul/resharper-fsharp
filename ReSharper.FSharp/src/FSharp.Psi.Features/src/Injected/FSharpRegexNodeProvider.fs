namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Injected

open System.Text.RegularExpressions
open JetBrains.ProjectModel
open JetBrains.ReSharper.Features.RegExp
open JetBrains.ReSharper.Plugins.FSharp.Psi
open JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Util
open JetBrains.ReSharper.Plugins.FSharp.Psi.Impl
open JetBrains.ReSharper.Plugins.FSharp.Psi.Tree
open JetBrains.ReSharper.Psi
open JetBrains.ReSharper.Psi.CodeAnnotations
open JetBrains.ReSharper.Psi.Impl.Shared.InjectedPsi
open JetBrains.ReSharper.Psi.Tree
open JetBrains.ReSharper.Psi.Util

[<SolutionComponent>]
type FSharpRegexNodeProvider(languageManager: ILanguageManager) =
    let invocationUtil = languageManager.GetService<IFSharpMethodInvocationUtil>(FSharpLanguage.Instance)

    let attributes = [| RegexPatternAnnotationProvider.RegexPatternAttributeShortName |]

    let rec evalOptionsArg (expr: IFSharpExpression) =
        match expr.IgnoreInnerParens() with
        | :? IReferenceExpr as refExpr ->
            match refExpr.Reference.Resolve().DeclaredElement with
            | :? IField as field when field.IsEnumMember || field.IsConstant ->
                let typeElement = field.Type.GetTypeElement()
                if isNotNull typeElement && typeElement.GetClrName() = RegExpPredefinedType.REGEX_OPTIONS_FQN then
                    let value = field.ConstantValue.Value :?> RegexOptions
                    value
                else
                    RegexOptions.None
            | _ -> RegexOptions.None

        | :? IBinaryAppExpr as binaryAppExpr ->
            let left = evalOptionsArg binaryAppExpr.LeftArgument
            let right = evalOptionsArg binaryAppExpr.RightArgument

            if isPredefinedInfixOpApp "|||" binaryAppExpr then left ||| right
            elif isPredefinedInfixOpApp "&&&" binaryAppExpr then left &&& right
            elif isPredefinedInfixOpApp "^^^" binaryAppExpr then left ^^^ right
            else RegexOptions.None

        | _ -> RegexOptions.None

    interface IInjectionNodeProvider with
        override _.Check(node, _, data) =
            data <- null

            let expr = node.Parent.As<ILiteralExpr>()
            if isNull expr then false else

            let argument = expr.As<IArgument>()
            if isNull argument then false else

            let argumentsOwner = invocationUtil.GetArgumentsOwner(expr)
            if isNull argumentsOwner then false else

            let matchingParam = argument.MatchingParameter
            if isNull matchingParam then false else

            let param = matchingParam.Element
            if isNull param || param.ShortName <> "pattern" then false else

            let containingType = param.ContainingParametersOwner.GetContainingType()
            if isNull containingType then false else

            let fullTypeName = containingType.GetClrName()
            if fullTypeName <> RegExpPredefinedType.REGEX_FQN then false else

            let optionsArg =
                argumentsOwner.Arguments
                |> Seq.tryPick (fun arg ->
                    match arg with
                    | :? IFSharpExpression as fsharpArg ->
                        match arg.MatchingParameter with
                        | null -> None
                        | matchingParam when matchingParam.Element.ShortName = "options" -> Some fsharpArg
                        | _ -> None
                    | _ -> None)

            data <-
                match optionsArg with
                | None -> RegexOptions.None
                | Some optionsArg -> evalOptionsArg optionsArg

            true

        override _.GetPrefix(_, _) = null
        override _.GetSuffix(_, _) = null
        override _.SupportedOriginalLanguage = FSharpLanguage.Instance :> _
        override _.ProvidedLanguageID = "FsRegex"
        override _.Summary = ".NET Regular Expressions in F#"
        override _.Description = "Injects .NET Regular Expression in calls from F# code to Regex members"
        override _.Guid = "7e4d8d57-335f-4692-9ff8-6b2fa003fb51"
        override _.Words = null
        override _.Attributes = attributes
