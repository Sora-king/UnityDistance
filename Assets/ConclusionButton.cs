using UnityEngine;

public class ConclusionButton : MonoBehaviour
{
    public void OnClick()
    {
        Debug.Log("結論ボタンがクリックされました。");
        
        // NameManagerのインスタンスを取得
        concManager concManager = FindObjectOfType<concManager>();

        if (concManager != null)
        {
            // OnShowInputButtonClickedメソッドを呼び出し
            concManager.OnClick();
        }
        else
        {
            Debug.LogError("concManagerが見つかりません！");
        }     
        
    }

   /* private void ShowConclusion()
    {
        // 仮の処理：結論の内容を表示する
        Debug.Log("結論を表示中...");
        // ここに具体的な結論表示のロジックを記述
        // 例: UIのテキストを更新したり、ポップアップを表示する
    }*/
}
