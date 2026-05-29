using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelFocusController : MonoBehaviour
{
    [Header("このパネルが開いた時に最初に選択するボタン")]
    [SerializeField] private GameObject firstSelectedButton;

    // パネルがアクティブ（表示）になった瞬間に実行される
    void OnEnable()
    {
        if (firstSelectedButton != null)
        {
            // 1フレーム待ってから選択しないと、たまに反映されないことがある
            Invoke(nameof(SetFocus), 0.01f);
        }
    }

    private void SetFocus()
    {
        // 現在の選択状態をクリアしてから新しく選択
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }
}