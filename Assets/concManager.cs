using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshProを使用
using UnityEngine.UI; // 必須：Button型を使用するため

public class concManager : MonoBehaviour
{
    public TMP_InputField concInputField; // 議題を入力するUI
    public Button concSaveButton;
    private bool isVisible = false;       // 現在の表示状態

    // Start is called before the first frame update
    void Start()
    {
        //public Button showInputButton; // InputFieldを表示するボタン
        concInputField.gameObject.SetActive(false);
        concSaveButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    public void OnClick()
    {
        Debug.Log("ボタンがクリックされました。");
        // テーマボタンの処理をここに記述
        // 表示状態を反転
        isVisible = !isVisible;
        
        // InputFieldと保存ボタンを表示
        concInputField.gameObject.SetActive(isVisible);
        concSaveButton.gameObject.SetActive(isVisible);        
    }
}