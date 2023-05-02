using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

namespace ChatGPTChess
{
    public class GamePlay : MonoBehaviour
    {
        private Coin.Type playerCoinType;

        private enum PlayerTurnMode
        {
            NothingSelected,
            StartSquareSelected,
            EndSquareSelected,
            Finished, 
            Waiting
        }

        private PlayerTurnMode playerTurnMode = PlayerTurnMode.NothingSelected;

        private Square startSquare;
        private Square endSquare;

        [SerializeField] private Button cancelSelection;
        [SerializeField] private Button approveSelection;

        [SerializeField] Transform coinsParent;
        [SerializeField] Transform squaresParent;
        List<CoinDetail> coins;
        Dictionary<string, Square> squares; 

        //ChatGPT
        [SerializeField] private ChatGPTClient gptClient;
        string initialPrompt = "We are going to play chess now. We can play describing the moves using algebraic notation. I will play first and move white coins. So you will be moving black coins. My move is ";
        List<string[]> chatHistory;

        [SerializeField] private MSTTS mstts;

        [SerializeField] private GameObject chatWindow; 
        [SerializeField] private TextMeshProUGUI chatWindowContent; 
        [SerializeField] private TMP_InputField chatField; 

        bool firstMoveDone = false;

        // Start is called before the first frame update
        void Start()
        {
            playerCoinType = Coin.Type.White;

            Initialize();
        }

        private void Initialize()
        {
            chatHistory = new List<string[]>();

            coins = new List<CoinDetail>();
            squares = new Dictionary<string, Square>(); 

            foreach (Transform child in coinsParent)
            {
                CoinDetail coinDetail = new CoinDetail
                {
                    coin = child.GetComponent<Coin>(),
                    initialPosition = child.localPosition
                };
                coins.Add(coinDetail);
            }

            foreach (Transform child in squaresParent)
            {
                squares.Add(child.GetComponent<Square>().GetValue(), child.GetComponent<Square>());
            }

            SetCoinsToSquaresInitially();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider != null)
                    {
                        if (hit.collider.gameObject.GetComponent<Square>() != null)
                        {
                            OnClickSquare(hit.collider.gameObject.GetComponent<Square>());
                        }
                    }
                }
            }

#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider != null)
                    {
                        if (hit.collider.gameObject.GetComponent<Square>() != null)
                        {
                            OnClickSquare(hit.collider.gameObject.GetComponent<Square>());
                        }
                    }
                }
            }
#endif
        }

        public void OnClickSquare(Square square)
        {
            if (startSquare == null)
            {
                if (square.GetCoin() == null || square.GetCoin().GetCoinType() != playerCoinType)
                {
                    return;
                }

                startSquare = square;
                startSquare.HighLight(true);

                cancelSelection.gameObject.SetActive(true);
                playerTurnMode = PlayerTurnMode.StartSquareSelected;

                return;
            }

            if (endSquare == null)
            {
                if (square.GetCoin() != null)
                {
                    if(square.GetCoin().GetCoinType() == playerCoinType)
                    {
                        return; 
                    }
                }

                endSquare = square;
                endSquare.HighLight(true);

                approveSelection.gameObject.SetActive(true);
                playerTurnMode = PlayerTurnMode.EndSquareSelected;
            }
        }

        public void OnClickCancelSelection()
        {
            switch (playerTurnMode)
            {
                case PlayerTurnMode.NothingSelected:
                    cancelSelection.gameObject.SetActive(false);
                    break;
                case PlayerTurnMode.StartSquareSelected:
                    cancelSelection.gameObject.SetActive(false);
                    startSquare.HighLight(false);
                    startSquare = null;
                    playerTurnMode = PlayerTurnMode.NothingSelected;
                    break;
                case PlayerTurnMode.EndSquareSelected:
                    approveSelection.gameObject.SetActive(false);
                    endSquare.HighLight(false);
                    endSquare = null;
                    playerTurnMode = PlayerTurnMode.StartSquareSelected;
                    break;
            }
        }

        public void OnPlayerMoveFinished()
        {
            playerTurnMode = PlayerTurnMode.Finished;

            cancelSelection.gameObject.SetActive(false);
            approveSelection.gameObject.SetActive(false);

            endSquare.SetCoin(startSquare.GetCoin());
            startSquare.SetCoin(null);

            startSquare.HighLight(false);
            endSquare.HighLight(false);
        }

        public void OnOpponentMove(string response)
        {
            playerTurnMode = PlayerTurnMode.NothingSelected;

            //if(response.Contains("Illegal") || response.Contains("illegal"))
            //{
            //    startSquare.SetCoin(endSquare.GetCoin());
            //    endSquare.SetCoin(null);

            //      return;
            //}

            startSquare = null;
            endSquare = null;

            string responseMove = response.Split('(', ')')[1];
            responseMove = responseMove.Replace(" ", "");
            string startSquareO = responseMove.Split(',')[0];
            string endSquareO = responseMove.Split(',')[1];

            squares[endSquareO].SetCoin(squares[startSquareO].GetCoin());
            squares[startSquareO].SetCoin(null);

        }

        public void OnClickApproveSelection()
        {
            PlayMove();
        }

        public void PlayMove()
        {
            if (!firstMoveDone)
            {
                firstMoveDone = true;
                PlayFirstMove("(" + startSquare.GetValue() + "," + endSquare.GetValue() + ")");
            }
            else
            {

                PlayNextMove("(" + startSquare.GetValue() + "," + endSquare.GetValue() + ")");
            }

            playerTurnMode = PlayerTurnMode.Finished;

            OnPlayerMoveFinished(); 
        }

        public void PlayFirstMove(string move)
        {
            AddToChatHistory(new string[] { "user", initialPrompt + move }); 

            StartCoroutine(gptClient.Ask(chatHistory, (response) =>
            {
                OnOpponentMove(response.Choices[0].Message.Content);
                mstts.Speak(response.Choices[0].Message.Content);
                AddToChatHistory(new string[] { "system", response.Choices[0].Message.Content }); 
            }));
        }

        public void PlayNextMove(string move)
        {
            AddToChatHistory(new string[] { "user", "My next move is" + move }); 

            Debug.Log(chatHistory.Count);
            StartCoroutine(gptClient.Ask(chatHistory, (response) =>
            {
                OnOpponentMove(response.Choices[0].Message.Content);
                mstts.Speak(response.Choices[0].Message.Content);
                AddToChatHistory(new string[] { "system", response.Choices[0].Message.Content }); 
            }));
        }

        public void AddToChatHistory(string[] content)
        {
            chatHistory.Add(content);

            string chatHistoryText = "";

            foreach (string[] chat in chatHistory)
            {
                if(chat[0] == "user")
                    chatHistoryText += "\n" + chat[0] + ": " + chat[1];
                else
                    chatHistoryText += "\n" + "<color=blue>" + chat[0] + ": " + chat[1] + "</color=blue>";
            }

            chatWindowContent.text = chatHistoryText; 
        }

        public void OnClickShowWindow()
        {
            chatWindow.SetActive(!chatWindow.activeSelf);
        }

        public void OnClickAsk()
        {
            if (string.IsNullOrEmpty(chatField.text)) return; 

            AddToChatHistory(new string[] { "user", chatField.text});

            StartCoroutine(gptClient.Ask(chatHistory, (response) =>
            {
                chatField.text = "";
                mstts.Speak(response.Choices[0].Message.Content);
                AddToChatHistory(new string[] { "system", response.Choices[0].Message.Content });
            }));
        }

        struct CoinDetail
        {
            public Coin coin;
            public Vector3 initialPosition;
        }

        public void SetCoinsToSquaresInitially()
        {
            List<string> keys = squares.Keys.ToList(); 
            for (int i = 0; i < keys.Count; i++)
            {
                if (i < 16)
                {
                    squares[keys[i]].SetCoin(coins[i].coin);
                }

                if (i > 47)
                {
                    squares[keys[i]].SetCoin(coins[i - 32].coin);
                }
            }
        }
    }
}