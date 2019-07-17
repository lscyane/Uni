using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MusicSelect
{
    public class MusicSelect : MonoBehaviour
    {
        //-------------------------------------------------------------------------------------
        // Initialize for inspector （インスペクタで初期化するオブジェクト）
        //-------------------------------------------------------------------------------------
        public UnityEngine.UI.Button Pref_Button;


        //-------------------------------------------------------------------------------------
        // Private Definitions
        //-------------------------------------------------------------------------------------
        const string songsFolderPath = "/Songs/";
        const int MAX_VIEW_LIST = 20;


        //-------------------------------------------------------------------------------------
        // Structure
        //-------------------------------------------------------------------------------------
        enum EState
        {
            FolderSelect,
            MusicSelect,
        }


        //-------------------------------------------------------------------------------------
        // Private member
        //-------------------------------------------------------------------------------------
        MusicLibrary library;
        EState state;
        int select_folder_index;
        int selectLine;
        UnityEngine.UI.Button[] button;


        //-------------------------------------------------------------------------------------
        // Method
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// Use this for initialization
        /// </summary>
        void Start()
        {
            // フォルダ、曲ファイルの検索、ライブラリ構築
            this.library = new MusicLibrary(songsFolderPath);

            // stateの初期状態
            this.state = EState.FolderSelect;

            // インスタンス生成
            this.button = new UnityEngine.UI.Button[MAX_VIEW_LIST];
            for (int i = 0;i< MAX_VIEW_LIST; ++i) 
            {
                this.button[i] = Instantiate(this.Pref_Button);                         // プレハブからインスタンス生成
                button[i].transform.SetParent(GameObject.Find("Canvas").transform);     // Canvasの子でないと表示されない
                // ボタンにイベントを登録する.
                int n = i;  // スコープの関係で i を n に一度代入する
                button[i].onClick.AddListener(() => this.OnButtonSelected(button[n]));
            }

            // 最初はフォルダセレクトを表示する
            this.FolderSelectSetup();
        }


        /// <summary>
        /// ボタンクリックイベント
        /// </summary>
        /// <param name="btn_index"></param>
        void OnButtonSelected(Button btn_index)
        {
            switch (this.state)
            {
                case EState.FolderSelect:
                    // 曲選択シーケンスに移動  
                    this.state = EState.MusicSelect;
                    this.select_folder_index = this.library.group.FindIndex(p => p.Name == btn_index.name);
                    this.MusicSelectSetup();
                    break;

                case EState.MusicSelect:
                    // すべてのボタンをInactiveにする
                    for (int i = 0; i < MAX_VIEW_LIST; i++)
                    {
                        button[i].gameObject.SetActive(false);
                    }
                    // シーン遷移
                    string arg = this.library.group[this.select_folder_index].musics.Find(p => p.Name == btn_index.name).Path;
                    //NavigationService.NavigateAsync("GameMain", arg, LoadSceneMode.Single).Subscribe();

                    

                    break;
            }
        }

        void FolderSelectSetup()
        {
            for (int i = 0; i < MAX_VIEW_LIST; i++)
            {
                if (i < this.library.group.Count)
                {
                    // ボタンを表示する
                    button[i].transform.localPosition = new Vector3(-300, 200 - i * 30);
                    button[i].GetComponent<RectTransform>().sizeDelta = new Vector2(300, 25);
                    button[i].transform.Find("Text").GetComponent<Text>().text = this.library.group[i].Name;
                    button[i].name = this.library.group[i].Name;
                    button[i].gameObject.SetActive(true);
                }
                else
                {
                    // 非表示
                    button[i].gameObject.SetActive(false);
                }
            }
        }

        void MusicSelectSetup()
        {
            for (int i = 0; i < MAX_VIEW_LIST; i++)
            {
                if (i < this.library.group[this.select_folder_index].musics.Count) {
                    // ボタンを表示する
                    button[i].transform.localPosition = new Vector3(-300, 200 - i * 30);
                    button[i].GetComponent<RectTransform>().sizeDelta = new Vector2(300, 25);
                    button[i].transform.Find("Text").GetComponent<Text>().text = this.library.group[this.select_folder_index].musics[i].Name;
                    button[i].name = this.library.group[this.select_folder_index].musics[i].Name;
                    button[i].gameObject.SetActive(true);
                }
                else
                {
                    // 非表示
                    button[i].gameObject.SetActive(false);
                }
            }
        }


        void OnGUI()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}



