using System;
using System.Collections;
using System.Diagnostics;
using UnityEditor;

public static class ProcessCommandUtility
{
    public static void ProcessCommand(string fileName, string argument, string workingDirectory)
    {
        UnityEngine.Debug.LogFormat("Execute {2} -> {0}/{1}", workingDirectory, argument, fileName);
        System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo(fileName);
        processStartInfo.Arguments = argument;
        processStartInfo.CreateNoWindow = false;
        processStartInfo.ErrorDialog = true;
        processStartInfo.UseShellExecute = true;
        processStartInfo.WorkingDirectory = workingDirectory;

        if (processStartInfo.UseShellExecute)
        {
            processStartInfo.RedirectStandardOutput = false;
            processStartInfo.RedirectStandardError = false;
            processStartInfo.RedirectStandardInput = false;
        }
        else
        {
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
            processStartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
        }

        System.Diagnostics.Process process = System.Diagnostics.Process.Start(processStartInfo);

        if (process == null) return;

        process.WaitForExit();
        process.Close();
    }


    public static IEnumerator ProcessCommandAsync(string fileName, string argument, string workingDirectory, Action callback)
    {
        UnityEngine.Debug.LogFormat("Execute {2} -> {0}/{1}", workingDirectory, argument, fileName);
        ProcessStartInfo processStartInfo = new ProcessStartInfo(fileName);
        processStartInfo.Arguments = argument;
        processStartInfo.CreateNoWindow = true;
        processStartInfo.ErrorDialog = true;
        processStartInfo.UseShellExecute = true;
        processStartInfo.WorkingDirectory = workingDirectory;
        // processStartInfo.Arguments = "-c \"echo $PATH\""; // 查看 PATH

        if (!processStartInfo.EnvironmentVariables.ContainsKey("DOTNET_ROOT"))
            processStartInfo.EnvironmentVariables.Add("DOTNET_ROOT", "/usr/local/share/dotnet");
        // processStartInfo.EnvironmentVariables.Add("PATH", $"/usr/local/share/dotnet:{Environment.GetEnvironmentVariable("PATH")}");
        // {
        //     // // 添加关键环境变量
        //     // ["PATH"] = $"/usr/local/share/dotnet:{Environment.GetEnvironmentVariable("PATH")}",
        //     //     ["DOTNET_ROOT"] = "/usr/local/share/dotnet"
        // }

        if (processStartInfo.UseShellExecute)
        {
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
            processStartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
        }
        else
        {
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
            processStartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
        }

        using (Process process = System.Diagnostics.Process.Start(processStartInfo))
        {

            process.OutputDataReceived += (sender, e) =>
           {
               if (!string.IsNullOrEmpty(e.Data))
               {
                   UnityEngine.Debug.Log($"Shell 输出: {e.Data}");
               }
           };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    UnityEngine.Debug.LogError($"Shell 错误: {e.Data}");
                }
            };


            // 启动进程
            process.Start();

            // 开始异步读取输出
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // 等待进程结束（在协程中异步等待）
            while (!process.HasExited)
            {
                yield return null;
            }

            AssetDatabase.Refresh();
            // 输出退出码
            UnityEngine.Debug.Log($"脚本执行完成，退出码：{process.ExitCode}");
        }


        callback?.Invoke();

    }
}
