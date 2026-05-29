using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ModeSetting : MonoBehaviour
{
    public enum GameMode { Survival, Score}
    public GameMode currentMode = GameMode.Survival;

    public static GameMode SelectMode = GameMode.Survival;

    public TMP_Text modeName;
    public TMP_Text modeDescription;

    public StageSetting Ss;

    public void Awake()
    {
        currentMode = SelectMode;
        UpdateModeText();
    }

    public void NextMode()
    {
        currentMode = (GameMode)(((int)currentMode + 1) % 2);
        ApplyChange();
    }

    public void PreviousMode()
    {
        int totalModes = System.Enum.GetValues(typeof(GameMode)).Length;
        currentMode = (GameMode)(((int)currentMode - 1 + totalModes) % totalModes);
        ApplyChange();
    }


    private void ApplyChange()
    {
        SelectMode = currentMode;

        GameManager_M.selectedGameMode=(currentMode == GameMode.Survival)? GameManager_M.Mode.Survival : GameManager_M.Mode.ScoreMode;

        UpdateModeText();

    }

    public void UpdateModeText()
    {
        switch(currentMode)
        {
            case GameMode.Survival: 
                modeName.text = "Survaival";
                modeDescription.text = "最後まで3回生き残れば勝ち。時間内に終わらなかったらサドンデスでワンパンでぶっ飛ばせるぞ！！";
                
                break;

            case GameMode.Score:    
                modeName.text = "Score";
                modeDescription.text = "落ちてくる球を拾って一番ポイントを稼いだ人の勝ち。何度でも復活できるぞ！！";

                break;
        }

        if (Ss != null)
        {
            int newLength = (currentMode == GameMode.Survival) ? Ss.Survive_stages.Length : Ss.Score_stages.Length;
            Ss.currentIndex = Mathf.Min(Ss.currentIndex, newLength - 1);
            Ss.UpdatePreview();
        }
    }
    public void OnConfirmSettings()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Start");
    }
}
