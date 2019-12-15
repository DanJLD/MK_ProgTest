using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
	public float playerScore = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Restart"))
		{
			RestartGame();
		}
    }

	public void RestartGame()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name); // simple solution for now+
	}

	public void GainScore(float n)
	{
		playerScore += n;
	}
}
