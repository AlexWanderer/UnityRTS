using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleSystemFree : MonoBehaviour {

// BSystem is core component for simulating RTS battles
// It has 6 phases for attack and gets all different game objects parameters inside.
// Attack phases are: Search, Approach target, Attack, Self-Heal, Die, Rot (Sink to ground).
// All 6 phases are running all the time and checking if object is matching criteria, then performing actions
// Movements between different phases are also described

	
	public float attackDistance = 70.0f;

	
	
	private float[] timeloops = new float[7];
	private float[] timeall = new float[7];
	
	private float[] performance = new float[7];

	private string message = " ";
	
	private string message1 = " ";
	private string message2 = " ";
	private string message3 = " ";
	private string message4 = " ";
	private string message5 = " ";
	private string message6 = " ";
	private bool displayMessage = false;


	
	private float dist;
	
	public KDTreeFree Tree = new KDTreeFree();
	
	public KDTreeFree Tree1 = new KDTreeFree();
	public KDTreeFree Tree2 = new KDTreeFree();
	
	public KDTreeFree FTree1 = new KDTreeFree();
	public KDTreeFree FTree2 = new KDTreeFree();
	

	
	
	public int countr1 = 0;
	public int countr2 = 0;
	
	public int countrf1 = 0;
	public int countrf2 = 0;
	
	
	
	public static int maxIndApproachers = 5;
	public static int maxIndAttackers = 5;
	

	




    public List<GameObject>[] runits = new List<GameObject>[10];
    public List<GameObject>[] rfunits = new List<GameObject>[10];
    
    public List<GameObject>[] cprunits = new List<GameObject>[10];
    public List<GameObject>[] cprfunits = new List<GameObject>[10];
	

	public List<GameObject> unitss = new List<GameObject>();
	

	public List<GameObject> unitsBuffer = new List<GameObject>();


    public List<GameObject> approachers = new List<GameObject>();

	public List<GameObject> sinks = new List<GameObject>();
	
	
	
	
	
	


	
	

	
	
	
	
	// Use this for initialization
	void Start () {
	
// allocating search arrays: runits - for searchable objects, rfunits - for fighters, which are searching	    
		
		for (int i = 1; i < runits.Length; i++)
        {
            runits[i] = new List<GameObject>();
        }
        
        for (int i = 1; i < rfunits.Length; i++)
        {
            rfunits[i] = new List<GameObject>();
        }
        
        
// allocating copies to prevent from dynamical changes   
        
        for (int i = 1; i < cprunits.Length; i++)
        {
            cprunits[i] = new List<GameObject>();
        }
        
        for (int i = 1; i < cprfunits.Length; i++)
        {
            cprfunits[i] = new List<GameObject>();
        }
        
        
        
 
		// starting spawner
		
		GameObject objSP1 =  GameObject.Find("spawnpointBTetra");
		GameObject objSP2 =  GameObject.Find("spawnpointBCube");
		
		objSP1.GetComponent<SpawnPointFree>().enabled = true;
		objSP2.GetComponent<SpawnPointFree>().enabled = true;
		
// starting to add units to main unitss array		
		StartCoroutine(AddBuffer());
		StartCoroutine(BoolChecker());
		
// Starts all 6 coroutines to start searching for possible units in unitss array.
		StartCoroutine(SearchPhase());
		StartCoroutine(ApproachTargetPhase());
		StartCoroutine(AttackPhase());
		StartCoroutine(SelfHealingPhase());
		StartCoroutine(DeathPhase());
		StartCoroutine(SinkPhase());
		
		StartCoroutine(ManualMover());
		

	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI (){
	
	// Display performance
		if ( displayMessage )
    	{
    		GUI.Label(new Rect(Screen.width * 0.05f, Screen.height * 0.05f, 500f, 20f), message);
    		
        	GUI.Label(new Rect(Screen.width * 0.05f, Screen.height * 0.2f, 500f, 20f), message1);
        	GUI.Label(new Rect(Screen.width * 0.05f, Screen.height * 0.3f, 500f, 20f), message2);
        	GUI.Label(new Rect(Screen.width * 0.05f, Screen.height * 0.4f, 500f, 20f), message3);
        	GUI.Label(new Rect(Screen.width * 0.05f, Screen.height * 0.5f, 500f, 20f), message4);
        	GUI.Label(new Rect(Screen.width * 0.05f, Screen.height * 0.6f, 500f, 20f), message5);
        	GUI.Label(new Rect(Screen.width * 0.05f, Screen.height * 0.7f, 500f, 20f), message6);
    	}
	}
	

	
	public IEnumerator SearchPhase()
	{
	
// The main coroutine, which starts to search for nearest enemies neighbours and set them for attack
// NN search works with kdtree.cs NN search class, implemented by A. Stark at 2009.
// Target candidates are put on kdtree, while attackers used to search for them.
// NN searches are based on position coordinates in 3D.

	float t1 = 0.0f;
	float t2 = 0.0f;
	float twaiter = 0.0f;
	float t3 = Time.realtimeSinceStartup;


	
	

	int indmindisti = 0;
	

	

	
  	yield return new WaitForSeconds(1.0f);
  		
  		while(true){
  		
  		t1 = Time.realtimeSinceStartup;
  		twaiter = 0;
  		

		
		
		
		KDTreeFree RTree1 = new KDTreeFree();
	    KDTreeFree RTree2 = new KDTreeFree();
	

		
// adding back units which becomes attackable (if they get less attackers than defined by critical number)

		for(int i = 0; i<unitss.Count; i++){
			GameObject go = unitss[i];
			if(go.GetComponent<UnitParsFree>().isReady){
				int alliance = go.GetComponent<UnitParsFree>().alliance;
			        if(go.GetComponent<UnitParsFree>().isAttackable == true){
			        	if(go.GetComponent<UnitParsFree>().onTargetSearch == false){
							runits[alliance].Add(go);
							go.GetComponent<UnitParsFree>().onTargetSearch = true;
						}
					}
			
        			rfunits[alliance].Add(go);
        	
        	        go.GetComponent<UnitParsFree>().isReady = false;
			}
		}
		

		
		
		
		
		
		
		

		
		
		
// resetting copies arrays.	
		
		cprunits[1].Clear();
		cprunits[2].Clear();
		
		cprfunits[1].Clear();
		cprfunits[2].Clear();
		
		
		for(int i =1; i<runits[1].Count; i++){
		
			cprunits[1].Add(runits[1][i]);
		
		}
		for(int i =0; i<runits[2].Count; i++){
		
			cprunits[2].Add(runits[2][i]);
		
		}
		
		for(int i =1; i<rfunits[1].Count; i++){
		
			cprfunits[1].Add(rfunits[1][i]);
		
		}
		for(int i =1; i<rfunits[2].Count; i++){
		
			cprfunits[2].Add(rfunits[2][i]);
		
		}
		
// finding counters for copies arrays		
		
		countr1 = cprunits[1].Count;
		countr2 = cprunits[2].Count;
		
		countrf1 = cprfunits[1].Count;
		countrf2 = cprfunits[2].Count;
		
		
	
		
		
	// initialising vector arrays	
		
		Vector3[] rtreePoints1 = new Vector3[countr1];
		Vector3[] rtreePoints2 = new Vector3[countr2];
		
		int [] iorig1 = new int[countr1];
		int [] iorig2 = new int[countr2];
		
		
		Vector3[] rftreePoints1 = new Vector3[countrf1];
		Vector3[] rftreePoints2 = new Vector3[countrf2];
		
		int [] iorigf1 = new int[countrf1];
		int [] iorigf2 = new int[countrf2];

		
// putting target candidates coordinates onto arrays and setting non attackable targets not to be used on search
		
	//	rtreePoints1[0] = new Vector3 (-999999999999.99f,-999999999999.99f,-999999999999.99f);
	//	iorig1[0]=0;
	//	rtreePoints2[0] = new Vector3 (-999999999999.99f,-999999999999.99f,-999999999999.99f);
	//	iorig2[0]=0;
		
		
		for(int i =1; i<countr1; i++){
		
			rtreePoints1[i]=cprunits[1][i].transform.position;
			iorig1[i]=i;
			if(cprunits[1][i].GetComponent<UnitParsFree>().isAttackable == false){
				if(cprunits[1][i].GetComponent<UnitParsFree>().onTargetSearch == true){
					cprunits[1][i].GetComponent<UnitParsFree>().onTargetSearch = false;
					runits[1].Remove(cprunits[1][i]);
				}
			}
		//	
		
		
		}
		for(int i =1; i<countr2; i++){
		
			rtreePoints2[i]=cprunits[2][i].transform.position;
			iorig2[i]=i;
			if(cprunits[2][i].GetComponent<UnitParsFree>().isAttackable == false){
				if(cprunits[2][i].GetComponent<UnitParsFree>().onTargetSearch == true){
					cprunits[2][i].GetComponent<UnitParsFree>().onTargetSearch = false;
					runits[2].Remove(cprunits[2][i]);
				}
			}
		
		}
		
// putting attackers candidates coordinates onto arrays and removing them from original arrays,
// as they all will get targets

		for(int i =1; i<countrf1; i++){
		
			rftreePoints1[i]=cprfunits[1][i].transform.position;
			iorigf1[i]=i;
			rfunits[1].Remove(cprfunits[1][i]);
		
		}
		for(int i =1; i<countrf2; i++){
		
			rftreePoints2[i]=cprfunits[2][i].transform.position;
			iorigf2[i]=i;
			rfunits[2].Remove(cprfunits[2][i]);
		
		}
		
// sorting vector arrays for faster searches in kdtree
		
//		HeapSort(rtreePoints1, iorig1);
//		HeapSort(rtreePoints2, iorig2);
		
//		HeapSort(rftreePoints1, iorigf1);
//		HeapSort(rftreePoints2, iorigf2);
		
// putting target candidates onto kdtree



		RTree1 = KDTreeFree.MakeFromPoints(rtreePoints1);
		RTree2 = KDTreeFree.MakeFromPoints(rtreePoints2);
		

		
		GameObject tempObj1;
		GameObject tempObj2;
		
// first team attackers searching and setting their targets

		for(int i=1;i<countrf1;i++){
		
				indmindisti = RTree2.FindNearest(rftreePoints1[i]);
				
				int io = iorigf1[i];
				int iomind = iorig2[indmindisti];
				
				tempObj1 = cprfunits[1][io];
				tempObj2 = cprunits[2][iomind];
				
				
				tempObj1.GetComponent<UnitParsFree>().target = tempObj2;
				
			// adding attacker to target attackers list	
				tempObj2.GetComponent<UnitParsFree>().attackers.Add(tempObj1);
				tempObj2.GetComponent<UnitParsFree>().noAttackers = tempObj2.GetComponent<UnitParsFree>().noAttackers + 1;
				
				tempObj1.GetComponent<UnitParsFree>().isApproaching = true;
				

    		
			

				
				if(Time.realtimeSinceStartup-t3>0.005f){
					twaiter = twaiter + 0.1f*(Time.realtimeSinceStartup-t3)+0.05f;
					yield return new WaitForSeconds(0.1f*(Time.realtimeSinceStartup-t3)+0.05f);
					t3=Time.realtimeSinceStartup;
				}
		
		}

		
		
		
		

		

		
		
	
// second team attackers searching and setting their targets	
		
		for(int i=1;i<countrf2;i++){
		
				indmindisti = RTree1.FindNearest(rftreePoints2[i]);
				
				int io = iorigf2[i];
				int iomind = iorig1[indmindisti];
				
				tempObj1 = cprfunits[2][io];
				tempObj2 = cprunits[1][iomind];
    		
    		
    			tempObj1.GetComponent<UnitParsFree>().target = tempObj2;
    			
    		// adding attacker to target attackers list		
    			tempObj2.GetComponent<UnitParsFree>().attackers.Add(tempObj1);
    			tempObj2.GetComponent<UnitParsFree>().noAttackers = tempObj2.GetComponent<UnitParsFree>().noAttackers + 1;
    			
				tempObj1.GetComponent<UnitParsFree>().isApproaching = true;
				

				
				if(Time.realtimeSinceStartup-t3>0.005f){
				    twaiter = twaiter + 0.1f*(Time.realtimeSinceStartup-t3)+0.05f;
					yield return new WaitForSeconds(0.1f*(Time.realtimeSinceStartup-t3)+0.05f);
					t3=Time.realtimeSinceStartup;
				}
		
		}

		

		

		
		
		t2= Time.realtimeSinceStartup;
	
		
		displayMessage = true;
		
		
		

		
		
  		
  		
  		
  		
		
		
// main coroutine wait statement and performance information collection from search coroutine		

	twaiter = twaiter + 0.5f+1.0f*(t2-t1);
	yield return new WaitForSeconds(0.5f+1.0f*(t2-t1));	
	
	float curTime = Time.realtimeSinceStartup;
	
	timeloops[1] = curTime - t1 - twaiter;
	timeall[1] = curTime - t1;
	performance[1] = (curTime - t1 - twaiter)*100.0f/(curTime - t1);
	
	message1 = ("Search: " + (countr1+countr2).ToString() + "; " + countrf1 + "; " + countrf2 + "; " + (curTime - t1 - twaiter).ToString() + "; " + (performance[1]).ToString() + "% ");

	}
	
	}
	
	
	
	
	
	
	public IEnumerator ApproachTargetPhase()
	{

// this phase starting attackers to move towards their targets
	    
	    float timeBeg;
	    float timeEnd;
	    float t3;
	    float twaiter;
	    
	    float ax;
	    float ay;
	    float az;
	    
	    float tx;
	    float ty;
	    float tz;
	    
	    float Rtarget;
	    
	    float stopDist = 2.0f;
	    float stoppDistance;
	    

	    
	    
	    
		while(true){
		    timeBeg = Time.realtimeSinceStartup;
		    
		    
		    int noApproachers = 0;
		    int ii = 0;
		    
		
		    
		    t3 = Time.realtimeSinceStartup;
		    twaiter = 0.0f;
		    
// checking through main unitss array which units are set to approach (isApproaching)

		    for(int i = 0; i<unitss.Count; i++){
		 		GameObject appr = unitss[i];
		 		UnitParsFree apprPars = appr.GetComponent<UnitParsFree>();
		 		ii = ii + 1;
		 		
		    	if(apprPars.isApproaching){
		    	
		    	
					GameObject targ = apprPars.target;
					
					NavMeshAgent apprNav = appr.GetComponent<NavMeshAgent>();
					NavMeshAgent targNav = targ.GetComponent<NavMeshAgent>();
					
					if(targ.GetComponent<UnitParsFree>().isApproachable == true){
		    	
		    			ax = appr.transform.position.x;
						ay = appr.transform.position.y;
						az = appr.transform.position.z;
				
						tx = targ.transform.position.x;
						ty = targ.transform.position.y;
						tz = targ.transform.position.z;
					
						
					    // stopping condition for NavMesh
					    
						apprNav.stoppingDistance = apprNav.radius/(appr.transform.localScale.x) + targNav.radius/(targ.transform.localScale.x);
				
				      // distance between approacher and target
				      
						Rtarget = Mathf.Sqrt((tx-ax)*(tx-ax)+(ty-ay)*(ty-ay)+(tz-az)*(tz-az));
						
					 
					 
						stoppDistance = (stopDist + appr.transform.localScale.x*targ.transform.localScale.x*apprNav.stoppingDistance);
					
					// counting increased distances (failure to approach) between attacker and target;
					// if counter failedR becomes bigger than critFailedR, preparing for new target search.
					
					    if(apprPars.prevR < Rtarget){
					    	apprPars.failedR = apprPars.failedR + 1;
					    	if(apprPars.failedR > apprPars.critFailedR){
					    		apprPars.isApproaching = false;
								apprPars.isReady = true;
								apprPars.failedR = 0;
								
								if(apprPars.changeMaterial){
									appr.GetComponent<Renderer>().material.color = Color.yellow;				
								}
								
					    	}
					    }
					    
					    else{
					    // if approachers already close to their targets
							if(Rtarget < stoppDistance){
								apprNav.SetDestination(appr.transform.position);
							
						// pre-setting for attacking
							
								apprPars.isApproaching = false;
								apprPars.isAttacking = true;
							
							}
							else{
						
				
								if(apprPars.changeMaterial){
									appr.GetComponent<Renderer>().material.color = Color.green;				
								}
				
			
			            // starting to move
			            
								if(apprPars.isMovable){
					        		noApproachers = noApproachers+1;
									apprNav.SetDestination(targ.transform.position);
									apprNav.speed = 3.5f;
								}
							
						// performance waiter for long approacher lists	
								if(ii > 1500){
									if(Time.realtimeSinceStartup-t3>0.005f){
								    	twaiter = twaiter + 0.1f*(Time.realtimeSinceStartup-t3)+0.05f;
										yield return new WaitForSeconds(0.1f*(Time.realtimeSinceStartup-t3)+0.05f);
										t3=Time.realtimeSinceStartup;
										ii = 0;
									}
								}
							}
						}
					// saving previous R
						apprPars.prevR = Rtarget;
					}
					
				// condition for non approachable targets	
					else{
					    apprPars.target = null;
						apprNav.SetDestination(appr.transform.position);
						
					
						
						apprPars.isApproaching = false;
						apprPars.isReady = true;
						
						if(apprPars.changeMaterial){
								appr.GetComponent<Renderer>().material.color = Color.yellow;				
						}
					}
		    	}
		    	
		    	
		   
		    	
		    }
		    
		    
		    
		    
		    
		    
		    
		    
		    
		    
		    
	
// main coroutine wait statement and performance information collection from approach coroutine			
		
			timeEnd = Time.realtimeSinceStartup;
			
			
			
			
			twaiter = twaiter + 1.0f*(timeEnd-timeBeg)+1.0f;
			yield return new WaitForSeconds(1.0f*(timeEnd-timeBeg)+1.0f);
			
			float curTime = Time.realtimeSinceStartup;
	
			timeloops[2] = curTime - timeBeg - twaiter;
			timeall[2] = curTime - timeBeg;
			
			performance[2] = (curTime-timeBeg-twaiter)*100.0f/(curTime-timeBeg);
			
			message2 = ("Approacher: " + (noApproachers).ToString() + "; " + (curTime-timeBeg-twaiter).ToString() + "; " + (performance[2]).ToString() + "% ");
		}
	
	}
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	public IEnumerator AttackPhase() {
    
    // Attacking phase set attackers to attack their targets and cause damage when they already approached their targets
    
    float timeBeg;
	float timeEnd;
	

	float twaiter;
	
	float ax;
	float ay;
	float az;
	    
	float tx;
	float ty;
	float tz;
	    
	float Rtarget;
	    
	float stopDist = 2.5f;
	float stoppDistance;
    

    
    float tempRand = 0.0f;
    
    float tempStrength = 1.0f;
    float tempDefence = 1.0f;
    
	while(true){
		timeBeg = Time.realtimeSinceStartup;
	    
        twaiter = 0.0f;
        
        
        
        int noAttackers = 0;
		    

		    
	// checking through main unitss array which units are set to approach (isAttacking)
        
        for(int i = 0; i<unitss.Count; i++){
        	GameObject att = unitss[i];
        	UnitParsFree attPars = att.GetComponent<UnitParsFree>();
        	
        	if(attPars.isAttacking){
        	
        			GameObject targ = attPars.target;
        			UnitParsFree targPars = targ.GetComponent<UnitParsFree>();
		    	
		    		ax = att.transform.position.x;
					ay = att.transform.position.y;
					az = att.transform.position.z;
				
					tx = targ.transform.position.x;
					ty = targ.transform.position.y;
					tz = targ.transform.position.z;
					
					NavMeshAgent attNav = att.GetComponent<NavMeshAgent>();
					NavMeshAgent targNav = targ.GetComponent<NavMeshAgent>();
					
					attNav.stoppingDistance = attNav.radius/(att.transform.localScale.x) + targNav.radius/(targ.transform.localScale.x);
				
				// distance between attacker and target
				
					Rtarget = Mathf.Sqrt((tx-ax)*(tx-ax)+(ty-ay)*(ty-ay)+(tz-az)*(tz-az));
					stoppDistance = (stopDist + att.transform.localScale.x*targ.transform.localScale.x*attNav.stoppingDistance);
					
					
				// if target moves away, resetting back to approach target phase
					
					if(Rtarget > stoppDistance){
						
					
						attPars.isApproaching = true;
						attPars.isAttacking = false;
					}
					
				// if targets becomes immune, attacker is reset to start searching for new target 	
					else if(targPars.isImmune == true){
					
							attPars.isAttacking = false;
							attPars.isReady = true;
							
							
							targPars.attackers.Remove(att);
							targPars.noAttackers = targPars.noAttackers - 1;
							
							if(attPars.changeMaterial){
								att.GetComponent<Renderer>().material.color = Color.yellow;				
							}
					}
				
				// attacker starts attacking their target	
					else{
						noAttackers = noAttackers + 1;
				
						if(attPars.changeMaterial){
							att.GetComponent<Renderer>().material.color = Color.red;				
						}
						tempRand = Random.value;
						
						tempStrength = attPars.strength;
						tempDefence = attPars.defence;
						
					// if attack passes target through target defence, cause damage to target
						
						if(tempRand > (tempStrength/(tempStrength+tempDefence))){
			            	targPars.health = targPars.health - 2.0f*tempStrength*Random.value;
						}
						
					}
        
        	}
        
        }
        
        
        






	    
	// main coroutine wait statement and performance information collection from attack coroutine	
		
		twaiter = twaiter + 1.5f;
		
		yield return new WaitForSeconds(1.5f);
		
		timeEnd = Time.realtimeSinceStartup;
		
	
		timeloops[3] = timeEnd - timeBeg - twaiter;
		timeall[3] = timeEnd - timeBeg;
		
		performance[3] = (timeEnd-timeBeg - twaiter)*100.0f/(timeEnd-timeBeg);
		
		message3 = ("Attacker: " + (noAttackers).ToString() + "; " + (timeEnd-timeBeg - twaiter).ToString() + "; " + (performance[3]).ToString() + "% ");
	}
    
    
    }


	
	
	public IEnumerator SelfHealingPhase() {
	
	// Self-Healing phase heals damaged units over time
	
	float timeBeg;
	float timeEnd;
	
	float twaiter;
	
	while(true){
		timeBeg = Time.realtimeSinceStartup;
		
		int noSelfHealers = 0;
		twaiter = 0.0f;
		
		
	// checking which units are damaged	
		
		for(int i = 0; i<unitss.Count; i++){
		
			GameObject sheal = unitss[i];
			UnitParsFree shealPars = sheal.GetComponent<UnitParsFree>();
			
			if(shealPars.health < shealPars.maxHealth){
			
			// if unit has less health than 0, preparing it to die
			
			    if(shealPars.health < 0.0f){
					shealPars.isHealing = false;
					shealPars.isImmune = true;
					shealPars.isDying = true;
				
				}
				
			// healing unit	
				else{
					shealPars.isHealing = true;
					shealPars.health = shealPars.health + shealPars.selfHealFactor;
				    noSelfHealers = noSelfHealers + 1;
				    
				 // if unit health reaches maximum, unset self-healing
				    
					if(shealPars.health >= shealPars.maxHealth){
						shealPars.health = shealPars.maxHealth;
						shealPars.isHealing = false;
						noSelfHealers = noSelfHealers - 1;
					}
				}
				
			}
			
		}
		
// main coroutine wait statement and performance information collection from self-healing coroutine
		
		twaiter = twaiter + 3.0f;

		yield return new WaitForSeconds(3.0f);
		
		timeEnd = Time.realtimeSinceStartup;
		
		timeloops[4] = timeEnd - timeBeg - twaiter;
		timeall[4] = timeEnd - timeBeg;
		
		performance[4] = (timeEnd-timeBeg-twaiter)*100.0f/(timeEnd-timeBeg);
		
		message4 = ("SelfHealing: " + (noSelfHealers).ToString() + "; " + (timeEnd-timeBeg-twaiter).ToString() + "; "+ (performance[4]).ToString() + "% ");
		
		
	}
	
	
	}
	
	public IEnumerator DeathPhase(){
	
// Death phase unset all unit activity and prepare to die
	
	float timeBeg;
	float timeEnd;
	
	float twaiter;
	
	
		while(true){
		    timeBeg = Time.realtimeSinceStartup;
		    
			int noDeads = 0;
			twaiter = 0.0f;
			
	// Getting dying units		
		
			for(int i = 0; i<unitss.Count; i++){
			
				GameObject dead = unitss[i];
				UnitParsFree deadPars = dead.GetComponent<UnitParsFree>();
				
				if(deadPars.isDying == true){
					    
					// If unit is dead long enough, prepare for rotting (sinking) phase and removing from the unitss list
					    
					    if(deadPars.deathCalls > deadPars.maxDeathCalls){
					
							deadPars.isDying = false;
							deadPars.isSinking = true;
						
							dead.GetComponent<NavMeshAgent>().enabled = false;
							sinks.Add(dead);
							unitss.Remove(dead);
							int alliance = deadPars.alliance;
							runits[alliance].Remove(dead);
							rfunits[alliance].Remove(dead);
							
							
							
							
						}
						
					// unsetting unit activity and keep it dying	
						else{
							deadPars.isMovable = false;
							deadPars.isReady = false;
							deadPars.isApproaching = false;
							deadPars.isAttacking = false;
							deadPars.isApproachable = false;
							deadPars.isAttackable = false;
							deadPars.isHealing = false;
							deadPars.target = null;
						
						// unselecting deads	
							dead.SendMessage("OnUnselected", SendMessageOptions.DontRequireReceiver);
							dead.transform.gameObject.tag = "Untagged";
							
						// unsetting attackers	
					/*		GameObject attTemp;
							for(int j=0;j<deadPars.attackers.Count;j++){
							
								attTemp = deadPars.attackers[j];
								UnitParsFree attTempPars = attTemp.GetComponent<UnitParsFree>();
								
								if((attTempPars.isDying == false)&&(attTempPars.isSinking == false)){
									attTemp.GetComponent<NavMeshAgent>().SetDestination(attTemp.transform.position);
									attTempPars.isReady = true;
									attTempPars.isApproaching = false;
									attTempPars.isAttacking = false;
								}
							}
							deadPars.attackers.Clear();
					*/		
							dead.GetComponent<NavMeshAgent>().SetDestination(dead.transform.position);
							deadPars.deathCalls = deadPars.deathCalls + 1;
						
							if(deadPars.changeMaterial){
								dead.GetComponent<Renderer>().material.color = Color.blue;				
							}
					    	noDeads = noDeads + 1;
					    }
				}
			}
			
// main coroutine wait statement and performance information collection from death coroutine			

			twaiter = twaiter + 1.0f;
			yield return new WaitForSeconds(1.0f);
			
			timeEnd = Time.realtimeSinceStartup;
			
			timeloops[5] = timeEnd - timeBeg - twaiter;
			timeall[5] = timeEnd - timeBeg;
			
			performance[5] = (timeEnd-timeBeg-twaiter)*100.0f/(timeEnd-timeBeg);
			
			message5 = ("Dead: " + (noDeads).ToString() + "; " + (timeEnd-timeBeg-twaiter).ToString() + "; " + (performance[5]).ToString() + "; ");
			
			
		}
	}
	
	
	
	public IEnumerator SinkPhase(){
	
// rotting or sink phase includes time before unit is destroyed: for example to perform rotting animation or sink object into the ground
	
		float timeBeg;
		float timeEnd;
		
		float twaiter;
		
		while(true){
		    
			timeBeg = Time.realtimeSinceStartup;
			twaiter = 0.0f;
			
	// checking in sinks array, which is already different from main units array
			
			for(int i = 0; i<sinks.Count; i++){
			
				GameObject sink = sinks[i];
				UnitParsFree sinkPars = sink.GetComponent<UnitParsFree>();
			
				if(sinkPars.isSinking == true){
				
				
					if(sinkPars.changeMaterial){
						sink.GetComponent<Renderer>().material.color = new Color((148.0f/255.0f),(0.0f/255.0f),(211.0f/255.0f),1.0f);				
					}
					
				// moving sinking object down into the ground	
					if(sink.transform.position.y>-1.0f){
			
						sink.transform.Translate(0,-0.1f,0,Space.World);
		
					}
					
				// destroy object if it has sinked enough	
					else if(sink.transform.position.y<-1.0f){
						sinks.Remove(sink);
						Destroy(sink.gameObject);
					}
				
				
				}
			}
			
// main coroutine wait statement and performance information collection from sink coroutine			
			twaiter = twaiter + 1.0f;
			yield return new WaitForSeconds(1.0f);
			
			timeEnd = Time.realtimeSinceStartup;
			
			timeloops[6] = timeEnd - timeBeg - twaiter;
			timeall[6] = timeEnd - timeBeg;
			
			performance[6] = (timeEnd-timeBeg-twaiter)*100.0f/(timeEnd-timeBeg);
		
			message6 = ("Sink: " + (sinks.Count).ToString() + "; " + (timeEnd-timeBeg-twaiter).ToString() + "; " + (performance[6]).ToString() + "% ");
		
			
		}
	}
	
	
	
	
	
	public IEnumerator BoolChecker(){
		
	// additional conditions check to set bool values	
		
		while(true){
		    int maxatt = 0;
			
			for(int i = 0; i<unitss.Count; i++){
				GameObject obj = unitss[i];
				UnitParsFree objPars = obj.GetComponent<UnitParsFree>();
			// Atackable modifications
			    if(objPars.noAttackers > maxatt){
			    	maxatt = objPars.noAttackers;
			    }
			
				if((objPars.isAttackable == true)&&
				   (objPars.noAttackers > objPars.maxAttackers)){
					objPars.isAttackable = false;
				}
				else if((objPars.isAttackable == false)&&
				   (objPars.noAttackers < objPars.maxAttackers)){
				    objPars.isAttackable = true;
				}
				
				
				
				
			}
			
		// total performance calculation from Battle System
		
			timeloops[0] = timeloops[1] + timeloops[2] + timeloops[3] + timeloops[4] + timeloops[5] + timeloops[6];
			timeall[0] = timeall[1] + timeall[2] + timeall[3] + timeall[4] + timeall[5] + timeall[6];
			
		
			                 
			performance[0] = (performance[1] +
			                  performance[2] +
			                  performance[3] +
			                  performance[4] +
			                  performance[5] +
			                  performance[6])/6.0f;
			                  
			
			
			
			
		
			message = ("BSystem: " + (unitss.Count).ToString() + "; "
			               //      + (timeEnd-timeBeg-twaiter).ToString() + "; " 
			                     + (timeloops[0]).ToString() + "; "
			                     + (timeall[0]).ToString() + "; "
			                     + (performance[0]).ToString() + "% ");
			
			yield return new WaitForSeconds(0.5f);
		}
	}
	
	
	public IEnumerator AddBuffer()
	
// adding new units from buffer to BSystem : 
// units, which are wanted to be used on BSystem should be placed to unitsBuffer array first	
	
	{
		while(true){
		
			int maxbuffer = unitsBuffer.Count;
			for(int i =0; i<maxbuffer; i++){
				unitss.Add(unitsBuffer[i]);
				
			}
			
		// cleaning up buffer
			
			for(int i =0; i<unitss.Count; i++){
				unitsBuffer.Remove(unitss[i]);
				
			
			}
			yield return new WaitForSeconds(0.5f);
		}
	}
	
	
// ManualMover controls unit if it is selected and target is defined by player
	
	public IEnumerator ManualMover()
	{
	float r;
	
	float ax;
	float ay;
	float az;
	
	float tx;
	float ty;
	float tz;
	
		while(true){
			
			for(int i =0; i<unitss.Count; i++){
			
				GameObject obj = unitss[i];
				ManualControlFree objSel = obj.GetComponent<ManualControlFree>();
				UnitParsFree objPars = obj.GetComponent<UnitParsFree>();
				
				if(objSel.isMoving){
				    
				    ax = obj.transform.position.x;
					ay = obj.transform.position.y;
					az = obj.transform.position.z;
				
					tx = objSel.manualDestination.x;
					ty = objSel.manualDestination.y;
					tz = objSel.manualDestination.z;
				    
				    
				    r = Mathf.Sqrt((tx-ax)*(tx-ax)+(ty-ay)*(ty-ay)+(tz-az)*(tz-az));
				    
					if(r >= objSel.prevDist){
					
					    
					    objSel.failedDist = objSel.failedDist+1;
					    if(objSel.failedDist > objSel.critFailedDist){
					    	objSel.failedDist = 0;
					    	objSel.isMoving = false;
					    	ResetSearching(obj);
					    }
						
					}
					objSel.prevDist = r;
				}
				
				if(objSel.prepareMoving){
					if(objPars.isMovable){
						UnSetSearching(obj);
					
						objSel.prepareMoving = false;
						objSel.isMoving = true;
					
						obj.GetComponent<NavMeshAgent>().SetDestination(objSel.manualDestination);
					}
				}
				
			}
			
			yield return new WaitForSeconds(0.5f);
		}
	}
	
	
	
	
	
// single action functions	
	
	public void AddUnit(GameObject go)
	{
		unitsBuffer.Add(go);
	}
	

	
	
	public void ResetSearching(GameObject go)
	{
		UnitParsFree goPars = go.GetComponent<UnitParsFree>();
	    
		goPars.isApproaching = false;
		goPars.isAttacking = false;
		goPars.target = null;
		
		go.GetComponent<NavMeshAgent>().SetDestination(go.transform.position);
		
		if(goPars.changeMaterial){
			go.GetComponent<Renderer>().material.color = Color.yellow;				
		}
		
		goPars.isReady = true;
		
	}
	
	
	
	public void UnSetSearching(GameObject go)
	{
		UnitParsFree goPars = go.GetComponent<UnitParsFree>();
	 //   unitsBuffer.Remove(go);
	    
	    goPars.isReady = false;
		goPars.isApproaching = false;
		goPars.isAttacking = false;
		goPars.target = null;
		
		go.GetComponent<NavMeshAgent>().SetDestination(go.transform.position);
		
		if(goPars.changeMaterial){
			go.GetComponent<Renderer>().material.color = Color.grey;				
		}
		
		
	}
	
	
	
	

	
	
	
	
// Heap sort used for sorting before passing to KDTree

    public static void HeapSort(Vector3[] input, int[] iorig)
   {
    	//Build-Max-Heap
    	int heapSize = input.Length;
    	for (int p = (heapSize - 1) / 2; p >= 0; p--)
        	MaxHeapify(input, iorig, heapSize, p);
 
    	for (int i = input.Length - 1; i > 0; i--)
    	{
        	//Swap
        	Vector3 temp = input[i];
        	input[i] = input[0];
        	input[0] = temp;
        	
        	int itemp = iorig[i];
        	iorig[i] = iorig[0];
        	iorig[0] = itemp;
 
        	heapSize--;
        	MaxHeapify(input, iorig, heapSize, 0);
    	}
    }


    private static void MaxHeapify(Vector3[] input, int[] iorig, int heapSize, int index)
    {
    	int left = (index + 1) * 2 - 1;
    	int right = (index + 1) * 2;
    	int largest = 0;
 
    	if (left < heapSize && input[left].x > input[index].x)
        	largest = left;
    	else
        	largest = index;
 
    	if (right < heapSize && input[right].x > input[largest].x)
        	largest = right;
 
    	if (largest != index)
    	{
        	Vector3 temp = input[index];
        	input[index] = input[largest];
        	input[largest] = temp;
        	
        	int itemp = iorig[index];
        	iorig[index] = iorig[largest];
        	iorig[largest] = itemp;
 
        	MaxHeapify(input, iorig, heapSize, largest);
    	}
    }	
	
	
	
	
	

	
	
}
