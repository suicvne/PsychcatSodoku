using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IgnoreSolutions.PsychSodoku
{
    /// <summary>
    /// Where T is the type of item we want to bind to the carousel.
    ///
    /// CarouselItem components will be updated on each of our display
    /// items and they will choose how to render.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BindCarouselItems<T> : MonoBehaviour where T : UnityEngine.Object
    {
        [SerializeField] protected internal RectTransform[] _RepresentedItems;
        [SerializeField] protected internal T[] _BoundItems;
        [SerializeField] Animator _CarouselAnimator;

        [SerializeField] protected internal int _SelectedIndex;
        [SerializeField] protected internal Vector2 _DistanceBetween;

        protected internal IEnumerator _AnimationInProgress;
        WaitForFixedUpdate _WaitForFixedUpdate = new WaitForFixedUpdate();

        void Start()
        {
            CalculateDistanceBetween();

            ResetCenter();
        }

        T GetBoundItem(int index)
        {
            if (_BoundItems == null || _BoundItems.Length == 0) return null;

            return _BoundItems[index];
        }

        void CalculateDistanceBetween()
        {
            int centerIndex = Mathf.FloorToInt(_RepresentedItems.Length / 2);
            RectTransform centerItem = _RepresentedItems[centerIndex];
            RectTransform nextItem = _RepresentedItems[centerIndex + 1];

            _DistanceBetween = new Vector2(Mathf.Abs(centerItem.anchoredPosition.x - nextItem.anchoredPosition.x),
                Mathf.Abs(centerItem.anchoredPosition.y - nextItem.anchoredPosition.y));
        }

        /// <summary>
        /// +1
        /// </summary>
        public void IncrementNext()
        {
            if (_RepresentedItems.Length < 5) return;

            _AnimationInProgress = AnimateIncrementNext();
            StartCoroutine(AnimateIncrementNext());
        }

        private int TransformIndexToOffset(int index) => (index + -(Mathf.FloorToInt(_RepresentedItems.Length / 2)));

        void ResetCenter()
        {
            int centerIndex = Mathf.FloorToInt(_RepresentedItems.Length / 2);
            RectTransform centerItem = _RepresentedItems[centerIndex];

            for (int i = _RepresentedItems.Length - 1; i >= 0; i--)
            {
                Vector2 origin = _RepresentedItems[i].anchoredPosition;
                int indexToOffset = TransformIndexToOffset(i);
                Vector2 distanceOffset = _DistanceBetween * (indexToOffset);
                Vector2 destination = distanceOffset;

                _RepresentedItems[i].anchoredPosition = centerItem.anchoredPosition + distanceOffset;
            }
        }

        IEnumerator AnimateIncrementNext()
        {
            int centerIndex = Mathf.FloorToInt(_RepresentedItems.Length / 2);
            RectTransform centerItem = _RepresentedItems[centerIndex];

            for (int i = _RepresentedItems.Length - 1; i >= 0; i--)
            {
                Vector2 origin = _RepresentedItems[i].anchoredPosition;
                int indexToOffset = TransformIndexToOffset(i) + 1;
                Vector2 distanceOffset = _DistanceBetween * (indexToOffset);
                Vector2 destination = _RepresentedItems[i].anchoredPosition - distanceOffset;


                _RepresentedItems[i].anchoredPosition = centerItem.anchoredPosition + distanceOffset;
                //for (float f = 0f; f <= 1.0f; f += 1.0f * Time.fixedDeltaTime)
                //{
                //    _RepresentedItems[i].anchoredPosition = centerItem.anchoredPosition + distanceOffset;
                //}
                yield return _WaitForFixedUpdate;
            }

            



            // Move all items back by _DistanceBetween * (i + 1)
            //for (int i = _RepresentedItems.Length - 1; i >= 0; i--)
            //{

            //}

            yield return _WaitForFixedUpdate;
        }

        /// <summary>
        /// -1
        /// </summary>
        public void DecrementNext()
        {

        }
    }
}