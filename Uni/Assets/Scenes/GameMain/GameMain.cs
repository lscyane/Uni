using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using CMS.Note;
using System.Linq;

namespace CMS
{
    public class GameMain : MonoBehaviour
    {
        //-------------------------------------------------------------------------------------
        // Initialize for inspector （インスペクタで初期化するオブジェクト）
        //-------------------------------------------------------------------------------------


        //-------------------------------------------------------------------------------------
        // Public member & property
        //-------------------------------------------------------------------------------------
        GameObject[] LaserSprite = new GameObject[CONST.DefApp.LANE_NUM];
        JudgeSplite judgeSprite;

        GameObject BGMNotes;
        GameObject TapNotes;
        UnityEngine.UI.Text comboText;

        //-------------------------------------------------------------------------------------
        // Method
        //-------------------------------------------------------------------------------------


        void Awake()
        {
            // Get the Rewired Player object for this player and keep it for the duration of the character's lifetime
            this.playerInput = Rewired.ReInput.players.GetPlayer("SYSTEM");
            this.systemInput = Rewired.ReInput.players.SystemPlayer;
        }



        /// <summary>
        /// Use this for initialization
        /// </summary>
        void Start()
        {
            // 前のシーンから渡された引数が取れる
            //string inputPath = Argument as string;
            string inputPath = "Assets/Songs/TestGroup/lapis_the_heavens_remix/yamajet-lapis_the_heavens_remix-7k2.cms";
            string loadFilePath = "";

            // 動的に追加するノートのルート (インスタンスはUnityに管理させる)
            GameObject NoteBaseObj = new GameObject();
            NoteBaseObj.transform.parent = this.transform.parent;
            NoteBaseObj.name = NoteBase.TAG_NAME;

            // Sound Manager Object (インスタンスはUnityに管理させる)
            GameObject obj = new GameObject();
            obj.name = CONST.System.SE_MANAGER_NAME;
            obj.AddComponent<SEManager>();

            // サウンドマネージャのセットアップ
            this.se_manager = GameObject.Find(CONST.System.SE_MANAGER_NAME).GetComponent(typeof(SEManager)) as SEManager;
            if (this.se_manager == null)
            {
                throw new System.Exception("サウンド管理オブジェクトのインスタンスがありません");
            }

            // リソースをロードしてインスタンス化(仮)
            for (int i = 0; i < CONST.DefApp.LANE_NUM; i++)
            {
                var go = new GameObject("Laser" + i.ToString());
                go.transform.parent = GameObject.Find("Canvas").transform;
                
                
                go.AddComponent<RectTransform>().rotation = new Quaternion(0, 0, 0, 0);
                go.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
                go.GetComponent<RectTransform>().sizeDelta = CONST.DefApp.LASER_SIZE;
                
                go.GetComponent<RectTransform>().localPosition = new Vector3(i - (CONST.DefApp.LANE_NUM / 2 - 0.5f), 0, 0);       // 画像の位置(アンカーポジション)を追加して位置設定
                go.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);                 // 縮尺がおかしいのでちゃんと等倍にする。

                go.AddComponent<UnityEngine.UI.Image>();
                go.GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("Textures/CMS/LaneSkins/laser");
                go.SetActive(false);
                this.LaserSprite[i] = go;
            }

            this.judgeSprite = new JudgeSplite();

            // Note Class init
            NoteBase.HighSpeed = 2.0f;

            this.comboText = GameObject.Find("ComboText").GetComponent<UnityEngine.UI.Text>();

            this.BGMNotes = new GameObject("BGMNotes");   // BGMノートのみをこの子オブジェクトとしてまとめる
            this.BGMNotes.transform.parent = NoteBaseObj.transform;
            this.TapNotes = new GameObject("TapNotes");   // Tap系ノートをこの子オブジェクトとしてまとめる
            this.TapNotes.transform.parent = NoteBaseObj.transform;


            // 譜面ファイル展開
#if ENABLE_ZIP_MUSIC
            if (System.IO.Path.GetExtension(inputPath) == ".zip")
            {
                string folder_name = System.IO.Path.GetFileNameWithoutExtension(inputPath);
                string file_name = "";

                // ファイルが指定された場合はzipファイルをTempに展開
                using (ZipFile zip = ZipFile.Read(inputPath))
                {
                    zip.ExtractExistingFile = ExtractExistingFileAction.OverwriteSilently;
                    foreach (ZipEntry entry in zip)
                    {
                        if (Regex.IsMatch(entry.FileName, CONST.DefApp.ZIP_FILE_REGEX))
                        {
                            // 指定拡張子のみ展開する
                            entry.Extract(Application.temporaryCachePath, ExtractExistingFileAction.OverwriteSilently);
                        }
                        if (System.IO.Path.GetExtension(entry.FileName) == ".cmsx")
                        {
                            file_name = entry.FileName;
                        }
                    }

                    if (file_name != "")
                    {
                        loadFilePath = Application.temporaryCachePath + "/" + folder_name + "/" + file_name;
                    }
                }
            }
            else
#endif
            {
                // フォルダが指定された場合はそのまま読み込む
                loadFilePath = inputPath;
            }

            // 譜面読み込み
            try
            {
                CMSLoader score = new CMSLoader(loadFilePath);
                UBMS_serializer.Package pack = score.LoadPackage(loadFilePath);

                // 譜面の展開・実行
                string dirPath = System.IO.Path.GetDirectoryName(loadFilePath);
                if (pack != null)
                {
                    this.execCMSX(dirPath, pack, 0);
                }
                else
                {
                    throw new System.Exception("ファイルの読み込みに失敗しました");
                }
            }
            catch //(System.Exception ex)
            {
                // TODO Message Boxの作成
                //MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                //NavigationService.NavigateAsync("MusicSelect", null, LoadSceneMode.Single).Subscribe();     // エラーの場合MusicSelectに戻る
            }
        }


        /// <summary>
        /// Update is called once per frame
        /// </summary>
        public virtual void Update()
        {
            // 曲の進行時間を更新
            TimeManager.Instance.Update();


            if (this.systemInput.GetButtonDown("HSUP"))
            {
                NoteBase.HighSpeed += CONST.DefApp.HIGH_SPEED_CHANGE_FREQ;
            }
            else if (this.systemInput.GetButtonDown("HSDN"))
            {
                NoteBase.HighSpeed -= CONST.DefApp.HIGH_SPEED_CHANGE_FREQ;
            }

            if (this.systemInput.GetButtonDown("AutoPlay"))
            {
                NoteBase.AutoTeacher = !NoteBase.AutoTeacher;
            }


            // 生成されているノート一覧を取得
            GameObject obj = GameObject.Find("TapNotes");
            GameObject[] childrens = obj.GetComponentsInChildren<Transform>(true)
                .Where(c => c != obj.transform)
                .Select(c => c.gameObject)
                .ToArray();

            // KeyPush判定の通知
            for (int i = 0; i < CONST.DefApp.LANE_NUM; i++)
            {
                if ((playerInput.GetButtonDown("Action" + (i * 2).ToString()))
                 || (playerInput.GetButtonDown("Action" + (i * 2 + 1).ToString())))
                {
                    // 判定対象のノートを探す
                    foreach (GameObject ob in childrens)
                    {
                        // ノートにKey押下情報を送る
                        if (ob.GetComponent<NoteBase>().KeyInputDown(i + 1))
                        {
                            break;  // 受理されたらこのKeyの判定を終了
                        }
                    }
                }
            }

            // KeyDown判定の通知
            bool[] keyHoldTable = new bool[CONST.DefApp.LANE_NUM];
            for (int i = 0; i < CONST.DefApp.LANE_NUM; i++)
            {
                if ((playerInput.GetButton("Action" + (i * 2).ToString()))
                 || (playerInput.GetButton("Action" + (i * 2 + 1).ToString())))
                {
                    keyHoldTable[i] = true;
                }
            }
            foreach (GameObject ob in childrens)
            {
                // ノートにKey押下情報を送る
                ob.GetComponent<NoteBase>().KeyInputHold(keyHoldTable);
            }

            // debug
            for (int i = -8; i < 8; ++i)
            {
                Debug.DrawLine(new Vector3(i, 0, 0), new Vector3(i, 0, 100), Color.gray);
            }
        }


        /// <summary>
        /// 判定の結果コールバック。ノートオブジェクトから発火する
        /// </summary>
        /// <param name="jt"></param>
        private void JudgeResult(EJudgeType jt)
        {
            if (jt != EJudgeType.Miss)
            {
                NoteBase.combo++;
            }
            else
            {
                NoteBase.combo = 0;
            }

            switch (jt)
            {
                case EJudgeType.JusticeCritical:    NoteBase.JusticeCritical++; break;
                case EJudgeType.Justice:            NoteBase.Justice++;         break;
                case EJudgeType.Attack:             NoteBase.Attack++;          break;
                case EJudgeType.Miss:               NoteBase.Miss++;            break;
            }

            this.judgeSprite.Request(jt, 8);    // TODO 位置は仮
        }


        public virtual void OnGUI()
        {
            this.drawDebugStatus();

            // コンボ数表示
            this.comboText.text = NoteBase.combo.ToString();

            // 空押し時のビーム表示
            for (int i = 0; i < CONST.DefApp.LANE_NUM; i++)
            {
                if((playerInput.GetButton("Action" + (i * 2).ToString()))
                 || (playerInput.GetButton("Action" + (i * 2+1).ToString())) )
                {
                    this.LaserSprite[i].SetActive(true);
                }
                else
                {
                    this.LaserSprite[i].SetActive(false);
                }
            }

            // 判定表示
            this.judgeSprite.Draw();
        }


        /// <summary>
        /// 譜面の展開・実行
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="pack"></param>
        /// <param name="index"></param>
        private void execCMSX(string dirPath, UBMS_serializer.Package pack, int index)
        {
            UBMS_serializer.ScoreStruct scr = pack.Scores[index];

            // テーブル読み込み
            this.se_manager.SetSETable(dirPath, pack.WAVTable);
            NoteBase.SetBPM((float)scr.BPM);

            // 時間順番にソート
            scr.Notes.Sort((x, y) => (int)(x.Timing[0].GetTiming_old() - y.Timing[0].GetTiming_old()));

            // コルーチンを実行
            StartCoroutine("createNote", scr);
        }


        // コルーチン  
        private IEnumerator createNote(UBMS_serializer.ScoreStruct scr)
        {
            // コルーチンの処理  
            // ノート生成
            foreach (UBMS_serializer.Note n in scr.Notes)
            {
                GameObject[] tagObjects = GameObject.FindGameObjectsWithTag(NoteBase.TAG_NAME);
                while (tagObjects.Length > CONST.DefApp.MAX_NOTE_NUM) {
                    // コルーチンの途中で中断して次のフレームで再開
                    yield return null;
                    tagObjects = GameObject.FindGameObjectsWithTag(NoteBase.TAG_NAME);
                }

                GameObject go;
                switch (n.NoteType)
                {
                    // TODO CubeよりPlaneの方が軽い？(要調査)

                    case UBMS_serializer.ENoteType.BGM:
                        // BGMノート
                        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        BGM bgm = go.AddComponent<BGM>();
                        bgm.Initialize(n);
                        go.transform.SetParent(this.BGMNotes.transform);
                        break;

                    case UBMS_serializer.ENoteType.Tap:
                    case UBMS_serializer.ENoteType.ExTap:
                        if (n.Timing.Count > 1)
                        {
                            if ((n.Timing.Count > 2 ) || (n.Timing[0].Position != n.Timing[1].Position))
                            {
                                // 2つ目以降のPositionにデータがある場合は Slide と判断する
                                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                Slide slide = go.AddComponent<Slide>();
                                slide.Initialize(n);
                                slide.JudgeCallback += JudgeResult;
                            }
                            else
                            {
                                // Hold と判断する
                                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                Hold hold = go.AddComponent<Hold>();
                                hold.Initialize(n);
                                hold.JudgeCallback += JudgeResult;
                            }
                        }
                        else
                        {
                            // 単体なので普通のTap
                            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            Tap tap = go.AddComponent<Tap>();
                            tap.Initialize(n);
                            tap.JudgeCallback += JudgeResult;
                        }
                        go.transform.SetParent(this.TapNotes.transform);
                        break;

                    case UBMS_serializer.ENoteType.Flick:
                        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        Flick flick = go.AddComponent<Flick>();
                        flick.Initialize(n);
                        flick.JudgeCallback += JudgeResult;
                        go.transform.SetParent(this.TapNotes.transform);
                        break;

                    default:
                        continue;
                }
            }
        }



        private void drawDebugStatus()
        {
            // make string with builder
            var text = new System.Text.StringBuilder();
            const float m_FontSizeBase = 15f;

            text.Append("HighSpeed : " + NoteBase.HighSpeed.ToString() + "\n");
            if (NoteBase.AutoTeacher) { text.Append("AutoMode  : " + NoteBase.AutoTeacher.ToString() + "\n"); }
            text.Append("JUSTICE CRITICAL : " + NoteBase.JusticeCritical.ToString()
                + "     JUSTICE : " + NoteBase.Justice.ToString()
                + "     ATTACK : " + NoteBase.Attack.ToString()
                + "     MISS : " + NoteBase.Miss.ToString()
                + "\n"
                );

            // draw
            int lineCount = text.ToString().ToList().Where(c => c.Equals('\n')).Count();

            // 左上に表示するように調整
            int textWidrh = 430;
            int left = 10;
            int top = 10;

            // 表示
            GUI.Box(new Rect(left, top, textWidrh - 5, (int)(m_FontSizeBase * lineCount) + 5), "");
            GUI.Label(new Rect(left + 5, top, Screen.width, Screen.height), text.ToString());
        }


        //-------------------------------------------------------------------------------------
        // Private member
        //-------------------------------------------------------------------------------------
        SEManager se_manager;
        Rewired.Player playerInput;
        Rewired.Player systemInput;
    }

}