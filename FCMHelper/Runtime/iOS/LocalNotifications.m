#import <UserNotifications/UserNotifications.h>


void _FCMHelper_ShowNotif( int timeIntervalSecs, NSString* uid, NSString* title, NSString* body, UNNotificationAttachment* attachment )
{
    UNMutableNotificationContent* content = [[UNMutableNotificationContent alloc] init];

    content.title = title;
    content.body  = body;
    
    if( attachment != nil )
        content.attachments = @[attachment];
    
    UNNotificationTrigger* trigger = [UNTimeIntervalNotificationTrigger triggerWithTimeInterval: timeIntervalSecs repeats:NO];
    UNNotificationRequest* request = [UNNotificationRequest requestWithIdentifier: uid content: content trigger: trigger];

    UNUserNotificationCenter* center = [UNUserNotificationCenter currentNotificationCenter];
    [center addNotificationRequest: request withCompletionHandler:^(NSError * _Nullable error) {
        if (error != NULL){
            NSLog(@"%@", [error localizedDescription]);
        }
    }];
}


void _FCMHelper_SendLocalNotification(char* instanceId, char* title, char* body, char* remoteURL, int timeIntervalSecs)
{
    NSString *nsTitle = [NSString stringWithUTF8String:title];
    NSString *nsBody  = [NSString stringWithUTF8String:body];
    NSString *nsId    = [NSString stringWithUTF8String:instanceId];
        
    if( remoteURL != nil )
    {
        NSString *nsURL = [NSString stringWithUTF8String:remoteURL];
        NSURL *URL      = [NSURL URLWithString:nsURL];
        
        NSURLSession *session          = [NSURLSession sessionWithConfiguration:[NSURLSessionConfiguration defaultSessionConfiguration]];
        NSURLSessionDownloadTask *task = [session downloadTaskWithURL:URL completionHandler:^(NSURL *location, NSURLResponse *response, NSError *error)
        {
            NSLog(@"Downloading notification attachment completed with %@", error == nil ? @"success" : [NSString stringWithFormat:@"error: %@", error]);

            if( error != nil )
            {
                _FCMHelper_ShowNotif( timeIntervalSecs, nsId, nsTitle, nsBody, nil );
                return;
            }
            
            NSError *fileError;
            // create a local URL with extension
            NSURL *urlWithExtension = [NSURL fileURLWithPath:[location.path stringByAppendingString:URL.lastPathComponent]];
            
            if( ![[NSFileManager defaultManager] moveItemAtURL:location toURL:urlWithExtension error:&fileError] )
            {
                NSLog(@"Could not append local attachment file name: %@", fileError);
                // show the notification without image
                _FCMHelper_ShowNotif( timeIntervalSecs, nsId, nsTitle, nsBody, nil );
                return;
            }

            UNNotificationAttachment *attachment = [UNNotificationAttachment attachmentWithIdentifier:URL.absoluteString URL:urlWithExtension options:nil error:&fileError];
            if( !attachment )
            {
                NSLog(@"Could not create local attachment file: %@", fileError);
                NSLog(@"DESC: %@", fileError.localizedDescription);
                _FCMHelper_ShowNotif( timeIntervalSecs, nsId, nsTitle, nsBody, nil );
                return;
            }

            NSLog(@"Adding attachment: %@", attachment);

            _FCMHelper_ShowNotif( timeIntervalSecs, nsId, nsTitle, nsBody, attachment );
            
        }];
        [task resume];
    }
    else
    {
        _FCMHelper_ShowNotif( timeIntervalSecs, nsId, nsTitle, nsBody, nil );
    }
}
