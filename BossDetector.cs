using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BossNotifier
{
    public class BossDetector : MonoBehaviour
    {
        private bool _noBossesNotified = false;
        private bool _firstScanDone = false;
        private BossNotificationManager _notificationManager;
        private GameWorld _gameWorld;
        private HashSet<string> _detectedBosses = new HashSet<string>();
        private bool _initialized = false;
        private float _checkInterval = 2f;
        private float _lastCheckTime = 0f;

        // Known boss types in Tarkov
        private static readonly HashSet<string> BossRoles = new HashSet<string>
        {
            "bossBully",      // Reshala
            "bossKilla",      // Killa
            "bossKojaniy",    // Shturman
            "bossSanitar",    // Sanitar
            "bossGluhar",     // Glukhar
            "bossTagilla",    // Tagilla
            "bossKnight",     // Knight (Goons)
            "followerBigPipe", // Big Pipe (Goons)
            "followerBirdEye", // Birdeye (Goons)
            "sectantPriest",  // Cultist Priest
            "bossZryachiy",   // Zryachiy (Lighthouse)
            "bossBoar",       // Kaban
            "bossBoarSniper"  // Kaban Sniper
        };

        private static readonly Dictionary<string, string> BossNames = new Dictionary<string, string>
        {
            { "bossBully", "Reshala" },
            { "bossKilla", "Killa" },
            { "bossKojaniy", "Shturman" },
            { "bossSanitar", "Sanitar" },
            { "bossGluhar", "Glukhar" },
            { "bossTagilla", "Tagilla" },
            { "bossKnight", "Knight" },
            { "followerBigPipe", "Big Pipe" },
            { "followerBirdEye", "Birdeye" },
            { "sectantPriest", "Cultist Priest" },
            { "bossZryachiy", "Zryachiy" },
            { "bossBoar", "Kaban" },
            { "bossBoarSniper", "Kaban Sniper" }
        };

        public void Initialize(BossNotificationManager notificationManager)
        {
            _notificationManager = notificationManager;
            _initialized = true;
        }

        private void Update()
        {
            if (!_initialized) return;

            if (Time.time - _lastCheckTime < _checkInterval)
                return;

            _lastCheckTime = Time.time;

            try
            {
                if (_gameWorld == null)
                {
                    _gameWorld = FindObjectOfType<GameWorld>();
                    if (_gameWorld == null)
                    {
                        Debug.Log("[BossNotifier] GameWorld not found (not in raid?)");
                        return;
                    }
                }

                Debug.Log("[BossNotifier] Running boss detection...");
                bool bossFound = CheckForBosses();

                if (!_firstScanDone)
                {
                    _firstScanDone = true;
                    if (!bossFound && !_noBossesNotified)
                    {
                        _noBossesNotified = true;
                        _notificationManager.ShowNotification("No bosses detected", "This raid has no bosses");
                        Debug.Log("[BossNotifier] No bosses detected in this raid.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BossNotifier] Error in boss detection: {ex.Message}");
            }
        }

    private bool CheckForBosses()
        {
            if (_gameWorld == null || _gameWorld.AllAlivePlayersList == null)
                return false;

            bool bossFound = false;
            foreach (var player in _gameWorld.AllAlivePlayersList)
            {
                if (player == null || player.Profile == null) continue;

                // Skip if this is the actual player
                if (player.IsYourPlayer) continue;

                string role = "";
                if (player.Profile.Info?.Settings?.Role != null)
                {
                    role = player.Profile.Info.Settings.Role.ToString();
                }
                
                if (BossRoles.Contains(role) && !_detectedBosses.Contains(player.ProfileId))
                {
                    _detectedBosses.Add(player.ProfileId);
                    bossFound = true;
                    string bossName = BossNames.ContainsKey(role) ? BossNames[role] : role;
                    string location = GetLocationName(player.Position);
                    _notificationManager.ShowNotification(bossName, location);
                    Debug.Log($"[BossNotifier] Boss detected: {bossName} at {location}");
                }
            }
            return bossFound;
        }

        private string GetLocationName(Vector3 position)
        {
            // Try to get a more specific location name
            // This is a simplified version - you can expand with actual zone detection
            try
            {
                if (_gameWorld != null)
                {
                    // Attempt to get zone name if available
                    // This will depend on SPT's implementation
                    return $"Grid: {GetGridPosition(position)}";
                }
            }
            catch
            {
                // Fall back to coordinates
            }

            return $"({position.x:F0}, {position.z:F0})";
        }

        private string GetGridPosition(Vector3 position)
        {
            // Simple grid conversion (adjust based on map size)
            int gridX = Mathf.FloorToInt(position.x / 100);
            int gridZ = Mathf.FloorToInt(position.z / 100);
            return $"{gridX}, {gridZ}";
        }

        private void OnDestroy()
        {
            _detectedBosses.Clear();
            _gameWorld = null;
        }
    }
}
