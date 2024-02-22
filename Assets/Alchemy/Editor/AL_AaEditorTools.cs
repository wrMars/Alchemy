/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Alchemy
{
    public class AL_AaEditorTools
    {
        public static void GetAaInfos(out Dictionary<string, AddressableAssetGroup> groups, out Dictionary<string, AddressableAssetEntry> entries, 
            out Dictionary<string, List<string>> path2Label, out Dictionary<string, List<string>> label2GroupName, out Dictionary<string, List<string>> label2AbName,
            out Dictionary<string, string> abName2GroupName, out Dictionary<string, List<string>> groupName2AbName)
        {
            groups = new Dictionary<string, AddressableAssetGroup>();
            entries = new Dictionary<string, AddressableAssetEntry>();
            path2Label = new Dictionary<string, List<string>>();
            label2GroupName = new Dictionary<string, List<string>>();
            label2AbName = new Dictionary<string, List<string>>();
            abName2GroupName = new Dictionary<string, string>();
            groupName2AbName = new Dictionary<string, List<string>>();
            foreach (var group in AddressableAssetSettingsDefaultObject.Settings.groups)
            {
                if (!groups.ContainsKey(group.Name)) groups.Add(group.Name, group);
                foreach (var addressableAssetEntry in group.entries)
                {
                    if (!entries.ContainsKey(addressableAssetEntry.AssetPath)) entries.Add(addressableAssetEntry.AssetPath, addressableAssetEntry);
                    foreach (var label in addressableAssetEntry.labels)
                    {
                        if (!path2Label.ContainsKey(addressableAssetEntry.AssetPath))
                        {
                            path2Label.Add(addressableAssetEntry.AssetPath, new List<string>());
                        }
                        if (path2Label.TryGetValue(addressableAssetEntry.AssetPath, out var arrLabels) && !arrLabels.Contains(label))
                        {
                            arrLabels.Add(label);
                        }

                        if (!label2GroupName.ContainsKey(label))
                        {
                            label2GroupName.Add(label, new List<string>());
                        }
                        if (label2GroupName.TryGetValue(label, out var arrGroupNames) && !arrGroupNames.Contains(addressableAssetEntry.parentGroup.Name))
                        {
                            arrGroupNames.Add(addressableAssetEntry.parentGroup.Name);
                        }
                        
                        if (!label2AbName.ContainsKey(label))
                        {
                            label2AbName.Add(label, new List<string>());
                        }
                        if (label2AbName.TryGetValue(label, out var arrAbNames))
                        {
                            var abname = GetAbName(addressableAssetEntry.parentGroup.Name, label);
                            if (!abName2GroupName.ContainsKey(abname)) abName2GroupName.Add(abname, addressableAssetEntry.parentGroup.Name);
                            if (!arrAbNames.Contains(abname)) arrAbNames.Add(abname);
                        }

                        if (!groupName2AbName.ContainsKey(group.Name))
                        {
                            groupName2AbName.Add(group.Name, new List<string>());
                        }
                        if (groupName2AbName.TryGetValue(group.Name, out arrAbNames))
                        {
                            var abname = GetAbName(group.Name, label);
                            if (!arrAbNames.Contains(abname)) arrAbNames.Add(abname);
                        }
                    }
                }
            }
        }

        public static string GetAbName(string groupName, string labelName)
        {
            return $"{groupName.ToLower()}_assets_{labelName.ToLower()}";
        }
        
        public static string Build_script = "Assets/AddressableAssetsData/DataBuilders/BuildScriptPackedMode.asset";
        public static string Settings_asset = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
        public static string Profile_name = "server";
        private static AddressableAssetSettings Settings;

        static void GetSettingsObject(string settingsAsset) {
            Settings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(settingsAsset) as AddressableAssetSettings;
            if (Settings == null)
                AL_LogTool.LogOnlyEditor($"{settingsAsset} couldn't be found or isn't " +
                               $"a settings object.");
        }

        static void SetProfile(string profile) {
            string profileId = Settings.profileSettings.GetProfileId(profile);
            if (String.IsNullOrEmpty(profileId))
                AL_LogTool.LogOnlyEditor($"Couldn't find a profile named, {profile}, " +
                                         $"using current profile instead.");
            else
                Settings.activeProfileId = profileId;
        }

        static void SetBuilder(IDataBuilder builder) {
            int index = Settings.DataBuilders.IndexOf((ScriptableObject)builder);

            if (index > 0)
                Settings.ActivePlayerDataBuilderIndex = index;
            else
                AL_LogTool.LogOnlyEditor($"{builder} must be added to the " +
                                         $"DataBuilders list before it can be made " +
                                         $"active. Using last run builder instead.");
        }

        static bool BuildAddressableContent() {
            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
            bool success = string.IsNullOrEmpty(result.Error);
            if (!success) {
                AL_LogTool.LogOnlyEditor("Addressables build error encountered: " + result.Error);
            }
            return success;
        }

        public static bool BuildAddressables() {
            return BuildAddressables(Build_script, Settings_asset, Profile_name);
        }
        
        public static bool BuildAddressables(string buildScriptPath, string settingPath, string profileName) {
            GetSettingsObject(settingPath);
            SetProfile(profileName);
            IDataBuilder builderScript = AssetDatabase.LoadAssetAtPath<ScriptableObject>(buildScriptPath) as IDataBuilder;

            if (builderScript == null) {
                AL_LogTool.LogOnlyEditor(buildScriptPath + " couldn't be found or isn't a build script.");
                return false;
            }
            SetBuilder(builderScript);
            return BuildAddressableContent();
        }
        
        public static void CleanAACache() {
            CleanAACache(Build_script, Settings_asset, Profile_name);
        }
        
        public static void CleanAACache(string buildScriptPath, string settingPath, string profileName) {
            GetSettingsObject(settingPath);
            SetProfile(profileName);
            IDataBuilder builderScript = AssetDatabase.LoadAssetAtPath<ScriptableObject>(buildScriptPath) as IDataBuilder;

            if (builderScript == null) {
                AL_LogTool.LogOnlyEditor(buildScriptPath + " couldn't be found or isn't a build script.");
                return;
            }

            SetBuilder(builderScript);
            AddressableAssetSettings.CleanPlayerContent(builderScript);//删除Library\com.unity.addressables下的aa内容
        }

        public static void BuildAddressablesAndPlayer() {
            bool contentBuildSucceeded = BuildAddressables();
            if (contentBuildSucceeded) {
                var options = new BuildPlayerOptions();
                BuildPlayerOptions playerSettings = BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(options);
                BuildPipeline.BuildPlayer(playerSettings);
            }
        }
        
        public static void BuildPlayer() {
            var options = new BuildPlayerOptions();
            BuildPlayerOptions playerSettings = BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(options);
            BuildPipeline.BuildPlayer(playerSettings);
        }
    }
}