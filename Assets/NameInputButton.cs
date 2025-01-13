using UnityEngine;

public class NameInputButton : MonoBehaviour
{
    public void OnClick()
    {
         // NameManagerのインスタンスを取得
        NameManager nameManager = FindObjectOfType<NameManager>();

        if (nameManager != null)
        {
            // OnShowInputButtonClickedメソッドを呼び出し
            nameManager.OnShowInputButtonClicked();
        }
        else
        {
            Debug.LogError("NameManagerが見つかりません！");
        }
    }
}
