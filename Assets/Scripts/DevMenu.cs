using UnityEngine;
using UnityEngine.SceneManagement;

public class DevMenu : MonoBehaviour
{
    public void OpenLearningLab()
    {
        SceneManager.LoadScene("LearningLab");
    }
}
