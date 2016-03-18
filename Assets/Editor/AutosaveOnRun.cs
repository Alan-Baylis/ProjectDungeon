using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class AutosaveOnRun : ScriptableObject
{
  static AutosaveOnRun()
  {
    EditorApplication.playmodeStateChanged = () =>
    {
      if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
      {
        //Debug.Log("Saving scene.");
        //EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        //EditorApplication.SaveAssets();


        //EditorSceneManager.LoadScene()
      }
    };
  }
}