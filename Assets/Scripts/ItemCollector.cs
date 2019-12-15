using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCollector : MonoBehaviour
{
	public LevelManager levelManager;

	public enum ITEM_TYPE
	{
		COIN_SML,
		COIN_MED,
		COIN_LRG,
	}
	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void GetItem(ITEM_TYPE type)
	{
		switch (type)
		{
			case ITEM_TYPE.COIN_SML:
				levelManager.GainScore(100f);
				levelManager.GainCoins(1);
				break;
			case ITEM_TYPE.COIN_MED:
				levelManager.GainScore(500f);
				levelManager.GainCoins(5);
				break;
			case ITEM_TYPE.COIN_LRG:
				levelManager.GainScore(2500f);
				levelManager.GainCoins(25);
				break;
		}
	}
}
