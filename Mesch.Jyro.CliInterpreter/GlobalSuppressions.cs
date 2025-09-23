// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "Sometimes changes semantics")]
[assembly: SuppressMessage("Performance", "CA1869:Cache and reuse 'JsonSerializerOptions' instances", Justification = "We only call it once", Scope = "member", Target = "~M:Mesch.Jyro.CliInterpreter.JyroScriptExecutor.OutputExecutionResultsAsync(Mesch.Jyro.JyroExecutionResult)~System.Threading.Tasks.Task")]
