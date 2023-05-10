package com.binouze;

public class LocalNotifications
{
    private static final String TAG = "LocalNotifications";

    public static void Send( String title, String message, String image, int color, String dlink, String channelId, String channelName, String icon, String packageName ) 
    {
        Log.i( TAG, "Send" );
        new PictureStyleNotification(UnityPlayer.currentActivity, title, message, color, image, dlink, channelId, channelName, icon, packageName).execute();
    }
}