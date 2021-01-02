using MessagePack;
using MessagePack.Resolvers;
using Nextwin.Protocol;
using Nextwin.Util;
using System;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace Nextwin.Client.Game
{
    public class MessagePackRegisterer
    {
        private static bool _isSerializerRegistered = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if(_isSerializerRegistered)
            {
                return;
            }

            InstallMessagePack();

            StaticCompositeResolver.Instance.Register(
                StandardResolver.Instance,
                LoadResolver("MessagePack.Resolvers.GeneratedResolver", true),
                LoadResolver("MessagePack.Unity.UnityResolver", false),
                LoadResolver("MessagePack.Unity.Extension.UnityBlitWithPrimitiveArrayResolver", false));

            var option = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);

            MessagePackSerializer.DefaultOptions = option;

            if(CheckRegisterSuccess())
            {
                _isSerializerRegistered = true;
            }
        }

        private static void InstallMessagePack()
        {
            Process process;

            try
            {
                ProcessStartInfo toolInstall = CreateProcessStartInfo("dotnet.exe", "tool install --global MessagePack.Generator", true);
                process = Process.Start(toolInstall);
            }
            catch(Exception)
            {
                Print.LogError("Please install .Net Core 3.1 first.");
                return;
            }

            try
            {
                process.WaitForExit();
                ProcessStartInfo newToolManifest = CreateProcessStartInfo("dotnet.exe", "new tool-manifest", true);
                process = Process.Start(newToolManifest);
            }
            catch(Exception e)
            {
                Print.LogError(e.ToString());
            }

            try
            {
                process.WaitForExit();
                ProcessStartInfo toolInstall = CreateProcessStartInfo("dotnet.exe", "tool install MessagePack.Generator", true);
                process = Process.Start(toolInstall);
            }
            catch(Exception e)
            {
                Print.LogError(e.ToString());
            }

            try
            {
                process.WaitForExit();
                ProcessStartInfo mpc = CreateProcessStartInfo("mpc.exe", "-i \"Assembly-CSharp.csproj\" -o \"\\Assets\\Scripts\\Generated\\MessagePackGenerated.cs\"", true);
                process = Process.Start(mpc);
            }
            catch(Exception e)
            {
                Print.LogError(e.ToString());
            }
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

        /// <summary>
        /// 동적으로 Resolver를 찾음
        /// </summary>
        /// <param name="className">namespace를 포함한 Resolver 클래스 이름</param>
        /// <param name="inCurAssembly">Resolver 클래스가 현재 Assembly에 있는지 여부</param>
        /// <returns></returns>
        private static IFormatterResolver LoadResolver(string className, bool inCurAssembly)
        {
            Assembly assembly;
            if(inCurAssembly)
            {
                assembly = Assembly.GetExecutingAssembly();
            }
            else
            {
                assembly = Assembly.Load("MessagePack, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            }

            Type type = assembly.GetType(className);
            Print.Log($"Found {className} in {assembly.FullName}");
            return Activator.CreateInstance(type, true) as IFormatterResolver;
        }

        private static bool CheckRegisterSuccess()
        {
            SerializableData serializingData = new SerializableData(59114);
            byte[] bytes = MessagePackSerializer.Serialize(serializingData);
            SerializableData deserializedData = MessagePackSerializer.Deserialize<SerializableData>(bytes);
            
            if(serializingData.MsgType == deserializedData.MsgType)
            {
                Print.Log("Success to register MessagePack.");
                return true;
            }

            Print.LogError("Fail to register MessagePack.");
            return false;
        }
    }
}
