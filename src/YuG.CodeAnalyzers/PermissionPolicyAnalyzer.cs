using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace YuG.CodeAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PermissionPolicyAnalyzer : DiagnosticAnalyzer
{
    private const string ErrorId = "YUG001";
    private const string SummaryId = "YUG002";

    private static readonly DiagnosticDescriptor ErrorRule = new(
        ErrorId,
        "权限编码格式不匹配",
        "授权策略名 '{0}' 与预期 '{1}' 不匹配，应为 {{控制器名小写}}:{{方法名小写}} 格式",
        "Naming",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor SummaryRule = new(
        SummaryId,
        "权限编码检查汇总",
        "{0}",
        "Naming",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(ErrorRule, SummaryRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var results = new ConcurrentBag<CheckResult>();

        context.RegisterSyntaxNodeAction(ctx =>
        {
            var result = CheckMethod(ctx);
            if (result is not null)
                results.Add(result);
        }, SyntaxKind.MethodDeclaration);

        context.RegisterCompilationEndAction(ctx =>
        {
            var allResults = results.ToArray();
            if (allResults.Length == 0) return;

            var passed = allResults.Count(r => r.Matched);
            var failed = allResults.Length - passed;

            var message = failed == 0
                ? $"权限编码检查完成：共检查 {allResults.Length} 个方法，全部匹配 ✓"
                : $"权限编码检查完成：共检查 {allResults.Length} 个方法，{passed} 个匹配 ✓，{failed} 个不匹配 ✗";

            ctx.ReportDiagnostic(Diagnostic.Create(SummaryRule, Location.None, message));
        });
    }

    private sealed class CheckResult
    {
        public string MethodFullName { get; }
        public string Policy { get; }
        public bool Matched { get; }

        public CheckResult(string methodFullName, string policy, bool matched)
        {
            MethodFullName = methodFullName;
            Policy = policy;
            Matched = matched;
        }
    }

    private static CheckResult? CheckMethod(SyntaxNodeAnalysisContext context)
    {
        var methodDecl = (MethodDeclarationSyntax)context.Node;

        var authorizeAttr = methodDecl.AttributeLists
            .SelectMany(al => al.Attributes)
            .FirstOrDefault(a => IsAuthorizeAttribute(a));

        if (authorizeAttr?.ArgumentList is null)
            return null;

        var policyArg = authorizeAttr.ArgumentList.Arguments
            .FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == "Policy");

        if (policyArg?.Expression is not LiteralExpressionSyntax policyLiteral)
            return null;

        var actualPolicy = context.SemanticModel.GetConstantValue(policyLiteral).Value as string;
        if (string.IsNullOrEmpty(actualPolicy))
            return null;

        var classDecl = methodDecl.Ancestors()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();

        if (classDecl is null)
            return null;

        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl);
        if (classSymbol is null)
            return null;

        if (!IsControllerBaseSubclass(classSymbol))
            return null;

        if (!classDecl.AttributeLists
            .SelectMany(al => al.Attributes)
            .Any(a => a.Name.ToString() is "ApiController" or "ApiControllerAttribute"))
            return null;

        var controllerName = classDecl.Identifier.Text;
        if (controllerName.EndsWith("Controller"))
            controllerName = controllerName.Substring(0, controllerName.Length - "Controller".Length);

        var methodName = methodDecl.Identifier.Text;
        var expectedPolicy = $"{controllerName.ToLowerInvariant()}:{methodName.ToLowerInvariant()}";

        var matched = actualPolicy == expectedPolicy;

        if (!matched)
        {
            var error = Diagnostic.Create(ErrorRule, policyArg.GetLocation(), actualPolicy, expectedPolicy);
            context.ReportDiagnostic(error);
        }

        return new CheckResult(
            $"{classSymbol.ContainingNamespace}.{classSymbol.Name}",
            actualPolicy,
            matched);
    }

    private static bool IsAuthorizeAttribute(AttributeSyntax attr)
    {
        var name = attr.Name.ToString();
        return name is "Authorize" or "AuthorizeAttribute";
    }

    private static bool IsControllerBaseSubclass(INamedTypeSymbol typeSymbol)
    {
        var current = typeSymbol.BaseType;
        while (current is not null)
        {
            if (current.Name == "ControllerBase")
                return true;
            current = current.BaseType;
        }
        return false;
    }
}
