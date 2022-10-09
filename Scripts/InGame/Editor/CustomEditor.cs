using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class CustomEditor : MonoBehaviour
{
    //By 주명님
    public static void OpenAllHierarchyTree()
    {
        // 하이어라키 윈도우를 얻기 위해 일단 실행(닫혀 있을 수도 있음으로)하고, 포커스된 윈도우를 얻어온다.
        EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
        var hierarchy_window = EditorWindow.focusedWindow;

        var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
        var methodInfo = type.GetMethod("SetExpandedRecursive");

        Debug.Assert(methodInfo != null, "Failed to find SetExpandedRecursive. Maybe it changed in this version of Unity.");

        foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            methodInfo.Invoke(hierarchy_window, new object[] { go.GetInstanceID(), true });
        }
    }
}
