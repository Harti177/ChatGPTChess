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

        private Vector3 originalPosition; 

        // Start is called before the first frame update
        void Start()
        {
            originalPosition = transform.localPosition;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetValue(string value)
        {
            squareValue = value; 
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
            if (holdingCoin != null)
            {
                if(originalPosition.x < 4)
                {
                    holdingCoin.transform.localPosition = new Vector3(originalPosition.x - 4, originalPosition.y, originalPosition.z);
                }
                else
                {
                    holdingCoin.transform.localPosition = new Vector3(originalPosition.x + 4, originalPosition.y, originalPosition.z);
                }
            }

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