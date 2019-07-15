using UnityEngine;
using System.Collections;
using Rewired;

/// <summary>
///     クラスの説明
/// </summary>
public class InputTest : MonoBehaviour
{
    //-------------------------------------------------------------------------------------
    // Initialize for inspector （インスペクタで初期化するオブジェクト）
    //-------------------------------------------------------------------------------------


    //-------------------------------------------------------------------------------------
    // Public member & property
    //-------------------------------------------------------------------------------------


    //-------------------------------------------------------------------------------------
    // Method
    //-------------------------------------------------------------------------------------
    void Awake()
    {
        // Get the Rewired Player object for this player and keep it for the duration of the character's lifetime
        player = ReInput.players.GetPlayer("System");
    }

    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start ()
    {
        // 確認用のパネルを生成する
        for (int i = 0; i < input.Length; i++)
        {
            this.inputView[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            this.inputView[i].transform.position = new Vector3(-7.5f + i / 2 * 1f, -4 + i % 2, 0);
            this.inputView[i].transform.localScale = new Vector3(0.90f, 0.95f, 1);
        }

        for (int i = 0; i < input_air.Length; i++)
        {
            this.input_airView[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            this.input_airView[i].transform.position = new Vector3(0, -2 + i, 0);
            this.input_airView[i].transform.localScale = new Vector3(14f, 0.05f, 1);
        }
    }


    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update ()
    {
        GetInput();
        ProcessInput();
    }


    /// <summary>
    /// 描画処理
    /// </summary>
    void OnGUI()
    {
    }


    private void GetInput()
    {
        for (int i = 0; i < input.Length; i++)
        {
            input[i] = player.GetButton("Action" + i.ToString());
        }

        for (int i = 0; i < input_air.Length; i++)
        {
            input_air[i] = player.GetButton("Air" + i.ToString());
        }
    }

    private void ProcessInput()
    {
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i])
            {
                this.inputView[i].GetComponent<Renderer>().material.color = Color.red;
            }
            else
            {
                this.inputView[i].GetComponent<Renderer>().material.color = Color.white;
            }
        }

        for (int i = 0; i < input_air.Length; i++)
        {
            if (input_air[i])
            {
                this.input_airView[i].GetComponent<Renderer>().material.color = Color.red;
            }
            else
            {
                this.input_airView[i].GetComponent<Renderer>().material.color = Color.white;
            }
        }

    }

    //-------------------------------------------------------------------------------------
    // Private member
    //-------------------------------------------------------------------------------------
    private Player player; // The Rewired Player

    GameObject[] inputView = new GameObject[32];
    bool[] input = new bool[32];

    GameObject[] input_airView = new GameObject[6];
    bool[] input_air = new bool[6];

}