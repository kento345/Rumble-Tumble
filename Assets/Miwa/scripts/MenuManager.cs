using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor.ShaderGraph.Drawing.Inspector.PropertyDrawers;
#endif

public class MenuManager : MonoBehaviour
{
    // サバイバルボタンに登録

    private const float DelayTime = 0.5f;
    private float canTransitionTime = 0f;

    public TMP_Text modeNameText;

    private GameManager_M.Mode[] modes =
    {
        GameManager_M.Mode.Survival,
        GameManager_M.Mode.ScoreMode
    };

    private int currentSelection = 0;

    private GameObject joinObj;

    void Start()
    {
        joinObj = GameObject.Find("JoinedManager");

        canTransitionTime = Time.time + DelayTime;

        UpdateModeDisplay();

    }

    public void OnClickNextMode()
    {
        currentSelection = (currentSelection + 1) % modes.Length;
        UpdateModeDisplay();
    }

    public void OnClickPrevMode()
    {
        currentSelection--;
        if (currentSelection < 0) currentSelection = modes.Length - 1;
        UpdateModeDisplay();
    }



   void UpdateModeDisplay()
    {
        GameManager_M.selectedGameMode = modes[currentSelection];

        if(modeNameText != null)
        {
            switch (modes[currentSelection])
            {
                case GameManager_M.Mode.Survival:
                    modeNameText.text = "SURVIVAL";
                    break;
                case GameManager_M.Mode.ScoreMode:
                    modeNameText.text = "SCORE ATTACK";
                    break;
            }
        }
    }

    public void LoadBattleScene(string sceneName)
    {
        if (Time.time < canTransitionTime) return;

        // 決定時のSE
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySE(SoundManager.Instance.gameStartBtnSE);

        SceneManager.LoadScene(sceneName);
        Destroy(joinObj);
    }
}
