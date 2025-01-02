using UnityEngine;
using TMPro;

public class ChatInputHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField userInputField;
    [SerializeField] private ChatGPTCommunicator chatGPTCommunicator;

    public void OnSendButtonClicked()
    {
        string userMessage = userInputField.text;
        if (!string.IsNullOrEmpty(userMessage))
        {
            chatGPTCommunicator.SendMessageToChatGPT(userMessage);
            userInputField.text = ""; // 入力欄をクリア
        }
    }
}
