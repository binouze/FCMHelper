using System;
using System.IO;
using System.Text.RegularExpressions;
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

        /// URL du Swift Package Firebase, telle que declaree par com.google.firebase.app
        /// (Firebase/Editor/AppDependencies.xml -> remoteSwiftPackage url=...)
        private const string FIREBASE_SPM_URL = "https://github.com/firebase/firebase-ios-sdk.git";

        /// produit SPM a linker dans l'extension (NotificationService utilise Messaging.serviceExtension())
        private const string FIREBASE_SPM_PRODUCT = "FirebaseMessaging";

        /// doit etre entre 40 et 50 : apres la generation du Podfile (40), avant "pod install" (50).
        /// La resolution Swift Package de l'iOS Resolver (EDM4U comme EDM 2.1) tourne avant, en 35 : quand on
        /// arrive ici, la reference de package Firebase est deja dans le pbxproj.
        public int callbackOrder => 41;

        /// <summary>
        ///   Implement this function to receive a callback after the build is complete.
        /// </summary>
        /// <param name="report">A BuildReport containing information about the build, such as the target platform and output path.</param>
        public void OnPostprocessBuild( BuildReport report )
        {
            Debug.Log( "FCMHelper.OnPreprocessBuild for target " + report.summary.platform + " at path " + report.summary.outputPath );
            PostProcessBuild_iOS( report.summary.platform, report.summary.outputPath );
        }

        private static void PostProcessBuild_iOS(BuildTarget target, string pathToBuiltProject)
        {
            if( target != BuildTarget.iOS )
                return;

            var projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            var podfile  = pathToBuiltProject + "/Podfile";

            // NotificationServiceExtension deja ajoutee (build en append)
            if( File.ReadAllText( projPath ).Contains( "NotificationServiceExtension.appex" ) )
                return;

            // Firebase passe-t-il par Swift Package Manager ?
            // Si oui on branche l'extension sur la meme reference de package, sinon on retombe
            // sur le patch du Podfile.
            var firebasePackageGuid = FindRemoteSwiftPackageGuid( projPath, FIREBASE_SPM_URL );

            if( string.IsNullOrEmpty( firebasePackageGuid ) )
            {
                // --- fallback CocoaPods ---
                // Attention : en full SPM, l'iOS Resolver ne genere plus de Podfile du tout
                // ("Found no pods to add, skipping generation of the Podfile"), d'ou le File.Exists.
                if( File.Exists( podfile ) )
                {
                    var podcontent = File.ReadAllText( podfile );
                    if( !podcontent.Contains( "target 'NotificationServiceExtension' do" ) )
                    {
                        // meme version que celle demandee pour UnityFramework par com.google.firebase.messaging :
                        // deux versions differentes de Firebase/Messaging dans le Podfile font echouer "pod install"
                        var version = Regex.Match( podcontent, @"pod 'Firebase/Messaging', '([^']+)'" );
                        var pod     = version.Success ? $"pod 'Firebase/Messaging', '{version.Groups[1].Value}'" : "pod 'Firebase/Messaging'";

                        using var sw = File.AppendText( podfile );
                        sw.WriteLine($"\ntarget 'NotificationServiceExtension' do\n  {pod}\nend");
                    }
                }
                else
                {
                    Debug.LogWarning( "FCMHelper: ni Swift Package Firebase ni Podfile trouves, " +
                                      "la NotificationServiceExtension ne pourra pas linker FirebaseMessaging." );
                }
            }

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
            var proj = new PBXProject();
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
            // Firebase iOS 12.x demande iOS 15 minimum
            proj.SetBuildProperty(notifExtensionTarget, "IPHONEOS_DEPLOYMENT_TARGET", PlayerSettings.iOS.targetOSVersionString);
            proj.SetBuildProperty(notifExtensionTarget, "MARKETING_VERSION", PlayerSettings.bundleVersion);

            // --- Swift Package Manager ---
            // On reutilise la reference de package deja posee par l'iOS Resolver (en 35) plutot que d'en creer
            // une nouvelle : PBXProject.AddRemotePackageReferenceAtVersion ne dedoublonne PAS, un second appel sur
            // la meme URL produirait deux XCRemoteSwiftPackageReference et Xcode refuserait le projet.
            // (L'attribut target="NotificationServiceExtension" d'EDM 2.1 ne convient pas : la resolution tourne
            // en 35, avant la creation de l'extension ici en 41, le target n'existe pas encore.)
            if( !string.IsNullOrEmpty( firebasePackageGuid ) )
            {
                proj.AddRemotePackageFrameworkToProject(
                    notifExtensionTarget,
                    FIREBASE_SPM_PRODUCT,
                    firebasePackageGuid,
                    false );

                Debug.Log( $"FCMHelper: {FIREBASE_SPM_PRODUCT} (SPM, package {firebasePackageGuid}) ajoute a la NotificationServiceExtension." );
            }

            // save
            proj.WriteToFile(projPath);
        }

        /// <summary>
        /// Retourne le GUID de la XCRemoteSwiftPackageReference pointant sur repositoryUrl,
        /// ou null si le projet n'utilise pas ce Swift Package.
        /// (l'API PBXProject n'expose pas de getter, on lit donc le pbxproj brut)
        /// </summary>
        private static string FindRemoteSwiftPackageGuid( string projPath, string repositoryUrl )
        {
            try
            {
                var content = File.ReadAllText( projPath );
                var idx     = content.IndexOf( repositoryUrl, StringComparison.Ordinal );
                if( idx < 0 )
                    return null;

                // le GUID est celui du dernier bloc ouvert avant l'URL
                var blocs = Regex.Matches( content.Substring( 0, idx ), @"([0-9A-Fa-f]{24})[^\n]*=\s*\{" );
                return blocs.Count > 0 ? blocs[blocs.Count - 1].Groups[1].Value : null;
            }
            catch( Exception e )
            {
                Debug.LogWarning( $"FCMHelper: lecture du pbxproj impossible ({e.Message})" );
                return null;
            }
        }
    }
}
