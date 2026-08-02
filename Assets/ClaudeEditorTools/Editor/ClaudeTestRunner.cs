using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

[InitializeOnLoad]
public static class ClaudeTestRunner
{
    public const string OutPath =
        @"C:\Users\haako\Documents\Unity Games\Moonlighter\.superpowers\sdd\2026-07-26-recipe-book-ui\testresult.txt";

    static ClaudeTestRunner()
    {
        var api = ScriptableObject.CreateInstance<TestRunnerApi>();
        api.RegisterCallbacks(ScriptableObject.CreateInstance<ClaudeTestCallbacks>());
    }

    [MenuItem("Claude/Run EditMode Tests")]
    public static void RunEditMode() => Run(TestMode.EditMode);

    [MenuItem("Claude/Run PlayMode Tests")]
    public static void RunPlayMode() => Run(TestMode.PlayMode);

    private static void Run(TestMode mode)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(OutPath));
        File.WriteAllText(OutPath, "RUNNING");
        var api = ScriptableObject.CreateInstance<TestRunnerApi>();
        api.Execute(new ExecutionSettings(new Filter { testMode = mode }));
    }
}

public class ClaudeTestCallbacks : ScriptableObject, ICallbacks
{
    public void RunStarted(ITestAdaptor testsToRun) { }

    public void TestStarted(ITestAdaptor test) { }

    public void TestFinished(ITestResultAdaptor result) { }

    public void RunFinished(ITestResultAdaptor result)
    {
        var sb = new StringBuilder();
        sb.AppendLine("DONE");
        sb.AppendLine($"passed={result.PassCount} failed={result.FailCount} skipped={result.SkipCount} inconclusive={result.InconclusiveCount}");
        AppendFailures(result, sb);
        File.WriteAllText(ClaudeTestRunner.OutPath, sb.ToString());
    }

    private static void AppendFailures(ITestResultAdaptor result, StringBuilder sb)
    {
        if (!result.HasChildren)
        {
            if (result.TestStatus == TestStatus.Failed)
            {
                sb.AppendLine($"FAIL: {result.FullName}");
                sb.AppendLine($"  {result.Message}");
            }
            return;
        }

        foreach (var child in result.Children)
            AppendFailures(child, sb);
    }
}
