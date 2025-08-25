#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public static class SceneFunctions
{
    [MenuItem("Tools/Clean Addressable", false, 0)]
    public static async void CleanAddressable()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            return;
        }

        List<int> missingGroupsIndices = new List<int>();
        for (int i = 0; i < settings.groups.Count; i++)
        {
            var g = settings.groups[i];
            if (g == null)
                missingGroupsIndices.Add(i);
        }

        if (missingGroupsIndices.Count > 0)
        {
            Debug.Log("Addressable settings contains " + missingGroupsIndices.Count +
                      " group reference(s) that are no longer there. Removing reference(s).");
            for (int i = missingGroupsIndices.Count - 1; i >= 0; i--)
            {
                settings.groups.RemoveAt(missingGroupsIndices[i]);
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupRemoved, null, true, true);
        }


        var assetfile = "Assets/AddressableAssetsData/AddressableImportSettings.asset";
        var data = AssetDatabase.LoadAssetAtPath<AddressableImportSettings>(assetfile);
        data.CleanEmptyGroup();

        assetfile = "Assets/_GameAssets/_AddressableAssets";

        List<string> assetPaths = new List<string>();
        assetPaths.Add(assetfile);
        
        AddressableImporter.FolderImporter.ReimportFolders(assetPaths);
    }
}
#endif