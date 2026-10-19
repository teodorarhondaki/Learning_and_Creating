using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CreateCustomizationRows
{
    const string Report = "Library/CustomizationRowsSetup.txt";
    [MenuItem("Tools/Character Customization/Create Remaining Color Rows")]
    static void Run()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += Run;
            return;
        }
        try
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.name != "CharacterLab" || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("CharacterLab must be open outside Play Mode.");
            var panel = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<RectTransform>(true))
                .Single(x => x.name == "CharacterCustomizationPanel");
            var skin = panel.Find("SkinColorRow");
            if (skin == null || skin.Find("Label")?.GetComponent<TMP_Text>() == null || skin.Find("ColorButton") == null)
                throw new InvalidOperationException("Completed SkinColorRow not found.");
            string[] names = { "Lips", "Eyes", "Hair", "Eyebrows", "Eyelashes" };
            if (names.Any(n => panel.Find(n + "ColorRow") != null))
                throw new InvalidOperationException("Some target rows already exist; stopped to preserve them.");
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Create remaining customization rows");
            for (int i = 0; i < names.Length; i++)
            {
                var row = UnityEngine.Object.Instantiate(skin.gameObject, panel, false);
                row.name = names[i] + "ColorRow";
                Undo.RegisterCreatedObjectUndo(row, "Create customization row");
                var rect = row.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -120 - 60 * i);
                row.transform.Find("Label").GetComponent<TMP_Text>().text = names[i];
            }
            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(scene);
            foreach (var name in names)
            {
                var row = panel.Find(name + "ColorRow");
                if (row == null || row.Find("Label").GetComponent<TMP_Text>().text != name)
                    throw new InvalidOperationException("Row verification failed: " + name);
            }
            File.WriteAllText(Report, "SUCCESS: Five rows cloned from live SkinColorRow. Labels verified; positions -120, -180, -240, -300, -360. Scene left unsaved for review; operation supports Undo.");
            Debug.Log("Customization rows created successfully.");
        }
        catch (Exception ex)
        {
            File.WriteAllText(Report, "FAILED: " + ex);
            Debug.LogException(ex);
        }
    }
}
