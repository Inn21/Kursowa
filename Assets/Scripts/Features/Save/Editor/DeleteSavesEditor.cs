using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _PROJECT.Scripts.Application.Features.Save.Editor
{
    namespace FlopHouse.Saves.Editor
    {
        public static class DeleteSavesEditor
        {
            private static string GetSavePath(SaveFileVariant variant)
            {
                var basePath = UnityEngine.Application.persistentDataPath;
                return variant switch
                {
                    SaveFileVariant.General => Path.Combine(basePath, "GeneralData.json"),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            [MenuItem("FlopHouse/Saves/Delete General Save")]
            public static void DeleteGeneralSaves()
            {
                DeleteSave(SaveFileVariant.General);
            }

            [MenuItem("FlopHouse/Saves/Delete Player Save")]

            private static void DeleteSave(SaveFileVariant variant)
            {
                var path = GetSavePath(variant);
                if (File.Exists(path))
                {
                    File.Delete(path);
                    Debug.Log($"[DeleteSavesEditor] Deleted {variant} save: {path}");
                }
                else
                {
                    Debug.LogWarning($"[DeleteSavesEditor] Save file not found for {variant}: {path}");
                }
            }
        }
    }
}