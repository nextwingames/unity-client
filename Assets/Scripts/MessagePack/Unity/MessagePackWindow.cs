#if UNITY_EDITOR

using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace MessagePack.Unity.Editor
{
    internal enum EOSPlatform
    {
        Window,
        Unix
    }

    internal class MessagePackWindow:EditorWindow
    {
        static MessagePackWindow window;
        
        public static EOSPlatform OS { get; private set; }
        public static string Dotnet { get; private set; }
        public static string Mpc { get; private set; }

        bool processInitialized;

        bool isDotnetInstalled;
        string dotnetVersion;

        bool isInstalledMpc;
        bool installingMpc;
        bool invokingMpc;

        MpcArgument mpcArgument;

        [MenuItem("Window/MessagePack/CodeGenerator")]
        public static void OpenWindow()
        {
            if(window != null)
            {
                window.Close();
            }

            // will called OnEnable(singleton instance will be set).
            GetWindow<MessagePackWindow>("MessagePack CodeGen").Show();
        }

        async void OnEnable()
        {
            CheckAndSetOS();

            window = this; // set singleton.
            try
            {
                var dotnet = await ProcessHelper.FindDotnetAsync();
                isDotnetInstalled = dotnet.found;
                dotnetVersion = dotnet.version;

                if(isDotnetInstalled)
                {
                    isInstalledMpc = await ProcessHelper.IsInstalledMpc();
                }
            }
            finally
            {
                mpcArgument = MpcArgument.Restore();
                processInitialized = true;
            }
        }

        void CheckAndSetOS()
        {
            string osPlatform = Environment.OSVersion.Platform.ToString();

            if(osPlatform.Contains("Win"))
            {
                OS = EOSPlatform.Window;

                Dotnet = "dotnet";
                Mpc = "mpc";
            }
            else if(osPlatform.Contains("Unix"))
            {
                OS = EOSPlatform.Unix;

                Dotnet = "/usr/local/share/dotnet/dotnet";
                Mpc = "/usr/local/share/dotnet/dotnet";
            }
        }

        async void OnGUI()
        {
            if(!processInitialized)
            {
                GUILayout.Label("Check .NET Core SDK/CodeGen install status.");
                return;
            }
            if(mpcArgument == null)
            {
                return;
            }

            if(!isDotnetInstalled)
            {
                GUILayout.Label(".NET Core SDK not found.");
                GUILayout.Label("MessagePack CodeGen requires .NET Core Runtime.");
                if(GUILayout.Button("Open .NET Core install page."))
                {
                    Application.OpenURL("https://dotnet.microsoft.com/download");
                }
                return;
            }

            if(!isInstalledMpc)
            {
                GUILayout.Label("MessagePack CodeGen does not installed.");
                EditorGUI.BeginDisabledGroup(installingMpc);

                if(GUILayout.Button("Install MessagePack CodeGen."))
                {
                    installingMpc = true;
                    try
                    {
                        var log = await ProcessHelper.InstallMpc();
                        if(!string.IsNullOrWhiteSpace(log))
                        {
                            UnityEngine.Debug.Log(log);
                        }
                        if(log != null && log.Contains("error"))
                        {
                            isInstalledMpc = false;
                        }
                        else
                        {
                            isInstalledMpc = true;
                        }
                    }
                    finally
                    {
                        installingMpc = false;
                    }
                    return;
                }

                EditorGUI.EndDisabledGroup();
                return;
            }

            EditorGUILayout.LabelField("-i input path(csproj or directory):");
            TextField(mpcArgument, x => x.Input, (x, y) => x.Input = y);

            EditorGUILayout.LabelField("-o output filepath(.cs) or directory(multiple):");
            TextField(mpcArgument, x => x.Output, (x, y) => x.Output = y);

            EditorGUILayout.LabelField("-m(optional) use map mode:");
            var newToggle = EditorGUILayout.Toggle(mpcArgument.UseMapMode);
            if(mpcArgument.UseMapMode != newToggle)
            {
                mpcArgument.UseMapMode = newToggle;
                mpcArgument.Save();
            }

            EditorGUILayout.LabelField("-c(optional) conditional compiler symbols(split with ','):");
            TextField(mpcArgument, x => x.ConditionalSymbol, (x, y) => x.ConditionalSymbol = y);

            EditorGUILayout.LabelField("-r(optional) generated resolver name:");
            TextField(mpcArgument, x => x.ResolverName, (x, y) => x.ResolverName = y);

            EditorGUILayout.LabelField("-n(optional) namespace root name:");
            TextField(mpcArgument, x => x.Namespace, (x, y) => x.Namespace = y);

            EditorGUILayout.LabelField("-ms(optional) Generate #if-- files by symbols, split with ','");
            TextField(mpcArgument, x => x.MultipleIfDirectiveOutputSymbols, (x, y) => x.MultipleIfDirectiveOutputSymbols = y);

            EditorGUI.BeginDisabledGroup(invokingMpc);
            if(GUILayout.Button("Generate"))
            {
                var commnadLineArguments = mpcArgument.ToString();
                UnityEngine.Debug.Log("Generate MessagePack Files, command:" + commnadLineArguments);

                invokingMpc = true;
                try
                {
                    ProcessHelper.ReadyForMessagePack();

                    var log = await ProcessHelper.InvokeProcessStartAsync(Mpc, $"{(OS == EOSPlatform.Unix ? "mpc" : "")} {commnadLineArguments}");
                    UnityEngine.Debug.Log(log);
                }
                finally
                {
                    invokingMpc = false;
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        void TextField(MpcArgument args, Func<MpcArgument, string> getter, Action<MpcArgument, string> setter)
        {
            var current = getter(args);
            var newValue = EditorGUILayout.TextField(current);
            if(newValue != current)
            {
                setter(args, newValue);
                args.Save();
            }
        }
    }

    internal class MpcArgument
    {
        public string Input;
        public string Output;
        public string ConditionalSymbol;
        public string ResolverName;
        public string Namespace;
        public bool UseMapMode;
        public string MultipleIfDirectiveOutputSymbols;

        static string Key => "MessagePackCodeGen." + Application.productName;

        public static MpcArgument Restore()
        {
            if(EditorPrefs.HasKey(Key))
            {
                var json = EditorPrefs.GetString(Key);
                return JsonUtility.FromJson<MpcArgument>(json);
            }
            else
            {
                return new MpcArgument();
            }
        }

        public void Save()
        {
            var json = JsonUtility.ToJson(this);
            EditorPrefs.SetString(Key, json);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("-i "); sb.Append(Input);
            sb.Append(" -o "); sb.Append(Output);
            if(!string.IsNullOrWhiteSpace(ConditionalSymbol))
            {
                sb.Append(" -c "); sb.Append(ConditionalSymbol);
            }
            if(!string.IsNullOrWhiteSpace(ResolverName))
            {
                sb.Append(" -r "); sb.Append(ResolverName);
            }
            if(UseMapMode)
            {
                sb.Append(" -m");
            }
            if(!string.IsNullOrWhiteSpace(Namespace))
            {
                sb.Append(" -n "); sb.Append(Namespace);
            }
            if(!string.IsNullOrWhiteSpace(MultipleIfDirectiveOutputSymbols))
            {
                sb.Append(" -ms "); sb.Append(MultipleIfDirectiveOutputSymbols);
            }

            return sb.ToString();
        }
    }

    internal static class ProcessHelper
    {
        const string InstallName = "messagepack.generator";

        public static async Task<bool> IsInstalledMpc()
        {
            var list = await InvokeProcessStartAsync(MessagePackWindow.Dotnet, "tool list -g");
            if(list.Contains(InstallName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static async Task<string> InstallMpc()
        {
            return await InvokeProcessStartAsync(MessagePackWindow.Dotnet, "tool install --global " + InstallName);
        }

        public static async Task<(bool found, string version)> FindDotnetAsync()
        {
            try
            {
                var version = await InvokeProcessStartAsync(MessagePackWindow.Dotnet, "--version");
                return (true, version);
            }
            catch
            {
                return (false, null);
            }
        }

        public static Task<string> InvokeProcessStartAsync(string fileName, string arguments)
        {
            var psi = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = Application.dataPath
            };

            Process p;
            try
            {
                p = Process.Start(psi);
            }
            catch(Exception ex)
            {
                return Task.FromException<string>(ex);
            }

            var tcs = new TaskCompletionSource<string>();
            p.EnableRaisingEvents = true;
            p.Exited += (object sender, System.EventArgs e) =>
            {
                var data = p.StandardOutput.ReadToEnd();
                p.Dispose();
                p = null;

                tcs.TrySetResult(data);
            };

            return tcs.Task;
        }

        public static void ReadyForMessagePack()
        {
            if(IsConfigCreated())
            {
                return;
            }

            Process process = null;

            try
            {
                ProcessStartInfo newToolManifest = CreateProcessStartInfo(MessagePackWindow.Dotnet, "new tool-manifest", true);
                process = Process.Start(newToolManifest);
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }

            try
            {
                process.WaitForExit();
                ProcessStartInfo toolInstall = CreateProcessStartInfo(MessagePackWindow.Dotnet, "tool install MessagePack.Generator", true);
                process = Process.Start(toolInstall);
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
        }

        private static bool IsConfigCreated()
        {
            if(GetDirectoryList().Contains(".config"))
            {
                UnityEngine.Debug.Log("Config file exist.");
                return true;
            }
            UnityEngine.Debug.Log("There is no config file. Creating it starts.");
            return false;
        }

        private static string GetDirectoryList()
        {
            string fileName, arguments;
            if(MessagePackWindow.OS == EOSPlatform.Window)
            {
                fileName = "cmd";
                arguments = "";
            }
            else if(MessagePackWindow.OS == EOSPlatform.Unix)
            {
                fileName = "/bin/bash";
                arguments = "-c \"ls -a\"";
            }
            else
            {
                fileName = "";
                arguments = "";
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            Process process = new Process
            {
                EnableRaisingEvents = false,
                StartInfo = startInfo
            };
            process.Start();

            if(MessagePackWindow.OS == EOSPlatform.Window)
            {
                process.StandardInput.Write("dir" + Environment.NewLine);
            }
            process.StandardInput.Close();
            string result = process.StandardOutput.ReadToEnd();

            process.WaitForExit(); 
            process.Close();
            UnityEngine.Debug.Log(result);
            return result;
        }

        private static ProcessStartInfo CreateProcessStartInfo(string fileName, string arguments, bool useShellExecute = false, bool redirectStandardOutput = false)
        {
            return new ProcessStartInfo()
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = useShellExecute,
                RedirectStandardOutput = redirectStandardOutput
            };
        }
    }
}

#endif
