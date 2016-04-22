using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DiplomacyFree : MonoBehaviour {
	
	public static int numberNations = 2;
	//public List<int>[] relations = new List<int>[numberNations+1];
	int[,] relations = new int[numberNations, numberNations];
	
	// Use this for initialization
	void Start () {
	
		
  //      SetAllPeace();
	
	}
	
	// Update is called once per frame
	void Update () {
	
		
	
	}
	
	public void SetAllPeace(){
		for (int i = 0; i < numberNations; i++){
			for (int j = 0; j < numberNations; j++){
				if(i!=j){
					relations[i,j] = 0;
				}
			}
		}
		print("peace");
	}
	
	public void SetAllWar(){
		for (int i = 0; i < numberNations; i++){
			for (int j = 0; j < numberNations; j++){
				if(i!=j){
					relations[i,j] = 1;
				}
			}
		}
		print("war");
	}
}
