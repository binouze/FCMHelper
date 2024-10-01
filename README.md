# FCM Helper

Firebase Messaging integration for Unity with image support on push notifications

## Installation

Choose your favourite method:

- **Plain install**
    - Clone or [download](https://github.com/binouze/FCMHelper/archive/refs/heads/master.zip) 
this repository and put it in the `Assets/Plugins` folder of your project.
- **Unity Package Manager (Manual)**:
    - Add the following line to *Packages/manifest.json*:
    - `"com.binouze.fcmhelper": "https://github.com/binouze/FCMHelper.git"`
- **Unity Package Manager (Auto)**
    - in the package manager, click on the + 
    - select `add package from GIT url`
    - paste the following url: `"https://github.com/binouze/FCMHelper.git"`


## How to use

The unity implementation of FirebaseMessaging does not supports Images in push notification by default on iOS.
This plugin just add this support.

See Firebase Messaging documentation here: https://firebase.google.com/docs/cloud-messaging/unity/client

## Foreground Notifications

FirebaseMessaging for Unity does not support showing push notifications while the app is in foreground.

- On Android you can send a local notification using `LocalNotification.Send()` function when receiving a message from inside the game like this:

```csharp
//...
FirebaseMessaging.MessageReceived += OnMessageReceived;
//...

private static void OnMessageReceived( object sender, MessageReceivedEventArgs e ) 
{
    if( e.Message.NotificationOpened )
    {
        // notification clicked
    }
    else if( e.Message.Notification != null )
    {
        // notification received while the app is in forground
        
        #if UNITY_ANDROID
        // get datas in notifiaction
        var img   = e.Message.Data.GetValue("image", string.Empty);
        var link  = e.Message.Data.GetValue("link", string.Empty);
        var notif = e.Message.Notification;
        // send a local notification
        LocalNotifications.Send(notif.Title, notif.Body, img, 0, notif.Color, link, notif.Android.ChannelId);
        #endif
    }
}

/// extension method to get a string value from a dictionary, with a default value if not exists
public static string GetValue( this IDictionary<string, string> dico, string key, string defaut = null )
{
    if( dico == null ) 
        return null;
    return dico.TryGetValue( key, out var val ) ? val : defaut;
}
```

- On iOS you must override tha app controller to handle the `willPresentNotification` function like this:

```objective-c
@interface UnityForegroundNotifController : UnityAppController<UNUserNotificationCenterDelegate> { }
@implementation UnityForegroundNotifController

-(BOOL) application:(UIApplication*) application 
        didFinishLaunchingWithOptions:(NSDictionary*) options
{
    // set the UNUserNotificationCenter delegate
    [UNUserNotificationCenter currentNotificationCenter].delegate = self;
    return [super application:application didFinishLaunchingWithOptions:options];
}

- (void) userNotificationCenter:(UNUserNotificationCenter* )center 
         willPresentNotification:(UNNotification* )notification 
         withCompletionHandler:(void (^)(UNNotificationPresentationOptions options))completionHandler 
{
    NSLog( @"Here handle push notification in foreground" );
    //For notification Banner - when app in foreground
    completionHandler(UNNotificationPresentationOptionAlert);
    // Print Notification info
    NSLog(@"Userinfo %@",notification.request.content.userInfo); 
}

- (void) application:(UIApplication *)application 
         didReceiveRemoteNotification:(nonnull NSDictionary *)userInfo
         fetchCompletionHandler:(nonnull void (^)(UIBackgroundFetchResult))completionHandler
{
    NSLog( @"didReceiveRemoteNotification fetchCompletionHandler" );
}

- (void) userNotificationCenter:(UNUserNotificationCenter *)center
         didReceiveNotificationResponse:(UNNotificationResponse *)response
         withCompletionHandler:(void (^)())completionHandler 
{
    NSLog( @"didReceiveNotificationResponse" );
}

// Tell Unity to use UnityForegroundNotifController as the main app controller:
IMPL_APP_CONTROLLER_SUBCLASS(UnityForegroundNotifController)

```


## Firebase SDK support

Work with Firebase SDK:
 - 12.3.0