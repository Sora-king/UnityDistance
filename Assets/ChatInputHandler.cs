using UnityEngine;
using TMPro;

public class ChatInputHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField userInputField;
    [SerializeField] private TMP_InputField NumInputField;
    [SerializeField] private ChatGPTCommunicator chatGPTCommunicator;
    [SerializeField] private TryGroup TryGroup;


    public void OnSendButtonClicked()
    {
        string userMessage = userInputField.text;
        string NumMessage = NumInputField.text;
        int Num;
        if (!string.IsNullOrEmpty(userMessage) && !string.IsNullOrEmpty(NumMessage))
        {
            chatGPTCommunicator.SendMessageToChatGPT(userMessage, true);
            userInputField.text = ""; // 入力欄をクリア

            Num = int.Parse(NumMessage);
            TryGroup.TryStartGroupFormation(Num);
            userInputField.text = ""; // 入力欄をクリア
            // InputFieldと保存ボタンを表示
            userInputField.gameObject.SetActive(false);
        }
    }
}
