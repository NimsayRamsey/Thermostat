using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using KModkit;

public class ThermostatScript : MonoBehaviour {

	public KMBombInfo Bomb;
	public KMAudio Audio;
	public KMBombModule Module;
	public KMGameInfo LightInfo; // Light Detection

	public Material[] LightColors;//Dark Red, Red
	public KMSelectable[] MoveButtons; //Up, Right, Down, Left
	public KMSelectable ModeButton;
	public KMSelectable SetButton;
	public Renderer CollisionLight;
	public TextMesh[] SegmentDisplay;
	public TextMesh[] WeatherDisplay;
	public TextMesh[] TemperatureType;
	public GameObject[] Backlight; // Normal Display, Backlit Display

	//-----------------------------------------------------//
	//VARIABLES
	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved = false;

	private int[] currentSpace = new int[] {2, 2, 0};
	private List<int> levelRefs = new List<int> {9, 9, 9};
	private int correctTemperature = 0;
	private int correctWeather = 0;
	private int[] correctSpace = new int[] {9, 9, 9};

	private bool backlit = true;
	private int TempType = 0;

	//-----------------------------------------------------//
	//READONLY LIBRARIES
	private string[] TempTypeNames = new string[] {"C", "F"};
	private int[] TempTypeConvers = new int[] {40, 100};

	private string[] mazeNames = new string[] {"Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta", "Theta"};
	private int[] mazeWeather = new int[] {     9,       2,      6,       4,       5,         7,      3,     8};
	private int[,] chartWeather = new int[,] {
		{1, 1}, {1, 0}, {2, 0},
		{2, 1}, {2, 2}, {0, 2},
		{1, 2}, {0, 1}, {0, 0}
	};
	private int[,] chartFull = new int[,] {
		{9, 8, 6},
		{2, 1, 7},
		{3, 4, 5}
	};

	private int[,,,] mazeDirections = new int[,,,] { //Up, Right, Down, Left
		{//Alpha
			{{0, 1, 1, 0}, {0, 1, 1, 1}, {0, 1, 1, 1}, {0, 1, 1, 1}, {0, 0, 1, 1}},
			{{1, 1, 1, 0}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 0, 1, 1}},
			{{1, 1, 1, 0}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 0, 1, 1}},
			{{1, 1, 1, 0}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 0, 1, 1}},
			{{1, 1, 0, 0}, {1, 1, 0, 1}, {1, 1, 0, 1}, {1, 1, 0, 1}, {1, 0, 0, 1}}
		},
		{//Beta
			{{0, 1, 1, 0}, {0, 1, 1, 1}, {0, 1, 1, 1}, {0, 1, 1, 1}, {0, 0, 1, 1}},
			{{1, 1, 1, 0}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 0, 1, 1}},
			{{1, 1, 1, 0}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 0, 1, 1}},
			{{1, 1, 1, 0}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 0, 1, 1}},
			{{1, 1, 0, 0}, {1, 1, 0, 1}, {1, 1, 0, 1}, {1, 1, 0, 1}, {1, 0, 0, 1}}
		},
		{//Gamma //DONE//
			{{0, 1, 1, 0}, {0, 1, 0, 1}, {1, 1, 1, 1}, {0, 1, 0, 1}, {0, 0, 1, 1}},
			{{1, 0, 1, 0}, {0, 1, 1, 0}, {1, 1, 0, 1}, {0, 0, 1, 1}, {1, 0, 1, 0}},
			{{1, 1, 1, 1}, {1, 0, 1, 1}, {0, 0, 0, 0}, {1, 1, 1, 0}, {1, 1, 1, 1}},
			{{1, 0, 1, 0}, {1, 1, 0, 0}, {0, 1, 1, 1}, {1, 0, 0, 1}, {1, 0, 1, 0}},
			{{1, 1, 0, 0}, {0, 1, 0, 1}, {1, 1, 1, 1}, {0, 1, 0, 1}, {1, 0, 0, 1}}
		},
		{//Delta
			{{0, 1, 1, 0}, {0, 1, 1, 1}, {0, 1, 1, 1}, {0, 1, 1, 1}, {0, 0, 1, 1}},
			{{1, 1, 1, 0}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 0, 1, 1}},
			{{1, 1, 1, 0}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 0, 1, 1}},
			{{1, 1, 1, 0}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 0, 1, 1}},
			{{1, 1, 0, 0}, {1, 1, 0, 1}, {1, 1, 0, 1}, {1, 1, 0, 1}, {1, 0, 0, 1}}
		},
		{//Epsilon //DONE//
			{{0, 1, 1, 0}, {0, 1, 1, 1}, {0, 1, 0, 1}, {0, 0, 1, 1}, {1, 0, 1, 0}},
			{{1, 0, 1, 0}, {1, 1, 0, 0}, {0, 1, 1, 1}, {1, 1, 1, 1}, {1, 0, 1, 1}},
			{{1, 0, 1, 0}, {0, 1, 1, 0}, {1, 0, 1, 1}, {1, 0, 1, 0}, {1, 0, 0, 0}},
			{{1, 1, 0, 1}, {1, 0, 1, 1}, {1, 0, 0, 0}, {1, 1, 1, 0}, {0, 1, 0, 1}},
			{{0, 1, 0, 0}, {1, 1, 0, 1}, {0, 1, 0, 1}, {1, 1, 0, 1}, {0, 0, 1, 1}}
		},
		{//Zeta
			{{0, 1, 1, 0}, {0, 1, 1, 1}, {0, 1, 1, 1}, {0, 1, 1, 1}, {0, 0, 1, 1}},
			{{1, 1, 1, 0}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 0, 1, 1}},
			{{1, 1, 1, 0}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 0, 1, 1}},
			{{1, 1, 1, 0}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 0, 1, 1}},
			{{1, 1, 0, 0}, {1, 1, 0, 1}, {1, 1, 0, 1}, {1, 1, 0, 1}, {1, 0, 0, 1}}
		},
		{//Eta //DONE//
			{{1, 0, 1, 0}, {1, 0, 1, 1}, {0, 1, 1, 0}, {1, 0, 0, 1}, {1, 0, 1, 1}},
			{{1, 0, 0, 1}, {1, 0, 1, 0}, {1, 1, 1, 0}, {0, 1, 1, 1}, {1, 1, 1, 1}},
			{{0, 1, 0, 1}, {1, 1, 0, 1}, {1, 0, 0, 1}, {1, 0, 1, 0}, {1, 1, 0, 0}},
			{{0, 1, 0, 1}, {0, 0, 1, 1}, {0, 1, 1, 0}, {1, 0, 0, 1}, {0, 1, 1, 0}},
			{{0, 1, 1, 0}, {1, 0, 1, 1}, {1, 1, 0, 0}, {0, 1, 1, 1}, {1, 0, 1, 1}}
		},
		{//Theta
			{{0, 1, 1, 0}, {0, 1, 1, 1}, {0, 1, 1, 1}, {0, 1, 1, 1}, {0, 0, 1, 1}},
			{{1, 1, 1, 0}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 0, 1, 1}},
			{{1, 1, 1, 0}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 0, 1, 1}},
			{{1, 1, 1, 0}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 0, 1, 1}},
			{{1, 1, 0, 0}, {1, 1, 0, 1}, {1, 1, 0, 1}, {1, 1, 0, 1}, {1, 0, 0, 1}}
		}
	};

	private int[,,,] mazeContents = new int[,,,] {
		{//Alpha
			{{01, 1}, {02, 2}, {03, 3}, {04, 4}, {05, 5}},
			{{06, 2}, {07, 3}, {08, 4}, {09, 5}, {10, 6}},
			{{11, 3}, {12, 4}, {13, 5}, {14, 6}, {15, 7}},
			{{16, 4}, {17, 5}, {18, 6}, {19, 7}, {20, 8}},
			{{21, 5}, {22, 6}, {23, 7}, {24, 8}, {25, 9}}
		},
		{//Beta
			{{26, 1}, {27, 2}, {28, 3}, {29, 4}, {30, 5}},
			{{31, 2}, {32, 3}, {33, 4}, {34, 5}, {35, 6}},
			{{36, 3}, {37, 4}, {38, 5}, {39, 6}, {40, 7}},
			{{41, 4}, {42, 5}, {43, 6}, {44, 7}, {45, 8}},
			{{46, 5}, {47, 6}, {48, 7}, {49, 8}, {50, 9}}
		},
		{//Gamma
			{{01, 1}, {02, 2}, {03, 3}, {04, 4}, {05, 5}},
			{{06, 2}, {07, 3}, {08, 4}, {09, 5}, {10, 6}},
			{{11, 3}, {12, 4}, {13, 5}, {14, 6}, {15, 7}},
			{{16, 4}, {17, 5}, {18, 6}, {19, 7}, {20, 8}},
			{{21, 5}, {22, 6}, {23, 7}, {24, 8}, {25, 9}}
		},
		{//Delta
			{{26, 1}, {27, 2}, {28, 3}, {29, 4}, {30, 5}},
			{{31, 2}, {32, 3}, {33, 4}, {34, 5}, {35, 6}},
			{{36, 3}, {37, 4}, {38, 5}, {39, 6}, {40, 7}},
			{{41, 4}, {42, 5}, {43, 6}, {44, 7}, {45, 8}},
			{{46, 5}, {47, 6}, {48, 7}, {49, 8}, {50, 9}}
		},
		{//Epsilon
			{{01, 1}, {02, 2}, {03, 3}, {04, 4}, {05, 5}},
			{{06, 2}, {07, 3}, {08, 4}, {09, 5}, {10, 6}},
			{{11, 3}, {12, 4}, {13, 5}, {14, 6}, {15, 7}},
			{{16, 4}, {17, 5}, {18, 6}, {19, 7}, {20, 8}},
			{{21, 5}, {22, 6}, {23, 7}, {24, 8}, {25, 9}}
		},
		{//Zeta
			{{26, 1}, {27, 2}, {28, 3}, {29, 4}, {30, 5}},
			{{31, 2}, {32, 3}, {33, 4}, {34, 5}, {35, 6}},
			{{36, 3}, {37, 4}, {38, 5}, {39, 6}, {40, 7}},
			{{41, 4}, {42, 5}, {43, 6}, {44, 7}, {45, 8}},
			{{46, 5}, {47, 6}, {48, 7}, {49, 8}, {50, 9}}
		},
		{//Eta
			{{01, 1}, {02, 2}, {03, 3}, {04, 4}, {05, 5}},
			{{06, 2}, {07, 3}, {08, 4}, {09, 5}, {10, 6}},
			{{11, 3}, {12, 4}, {13, 5}, {14, 6}, {15, 7}},
			{{16, 4}, {17, 5}, {18, 6}, {19, 7}, {20, 8}},
			{{21, 5}, {22, 6}, {23, 7}, {24, 8}, {25, 9}}
		},
		{//Theta
			{{26, 1}, {27, 2}, {28, 3}, {29, 4}, {30, 5}},
			{{31, 2}, {32, 3}, {33, 4}, {34, 5}, {35, 6}},
			{{36, 3}, {37, 4}, {38, 5}, {39, 6}, {40, 7}},
			{{41, 4}, {42, 5}, {43, 6}, {44, 7}, {45, 8}},
			{{46, 5}, {47, 6}, {48, 7}, {49, 8}, {50, 9}}
		}
	};
	
	private string[] WeatherNames = new string[] {"Sunny", "Cloudy", "Rain", "Hard Rain", "Snow", "Thunder & Rain", "Thunder & Hard Rain", "Thunder", "Partially Cloudy"};

	private char[] alphabet = new char[] {//The Alphabet
		'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
	  // 1    2    3    4    5    6    7    8    9    10   11   12   13   14   15   16   17   18   19   20   21   22   23   24   25   26
	};

	private int[] lightLag = new int[] {0, 3};
	private int LightDurr = 5;
	private bool lightStart = false;

	//-----------------------------------------------------//
	IEnumerator HandleLight() {
		while(true) {
			if(!backlit){
				Backlight[0].SetActive(false);
				Backlight[1].SetActive(true);
			} else {
				Backlight[1].SetActive(false);
				Backlight[0].SetActive(true);
			}
			yield return new WaitForSeconds(0.1f);
		}
	}

	private void Awake() {
		moduleId = moduleIdCounter++;
		foreach (KMSelectable NAME in MoveButtons) {
			KMSelectable pressedObject = NAME;
			NAME.OnInteract += delegate () { pressMoveButton(pressedObject); return false; };
		}
		ModeButton.OnInteract += delegate () { pressModeButton(); return false; };
		SetButton.OnInteract += delegate () { pressSetButton(); return false; };

		// Borrowed from Simon Literally Says. I have no idea what deligates do :/
		//LightInfo.OnLightsChange += delegate(bool state) { Debug.Log("Lights Changed"); };//if(backlit != state) { backlit = state; }
		//StartCoroutine(HandleLight());
		Backlight[1].SetActive(false);
		Backlight[0].SetActive(true);
	}

	void Start() {
		TempType = UnityEngine.Random.Range(0, 2);
		TemperatureType[0].text = TempTypeNames[TempType];
		TemperatureType[1].text = TempTypeNames[TempType];
		TemperatureType[2].text = TempTypeNames[TempType];
		Debug.LogFormat("[Thermostat #{0}] Top left of a layer is 0, 0. Bottom right is 4, 4", moduleId);
		InitSolution();
		CollisionLight.material = LightColors[0];
		renderDisplay();
	}

	void InitSolution() {
		currentSpace[0] = UnityEngine.Random.Range(0, 5);
		currentSpace[1] = UnityEngine.Random.Range(0, 5);

		for (int i = 0; i < 3; i++){
			bool loopback = true;
			while(loopback){
				loopback = false;
				levelRefs[i] = UnityEngine.Random.Range(0, 8);
				for (int j = 0; j < i; j++){
					if (levelRefs[i] == levelRefs[j]) { loopback = true;}
				}
			}
		}

		Debug.LogFormat("[Thermostat #{0}] Module mazes are {1}, {2}, {3}", moduleId, mazeNames[levelRefs[0]], mazeNames[levelRefs[1]], mazeNames[levelRefs[2]]);

		int startingLayer = 0;
		int shiftDown = 0;
		int shiftRight = 0;
		
		string serialNum = Bomb.GetSerialNumber();
		for (int x = 0; x < 6; x++) {
			if (Regex.IsMatch(serialNum[x].ToString(), "[0-9]")) {
				startingLayer += serialNum[x] - '0';
				if (x == 2) {
					shiftDown += serialNum[x] - '0';
				} else if (x == 5) {
					shiftRight += serialNum[x] - '0';
				}
			} else {
				correctTemperature += Array.IndexOf(alphabet, serialNum[x]) + 1;
				//Debug.Log(Array.IndexOf(alphabet, serialNum[x]) + 1);
			}
		}

		if (correctTemperature > TempTypeConvers[TempType]) {correctTemperature = correctTemperature % TempTypeConvers[TempType];}
		startingLayer = startingLayer % 3;
		if (startingLayer == 0) {startingLayer = 3;}
		startingLayer -= 1;

		Debug.LogFormat("[Thermostat #{0}] The starting weather is {1} from maze {2}. Shifts are {3} down and {4} right.", moduleId, WeatherNames[mazeWeather[levelRefs[startingLayer]]-1], startingLayer+1, shiftDown, shiftRight);

		shiftDown += chartWeather[mazeWeather[levelRefs[startingLayer]]-1, 0];
		shiftDown = shiftDown % 3;
		shiftRight += chartWeather[mazeWeather[levelRefs[startingLayer]]-1, 1];
		shiftRight = shiftRight % 3;

		correctWeather = chartFull[shiftDown, shiftRight];
		Debug.LogFormat("[Thermostat #{0}] The target temperature is {1}*{2}", moduleId, correctTemperature, TempTypeNames[TempType]);
		Debug.LogFormat("[Thermostat #{0}] Target weather is {1}", moduleId, WeatherNames[correctWeather-1]);

		InitBoardShuffle();
	}

	void InitBoardShuffle() {
		int[] SolAdd = new int[] {1, UnityEngine.Random.Range(1, 4)};
		int BoardCount = 0;
		int[] dummySet = new int[] {0, 0};
		//Debug.Log(SolAdd[1]);
		for (int BOARD = 0; BOARD < 8; BOARD++){
			if (levelRefs.Contains(BOARD)) {
				BoardCount += 1;
				dummySet[0] = UnityEngine.Random.Range(0, 5);
				dummySet[1] = UnityEngine.Random.Range(0, 5);
				for (int ROW = 0; ROW < 5; ROW++){
					for (int COL = 0; COL < 5; COL++){
						if (SolAdd[0] == 1 && SolAdd[1] == 1 && ROW == dummySet[0] && COL == dummySet[1]){
							mazeContents[BOARD, ROW, COL, 0] = correctTemperature;
							mazeContents[BOARD, ROW, COL, 1] = correctWeather;
							correctSpace[0] = levelRefs.IndexOf(BOARD) + 1;
							correctSpace[1] = COL;
							correctSpace[2] = ROW;
							SolAdd[0] = 0;
							SolAdd[1] -= 1;
						} else if (SolAdd[0] == 1 && SolAdd[1] != 1 && ROW == dummySet[0] && COL == dummySet[1]) {
							mazeContents[BOARD, ROW, COL, 0] = correctTemperature;
							mazeContents[BOARD, ROW, COL, 1] = UnityEngine.Random.Range(1, 10);
							while (mazeContents[BOARD, ROW, COL, 1] == correctWeather) {
								mazeContents[BOARD, ROW, COL, 1] = UnityEngine.Random.Range(1, 10);
							}
							SolAdd[0] = 0;
							SolAdd[1] -= 1;
							//Debug.LogFormat("[Thermostat #{0}] Dummy space is located on maze {1} at {2}, {3}", moduleId, levelRefs.IndexOf(BOARD) + 1, COL, ROW);
						} else {
							mazeContents[BOARD, ROW, COL, 0] = UnityEngine.Random.Range(2, TempTypeConvers[TempType]+1);
							mazeContents[BOARD, ROW, COL, 1] = UnityEngine.Random.Range(1, 10);
							while (mazeContents[BOARD, ROW, COL, 0] == correctTemperature) {
								mazeContents[BOARD, ROW, COL, 0] = UnityEngine.Random.Range(2, TempTypeConvers[TempType]+1);
							}
						}
					}
				}
			}
			SolAdd[0] = 1;
		}

		Debug.LogFormat("[Thermostat #{0}] Target space is located on maze {1} at {2}, {3}", moduleId, correctSpace[0], correctSpace[1], correctSpace[2]);
	}

	void pressMoveButton(KMSelectable buttonObject) {//KMSelectable button
		int buttonNum = Array.IndexOf(MoveButtons, buttonObject);
		buttonObject.AddInteractionPunch();
		if (moduleSolved) {return;}

		int[] directions = new int[] {-1, 1, 1, -1};

		if (mazeDirections[levelRefs[currentSpace[2]], currentSpace[0], currentSpace[1], buttonNum] == 1) {
			currentSpace[buttonNum % 2] += directions[buttonNum];
			if (currentSpace[buttonNum % 2] < 0) { currentSpace[buttonNum % 2] = 4;}
			if (currentSpace[buttonNum % 2] > 4) { currentSpace[buttonNum % 2] = 0;}
			Audio.PlaySoundAtTransform("Beep6", transform);
		} else {
			lightStart = true;
			Audio.PlaySoundAtTransform("Beep14", transform);
		}
		renderDisplay();
	}

	void pressModeButton() {//KMSelectable button
		ModeButton.AddInteractionPunch();
		if (moduleSolved) {return;}
		Audio.PlaySoundAtTransform("Beep5", transform);
		lightStart = true;

		currentSpace[2] += 1;
		if (currentSpace[2] == 3){currentSpace[2] = 0;}
		renderDisplay();
		Debug.Log(currentSpace[2]);
	}

	void pressSetButton() {//KMSelectable button
		SetButton.AddInteractionPunch();
		if (moduleSolved) {return;}
		Audio.PlaySoundAtTransform("Beep4", transform);

		if (positionData(0) == correctTemperature && positionData(1) == correctWeather){//currTemperature == correctTemperature && 
			Debug.LogFormat("[Thermostat #{0}] Submitted {1}*{2} & {3}...Module Defused", moduleId, positionData(0), TempTypeNames[TempType], WeatherNames[positionData(1)-1]);
			GetComponent<KMBombModule>().HandlePass();
			moduleSolved = true;
		} else {
			Debug.LogFormat("[Thermostat #{0}] Submitted {1}*{2} & {3}...WRONG! Recieved strike", moduleId, positionData(0), TempTypeNames[TempType], WeatherNames[positionData(1)-1]);
			GetComponent<KMBombModule>().HandleStrike();
		}
	}

	void renderDisplay() {
		//currTemperature = UnityEngine.Random.Range(2, 100);
		string tempFull = positionData(0).ToString();
		if (positionData(0) < 10) {tempFull = "0" + tempFull;}

		SegmentDisplay[0].text = tempFull;
		SegmentDisplay[1].text = tempFull;
		SegmentDisplay[2].text = tempFull;
		WeatherDisplay[0].text = positionData(1).ToString();
		WeatherDisplay[1].text = positionData(1).ToString();
		WeatherDisplay[2].text = positionData(1).ToString();
	}

	int positionData (int mode) {
		return mazeContents[levelRefs[currentSpace[2]], currentSpace[0], currentSpace[1], mode];
	}

	void Update() {
		if (lightStart) {
			lightLag[0] = LightDurr;
			lightLag[1] = 0;
			lightStart = false;
		}
		if (lightLag[1] != 3) {
			//Debug.Log(lightLag[0] + ", " + lightLag[1]);
			if (lightLag[1] % 2 == 0){ CollisionLight.material = LightColors[1]; } else { CollisionLight.material = LightColors[0]; }
			if (lightLag[0] == 0) {
				lightLag[0] = LightDurr;
				lightLag[1] += 1;
				if (lightLag[1] == 3) { CollisionLight.material = LightColors[0]; }
			} else {lightLag[0] -= 1;}
		}
	}
}
