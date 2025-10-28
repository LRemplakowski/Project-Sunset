#if UNITY_EDITOR

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace ShaderCrew.TheToonShader
{
    [InitializeOnLoad]
    public static class SeeThroughShaderPackageInitializer
    {
        private const string CurrentPackageVersion = "1.8.7";
        private const string VersionKey = "IsSeeThroughPackageInitialized";

        static SeeThroughShaderPackageInitializer()
        {
            EditorApplication.delayCall += RunInitialization;
            Events.registeredPackages += OnPackagesRegistered;
        }

        private static void RunInitialization()
        {
            string storedVersion = EditorPrefs.GetString(VersionKey + Application.productName, "");
            if (storedVersion != CurrentPackageVersion)
            {
                EditorPrefs.SetString(VersionKey + Application.productName, CurrentPackageVersion);
                DoTildeShaderFolderAdjustmentForBothRPs();
                //Debug.Log("RunInitialization()");
            }
        }

        private static void OnPackagesRegistered(PackageRegistrationEventArgs args)
        {
            if (args.added != null && args.added.Any(pkg => pkg.name == "com.shadercrew.seethroughshader.core") ||
                args.changedFrom != null && args.changedFrom.Any(pkg => pkg.name == "com.shadercrew.seethroughshader.core"))
            {
                //Debug.Log("com.shadercrew.seethroughshader.core was added");
                DoTildeShaderFolderAdjustmentForBothRPs();

            }
        }

        private static void DoTildeShaderFolderAdjustmentForBothRPs()
        {
            DoTildeShaderFolderAdjustmentsURP();
            DoTildeShaderFolderAdjustmentsHDRP();
        }

        private static void DoTildeShaderFolderAdjustmentsURP()
        {
            string nativeShaderFolderRP = "Packages/com.shadercrew.seethroughshader.core/Scripts/Shaders/Native/URP";
            string nativeShaderFolderRPTilde = nativeShaderFolderRP + "~";


#if USING_URP
            RenameFolder(nativeShaderFolderRPTilde, nativeShaderFolderRP);
            DoTildeShaderFolderAdjustmentsVersionDependent(nativeShaderFolderRP);
#else
            RenameFolder(nativeShaderFolderRP, nativeShaderFolderRPTilde);
#endif

        }

        private static void DoTildeShaderFolderAdjustmentsHDRP()
        {
            string nativeShaderFolderRP = "Packages/com.shadercrew.seethroughshader.core/Scripts/Shaders/Native/HDRP";
            string nativeShaderFolderRPTilde = nativeShaderFolderRP + "~";


#if USING_HDRP
            RenameFolder(nativeShaderFolderRPTilde, nativeShaderFolderRP);
            DoTildeShaderFolderAdjustmentsVersionDependent(nativeShaderFolderRP);
#else
            RenameFolder(nativeShaderFolderRP, nativeShaderFolderRPTilde);
#endif

        }

        private static void DoTildeShaderFolderAdjustmentsVersionDependent(string nativeShaderFolderRP)
        {

            string nativeShaderFolderRP2019 = nativeShaderFolderRP + "/2019";
            string nativeShaderFolderRP2019Tilde = nativeShaderFolderRP2019 + "~";

            string nativeShaderFolderRP2020 = nativeShaderFolderRP + "/2020";
            string nativeShaderFolderRP2020Tilde = nativeShaderFolderRP2020 + "~";

            string nativeShaderFolderRP2021 = nativeShaderFolderRP + "/2021";
            string nativeShaderFolderRP2021Tilde = nativeShaderFolderRP2021 + "~";

            string nativeShaderFolderRP2022 = nativeShaderFolderRP + "/2022";
            string nativeShaderFolderRP2022Tilde = nativeShaderFolderRP2022 + "~";

            string nativeShaderFolderRPUnity6 = nativeShaderFolderRP + "/Unity6";
            string nativeShaderFolderRPUnity6Tilde = nativeShaderFolderRPUnity6 + "~";


#if UNITY_2019_1_OR_NEWER && !UNITY_2020_1_OR_NEWER
            RenameFolder(nativeShaderFolderRP2019Tilde, nativeShaderFolderRP2019);
#else
            RenameFolder(nativeShaderFolderRP2019, nativeShaderFolderRP2019Tilde);
#endif


#if UNITY_2020_1_OR_NEWER && !UNITY_2021_1_OR_NEWER
            RenameFolder(nativeShaderFolderRP2020Tilde, nativeShaderFolderRP2020);
#else
            RenameFolder(nativeShaderFolderRP2020, nativeShaderFolderRP2020Tilde);
#endif

#if UNITY_2021_1_OR_NEWER && !UNITY_2022_1_OR_NEWER
            RenameFolder(nativeShaderFolderRP2021Tilde, nativeShaderFolderRP2021);
#else
            RenameFolder(nativeShaderFolderRP2021, nativeShaderFolderRP2021Tilde);
#endif

#if UNITY_2022_1_OR_NEWER && !UNITY_2023_1_OR_NEWER
            RenameFolder(nativeShaderFolderRP2022Tilde, nativeShaderFolderRP2022);
#else
            RenameFolder(nativeShaderFolderRP2022, nativeShaderFolderRP2022Tilde);
#endif

#if UNITY_2023_1_OR_NEWER
            RenameFolder(nativeShaderFolderRPUnity6Tilde, nativeShaderFolderRPUnity6);
#else
            RenameFolder(nativeShaderFolderRPUnity6, nativeShaderFolderRPUnity6Tilde);
#endif
        }

        private static void RenameFolder(string oldName, string newName)
        {
            if (Directory.Exists(oldName))
            {
                Directory.Move(oldName, newName);
                string meta = oldName + ".meta";
                if (File.Exists(meta))
                {
                    File.Delete(meta);
                }
                AssetDatabase.Refresh();
            }
        }

    }
}
#endif
