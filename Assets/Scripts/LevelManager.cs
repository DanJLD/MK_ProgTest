using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
	public TerrainManager terrainManager;
	public float playerScore = 0f;
	public int playerCoins= 0;
	public float playerDistance = 0f;

	public GameObject gameplayUI;
	public TMPro.TextMeshPro scoreText;
	public TMPro.TextMeshPro coinsText;

	public GameObject finalScoreBoard;
	public TMPro.TextMeshPro finalScoreText;
	public TMPro.TextMeshPro finalCoinText;
	public TMPro.TextMeshPro finalDistanceText;
	public GameObject finalRestartButton;

	private bool gameOver = false;

	// Start is called before the first frame update
	void Start()
    {
		if (finalScoreBoard != null)
			finalScoreBoard.SetActive(false);
		if (gameplayUI != null)
			gameplayUI.SetActive(true);
		if (finalRestartButton != null)
			finalRestartButton.SetActive(false);
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
		Time.timeScale = 1f;
		SceneManager.LoadScene(SceneManager.GetActiveScene().name); // simple solution for now+
	}

	public void GainScore(float n)
	{
		if (!gameOver)
			playerScore += n;
	}

	public void GainCoins(int n)
	{
		if (!gameOver)
			playerCoins += n;
	}

	public void EndGame()
	{
		// turn off gameplay UI elements
		if (gameplayUI != null)
			gameplayUI.SetActive(false);

		// display end scorecard + button to restart
		if (finalScoreBoard != null)
			finalScoreBoard.SetActive(true);
		if (finalScoreText != null)
			finalScoreText.text = Mathf.Floor(playerScore).ToString() + "pts";
		if (finalDistanceText != null)
			finalDistanceText.text = Mathf.Floor(playerDistance).ToString() + "m";
		if (finalCoinText != null)
			finalCoinText.text = playerCoins.ToString();

		Invoke("EndGameP2", 0.75f); // continue in x seconds

		// stop the game
		if (terrainManager != null)
			terrainManager.isRunning = false;

		gameOver = true;
	}

	private void EndGameP2()
	{
		if (finalRestartButton != null)
			finalRestartButton.SetActive(true); // delay restart button activation so that players dont accidentally restart instantly
	}
}
