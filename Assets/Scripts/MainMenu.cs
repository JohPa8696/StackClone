using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MainMenu : MonoBehaviour {


	public Text score;
	private void Start ()
	{

		score.text = PlayerPrefs.GetInt ("score").ToString();


	}


	public void ToGame(){
		SceneManager.LoadScene("Main");
	}
}
