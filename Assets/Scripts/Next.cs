using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Next : MonoBehaviour
{
    public ModeSetting Ms;
    public StageSetting Ss;

    public string Scene;

    //public string SceneName = Stage.sceneAsset.name;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NextScene()
    {
        if (Ms.currentMode == ModeSetting.GameMode.Survival)
        {
            StageSetting.SelectedSceneName = Ss.Survive_stages[Ss.currentIndex].sceneName;
        }
        else if (Ms.currentMode == ModeSetting.GameMode.Score)
        {
            StageSetting.SelectedSceneName = Ss.Score_stages[Ss.currentIndex].sceneName;
        }

        Debug.Log(Scene + "をロード中");
        SceneManager.LoadScene("prot");
    }
}
