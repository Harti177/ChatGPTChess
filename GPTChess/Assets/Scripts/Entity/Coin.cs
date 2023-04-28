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
            StartCoroutine(LerpPosition(position, 2f));
        }

        IEnumerator LerpPosition(Vector3 targetPosition, float duration)
        {
            float time = 0;
            Vector3 startPosition = transform.localPosition;
            while (time < duration)
            {
                transform.localPosition = Vector3.Lerp(startPosition, targetPosition, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            transform.localPosition = targetPosition;
        }
    }
}