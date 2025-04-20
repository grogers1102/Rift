using UnityEngine;
using System.Collections;

namespace Rift.Level
{
    public class TransitionManager : MonoBehaviour
    {
        public static TransitionManager Instance { get; private set; }

        [Header("Transition Settings")]
        public Animator transitionAnimator;
        public float transitionTime = 1f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public IEnumerator FadeOut()
        {
            if (transitionAnimator != null)
            {
                transitionAnimator.SetTrigger("FadeOut");
                yield return new WaitForSeconds(transitionTime);
            }
        }

        public IEnumerator FadeIn()
        {
            if (transitionAnimator != null)
            {
                transitionAnimator.SetTrigger("FadeIn");
                yield return new WaitForSeconds(transitionTime);
            }
        }
    }
} 