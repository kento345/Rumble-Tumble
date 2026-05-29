using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI; // ★Buttonコンポーネントを使用

[System.Serializable]
public class PanelData
{
    public string panelName;
    public GameObject panel;
    public Animator animator;

    [Header("このパネル用のアニメーターパラメータ名")]
    public string parameterName;

    [Header("制限設定")]
    [Tooltip("チェックを入れると、このパネルが開いている間は他のパネルを開けなくなります")]
    public bool isModal;

    [Header("コントローラー初期フォーカスボタン")]
    public GameObject firstSelectedButton;
}

public class OptionUIManager : MonoBehaviour
{
    [Header("ゲーム起動時に最初に選択させたいメインボタン")]
    [SerializeField] private GameObject mainFirstSelectedButton;

    [Header("【アタッチ用】各ボタンの本体")]
    [SerializeField] private Button actualStartButton;   // Aボタンで実行したいStartButton
    [SerializeField] private Button actualOptionButton;  // OPTIONSボタンで実行したいOption_BT ★追加

    [Header("複数UIパネルの設定")]
    [SerializeField] private List<PanelData> panels = new List<PanelData>();

    [Tooltip("閉じるアニメーションの秒数")]
    [SerializeField] private float closeAnimationDuration = 0.5f;

    [Header("ゲーム起動時に自動で開くパネルの番号（開かない場合は -1）")]
    [SerializeField] private int defaultOpenPanelIndex = -1;

    private Coroutine currentCoroutine = null;

    private void OnEnable()
    {
        Time.timeScale = 1f;

        // 全パネルの初期化（非表示にする）
        foreach (var data in panels)
        {
            if (data.panel != null) data.panel.SetActive(false);
            if (data.animator != null)
            {
                data.animator.keepAnimatorStateOnDisable = false;
                if (!string.IsNullOrEmpty(data.parameterName))
                    data.animator.SetBool(data.parameterName, false);
                data.animator.Play("Idle", 0, 0f);
            }
        }

        // メインボタンへの初期フォーカス
        if (mainFirstSelectedButton != null)
        {
            StartCoroutine(FocusMainButtonRoutine());
        }

        // 初期表示パネルがあれば開く
        if (defaultOpenPanelIndex >= 0 && defaultOpenPanelIndex < panels.Count)
        {
            ToggleOptionPanel(defaultOpenPanelIndex);
        }
    }

    private void Update()
    {
        var gamepad = Gamepad.current;
        var keyboard = Keyboard.current;

        // ----------------------------------------------------
        // ① OPTIONSボタン（ゲームパッドのMenu / キーボードのEsc）
        // ----------------------------------------------------
        bool optionsPressed = false;
        if (gamepad != null && gamepad.startButton.wasPressedThisFrame) optionsPressed = true;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) optionsPressed = true;

        if (optionsPressed)
        {
            // ★超重要：すでにオプション画面（Option_conなど）が開いているなら
            // OPTIONSボタンの入力は「1ミリも受け付けず、完全に無視」する
            if (IsAnyPanelActive())
            {
                Debug.Log("【OPTIONSボタン】ガード：オプション画面が開いているため、入力を完全にブロックしました。×ボタンで閉じてください。");
                return;
            }

            // パネルが閉じていて、かつOption_BTがインスペクターに設定されている場合のみ実行
            if (actualOptionButton != null)
            {
                Debug.Log("【OPTIONSボタン】オプション画面を開きます。");

                // 選択状態の重複バグを防ぐため、一旦EventSystemの選択をリセット
                if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);

                actualOptionButton.onClick.Invoke();
            }
        }

        // ----------------------------------------------------
        // ② Aボタン（ゲームパッドのAボタン / キーボードのSpace）
        // ----------------------------------------------------
        bool aButtonPressed = false;
        if (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame) aButtonPressed = true;
        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame) aButtonPressed = true;

        if (aButtonPressed)
        {
            // オプション画面が開いている間は、メイン画面のStartButton暴発を絶対に防ぐ
            if (!IsAnyPanelActive())
            {
                if (EventSystem.current != null)
                {
                    GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
                    if (currentSelected != null)
                    {
                        Debug.Log($"現在の選択UI: {currentSelected.name} / 通常の決定処理を行います。");
                    }
                    else
                    {
                        // 完全にフォーカスが外れている時だけの救済スタート
                        if (actualStartButton != null)
                        {
                            Debug.Log("【救済措置】フォーカス迷子のため、AボタンでStartButtonを実行。");
                            actualStartButton.onClick.Invoke();
                        }
                    }
                }
            }
        }
    }

    // 安全にフォーカスを当てるコルーチン
    private IEnumerator FocusMainButtonRoutine()
    {
        yield return new WaitForSecondsRealtime(0.05f);

        if (EventSystem.current != null && mainFirstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(mainFirstSelectedButton);
        }
    }

    // パネルの開閉処理（トグル）
    public void ToggleOptionPanel(int panelIndex)
    {
        if (panelIndex < 0 || panelIndex >= panels.Count) return;
        if (currentCoroutine != null) return;

        // モーダルブロックチェック
        for (int i = 0; i < panels.Count; i++)
        {
            if (panels[i].isModal && panels[i].panel.activeSelf && i != panelIndex)
            {
                Debug.Log($"{panels[i].panelName} が開いているため、別のパネルは開けません！");
                return;
            }
        }

        currentCoroutine = StartCoroutine(SwitchPanelSequence(panelIndex));
    }

    private IEnumerator SwitchPanelSequence(int targetIndex)
    {
        // --- 古いパネルを閉じる ---
        for (int i = 0; i < panels.Count; i++)
        {
            if (i != targetIndex && panels[i].panel.activeSelf)
            {
                if (panels[i].animator != null && !string.IsNullOrEmpty(panels[i].parameterName))
                {
                    panels[i].animator.SetBool(panels[i].parameterName, false);
                }

                yield return new WaitForSeconds(closeAnimationDuration);
                panels[i].panel.SetActive(false);
            }
        }

        // --- 新しいパネルを開く（または閉じる） ---
        PanelData targetData = panels[targetIndex];
        bool isCurrentlyActive = targetData.panel.activeSelf;

        if (!isCurrentlyActive)
        {
            targetData.panel.SetActive(true);
            if (targetData.animator != null && !string.IsNullOrEmpty(targetData.parameterName))
            {
                targetData.animator.SetBool(targetData.parameterName, true);
            }

            // パネルが開いたら、その中の初期ボタンにフォーカス
            if (targetData.firstSelectedButton != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(targetData.firstSelectedButton);
            }
        }
        else
        {
            if (targetData.animator != null && !string.IsNullOrEmpty(targetData.parameterName))
            {
                targetData.animator.SetBool(targetData.parameterName, false);
                yield return new WaitForSecondsRealtime(closeAnimationDuration);
            }
            targetData.panel.SetActive(false);

            // パネルが閉じたら、メイン画面のStartButtonにフォーカスを戻す
            if (mainFirstSelectedButton != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(mainFirstSelectedButton);
            }
        }

        currentCoroutine = null;
    }

    // いずれかのサブパネルが開いているか確認
    private bool IsAnyPanelActive()
    {
        foreach (var data in panels)
        {
            if (data.panel != null && data.panel.activeSelf) return true;
        }
        return false;
    }
}