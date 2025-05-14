using UnityEngine;

public class MenuTransition : MonoBehaviour
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)){
            UnityEngine.SceneManagement.SceneManager.LoadScene("Level1");
        }
    }
    public void StartLevel1(){
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level1");
    }
}
