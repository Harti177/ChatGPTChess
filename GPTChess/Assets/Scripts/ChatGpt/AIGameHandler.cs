using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(ChatGPTClient))]
public class AIGameHandler : MonoBehaviour
{
    [SerializeField] private ChatGPTClient gptClient;

    string initialPrompt = "We are going to play chess now. We can play describing the moves using algebraic notation. I will be white. Tell your move in the same format as I said. Don't repeat my move. My move is ";

    List<string[]> chatHistory; 

    public void Start()
    {
        chatHistory = new List<string[]>(); 
    }

    public void PlayFirstMove(string move)
    {
        chatHistory.Add(new string[] { "user", initialPrompt + move });

        StartCoroutine(gptClient.Ask(chatHistory, (response) =>
        {
            Debug.Log(response.Choices[0].Message.Content);
        }));
    }
}
