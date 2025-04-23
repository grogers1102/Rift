using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; 
    public void Awake()
    {
        if(GameManager.instance != null){
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
