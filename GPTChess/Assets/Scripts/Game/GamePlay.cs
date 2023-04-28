using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        // Start is called before the first frame update
        void Start()
        {
            playerCoinType = Coin.Type.White;
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
                if (square.GetCoin() == null) return;

                if (square.GetCoin().GetCoinType() != playerCoinType)
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

        public void OnClickApproveSelection()
        {
            PlayMove();

            playerTurnMode = PlayerTurnMode.Finished;

            cancelSelection.gameObject.SetActive(false);
            approveSelection.gameObject.SetActive(false);
        }

        public void PlayMove()
        {
            Debug.Log(startSquare.GetValue() + endSquare.GetValue());

            OnMoveValid();
        }

        public void OnMoveNotValid()
        {
            playerTurnMode = PlayerTurnMode.NothingSelected;
        }

        public void OnMoveValid()
        {
            playerTurnMode = PlayerTurnMode.Waiting;

            startSquare.HighLight(false);
            endSquare.HighLight(false);

            endSquare.SetCoin(startSquare.GetCoin());
            startSquare.SetCoin(null);

            startSquare = null;
            endSquare = null;
        }
    }
}