using System;
using System.Collections.Generic;
using UnityEngine;

namespace BossNotifier
{
    public class BossNotificationManager : MonoBehaviour
    {
        private List<BossNotification> _notifications = new List<BossNotification>();
        private BossNotification _lastNotification;
        private float _notificationDuration = 5f;
        private GUIStyle _notificationStyle;
        private GUIStyle _titleStyle;
        private bool _stylesInitialized = false;

        // Notification settings
        private const float NotificationWidth = 300f;
        private const float NotificationHeight = 100f;
        private const float NotificationPadding = 10f;
        private const float NotificationSpacing = 10f;
        private const float ScreenPadding = 20f;

        public void Initialize(float duration)
        {
            _notificationDuration = duration;
        }

        public void ShowNotification(string bossName, string location)
        {
            var notification = new BossNotification
            {
                BossName = bossName,
                Location = location,
                ShowTime = Time.time,
                IsVisible = true
            };

            _notifications.Add(notification);
            _lastNotification = notification;

            Debug.Log($"[BossNotifier] Showing notification: {bossName} at {location}");
        }

        public void ShowLastNotification()
        {
            if (_lastNotification != null)
            {
                _lastNotification.ShowTime = Time.time;
                _lastNotification.IsVisible = true;

                if (!_notifications.Contains(_lastNotification))
                {
                    _notifications.Add(_lastNotification);
                }

                Debug.Log($"[BossNotifier] Re-showing last notification");
            }
        }

        private void InitializeStyles()
        {
            if (_stylesInitialized) return;

            // Notification background style
            _notificationStyle = new GUIStyle(GUI.skin.box)
            {
                normal =
                {
                    background = MakeBackgroundTexture(new Color(0.1f, 0.1f, 0.1f, 0.95f)),
                    textColor = Color.white
                },
                padding = new RectOffset(15, 15, 15, 15),
                border = new RectOffset(2, 2, 2, 2),
                alignment = TextAnchor.MiddleLeft,
                fontSize = 14
            };

            // Title style for boss name
            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.3f, 0.3f, 1f) },
                alignment = TextAnchor.MiddleLeft
            };

            _stylesInitialized = true;
        }

        private Texture2D MakeBackgroundTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private void OnGUI()
        {

            if (_notifications.Count == 0) return;

            // Debug: show when OnGUI is called
            Debug.Log("[BossNotifier] OnGUI called, notifications count: " + _notifications.Count);

            InitializeStyles();

            // Remove expired notifications
            _notifications.RemoveAll(n => Time.time - n.ShowTime > _notificationDuration);

            // Display notifications from bottom to top
            float yOffset = Screen.height - ScreenPadding;
            
            for (int i = _notifications.Count - 1; i >= 0; i--)
            {
                var notification = _notifications[i];
                if (!notification.IsVisible) continue;

                // Calculate fade effect
                float timeAlive = Time.time - notification.ShowTime;
                float alpha = 1f;
                
                // Fade out in the last second
                if (timeAlive > _notificationDuration - 1f)
                {
                    alpha = _notificationDuration - timeAlive;
                }

                // Calculate position (bottom right corner)
                Rect notificationRect = new Rect(
                    Screen.width - NotificationWidth - ScreenPadding,
                    yOffset - NotificationHeight,
                    NotificationWidth,
                    NotificationHeight
                );

                // Apply alpha to styles
                Color oldColor = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, alpha);

                // Draw notification background
                GUI.Box(notificationRect, "", _notificationStyle);

                // Draw boss name
                Rect titleRect = new Rect(
                    notificationRect.x + NotificationPadding,
                    notificationRect.y + NotificationPadding,
                    notificationRect.width - NotificationPadding * 2,
                    30
                );
                
                Color oldTitleColor = _titleStyle.normal.textColor;
                _titleStyle.normal.textColor = new Color(1f, 0.3f, 0.3f, alpha);
                GUI.Label(titleRect, $"⚠ {notification.BossName}", _titleStyle);
                _titleStyle.normal.textColor = oldTitleColor;

                // Draw location
                Rect locationRect = new Rect(
                    notificationRect.x + NotificationPadding,
                    notificationRect.y + NotificationPadding + 35,
                    notificationRect.width - NotificationPadding * 2,
                    20
                );

                GUIStyle locationStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    normal = { textColor = new Color(0.8f, 0.8f, 0.8f, alpha) }
                };
                GUI.Label(locationRect, $"Location: {notification.Location}", locationStyle);

                // Draw hint text
                Rect hintRect = new Rect(
                    notificationRect.x + NotificationPadding,
                    notificationRect.y + notificationRect.height - 25,
                    notificationRect.width - NotificationPadding * 2,
                    20
                );

                GUIStyle hintStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 10,
                    fontStyle = FontStyle.Italic,
                    normal = { textColor = new Color(0.6f, 0.6f, 0.6f, alpha * 0.7f) }
                };
                GUI.Label(hintRect, "Press Numpad 0 to show again", hintStyle);

                GUI.color = oldColor;

                // Move up for next notification
                yOffset -= NotificationHeight + NotificationSpacing;
            }
        }

        private class BossNotification
        {
            public string BossName { get; set; }
            public string Location { get; set; }
            public float ShowTime { get; set; }
            public bool IsVisible { get; set; }
        }
    }
}
