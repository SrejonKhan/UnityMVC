using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace UnityMVC
{
    public class AddressableEditor
    {
        public static AddressableAssetSettings GetDefaultSettings()
        {
            return AddressableAssetSettingsDefaultObject.Settings;
        }

        public static AddressableAssetGroup GetOrCreateGroup(string groupName, AddressableAssetSettings settings = null)
        {
            settings = settings ?? GetDefaultSettings();

            if (settings == null)
            {
                Debug.LogError("Default settings of Addressabale not found. Maybe it was not created or not assigned.");
                return null;
            }

            // try find
            var group = settings.FindGroup(groupName);

            // create new
            if (!group) group = settings.CreateGroup(groupName, false, false, true, null,
                    typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));

            return group;
        }

        public static (bool, AddressableAssetEntry) CheckAssetEntry(Object asset, AddressableAssetGroup group)
        {
            if (asset == null || group == null)
                return (false, null);

            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset.GetInstanceID(),
                out string guid, out long _);

            var assetEntry = group.GetAssetEntry(guid);

            if (assetEntry != null)
                return (true, assetEntry);

            return (false, assetEntry);
        }

        public static (bool, AddressableAssetEntry) ChechAssetEntryInAllGroups(Object asset)
        {
            if (asset == null)
                return (false, null);

            var settings = GetDefaultSettings();

            if (settings == null)
            {
                Debug.LogError("Default settings of Addressabale not found. Maybe it was not created or not assigned.");
                return (false, null);
            }

            AddressableAssetEntry assetEntry = null;

            var groups = settings.groups;

            foreach (var group in groups)
            {
                var (_, entry) = CheckAssetEntry(asset, group);
                assetEntry = entry;

                if (assetEntry != null)
                    break;
            }

            bool hasEntry = assetEntry != null;

            return (hasEntry, assetEntry);
        }

        public static List<AddressableAssetEntry> GetAllEntriesInAllGroups(AddressableAssetSettings settings = null)
        {
            List<AddressableAssetEntry> entries = new List<AddressableAssetEntry>();

            settings = settings ?? GetDefaultSettings();

            if (settings == null)
            {
                Debug.LogError("Default settings of Addressabale not found. Maybe it was not created or not assigned.");
                return null;
            }

            var groups = settings.groups;

            foreach (var group in groups)
                entries.AddRange(group.entries);

            return entries;
        }

        public static bool ChangeEntryGroup(AddressableAssetEntry entry, AddressableAssetGroup group)
        {
            if (entry == null || group == null)
                return false;

            var settings = GetDefaultSettings();

            if (settings == null)
            {
                Debug.LogError("Default settings of Addressabale not found. Maybe it was not created or not assigned.");
                return false;
            }

            try
            {
                settings.MoveEntry(entry, group);

                var entriesModified = new List<AddressableAssetEntry> { entry };

                // make dirty
                group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesModified, false, true);
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesModified, true, false);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Encountered error while changing group - " + ex.Message);
                return false;
            }


            return true;
        }

        public static bool ChangeEntryAddress(AddressableAssetEntry entry, string newAddress)
        {
            if (entry == null || string.IsNullOrEmpty(newAddress))
                return false;

            try
            {
                entry.SetAddress(newAddress);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Encountered error while changing group - " + ex.Message);
                return false;
            }

            return true;
        }

        public static AddressableAssetEntry CreateEntry(Object obj, AddressableAssetGroup group, string address = null)
        {
            if (obj == null || group == null)
                return null;

            var settings = GetDefaultSettings();

            if (settings == null)
            {
                Debug.LogError("Default settings of Addressabale not found. Maybe it was not created or not assigned.");
                return null;
            }

            var assetpath = AssetDatabase.GetAssetPath(obj);
            var guid = AssetDatabase.AssetPathToGUID(assetpath);

            var entry = settings.CreateOrMoveEntry(guid, group, false, false);

            if (!string.IsNullOrEmpty(address))
                ChangeEntryAddress(entry, address);

            var entriesAdded = new List<AddressableAssetEntry> { entry };

            group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, false, true);
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, true, false);

            return entry;
        }

        public static AddressableAssetEntry FindEntryByAddress(string address)
        {
            var settings = GetDefaultSettings();

            if (settings == null)
            {
                Debug.LogError("Default settings not found. Maybe it was not created or not assigned.");
                return null;
            }

            List<AddressableAssetEntry> allEntries = settings.groups.SelectMany(g => g.entries).ToList();
            AddressableAssetEntry entry = allEntries.FirstOrDefault(e => e.address == address);

            return entry; 
        }

        public static AddressableAssetEntry RemoveEntry(AddressableAssetEntry entry)
        {
            if (entry == null)
                return null;

            var settings = GetDefaultSettings();

            if (settings == null)
            {
                Debug.LogError("Default settings of Addressabale not found. Maybe it was not created or not assigned.");
                return null;
            }

            var tmpEntry = entry;

            // remove
            settings.RemoveAssetEntry(entry.guid);
            
            return tmpEntry;
        }
    }
}
