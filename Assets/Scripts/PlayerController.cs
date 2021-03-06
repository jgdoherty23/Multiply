﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;


public class PlayerController : NetworkBehaviour {

	public float movementSpeed;
	public float turnSpeed;
	public float bulletSpeed;
	public float donutSpeed;
	public GameObject bulletPrefab;
	public Transform bulletSpawn;
	public Transform donutSpawn;
	public GameObject donutPrefab;
	public GameObject clonePrefab;
	public Material localPlayerColor;
	public Material enemyPlayerColor;
	public int clonesLeft;
	public int donutsLeft;

	private Vector3 movement;
	private float turn;
	private Game gameScript;
	private Renderer rend;
	private Rigidbody rigidbody;
	private MyHud myHud;

	void Awake() {
		gameScript = GameObject.Find("Game").GetComponent<Game>();
		GameObject managerObj = GameObject.Find ("NetworkManager");
		myHud = managerObj.GetComponent<MyHud> ();
		rend = GetComponent<MeshRenderer> ();
		rend.enabled = false;
		rigidbody = GetComponent<Rigidbody> ();
		rigidbody.freezeRotation = true;
		rigidbody.isKinematic = true;
		transform.Find ("Circle").gameObject.SetActive (false);
		transform.Find ("PlayerHUD").gameObject.SetActive (false);
		transform.Find ("Visor").gameObject.SetActive (false);
		transform.Find ("Gun").gameObject.SetActive (false);
		transform.Find ("BulletSpawn").gameObject.SetActive (false);
		transform.Find ("HealthbarCanvas").gameObject.SetActive (false);
	}

	void Start() {
		if (gameScript.gameStarted) {
			if (isLocalPlayer) {
				myHud.LeaveMatch ();
			}
		} else {
			ActivatePlayer ();
		}
	}

	void Update () {
		if (isLocalPlayer) {
			CursorVisibility ();
			if (!myHud.gamePaused) {
				GunControl ();
				DonutControl ();
				MovementControl ();
				TurnControl ();
				SpawnControl ();
			}
		}
	}

	void ActivatePlayer() {
		rend.enabled = true;
		if (isLocalPlayer) {
			rend.material = localPlayerColor;
			rend.enabled = true;
			SetupCamera ();
			transform.Find ("Circle").gameObject.SetActive (true);
			transform.Find ("PlayerHUD").gameObject.SetActive (true);
			CmdEnterGame ();
		} else {
			rend.material = enemyPlayerColor;
		}
		rigidbody.isKinematic = false;
		transform.Find ("Visor").gameObject.SetActive (true);
		transform.Find ("Gun").gameObject.SetActive (true);
		transform.Find ("BulletSpawn").gameObject.SetActive (true);
		transform.Find ("HealthbarCanvas").gameObject.SetActive (true);
	}

	void SetupCamera() {
		Camera.main.transform.position = transform.position
			- transform.forward * 10
			+ transform.up * 3;
		Camera.main.transform.rotation = transform.rotation;
		Camera.main.transform.parent = transform;
	}
		
	void GunControl() {
		if (Input.GetMouseButtonDown(0)) {
			CmdFire ();
		}
	}

	void DonutControl() {
		if (donutsLeft > 0) {
			if (Input.GetMouseButtonDown (1)) {
				CmdThrowDonut ();
				donutsLeft--;
			}
		}
	}

	void MovementControl() {
		float horizontal = Input.GetAxisRaw ("Horizontal");
		float vertical = Input.GetAxisRaw ("Vertical");
		movement.Set(horizontal, 0, vertical);
		movement = movement.normalized * Time.deltaTime * movementSpeed;
		transform.Translate(movement);
	}

	void TurnControl() {
		turn = Input.GetAxis ("Mouse X") * turnSpeed;
		transform.Rotate (0, turn, 0);
	}

	void SpawnControl() {
		if (clonesLeft > 0) {
			if (Input.GetKeyDown (KeyCode.Space)) {
				CmdSpawnClone ();		
				clonesLeft--;
			}
		}
	}

	void CursorVisibility() {
		if (myHud.gamePaused) {
			Cursor.lockState = CursorLockMode.None;
		} else {
			Cursor.lockState = CursorLockMode.Locked;
		}
	}

	void PrintNetID() {
		if (isLocalPlayer) {
			print ("local netID: " + netId);
		} else {
			print ("other player: " + netId);
		}
	}

	[Command]
	void CmdSpawnClone() {
		GameObject clone = (GameObject)Instantiate (clonePrefab, 
			transform.position, 
			transform.rotation);
		CloneController cloneScript = clone.GetComponent<CloneController> ();
		cloneScript.creator = gameObject;
		NetworkServer.Spawn (clone);
	}

	[Command]
	void CmdFire() {
		var bullet = (GameObject)Instantiate (bulletPrefab,
			bulletSpawn.position,
			bulletSpawn.rotation);
		bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * bulletSpeed;
		NetworkServer.Spawn (bullet);

		// destroy bullet after 10 seconds
		Destroy(bullet, 10.0f);
	}

	[Command]
	void CmdThrowDonut() {
		var donut = (GameObject)Instantiate (donutPrefab, 
			donutSpawn.position, 
			donutSpawn.rotation);
		donut.GetComponent<Rigidbody> ().velocity = donut.transform.forward * donutSpeed;
		donut.GetComponent<Donut> ().creator = gameObject;
		// TODO: donut color
		NetworkServer.Spawn (donut);
	}

	[Command]
	public void CmdEndGame() {
		gameScript.EndGame ();
	}

	[Command]
	public void CmdEnterGame() {
		gameScript.EnterGame (netId);
	}
}
