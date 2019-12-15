using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
	public float playerScore = 0f;
	public int playerCoins= 0;

	public TMPro.TextMeshPro scoreText;
	public TMPro.TextMeshPro coinsText;
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

		//updateUI
		scoreText.text = Mathf.Floor(playerScore).ToString();
		coinsText.text = playerCoins.ToString();
	}

	public void RestartGame()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name); // simple solution for now+
	}

	public void GainScore(float n)
	{
		playerScore += n;
	}

	public void GainCoins(int n)
	{
		playerCoins += n;
	}
}
