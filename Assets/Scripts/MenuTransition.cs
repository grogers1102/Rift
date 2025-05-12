using UnityEngine;

public class MenuTransition : MonoBehaviour
{
    public void StartLevel1(){
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level1");
    }
}
