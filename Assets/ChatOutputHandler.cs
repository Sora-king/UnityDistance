using UnityEngine;
using TMPro;

public class ChatOutputHandler : MonoBehaviour
{
    [SerializeField] private TMP_Text chatOutputText;
    [SerializeField] private ChatGPTCommunicator chatGPTCommunicator;

    private void OnEnable()
    {
        chatGPTCommunicator.ReplyReceived += DisplayReply;
    }

    private void OnDisable()
    {
        chatGPTCommunicator.ReplyReceived -= DisplayReply;
    }

    private void DisplayReply(string reply)
    {
        chatOutputText.text += $"\nChatGPT: {reply}";
    }
}