﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Soomla.Store;

/// <summary>
/// UI character item.
/// 
/// This class is for character item
/// </summary>
public class UICharacterItem : UIVirtualGood 
{
	/// <summary>
	/// Localization key
	/// This should match key in localization file
	/// </summary>
	public string selectKey = "Select";
	public string selectedKey = "Selected";
	public string buyKey = "Buy";
	public string purchasedKey = "Purchased";

	/// <summary>
	/// Reference to UI price label.
	/// </summary>
	public UILabel priceLabel;
	
	/// <summary>
	/// Reference to UI protrait sprite 
	/// </summary>
	public UISprite portrait;

	/// <summary>
	/// The protrait border.
	/// </summary>
	public UISprite protraitBorder;

	/// <summary>
	/// The button.
	/// </summary>
	public GameObject buyButton;

	protected override void Awake()
	{
		base.Awake ();


	}
	
	protected override void Start()
	{
		base.Start ();
	}
	
	protected override void InitVirtualGood()
	{
		base.InitVirtualGood ();
		
		//set price label
		priceLabel.text = ((int)price).ToString();
		
		//set image
		portrait.spriteName = portraitImageName;

		//check if character is equipped
		if(StoreInventory.IsVirtualGoodEquipped(virtualGoodId))
		{
			//turn on protrait border
			protraitBorder.alpha = 1f;
		}
		else
		{
			//turn off protrait border
			protraitBorder.alpha = 0f;
		}

		ConfigureButton ();
		ConfigurePriceLabel ();

	}
	
	public override void StartPurchase()
	{
		base.StartPurchase ();

		//if(!StoreInventory.NonConsumableItemExists(virtualGoodId))
		if(StoreInventory.GetItemBalance(virtualGoodId) <= 0)
		{
			//show purchase control window
			uiStoreRoot.purchaseControl.ShowPurchaseWindow (this);
		}

		
	}

	protected override void PurchaseWindowItemPurchased(UIPurchaseControl control, string itemId)
	{
		if(itemId != virtualGoodId)
		{
			return;
		}

		base.PurchaseWindowItemPurchased (control, itemId);
		
		
		ConfigureButton ();
		ConfigurePriceLabel ();

	}

	void ConfigureButton()
	{

		if(gameObject.activeInHierarchy)
		{
			//if character has been bought
			if(StoreInventory.GetItemBalance(virtualGoodId) > 0)
			{
				
				//if character is selected
				if(StoreInventory.IsVirtualGoodEquipped(virtualGoodId))
				{
					//disable button
					buyButton.GetComponent<UIImageButton>().isEnabled = false;
					
					//change button function name to non
					UIButtonMessage btnMsg = buyButton.GetComponent<UIButtonMessage>();
					btnMsg.functionName = "";
					
					//change localize key to selected
					buyButton.GetComponentInChildren<UILocalize>().key = selectedKey;
					
					//change label to selected
					buyButton.GetComponentInChildren<UILabel>().text = Localization.Localize(selectedKey);
				}
				else
				{
					//enable button
					buyButton.GetComponent<UIImageButton>().isEnabled = true;
					
					//change button function name to Select
					UIButtonMessage btnMsg = buyButton.GetComponent<UIButtonMessage>();
					btnMsg.functionName = "Select";
					
					//change localize key to select
					buyButton.GetComponentInChildren<UILocalize>().key = selectKey;
					
					//change label to select
					buyButton.GetComponentInChildren<UILabel>().text = Localization.Localize(selectKey);
				}
				
				
			}
			else
			{
				//change button function name to StartPurchase
				UIButtonMessage btnMsg = buyButton.GetComponent<UIButtonMessage>();
				btnMsg.functionName = "StartPurchase";
				
				//change localize key to select
				buyButton.GetComponentInChildren<UILocalize>().key = buyKey;
				
				//change label to select
				buyButton.GetComponentInChildren<UILabel>().text = Localization.Localize(buyKey);
			}
		}

	}

	void ConfigurePriceLabel()
	{
		if(gameObject.activeInHierarchy)
		{
			//if character has been bought...change price label to purchased
			if(StoreInventory.GetItemBalance(virtualGoodId) > 0)
			{
				priceLabel.text = Localization.Localize(purchasedKey);
			}
			else
			{
				//set price
				priceLabel.text = ((int)price).ToString();
			}
		}

	}

	/// <summary>
	/// Select this character
	/// </summary>
	public void Select()
	{
		List<UICharacterItem> items = GetItems<UICharacterItem> ();

		//Deselect all characters
		for(int i=0; i<items.Count; i++)
		{
			if(items[i].virtualGoodId != virtualGoodId)
			{
				items[i].Deselect();
			}

		}

		//select character
		uiStoreRoot.SelectCharacter (virtualGoodId);


		//turn on protrait border
		protraitBorder.alpha = 1f;

		ConfigureButton ();
	}

	/// <summary>
	/// Deselect this character.
	/// </summary>
	public void Deselect()
	{

		//deselect character
		uiStoreRoot.DeselectCharacter (virtualGoodId);

		protraitBorder.alpha = 0f;

		ConfigureButton ();
	}
}
