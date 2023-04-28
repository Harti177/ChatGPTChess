using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChatGPTChess
{
    public class Square : MonoBehaviour
    {
        [SerializeField] private string squareValue;

        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Material hightlightMaterial;
        [SerializeField] private Material normalMaterial;

        [SerializeField] private Coin holdingCoin = null;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public string GetValue()
        {
            return squareValue;
        }

        public Coin GetCoin()
        {
            return holdingCoin;
        }

        public void SetCoin(Coin coin)
        {
            holdingCoin = coin;

            if(holdingCoin != null) holdingCoin.MoveTo(transform.localPosition);
        }

        public void HighLight(bool highLight)
        {
            if (highLight)
            {
                meshRenderer.enabled = true;
                meshRenderer.material = hightlightMaterial;
            }
            else
            {
                meshRenderer.enabled = false; 
                meshRenderer.material = normalMaterial;
            }

            //if (holdingCoin != null)
            //{
            //    holdingCoin.HighLight(highLight);
            //}
        }
    }

}