#import <UserNotifications/UserNotifications.h>

void _FCMHelper_SendLocalNotification(char* instanceId, char* title, char* body, char* attachmentFile, int timeIntervalSecs)
{
    UNMutableNotificationContent* content = [[UNMutableNotificationContent alloc] init];

    content.title = [NSString stringWithUTF8String:title];
    content.body = [NSString stringWithUTF8String:body];
        

    if (attachmentFile != nil)
    {
        //TODO try to make it work with external urls
    
        NSString *attachmentRes =  [NSString stringWithUTF8String:attachmentFile];
        
        NSURL *URL   = [NSURL fileURLWithPath:attachmentRes];
        //NSData *data = [NSData dataWithContentsOfURL:URL];
        
        NSError *attachmentError = nil;
        //note when you attach the file is automatically deleted
        UNNotificationAttachment* attachment = [UNNotificationAttachment attachmentWithIdentifier: [attachmentRes lastPathComponent]
                                                URL:URL
                                                options:nil
                                                 error:&attachmentError];
        if (attachmentError)
        {
            NSLog(@"%@", attachmentError.localizedDescription);
            NSLog(@"Attachment BAD %@", attachmentRes);
        }else{
            content.attachments = @[attachment];
            NSLog(@"Attachment OK %@", attachmentRes);
        }
    }
    
    UNNotificationTrigger* trigger = [UNTimeIntervalNotificationTrigger triggerWithTimeInterval: timeIntervalSecs repeats:NO];
    
    UNNotificationRequest* request = [UNNotificationRequest requestWithIdentifier: @(instanceId) content: content trigger: trigger];

    UNUserNotificationCenter* center = [UNUserNotificationCenter currentNotificationCenter];
    [center addNotificationRequest: request withCompletionHandler:^(NSError * _Nullable error) {
        if (error != NULL){
            NSLog(@"%@", [error localizedDescription]);
        }            
    }];
}
