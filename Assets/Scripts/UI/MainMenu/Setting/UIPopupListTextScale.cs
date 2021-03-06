﻿using UnityEngine;
using System.Collections;

/// <summary>
/// UI popup list text scale.
/// 
/// This class handle auto adjust text scale for UIPopupList dependen on screen size
/// </summary>
public class UIPopupListTextScale : MonoBehaviour 
{
	//design height
	public int designHeight = 1024;

	void Awake()
	{
		//find UIPopupList
		UIPopupList pop = GetComponent<UIPopupList> ();

		//calcualte text scale
		float textScale = ((float)Screen.height * pop.textScale) / (float)designHeight;

		//set text scale
		pop.textScale = textScale;
	}

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
}
