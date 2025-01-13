using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshProを使用
using UnityEngine.UI; // 必須：Button型を使用するため

public class themeManager : MonoBehaviour
{
    public TMP_InputField themeInputField; // 議題を入力するUI
    public Button themeSaveButton;
    private bool isVisible = false;       // 現在の表示状態
    
    // Start is called before the first frame update
    void Start()
    {
        //public Button showInputButton; // InputFieldを表示するボタン
        themeInputField.gameObject.SetActive(false);
        themeSaveButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    public void OnClick()
    {
        Debug.Log("テーマボタンがクリックされました。");
        // テーマボタンの処理をここに記述
        // 表示状態を反転
        isVisible = !isVisible;
        
        // InputFieldと保存ボタンを表示
        themeInputField.gameObject.SetActive(isVisible);
        themeSaveButton.gameObject.SetActive(isVisible);        
    }
}
