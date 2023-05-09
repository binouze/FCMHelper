using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;

namespace com.binouze.fcmhelper.Plugins.FCMHelper.Editor
{
    public static class BuildProcessor
    {
        private const string PATH_TO_NOTIFICATION_SERVICE = "Packages/com.binouze.fcmhelper/FCMHelper/NotificationServiceExtension";
        
        [PostProcessBuild( 42 )] //must be between 40 and 50 to ensure that it's not overriden by Podfile generation (40) and that it's added before "pod install" (50)
        private static void PostProcessBuild_iOS(BuildTarget target, string pathToBuiltProject)
        {
            if( target == BuildTarget.iOS )
            {
                // Patching the PodFile
                using var sw = File.AppendText(pathToBuiltProject + "/Podfile");
                // Add FirebaseMessaging to NotificationServiceExtension
                sw.WriteLine("\ntarget 'NotificationServiceExtension' do\n  pod 'Firebase/Messaging', '10.7.0'\nend");
                
                
                var projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
                var proj     = new PBXProject();
                proj.ReadFromFile (projPath);
                var targetGUID = proj.TargetGuidByName ("Unity-iPhone");
     
                var plistPath = pathToBuiltProject + "/Info.plist";
                var plist     = new PlistDocument();
                plist.ReadFromFile(plistPath);
     
                const string notificationServicePlistPath = PATH_TO_NOTIFICATION_SERVICE + "/Info.plist";
                
                var notificationServicePlist = new PlistDocument();
                notificationServicePlist.ReadFromFile(notificationServicePlistPath);
                notificationServicePlist.root.SetString("CFBundleShortVersionString", PlayerSettings.bundleVersion);
                notificationServicePlist.root.SetString("CFBundleVersion", PlayerSettings.iOS.buildNumber);
                
                var notificationServiceTarget = proj.AddAppExtension( targetGUID, "notificationservice", PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS) + ".notificationservice", notificationServicePlistPath);
                proj.AddFileToBuild(notificationServiceTarget, proj.AddFile(PATH_TO_NOTIFICATION_SERVICE + "/NotificationService.swift", "NotificationServiceExtension/NotificationService.swift"));
                proj.AddFrameworkToProject(notificationServiceTarget, "NotificationCenter.framework", true);
                proj.AddFrameworkToProject(notificationServiceTarget, "UserNotifications.framework", true);
                proj.SetBuildProperty(notificationServiceTarget, "ARCHS", "$(ARCHS_STANDARD)");
                proj.SetBuildProperty(notificationServiceTarget, "DEVELOPMENT_TEAM", PlayerSettings.iOS.appleDeveloperTeamID);
                notificationServicePlist.WriteToFile(notificationServicePlistPath);
     
                proj.WriteToFile(projPath);
                plist.WriteToFile(plistPath);
            }
        }
    }
}