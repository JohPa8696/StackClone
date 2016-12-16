using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class Stack : MonoBehaviour {
	public Color32[] gameColors= new Color32[4];
	public Material mat;
	public Text score;
	public GameObject endPanel;

	private const float BOUND_SIZE = 3.5f;
	private const float STACK_MOVING_SPEED = 5.0f;
	private const float ERROR_MARGIN = 0.1F;
	private const float GAIN = 0.5f;
	private const int COMBO_GAIN = 5;

	private GameObject[] theStack;

	private Vector2 stackBounds = new Vector2 (BOUND_SIZE, BOUND_SIZE);

	private int nextPosition=1;
	private int stackIndex;
	private int scoreCount =0 ;
	private int combo;

	private float tileTransition =0.0f;
	private float tileSpeed = 2.5f;
	private float secPosition;

	private bool isOnX = false;
	private bool gameOver=false;

	private Vector3 desiredPosition;
	private Vector3 lastTilePosition;


	// Use this for initialization
	private void Start () {
		//Initialize the stack
		theStack = new GameObject[transform.childCount];
		for (int i = 0; i < transform.childCount; i++) {
			theStack[i]=transform.GetChild(i).gameObject;
			ColorMesh (theStack [i].GetComponent<MeshFilter> ().mesh);
 		}
		//Initialize the index of the object in the stack for the transition
		stackIndex = transform.childCount - 1;
	}
	private void CreateRubble(Vector3 pos, Vector3 scale){
		GameObject go = GameObject.CreatePrimitive (PrimitiveType.Cube);
		go.transform.localPosition = pos;
		go.transform.localScale = scale;
		go.AddComponent<Rigidbody> ();
		go.GetComponent<MeshRenderer> ().material = mat;
		ColorMesh(go.GetComponent<MeshFilter>().mesh);
	}

	// Update is called once per frame
	void Update () {
		if (gameOver)
			return;
		if (Input.GetMouseButtonDown (0)) {
			if (PlaceTile ()) {
				SpawnTile ();
				scoreCount++;
				score.text = scoreCount.ToString();
			} else {
				EndGame ();
			}
		}

		MoveTile ();
		//Move the stack
		transform.position= Vector3.Lerp(transform.position,desiredPosition,STACK_MOVING_SPEED*Time.deltaTime);
	}

	private void MoveTile(){

		tileTransition += Time.deltaTime * tileSpeed;
		if (isOnX) {
			theStack [stackIndex].transform.localPosition = new Vector3 (Mathf.Sin (tileTransition) * BOUND_SIZE, scoreCount, secPosition);	
		} else {
			theStack [stackIndex].transform.localPosition = new Vector3 (secPosition,scoreCount,Mathf.Sin (tileTransition) * BOUND_SIZE);
		}

	}

	private void SpawnTile(){
		lastTilePosition = theStack [stackIndex].transform.localPosition;
		stackIndex--;
		if (stackIndex < 0) {
			stackIndex = transform.childCount -1;
		}
		desiredPosition = Vector3.down * scoreCount;
		theStack[stackIndex].transform.localPosition = new Vector3 (0, scoreCount, 0);
		theStack[stackIndex].transform.localScale = new Vector3 (stackBounds.x, 1, stackBounds.y);
		ColorMesh(theStack[stackIndex].GetComponent<MeshFilter>().mesh);
	}
	private bool PlaceTile(){	

		Transform t = theStack [stackIndex].transform;

		if (isOnX) {
			float deltaX = lastTilePosition.x - t.position.x;
			if (Mathf.Abs (deltaX) > ERROR_MARGIN) {
				//Cut the tile
				combo = 0;
				stackBounds.x -= Mathf.Abs (deltaX);
				if (stackBounds.x <= 0) {
					return false;
				}
				float middle = lastTilePosition.x + t.localPosition.x / 2;
				t.localScale = new Vector3 (stackBounds.x, 1, stackBounds.y);
				CreateRubble (
					new Vector3((t.position.x>0) 
						? t.position.x + (t.localScale.x/2)
						: t.position.x - (t.localScale.x/2), 
						t.position.y,t.position.z),
					new Vector3(Mathf.Abs(deltaX),1,t.localScale.z)
				);
				t.localPosition = new Vector3 (middle - (lastTilePosition.x / 2), scoreCount, lastTilePosition.z);
			} else {
				if (combo > COMBO_GAIN) {
					stackBounds.x += GAIN;
					if (stackBounds.x > BOUND_SIZE) {
						stackBounds.x = BOUND_SIZE;
					}
					float middle = lastTilePosition.x + t.localPosition.x / 2;
					t.localScale = new Vector3 (stackBounds.x, 1, stackBounds.y);
					t.localPosition = new Vector3 (middle - (lastTilePosition.x / 2), scoreCount, lastTilePosition.z);
				}
				combo++;
				t.localPosition = new Vector3 (lastTilePosition.x, scoreCount, lastTilePosition.z);
			}
		} else {
			float deltaZ = lastTilePosition.z - t.position.z;
			if (Mathf.Abs (deltaZ) > ERROR_MARGIN) {
				//Cut the tile
				combo = 0;
				stackBounds.y -= Mathf.Abs (deltaZ);
				if (stackBounds.y <= 0) {
					return false;
				}
				float middle = lastTilePosition.z + t.localPosition.z / 2;
				t.localScale = new Vector3 (stackBounds.x, 1, stackBounds.y);
				CreateRubble (
					new Vector3(t.position.x, t.position.y,
						(t.position.z >0) 
						? t.position.z + (t.localScale.z/2)
						: t.position.z - (t.localScale.z/2)),
					new Vector3(t.localScale.x,1,Mathf.Abs(deltaZ))
				);
				t.localPosition = new Vector3 (lastTilePosition.x, scoreCount, middle - lastTilePosition.z / 2);
			} else {
				if (combo > COMBO_GAIN) {
					stackBounds.y += GAIN;
					if (stackBounds.y > BOUND_SIZE) {
						stackBounds.y = BOUND_SIZE;
					}
					float middle = lastTilePosition.z + t.localPosition.z / 2;
					t.localScale = new Vector3 (stackBounds.x, 1, stackBounds.y);
					t.localPosition = new Vector3 (lastTilePosition.x, scoreCount, middle - lastTilePosition.z / 2);
				}
				combo++;
				t.localPosition = new Vector3 (lastTilePosition.x, scoreCount, lastTilePosition.z);
			}
		}
		secPosition = (isOnX) ? t.localPosition.x : t.localPosition.z; 
		isOnX = !isOnX;
		return true;
	}

	private void EndGame(){
		if (PlayerPrefs.GetInt ("score") < scoreCount)
			PlayerPrefs.SetInt ("score", scoreCount);
		gameOver = true;
		theStack [stackIndex].AddComponent<Rigidbody> ();
		endPanel.SetActive(true);

	}

	private Color32 Lerp4(Color32 a,Color32 b,Color32 c,Color32 d,float f){
		if (f < 0.33f) {
			return Color.Lerp (a, b, f / 0.33f);
		} else if (f < 0.66f) {
			return Color.Lerp (b, c, (f - 0.33f) / 0.33f);
		} else {
			return Color.Lerp (c, d, (f - 0.66f) / 0.66f);
		}
	}
	private void ColorMesh(Mesh mesh){
		Vector3[] vertices = mesh.vertices;
		Color32[] colors = new Color32[vertices.Length];
		float f= Mathf.Sin(scoreCount*0.25f);

		for (int i = 0; i < vertices.Length; i++) {
			colors [i] = Lerp4 (gameColors [0], gameColors [1], gameColors [2], gameColors [3], f);
		}
		mesh.colors32 = colors;
	}

	public void OnButtonClick(string scene){
		SceneManager.LoadScene (scene);
	}

}
