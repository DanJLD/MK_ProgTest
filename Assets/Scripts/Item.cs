using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
	public ItemCollector.ITEM_TYPE itemType;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.tag == "Player")
		{
			collision.GetComponent<ItemCollector>().GetItem(itemType);
			Destroy(this.gameObject);
		}
	}

	private void OnDestroy()
	{
		// spawn death particles / sounds / whatever else here
	}
}
