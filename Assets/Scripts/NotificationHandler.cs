using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationHandler : MonoBehaviour
{
	//private void Start()
	//{
	//	// Clear all previously scheduled notifications (optional)
	//	AndroidNotificationCenter.CancelAllNotifications();

	//	// Schedule notifications for the next 3 days
	//	for (int i = 0; i < 3; i++)
	//	{
	//		ScheduleCustomNotification(i + 1, "Custom Title " + (i + 1), "Custom Description " + (i + 1));
	//	}
	//}

	//private void ScheduleCustomNotification(int dayOffset, string title, string description)
	//{
	//	// Calculate the notification time
	//	DateTime notificationTime = DateTime.Now.AddDays(dayOffset);

	//	// Create a notification
	//	var notification = new AndroidNotification
	//	{
	//		Title = title,
	//		Text = description,
	//		FireTime = notificationTime,
	//		RepeatInterval = TimeSpan.FromDays(1), // Set to TimeSpan.Zero for non-repeating
	//		LargeIcon = "icon_large", // Specify the name of the large icon in your Resources folder
	//		SmallIcon = "icon_small", // Specify the name of the small icon in your Resources folder
	//		Style = NotificationStyle.BigTextStyle
	//	};

	//	// Schedule the notification
	//	var notificationId = AndroidNotificationCenter.SendNotification(notification);

	//	// Optionally, you can use notificationId to cancel or update the notification later
	//}
}
