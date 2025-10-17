using BepInEx;
using BepInEx.Configuration;
using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BossNotifier
{
    [BepInPlugin("com.spt.bossnotifier", "Boss Notifier", "1.0.0")]
    public class BossNotifierPlugin : BaseUnityPlugin
    {
        private static BossNotifierPlugin _instance;
        public static BossNotifierPlugin Instance => _instance;

        private ConfigEntry<KeyCode> _showNotificationKey;
        private ConfigEntry<float> _notificationDuration;
        private ConfigEntry<bool> _enableMod;

        private BossNotificationManager _notificationManager;
        private BossDetector _bossDetector;

        private void Awake()
        {
            _instance = this;

            // Configuration
            _enableMod = Config.Bind("General", "Enable", true, "Enable or disable the boss notifier");
            _showNotificationKey = Config.Bind("Hotkeys", "ShowNotificationKey", KeyCode.Keypad0, "Key to re-show the boss notification");
            _notificationDuration = Config.Bind("Notification", "Duration", 5f, "How long the notification stays visible (in seconds)");

            if (_enableMod.Value)
            {
                Logger.LogInfo("Boss Notifier plugin loaded successfully!");
            }
        }

        private void Start()
        {
            if (!_enableMod.Value) return;

            _notificationManager = gameObject.AddComponent<BossNotificationManager>();
            _notificationManager.Initialize(_notificationDuration.Value);

            _bossDetector = gameObject.AddComponent<BossDetector>();
            _bossDetector.Initialize(_notificationManager);
        }

        // Use OnGUI for input detection (Unity 2019+)
        private void OnGUI()
        {
            if (!_enableMod.Value) return;
            // Only check input if we're in-raid and notification manager exists
            if (_notificationManager == null) return;

            // Numpad 0 hotkey (KeyCode.Keypad0)
            Event e = Event.current;
            if (e != null && e.type == EventType.KeyDown && e.keyCode == _showNotificationKey.Value)
            {
                Logger.LogInfo($"Numpad 0 pressed, showing last boss notification");
                _notificationManager.ShowLastNotification();
            }
        }

        public void OnDestroy()
        {
            if (_notificationManager != null)
            {
                Destroy(_notificationManager);
            }
            if (_bossDetector != null)
            {
                Destroy(_bossDetector);
            }
        }
    }
}
