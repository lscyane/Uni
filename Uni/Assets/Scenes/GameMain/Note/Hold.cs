using UnityEngine;
using System.Collections;
using System;

namespace CMS.Note
{
    class Hold : NoteBase
    {
        public event JudgeCallbackEventHandler JudgeCallback;

        private bool firstJudged = false;   // 先頭が判定済みかどうか
        private int judgedBeats;            // (先頭を除く)判定済みの判定数
        private int judgeBeats;             // (先頭を除く)判定数

        //-------------------------------------------------------------------------------------
        // Property （ゲーム内共有メンバ）
        //-------------------------------------------------------------------------------------


        //-------------------------------------------------------------------------------------
        // Method
        //-------------------------------------------------------------------------------------

        /// <summary>
        /// Use this for initialization
        /// </summary>
        void Start()
        {
            // リソースをロードしてインスタンス化(仮)
            if (this.note_obj.NoteType == UBMS_serializer.ENoteType.Tap)
            {
                //Texture2D texture = Resources.Load<Texture2D>(TEXTURES_PATH + "default/note_hold");
                //this.gameObject.GetComponent<Renderer>().material.mainTexture = texture;


                Texture2D texture = Resources.Load<Texture2D>(base.NoteTexturePath + "note_hold");
                this.gameObject.GetComponent<Renderer>().material.mainTexture = texture;
            }
            else
            {
                Texture2D texture = Resources.Load<Texture2D>(base.NoteTexturePath + "note_extap");
                this.gameObject.GetComponent<Renderer>().material.mainTexture = texture;
            }
        }


        public new void Initialize(UBMS_serializer.Note note)
        {
            // 共通処理
            base.Initialize(note);

            // 終端ノートの生成
            HoldDummy end_note = GameObject.CreatePrimitive(PrimitiveType.Cube).AddComponent<HoldDummy>();
            UBMS_serializer.Note sub_note = new UBMS_serializer.Note();
            sub_note.NoteType = UBMS_serializer.ENoteType.Null;
            sub_note.Width = note.Width;
            sub_note.Position = note.Position;
            sub_note.Timing = new System.Collections.Generic.List<UBMS_serializer.Time>();
            sub_note.Timing.Add(note.Timing[1]);
            end_note.Initialize(sub_note);

            // Hold
            GameObject obj = new GameObject();
            obj.AddComponent<MeshFilter>();
            obj.AddComponent<MeshRenderer>();
            effect_note = obj.AddComponent<HoldEffect>();
            effect_note.Initialize(note);
            effect_note.HoldSetup(note.Timing[0], note.Timing[1]);

            // Hold中判定数をセット(で先頭のノートは含めない)
            this.judgeBeats = (int)((note.Timing.FindLast(x => true).GetTiming() - note.Timing[0].GetTiming()) * CONST.DefApp.JUDGE_LONGNOTE_INTERVAL);
            this.judgedBeats = 0;
        }
        HoldEffect effect_note;


        /// <summary>
        /// Update is called once per frame
        /// </summary>
        new void Update()
        {
            // 共通処理
            base.Update();

            // Miss判定
            if (!this.firstJudged)
            {
                if (this.GetAtTimeMS() < -CONST.DefApp.JUDGE_RANGE_TAP_A)
                {
                    this.JudgeCallback(EJudgeType.Miss);
                    this.firstJudged = true;
                }
            }
        }


        /// <summary>
        /// HighSpeedが変更された時の処理
        /// </summary>
        protected override void OnHighSpeedChanged()
        {
            this.effect_note.HoldSetup(base.note_obj.Timing[0], base.note_obj.Timing[1]);    // メッシュを再描画
        }


        public override bool KeyInputDown(int keyPos)
        {
            bool retval = false;
            EJudgeType judge = EJudgeType.None;

            if (!this.firstJudged)
            {
                // タイミング判定
                float absAtTime = Math.Abs(this.GetAtTimeMS());
                if (this.note_obj.NoteType == UBMS_serializer.ENoteType.Tap)
                {
                    // Tapの判定
                    if (absAtTime < CONST.DefApp.JUDGE_RANGE_TAP_A)
                    {
                        judge = EJudgeType.Attack;
                    }
                    else if (absAtTime < CONST.DefApp.JUDGE_RANGE_TAP_J)
                    {
                        judge = EJudgeType.Justice;
                    }
                    else if (absAtTime < CONST.DefApp.JUDGE_RANGE_TAP_JC)
                    {
                        judge = EJudgeType.JusticeCritical;
                    }
                }
                else
                {
                    // ExTapの判定
                    if (absAtTime < CONST.DefApp.JUDGE_RANGE_TAP_A)
                    {
                        judge = EJudgeType.JusticeCritical;
                    }
                }

                // 判定による処理実行
                if (judge != EJudgeType.None)
                {
                    if (this.Hit(keyPos))
                    {
                        retval = true;
                        this.JudgeCallback(judge);
                        this.firstJudged = true;
                    }
                }
            }

            return retval;
        }


        float lastInputTimeMS = 0;
        public override void KeyInputHold(bool[] keyHoldTable)
        {
            if ((this.judgedBeats >= 0) && (this.judgedBeats < this.judgeBeats))
            {
                float judgeTiming = base.note_obj.Timing[0].GetTiming() + (this.judgedBeats + 1) * (1f / CONST.DefApp.JUDGE_LONGNOTE_INTERVAL);

                // 入力チェック
                for (int i=base.note_obj.Position; i < base.note_obj.Position + base.note_obj.Width; i++)
                {
                    if (keyHoldTable[i-1])  // Positionの値は1Baseなので-1する
                    {
                        lastInputTimeMS = TimeManager.Instance.GetMS();
                        break;
                    }
                }

                // 判定
                if (judgeTiming <= TimeManager.Instance.MusicTimeLine)
                {   
                    float atTime = TimeManager.Instance.GetMS() - lastInputTimeMS;  // 手放し経過時間
                    this.judgedBeats++;

                    if (atTime <= CONST.DefApp.JUDGE_RANGE_LONG_JC)
                    {
                        this.JudgeCallback(EJudgeType.JusticeCritical);
                    }
                    else if (atTime <= CONST.DefApp.JUDGE_RANGE_LONG_J)
                    {
                        this.JudgeCallback(EJudgeType.Justice);
                    }
                    else if (atTime <= CONST.DefApp.JUDGE_RANGE_LONG_A)
                    {
                        this.JudgeCallback(EJudgeType.Attack);
                    }
                    else
                    {
                        this.JudgeCallback(EJudgeType.Miss);
                    }
                }
            }

            // 全行程終了したので破棄
            if (this.judgedBeats >= this.judgeBeats)
            {
                this.DisableNote();
            }
        }


        //-------------------------------------------------------------------------------------
        // Member
        //-------------------------------------------------------------------------------------

    }


    class HoldEffect : NoteBase
    {
        /// <summary>
        /// Use this for initialization
        /// </summary>
        void Start()
        {
            // リソースをロードしてインスタンス化(仮)
            Texture2D texture_start = Resources.Load<Texture2D>(TEXTURES_PATH + "default/note_hold_relay");
            base.gameObject.GetComponent<MeshRenderer>().material.mainTexture = texture_start;
        }


        public void HoldSetup(UBMS_serializer.Time start, UBMS_serializer.Time end)
        {

            //float end_pos = GetTimePos(Bpm, end) - GetTimePos(Bpm, start);
            float end_pos = end.GetTiming() - start.GetTiming();
            end_pos *= HighSpeed * 10;
            {
                Mesh mesh = new Mesh();

                // 頂点の指定
                System.Collections.Generic.List<Vector3> vert = new System.Collections.Generic.List<Vector3>();
                vert.Add(new Vector3(-0.5f, 0, 0));
                vert.Add(new Vector3(-0.5f, 0, end_pos));
                vert.Add(new Vector3(+0.5f, 0, end_pos));
                vert.Add(new Vector3(+0.5f, 0, 0));
                mesh.vertices = vert.ToArray();

                // UV座標の指定
                mesh.uv = new Vector2[] {
                    new Vector2(0, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                    new Vector2(1, 0),
                };

                // 頂点インデックスの指定
                mesh.triangles = new int[] {
                    0, 1, 2,
                    0, 2, 3,
                };

                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                base.gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;

            }
        }


        /// <summary>
        /// Update is called once per frame
        /// </summary>
        public new void Update()
        {
            // 共通処理
            base.Update();
        }


        public override bool KeyInputDown(int keyPos)
        {
            // TODO throw new NotImplementedException();
            return false;
        }


        public override void KeyInputHold(bool[] keyHoldTable)
        {
            // TODO throw new NotImplementedException();
        }
    }


    class HoldDummy : NoteBase
    {
        /// <summary>
        /// Use this for initialization
        /// </summary>
        void Start()
        {
            // リソースをロードしてインスタンス化(仮)
            Texture2D texture_start = Resources.Load<Texture2D>(TEXTURES_PATH + "default/note_hold");
            base.gameObject.GetComponent<Renderer>().material.mainTexture = texture_start;
        }


        /// <summary>
        /// Update is called once per frame
        /// </summary>
        new void Update()
        {
            // 共通処理
            base.Update();
        }


        public override bool KeyInputDown(int keyPos)
        {
            // No Action
            return false;
        }


        public override void KeyInputHold(bool[] keyHoldTable)
        {
            // No Action
        }
    }
}