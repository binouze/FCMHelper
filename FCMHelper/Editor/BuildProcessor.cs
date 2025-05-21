using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
using UnityEngine;

namespace com.binouze.FCMHelper.Editor
{
    public class BuildProcessor : IPostprocessBuildWithReport
    {
        private const string PATH_TO_NOTIFICATION_SERVICE = "Packages/com.binouze.fcmhelper/FCMHelper/Editor/NotificationServiceExtension";
        
        public int  callbackOrder => 41; //must be between 40 and 50 to ensure that it's not overriden by Podfile generation (40) and that it's added before "pod install" (50)

        /// <summary>
        ///   Implement this function to receive a callback after the build is complete.
        /// </summary>
        /// <param name="report">A BuildReport containing information about the build, such as the target platform and output path.</param>
        public void OnPostprocessBuild( BuildReport report )
        {
            Debug.Log( "FCMHelper.OnPreprocessBuild for target " + report.summary.platform + " at path " + report.summary.outputPath );
            PostProcessBuild_iOS( report.summary.platform, report.summary.outputPath );
        }

        //[PostProcessBuild( 42 )] //must be between 40 and 50 to ensure that it's not overriden by Podfile generation (40) and that it's added before "pod install" (50)
        private static void PostProcessBuild_iOS(BuildTarget target, string pathToBuiltProject)
        {
            if( target == BuildTarget.iOS )
            {
                // Patching the PodFile
                using var sw = File.AppendText(pathToBuiltProject + "/Podfile");
                // Add FirebaseMessaging to NotificationServiceExtension
                sw.WriteLine("\ntarget 'NotificationServiceExtension' do\n  pod 'Firebase/Messaging', '11.10.0'\n  pod 'GoogleUtilities', '~> 8.0'\nend");

                var dirname   = pathToBuiltProject + "/NotificationServiceExtension";
                var swiftFile = dirname            + "/NotificationService.swift";
                var plistFile = dirname            + "/Info.plist";
                // create directory if not exists
                Directory.CreateDirectory( dirname );
                // remove previous file if exists (build with append)
                File.Delete( swiftFile );
                File.Delete( plistFile );
                // copy files to destination directory
                File.Copy(PATH_TO_NOTIFICATION_SERVICE +"/NotificationService.swift", swiftFile);
                File.Copy(PATH_TO_NOTIFICATION_SERVICE +"/Info.plist",                plistFile);
                
                // get main project
                var projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
                var proj     = new PBXProject();
                proj.ReadFromFile(projPath);
                var mainGUID = proj.GetUnityMainTargetGuid();

                // get iOS bundleID
                var bundleID = PlayerSettings.GetApplicationIdentifier( BuildTargetGroup.iOS ) + ".NotificationServiceExtension";
                
                // add the extension
                var notifExtensionTarget = proj.AddAppExtension( 
                    mainGUID, 
                    "NotificationServiceExtension", 
                    bundleID, 
                    "NotificationServiceExtension/Info.plist");
                
                // add the files needed
                proj.AddFileToBuild(notifExtensionTarget, proj.AddFile(swiftFile, "NotificationServiceExtension/NotificationService.swift"));
                proj.AddFile( plistFile, "NotificationServiceExtension/Info.plist");
                
                // define build properties
                proj.SetBuildProperty(notifExtensionTarget, "DEVELOPMENT_TEAM", PlayerSettings.iOS.appleDeveloperTeamID);
                proj.SetBuildProperty(notifExtensionTarget, "ENABLE_BITCODE", "NO");
                proj.SetBuildProperty(notifExtensionTarget, "SWIFT_VERSION", "5.0");
                proj.SetBuildProperty(notifExtensionTarget, "TARGETED_DEVICE_FAMILY", "1,2");
                proj.SetBuildProperty(notifExtensionTarget, "GENERATE_INFOPLIST_FILE", "YES");
                proj.SetBuildProperty(notifExtensionTarget, "ALWAYS_SEARCH_USER_PATHS", "NO");
                proj.SetBuildProperty(notifExtensionTarget, "CODE_SIGN_IDENTITY", "iPhone Developer");
                proj.SetBuildProperty(notifExtensionTarget, "ENABLE_NS_ASSERTIONS", "NO");
                proj.SetBuildProperty(notifExtensionTarget, "SKIP_INSTALL", "YES");
                proj.SetBuildProperty(notifExtensionTarget, "COPY_PHASE_STRIP", "NO");
                proj.SetBuildProperty(notifExtensionTarget, "INFOPLIST_KEY_CFBundleDisplayName", "NotificationServiceExtension");
                proj.SetBuildProperty(notifExtensionTarget, "PRODUCT_BUNDLE_IDENTIFIER", bundleID);
                proj.SetBuildProperty(notifExtensionTarget, "GCC_C_LANGUAGE_STANDARD", "gnu11");
                proj.SetBuildProperty(notifExtensionTarget, "CLANG_CXX_LANGUAGE_STANDARD", "gnu++20");
                proj.SetBuildProperty(notifExtensionTarget, "CLANG_ENABLE_MODULES", "YES");
                proj.SetBuildProperty(notifExtensionTarget, "CLANG_ENABLE_OBJC_ARC", "YES");
                proj.SetBuildProperty(notifExtensionTarget, "CLANG_ENABLE_OBJC_WEAK", "YES");
                proj.SetBuildProperty(notifExtensionTarget, "CURRENT_PROJECT_VERSION", PlayerSettings.iOS.buildNumber);
                proj.SetBuildProperty(notifExtensionTarget, "SWIFT_EMIT_LOC_STRINGS", "YES");
                proj.SetBuildProperty(notifExtensionTarget, "VALIDATE_PRODUCT", "YES");

                // save
                proj.WriteToFile(projPath);
            }
        }
    }
}