using UnityEditor;
using UnityEngine;
using Utils.Scenes;

namespace Utils.EditorTools
{
    public static class SceneEditorTool
    {
        [MenuItem("SceneEditorTool/GotoFirstScene")]
        private static void GotoScene0() => GotoScene(0);

        [MenuItem("SceneEditorTool/GotoTestLargeScene")]
        private static void GotoScene9() => GotoScene(9);

        private static void GotoScene(int lvlIndex)
        {
            if (Application.isPlaying)
                ScenesChanger.GotoScene($"Level{lvlIndex}");
        }
    }
}