using UnityEngine;
using System.Collections;
using Rewired;

namespace CMS.Note
{
    abstract class NoteBase : MonoBehaviour
    {
        protected const string TEXTURES_PATH = "Textures/NoteSkins/";

        protected string NoteTexturePath
        {
            get { return TEXTURES_PATH + CONST.DefApp.NOTE_SKIN_SELECT + "/"; }
        }

        /// <summary> オブジェクト取得用のタグ名(UnityEditorで設定) </summary>
        public const string TAG_NAME = "NoteObject";


        //-------------------------------------------------------------------------------------
        // Initialize for inspector （インスペクタで初期化するオブジェクト）
        //-------------------------------------------------------------------------------------


        //-------------------------------------------------------------------------------------
        // Property （ゲーム内共有メンバ）
        //-------------------------------------------------------------------------------------
        public static float HighSpeed { get; set; }
        private float oldHighSpeed;

        public static bool AutoTeacher { get; set; }

        public static int combo = 0;
        public static int JusticeCritical;
        public static int Justice;
        public static int Attack;
        public static int Miss;
        public delegate void JudgeCallbackEventHandler(EJudgeType jt);


        //-------------------------------------------------------------------------------------
        // Method
        //-------------------------------------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public void Initialize(UBMS_serializer.Note note)
        {
            this.tag = TAG_NAME;
            this.note_obj = note;
            this.seId = (ushort)note_obj.NoteValue;
            this.isAlive = true;

            this.NoteSetup(this.gameObject, this.note_obj.Width, this.note_obj.Position);

            if (se_manager == null)
            { 
                se_manager = GameObject.Find(CONST.System.SE_MANAGER_NAME).GetComponent(typeof(SEManager)) as SEManager;
                if (se_manager == null)
                {
                    throw new System.Exception("サウンド管理オブジェクトのインスタンスがありません");
                }
            }
        }


        protected void NoteSetup(GameObject obj, float width, float pos)
        {
            Renderer rend = obj.GetComponent<Renderer>() as Renderer;
            if (rend != null)
            {
                obj.transform.localScale = new Vector3(width, 0.001f, 1);
                obj.transform.position = new Vector3(GetLanePos(pos, width), 0, -500);  // 初期出現位置は適当な画面外
                obj.transform.rotation = new Quaternion(0, 0, 0, 0);
                //rend.material.shader = Shader.Find("Toon/Basic");
            }
        }


        public static void SetBPM(float bpm)
        {
            TimeManager.Instance.Bpm = bpm;
        }


        /// <summary>
        /// 呼称Posからノートのx座標を取得する
        /// </summary>
        /// <param name="pos">呼称Pos [1～16]</param>
        /// <param name="width">ノート幅 [1～16]</param>
        /// <returns></returns>
        private float GetLanePos(float pos, float width)
        {
            return pos - (CONST.DefApp.LANE_NUM / 2) + (width / 2) - 1;
        }


        /// <summary>
        /// 曲の進行時間からノートのy座標を取得する
        /// </summary>
        /// <returns></returns>
        //protected float GetTimePos(float t)
        //{
        //    return (t - MusicTimeLine) * HighSpeed / 100;
        //}
        //protected float GetTimePos(float bpm, UBMS_serializer.Time t)
        //{
        //    return GetTimePos(t.GetTiming_old(bpm));
        //}


        protected void HitNote()
        {
            // ノート種別による音量調整
            float vol = CONST.DefApp.NOTE_DEFAULT_VOLUME;
            if (this.note_obj.NoteType == UBMS_serializer.ENoteType.BGM)
            {
                vol = CONST.DefApp.NOTE_BGM_VOLUME;
            }

            // 発音
            se_manager.PlaySE(this.seId, vol);
        }


        protected void DisableNote()
        {
            this.isAlive = false;
            this.GetComponent<Renderer>().enabled = false;
            Destroy(this.gameObject);
        }


        /// <summary>
        /// Use this for initialization
        /// </summary>
        void Start()
        {
        }


        /// <summary>
        /// Update はフレームごとに一度呼び出されます。
        /// </summary>
        protected void Update()
        {
            float atTime = this.GetAtTime();

            // HighSpeedの変更を監視
            if (HighSpeed != oldHighSpeed)
            {
                oldHighSpeed = HighSpeed;
                this.OnHighSpeedChanged();
            }

            // [debug] Auto先生
            if ((AutoTeacher) && (atTime <= 0) && (this.isAlive))
            {
                this.HitNote();
            }

        }


        /// <summary>
        /// GUI イベントに応じて、フレームごとに複数回呼び出されます。
        /// </summary>
        protected void OnGUI()
        {
            if (isAlive)
            {
                // ノートの座標をセット
                Vector3 nowPos = this.gameObject.transform.position;
                nowPos.z = GetAtTime() * HighSpeed * 10;//GetTimePos(Bpm, this.note_obj.Timing[0]);
                this.gameObject.transform.position = nowPos;
            }
        }


        public bool Hit(int keyPos)
        {
            bool retval = false;
            if ((isAlive)
                && (this.note_obj.Position <= keyPos)
                && (keyPos < this.note_obj.Position + this.note_obj.Width))
            {
                HitNote();
                retval = true;
            }
            return retval;
        }


        public float GetAtTime(int num = 0)
        {
            return this.note_obj.Timing[num].GetTiming() - TimeManager.Instance.MusicTimeLine;
            //return this.note_obj.Timing[num].GetTiming_old(Bpm) - MusicTimeLine;
        }


        public float GetAtTimeMS(int num = 0)
        {
            return GetAtTime(num) * 4 * (60 / TimeManager.Instance.Bpm) * 1000;
        }


        /// <summary>
        /// Keyの押下。外部からの入力
        /// </summary>
        /// <param name="keyPos"></param>
        /// <returns></returns>
        public abstract bool KeyInputDown(int keyPos);


        /// <summary>
        /// 全Keyの押下状態。外部からの入力
        /// </summary>
        /// <param name="keyHoldTable"></param>
        public abstract void KeyInputHold(bool[] keyHoldTable);


        /// <summary>
        /// HighSpeedが変更された時。内部から発火
        /// </summary>
        protected virtual void OnHighSpeedChanged() { }


        //-------------------------------------------------------------------------------------
        // Member
        //-------------------------------------------------------------------------------------
        protected UBMS_serializer.Note note_obj;
        protected ushort seId;
        protected bool isAlive;

        protected static SEManager se_manager;
    }


    public enum EJudgeType
    {
        None,

        Miss,
        Attack,
        Justice,
        JusticeCritical,
    }
}