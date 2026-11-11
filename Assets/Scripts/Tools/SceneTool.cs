using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTool
{
    public static Transform GetSceneTransformRoot(string sceneName, string name)
    {
        List<GameObject> allRoots = new List<GameObject>();
        for (int i = 0; i < SceneManager.sceneCount; i++) {
            Scene scene = SceneManager.GetSceneAt(i);
            
            if(scene.name != sceneName) continue;
            if (!scene.isLoaded) continue;
            
            allRoots.AddRange(scene.GetRootGameObjects());
        }

        foreach (var root in allRoots)
        {
            if (root.name != name) continue;
            
            return root.transform;
        }

        return null;
    }
}
