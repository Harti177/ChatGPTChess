using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChatGPTChess
{
    public class Coin : MonoBehaviour
    {
        public enum Type{
            Black,
            White
        }

        [SerializeField] private Type type; 

        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Material hightlightMaterial;
        [SerializeField] private Material normalMaterial;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public Type GetCoinType()
        {
            return type;
        }

        public void HighLight(bool highLight)
        {
            if (highLight)
            {
                meshRenderer.material = hightlightMaterial;
            }
            else
            {
                meshRenderer.material = normalMaterial;
            }
        }

        public void MoveTo(Vector3 position)
        {
            StartCoroutine(LerpAndMove(position, 1f));
        }

        IEnumerator LerpAndMove(Vector3 targetPosition, float duration)
        {
            Vector3 firstPosition = (new Vector3(transform.localPosition.x, 2f, transform.localPosition.z)); 
            float time = 0;
            Vector3 startPosition = transform.localPosition;
            while (time < duration/3)
            {
                transform.localPosition = Vector3.Lerp(startPosition, firstPosition, time / (duration/3) );
                time += Time.deltaTime;
                yield return null;
            }
            transform.localPosition = firstPosition;

            Vector3 secondPosition = (new Vector3(targetPosition.x, 2f, targetPosition.z));
            time = 0;
            startPosition = transform.localPosition;
            while (time < duration / 3)
            {
                transform.localPosition = Vector3.Lerp(startPosition, secondPosition, time / (duration / 3));
                time += Time.deltaTime;
                yield return null;
            }
            transform.localPosition = secondPosition;

            Vector3 thirdPosition = (new Vector3(transform.localPosition.x, 0f, transform.localPosition.z));
            time = 0;
            startPosition = transform.localPosition;
            while (time < duration / 3)
            {
                transform.localPosition = Vector3.Lerp(startPosition, thirdPosition, time / (duration / 3));
                time += Time.deltaTime;
                yield return null;
            }
            transform.localPosition = thirdPosition;
        }
    }
}