package com.binouze;

import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.os.Build;
import android.graphics.Color;

import androidx.core.app.NotificationCompat;
import androidx.core.app.NotificationManagerCompat;

import java.io.IOException;
import java.io.InputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

import android.net.Uri;
import android.os.Handler;
import android.os.Looper;

public class PictureStyleNotification// extends AsyncTask<String, Void, Bitmap>
{
    private static int  ID = 100;

    private final Context mContext;
    private final String titre;
    private final String texte;
    private final String image;
    private final String dlink;
    private final String channelId;
    private final String channelName;
    private final String icon;
    private final String packageName;
    private final int color;

    private Bitmap bmpData;

    public PictureStyleNotification(Context context, String titre, String texte, String colorString, String image, String dlink, String channelId, String channelName, String icon, String packageName )
    {
        super();

        this.mContext      = context;
        this.image         = image;
        this.titre         = titre;
        this.texte         = texte;
        //this.color         = Color.parseColor(colorString);
        
        int tempColor;
        try {
            tempColor = Color.parseColor(colorString);
        } catch (Exception ex) {
            // Handle exception here, ex.printStackTrace(), show a message or set a default color
            tempColor = Color.WHITE; // default color if the string is not valid
        }
        
        this.color         = tempColor;
        this.dlink         = dlink;
        this.channelId     = channelId;
        this.channelName   = channelName;
        this.icon          = icon;
        this.packageName   = packageName;
    }

    public void afficher()
    {
        // si il y a une image, on doit d'abord la telecharger
        if( image != null && image.length() > 0 )
        {
            ExecutorService executor = Executors.newSingleThreadExecutor();
            Handler handler          = new Handler(Looper.getMainLooper());

            executor.execute( () ->
            {
                InputStream in;
                try
                {
                    URL url = new URL(image);
                    HttpURLConnection connection = (HttpURLConnection) url.openConnection();
                    connection.setDoInput(true);
                    connection.connect();
                    in = connection.getInputStream();
                    bmpData = BitmapFactory.decodeStream(in);
                }
                catch( IOException e )
                {
                    e.printStackTrace();
                }

                handler.post( () -> onPostExecute( bmpData ) );
            });
        }
        else
        {
            onPostExecute( null );
        }
    }

    protected void onPostExecute( Bitmap result )
    {
        Intent intent; 
        if( dlink != null && dlink.length() > 1 ) { intent = new Intent( Intent.ACTION_VIEW, Uri.parse(dlink) );                                }
        else                                      { intent = new Intent( mContext, com.unity3d.player.UnityPlayer.currentActivity.getClass() ); }
        intent.addFlags( Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_SINGLE_TOP );
        
        PendingIntent pendingIntent = null;
        if( android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.S ) 
        {
            // convert implicit intent to explicit intent for Android 14 compatibility
            intent.setPackage(mContext.getPackageName());
            pendingIntent = PendingIntent.getActivity( mContext, 0, intent, PendingIntent.FLAG_MUTABLE );
        }
        else
        {
             pendingIntent = PendingIntent.getActivity(mContext, 0, intent, PendingIntent.FLAG_ONE_SHOT );
        }
        
        int notifID = ID++;

        // recuperer le gestionnaire de notifications
        NotificationManagerCompat notificationManager = NotificationManagerCompat.from( mContext );

        // Since android Oreo notification channel is needed.
        if( Build.VERSION.SDK_INT >= Build.VERSION_CODES.O ) 
        {
            NotificationChannel channel = new NotificationChannel( channelId, channelName, NotificationManager.IMPORTANCE_HIGH );
            notificationManager.createNotificationChannel( channel );
        }

        NotificationCompat.Builder notificationBuilder = new NotificationCompat.Builder( mContext, channelId )
                .setSmallIcon( mContext.getResources().getIdentifier(icon, "drawable", packageName) )
                .setContentTitle( titre )
                .setContentText( texte )
                .setStyle(new NotificationCompat.BigTextStyle().bigText(texte))
                .setAutoCancel(true)
                .setContentIntent( pendingIntent )
                .setColor( color )
                .setDefaults( NotificationCompat.DEFAULT_ALL )
                .setPriority( NotificationCompat.PRIORITY_HIGH )
                .setGroup(packageName+"default");

        if( result != null )
            notificationBuilder.setLargeIcon( result );

        NotificationCompat.Builder groupBuilder = new NotificationCompat.Builder( mContext, channelId )
                .setSmallIcon( mContext.getResources().getIdentifier(icon, "drawable", packageName) )
                .setContentTitle( titre )
                .setContentText( texte )
                //.setStyle(new NotificationCompat.BigTextStyle().bigText(texte))
                .setColor( color )
                .setGroup(packageName+"default")
                .setGroupAlertBehavior( NotificationCompat.GROUP_ALERT_CHILDREN )
                .setGroupSummary( true );

        notificationManager.notify( notifID, notificationBuilder.build());
        notificationManager.notify( 0,   groupBuilder.build());
    }
}
