using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadTest : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        Debug.Log("Loading scene: \"" + sceneName + "\"");

        // Check if the scene exists in the build settings
        if (SceneUtility.GetBuildIndexByScenePath(sceneName) != -1)
        {
            // Load the scene
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            Debug.Log("Scene load request sent for: " + sceneName);
        }
        else
        {
            Debug.LogError("Scene not found in build settings: " + sceneName);
        }
    }
}
