using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace CMS.Note
{
    class Slide : NoteBase
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
                Texture2D texture = Resources.Load<Texture2D>(base.NoteTexturePath + "note_slide");
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
            var end_note = GameObject.CreatePrimitive(PrimitiveType.Cube).AddComponent<SlideDummy>();
            UBMS_serializer.Note sub_note = new UBMS_serializer.Note();
            sub_note.NoteType = UBMS_serializer.ENoteType.Null;
            sub_note.Width = note.Width;
            sub_note.Position = note.Position;
            sub_note.Timing = new System.Collections.Generic.List<UBMS_serializer.Time>();
            sub_note.Timing.Add(note.Timing.FindLast(x => true));
            end_note.Initialize(sub_note);

            // Hold
            GameObject obj = new GameObject();
            obj.AddComponent<MeshFilter>();
            obj.AddComponent<MeshRenderer>();
            effect_note = obj.AddComponent<SlideEffect>();
            effect_note.Initialize(note);
            effect_note.HoldSetup(note.Timing);

            // Hold中判定数をセット(で先頭のノートは含めない)
            this.judgeBeats = (int)((note.Timing.FindLast(x => true).GetTiming() - note.Timing[0].GetTiming()) * CONST.DefApp.JUDGE_LONGNOTE_INTERVAL);
            this.judgedBeats = 0;

        }
        SlideEffect effect_note;


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
            this.effect_note.HoldSetup(base.note_obj.Timing);
        }


        public override bool KeyInputDown(int keyPos)
        {
            bool retval = false;
            EJudgeType judge = EJudgeType.None;

            if (this.judgedBeats == 0)
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


        int notePosTimingCount = 0;
        float lastInputTimeMS = 0;
        public override void KeyInputHold(bool[] keyHoldTable)
        {
            if ((this.judgedBeats >= 0) && (this.judgedBeats < this.judgeBeats))
            {
                float judgeLine = base.note_obj.Timing[0].GetTiming() + (this.judgedBeats + 1) * (1f / CONST.DefApp.JUDGE_LONGNOTE_INTERVAL);

                // 入力チェック(前準備)
                if (base.note_obj.Timing[this.notePosTimingCount].GetTiming() <= TimeManager.Instance.MusicTimeLine)
                {
                    // MTL が notePosTimingCount と notePosTimingCount-1 の間に来るようにする
                    this.notePosTimingCount++;
                }
                // 入力チェック
                if ((base.note_obj.Timing[0].GetTiming() <= TimeManager.Instance.MusicTimeLine)
                 && (this.notePosTimingCount < base.note_obj.Timing.Count)
                ) {
                    // 前Timingを0%、次のTimingを100%として、現在時間の比率を出す
                    double prevTiming = base.note_obj.Timing[this.notePosTimingCount-1].GetTiming();
                    double nextTiming = base.note_obj.Timing[this.notePosTimingCount].GetTiming();
                    double nowTiming = TimeManager.Instance.MusicTimeLine;
                    double ratio = (nowTiming - prevTiming) / (nextTiming - prevTiming);

                    // 現在時間の判定位置を算出
                    int prevPos = base.note_obj.Timing[this.notePosTimingCount - 1].Position;
                    int nextPos = base.note_obj.Timing[this.notePosTimingCount].Position;
                    double nowPos = prevPos + (nextPos - prevPos) * ratio;
                    int toWidth = (int)Math.Ceiling(nowPos + base.note_obj.Width) - 1;    // Pos=1 + Width=1 → 2 になってしまうので -1 する  

                    // 入力更新 (Posは切り捨て、Widthは切り上げ)
                    for (int i = (int)nowPos; i <= toWidth; i++)
                    {
                        if (keyHoldTable[i - 1])  // Positionの値は1Baseなので-1する
                        {
                            lastInputTimeMS = TimeManager.Instance.GetMS();
                            break;
                        }
                    }
                }

                // 判定 (Holdと同じ)
                if (judgeLine <= TimeManager.Instance.MusicTimeLine)
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



    class SlideEffect : NoteBase
    {
        /// <summary>
        /// Use this for initialization
        /// </summary>
        void Start()
        {
            // リソースをロードしてインスタンス化(仮)
            Texture2D texture_start = Resources.Load<Texture2D>(TEXTURES_PATH + "default/note_slide_relay");
            base.gameObject.GetComponent<MeshRenderer>().material.mainTexture = texture_start;
        }


        public void HoldSetup(List<UBMS_serializer.Time> note_time)
        {
            //float end_pos = GetTimePos(Bpm, end) - GetTimePos(Bpm, start);
            float end_pos = note_time.FindLast(x => true).GetTiming() - note_time[0].GetTiming();

            float start_timing = note_time[0].GetTiming();
            float end_timing = note_time.FindLast(x => true).GetTiming();
            end_pos *= HighSpeed * 10;
            {
                Mesh mesh = new Mesh();

                // 頂点の指定
                System.Collections.Generic.List<Vector3> vert = new System.Collections.Generic.List<Vector3>();
                for (int i=0; i<note_time.Count; i++)
                {
                    float timing = (note_time[i].GetTiming() - start_timing) * HighSpeed * 10;
                    float pos = (base.note_obj.Position - note_time[i].Position) / base.note_obj.Width;


                    vert.Add(new Vector3(-0.5f - pos, 0, timing));
                    vert.Add(new Vector3(+0.5f - pos, 0, timing));
                }
                mesh.vertices = vert.ToArray();

                // UV座標の指定
                List<Vector2> uv = new List<Vector2>();
                for (int i = 0; i < note_time.Count; i++)
                {
                    float pos = (end_timing - note_time[i].GetTiming()) / (end_timing - start_timing);
                    uv.Add(new Vector2(0, pos));
                    uv.Add(new Vector2(1, pos));
                }
                mesh.uv = uv.ToArray();

                // 頂点インデックスの指定
                List<int> triangles = new List<int>();
                for (int i = 0; i < note_time.Count-1; i++)
                {
                    triangles.Add((i * 2) + 0);
                    triangles.Add((i * 2) + 2);
                    triangles.Add((i * 2) + 3);
                    triangles.Add((i * 2) + 0);
                    triangles.Add((i * 2) + 3);
                    triangles.Add((i * 2) + 1);
                }
                mesh.triangles = triangles.ToArray();

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


    class SlideDummy : NoteBase
    {
        /// <summary>
        /// Use this for initialization
        /// </summary>
        void Start()
        {
            // リソースをロードしてインスタンス化(仮)
            Texture2D texture_start = Resources.Load<Texture2D>(TEXTURES_PATH + "default/note_slide");
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
            // TODO throw new NotImplementedException();
            return false;
        }


        public override void KeyInputHold(bool[] keyHoldTable)
        {
            // No Action
        }
    }
}