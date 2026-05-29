using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class Stage
{
    public string stageName;
    public string description;
    public Sprite previewSprite;

#if UNITY_EDITOR
    public SceneAsset sceneAsset;

    // Inspector‚Å•ÏX‚³‚ê‚½‚Æ‚«©“®‚ÅsceneName‚É•Û‘¶
#endif

    public string sceneName;
}