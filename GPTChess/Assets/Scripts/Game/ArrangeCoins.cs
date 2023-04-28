using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChatGPTChess
{
    public class ArrangeCoins : MonoBehaviour
    {
        [SerializeField] Transform coinsParent;
        [SerializeField] Transform squaresParent;

        List<Coin> coins;
        List<Square> squares;

        List<Vector3> coinPositions;

        // Start is called before the first frame update
        void Start()
        {
            coins = new List<Coin>();
            squares = new List<Square>();
            coinPositions = new List<Vector3>();

            foreach (Transform child in coinsParent)
            {
                coins.Add(child.GetComponent<Coin>());
                coinPositions.Add(child.localPosition);
            }

            foreach (Transform child in squaresParent)
            {
                squares.Add(child.GetComponent<Square>());
            }

            for (int i = 0; i < squares.Count; i++)
            {
                if(i < 16)
                {
                    squares[i].SetCoin(coins[i]);
                }

                if(i > 47)
                {
                    squares[i].SetCoin(coins[i-32]); 
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        void ReArrange()
        {

        }
    }

}