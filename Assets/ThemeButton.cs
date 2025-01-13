using UnityEngine;
using TMPro; // TextMeshProを使用
using UnityEngine.UI; // 必須：Button型を使用するため

public class ThemeButton : MonoBehaviour
{
    public void OnClick()
    {
        // NameManagerのインスタンスを取得
        themeManager themeManager = FindObjectOfType<themeManager>();

        if (themeManager != null)
        {
            // OnShowInputButtonClickedメソッドを呼び出し
            themeManager.OnClick();
        }
        else
        {
            Debug.LogError("NameManagerが見つかりません！");
        }     
    }
}
