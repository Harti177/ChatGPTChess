using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using Newtonsoft.Json;

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

        List<string[]> chatHistory;

        [SerializeField] private MSTTS mstts;
        [SerializeField] private MSSTT mssts;

        [SerializeField] private GameObject chatWindow; 
        [SerializeField] private TextMeshProUGUI chatWindowContent; 
        [SerializeField] private TMP_InputField chatField;

        string responseMove;
        ChessMoves opponentMoves; 

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

                PlayMove();
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

            mstts.Speak(response.Split('@', '@')[0]);
            responseMove = response.Split('@', '@')[1];

            opponentMoves = JsonConvert.DeserializeObject<ChessMoves>(responseMove);

            string startSquareO = opponentMoves.Moves[opponentMoves.Moves.Count - 1].StartSquare;
            string endSquareO = opponentMoves.Moves[opponentMoves.Moves.Count - 1].EndSquare;

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
                ChessMove move = new ChessMove();
                move.StartSquare = startSquare.GetValue();
                move.EndSquare = endSquare.GetValue();
                move.Type = startSquare.GetCoin().GetCoinCharacter().ToString();
                ChessMoves moves = new ChessMoves();
                moves.Moves = new List<ChessMove>() { move };
                PlayFirstMove(JsonConvert.SerializeObject(moves));
            }
            else
            {
                ChessMove move = new ChessMove();
                move.StartSquare = startSquare.GetValue();
                move.EndSquare = endSquare.GetValue();
                move.Type = startSquare.GetCoin().GetCoinCharacter().ToString();
                opponentMoves.Moves.Add(move);
                PlayNextMove(JsonConvert.SerializeObject(opponentMoves));
            }

            playerTurnMode = PlayerTurnMode.Finished;

            OnPlayerMoveFinished(); 
        }

        public void PlayFirstMove(string move)
        {
            AddToChatHistory(new string[] { "user", @"You are a grandmaster playing chess with me. I tell my move in the following json format. You append your move and just provide the updated json. Always place the json between @ and @." + move }); 

            StartCoroutine(gptClient.Ask(chatHistory, (response) =>
            {
                OnOpponentMove(response.Choices[0].Message.Content);
                AddToChatHistory(new string[] { "system", response.Choices[0].Message.Content }); 
            }));
        }

        public void PlayNextMove(string move)
        {
            AddToChatHistory(new string[] { "user", move }); 

            Debug.Log(chatHistory.Count);
            StartCoroutine(gptClient.Ask(chatHistory, (response) =>
            {
                OnOpponentMove(response.Choices[0].Message.Content);
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
                    chatHistoryText += "\n" + "<color=blue>" + chat[0] + ": " + chat[1].Split('@', '@')[0] + "</color=blue>";
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
                mssts.ClearMessage(); 
                chatField.text = "";
                mstts.Speak(response.Choices[0].Message.Content);
                AddToChatHistory(new string[] { "system", response.Choices[0].Message.Content });
            }));
        }

        public void OnClickClear()
        {
            mssts.ClearMessage();
            chatField.text = "";
        }

        struct CoinDetail
        {
            public Coin coin;
            public Vector3 initialPosition;
        }

        public class ChessMoves
        {
            public List<ChessMove> Moves { get; set; }
        }

        public class ChessMove
        {
            public string StartSquare { get; set; }
            public string EndSquare { get; set; }
            public string Type { get; set; }
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