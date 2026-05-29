using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageSetting : MonoBehaviour
{
    public ModeSetting Ms;

    public TMP_Text stageName;
    public TMP_Text stageDescriptionText;
    public Image stagePreviewImage;
    public Stage[] Survive_stages;
    public Stage[] Score_stages;

    public int currentIndex = 0;

    public static string SelectedSceneName;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // InspectorでSceneAssetをセットしたとき自動でsceneNameに転写
        foreach (var s in Survive_stages)
            if (s.sceneAsset != null)
                s.sceneName = s.sceneAsset.name;

        foreach (var s in Score_stages)
            if (s.sceneAsset != null)
                s.sceneName = s.sceneAsset.name;
    }
#endif

    private void Start()
    {
        currentIndex = 0;
        UpdatePreview();
    }

    public void NextStage()
    {
        if (Ms.currentMode == ModeSetting.GameMode.Survival)
        {
            currentIndex = (currentIndex + 1) % Survive_stages.Length;
        }
        else if (Ms.currentMode == ModeSetting.GameMode.Score)
        {
            currentIndex = (currentIndex + 1) % Score_stages.Length;
        }
        UpdatePreview();

    }

    public void PreviousStage()
    {
        if (Ms.currentMode == ModeSetting.GameMode.Survival)
        {
            currentIndex = (currentIndex + Survive_stages.Length - 1) % Survive_stages.Length;
        }
        else if(Ms.currentMode == ModeSetting.GameMode.Score)
        {
            currentIndex = (currentIndex + Score_stages.Length - 1) % Score_stages.Length;
        }
        UpdatePreview();

    }

    public void UpdatePreview()
    {
        if (Survive_stages == null || Survive_stages.Length == 0 || Score_stages == null || Score_stages.Length == 0)
        {
            return;
        }

        if (Ms.currentMode == ModeSetting.GameMode.Survival)
        {
            stageName.text = Survive_stages[currentIndex].stageName;
            stagePreviewImage.sprite = Survive_stages[currentIndex].previewSprite;
            stageDescriptionText.text = Survive_stages[currentIndex].description;
        }
        else if (Ms.currentMode == ModeSetting.GameMode.Score)
        {
            stageName.text = Score_stages[currentIndex].stageName;
            stagePreviewImage.sprite = Score_stages[currentIndex].previewSprite;
            stageDescriptionText.text = Score_stages[currentIndex].description;
        }

        if (Ms.currentMode == ModeSetting.GameMode.Survival)
        {
            SelectedSceneName = Survive_stages[currentIndex].sceneName;
        }
        else if(Ms.currentMode==ModeSetting.GameMode.Score)
        {
            SelectedSceneName = Score_stages[currentIndex].sceneName;
        }
    }
}
