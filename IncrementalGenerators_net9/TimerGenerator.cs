using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace IncrementalGenerators_net9
{
    [Generator]
    public class TimerGenerator : IIncrementalGenerator
    {
        private static string _currentNamespace = MethodBase.GetCurrentMethod()?.DeclaringType?.Namespace;
        static TimerGenerator()
        {
            Console.WriteLine("TimerGenerator ctor");
        }
        
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var currentNamespace = MethodBase.GetCurrentMethod()?.DeclaringType?.Namespace;

            context.RegisterPostInitializationOutput(static postInitializationContext =>
                postInitializationContext.AddSource("GeneratedAttribute.cs", SourceText.From($$$"""
                    using System;
                    namespace {{{_currentNamespace}}}
                    {
                        [AttributeUsage(AttributeTargets.Method)]
                        internal sealed class TimedAttribute : Attribute
                        {
                        }
                    }
                    """, Encoding.UTF8)));

            var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: $"{currentNamespace}.TimedAttribute",
                predicate: static (syntaxNode, cancellationToken) => syntaxNode is BaseMethodDeclarationSyntax,
                transform: static (innerContext, cancellationToken) =>
                {
                    var containingClass = innerContext.TargetSymbol.ContainingType;

                    return new MethodModel(
                        Namespace: containingClass.ContainingNamespace?.ToDisplayString(
                            SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(
                                SymbolDisplayGlobalNamespaceStyle.Omitted)),
                        ClassName: containingClass.Name,
                        MethodName: innerContext.TargetSymbol.Name
                    );
                }
            );

            context.RegisterSourceOutput(pipeline, (innerContext, model) =>
            {
                var sourceText = SourceText.From($$$"""
                                                    using System;
                                                    using System.Diagnostics;
                                                    namespace {{{model.Namespace}}};
                                                    public partial class {{{model.ClassName}}}
                                                    {
                                                        public void Incremental_{{{model.MethodName}}}()
                                                        {
                                                             long timestamp = Stopwatch.GetTimestamp();
                                                             //Console.WriteLine($""正在生成 {{{model.Namespace}}}.{{{model.ClassName}}} 的 Incremental_{{{model.MethodName}}} 方法"");
                                                             {{{model.MethodName}}}();
                                                             Console.WriteLine($"Incremental_{{{model.MethodName}}} Elapsed time: {Stopwatch.GetElapsedTime(timestamp)} ");
                                                        }
                                                    }
                                                    """, Encoding.UTF8);


                innerContext.AddSource($"{model.ClassName}_{model.MethodName}.g.cs", sourceText);
            });
        }
        private record MethodModel(string Namespace, string ClassName, string MethodName);
    }
}