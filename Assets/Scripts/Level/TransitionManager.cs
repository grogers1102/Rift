using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace Rift.Level
{
    public class TransitionManager : MonoBehaviour
    {
        public static TransitionManager Instance { get; private set; }

        [Header("Transition Settings")]
        [SerializeField] private Animator transitionAnimator;
        [SerializeField] private float transitionTime = 1f;
        [SerializeField] private float minTimeBetweenTransitions = 0.5f;

        private List<GameObject> activeEnemies = new List<GameObject>();
        private bool isTransitioning = false;
        private float lastTransitionTime;
        private bool hasPlayedInitialFadeIn = false;

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

        private void Start()
        {
            if (transitionAnimator == null)
            {
                transitionAnimator = GetComponent<Animator>();
                if (transitionAnimator == null)
                {
                    Debug.LogError("TransitionManager: No Animator component found!");
                    return;
                }
            }

            // Find all enemies in the scene
            FindAllEnemies();
            
            // Play fade in animation when level starts
            if (!hasPlayedInitialFadeIn)
            {
                StartCoroutine(InitialFadeIn());
            }
        }

        private IEnumerator InitialFadeIn()
        {
            hasPlayedInitialFadeIn = true;
            yield return StartCoroutine(FadeIn());
        }

        private void FindAllEnemies()
        {
            // Find all enemies with the EnemyAI component
            EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
            activeEnemies.Clear();
            foreach (EnemyAI enemy in enemies)
            {
                activeEnemies.Add(enemy.gameObject);
            }
        }

        public void RegisterEnemy(GameObject enemy)
        {
            if (!activeEnemies.Contains(enemy))
            {
                activeEnemies.Add(enemy);
            }
        }

        public void UnregisterEnemy(GameObject enemy)
        {
            if (activeEnemies.Contains(enemy))
            {
                activeEnemies.Remove(enemy);
                CheckLevelCompletion();
            }
        }

        private void CheckLevelCompletion()
        {
            if (activeEnemies.Count == 0 && !isTransitioning && CanTransition())
            {
                StartCoroutine(CompleteLevel());
            }
        }

        private bool CanTransition()
        {
            return Time.time - lastTransitionTime >= minTimeBetweenTransitions;
        }

        private IEnumerator CompleteLevel()
        {
            isTransitioning = true;
            lastTransitionTime = Time.time;
            
            // Play fade out animation
            yield return StartCoroutine(FadeOut());
            
            // Load next level
            LoadNextLevel();
        }

        public void PlayFadeIn()
        {
            if (transitionAnimator != null && CanTransition())
            {
                transitionAnimator.SetBool("Start", true);
                transitionAnimator.SetBool("Completed", false);
                lastTransitionTime = Time.time;
            }
        }

        public void PlayFadeOut()
        {
            if (transitionAnimator != null && CanTransition())
            {
                transitionAnimator.SetBool("Start", false);
                transitionAnimator.SetBool("Completed", true);
                lastTransitionTime = Time.time;
            }
        }

        public IEnumerator FadeOut()
        {
            if (CanTransition())
            {
                PlayFadeOut();
                yield return new WaitForSeconds(transitionTime);
            }
        }

        public IEnumerator FadeIn()
        {
            if (CanTransition())
            {
                PlayFadeIn();
                yield return new WaitForSeconds(transitionTime);
            }
        }

        public void LoadNextLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        // Call this when the level is completed
        public void OnLevelComplete()
        {
            if (!isTransitioning && CanTransition())
            {
                StartCoroutine(CompleteLevel());
            }
        }
    }
} 