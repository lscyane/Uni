using UnityEngine;
using System.Collections;
using System;

namespace CMS.Note
{
    class Tap : NoteBase
    {
        public event JudgeCallbackEventHandler JudgeCallback;

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
                Texture2D texture = Resources.Load<Texture2D>(TEXTURES_PATH + "default/note_tap");
                this.gameObject.GetComponent<Renderer>().material.mainTexture = texture;
            }
            else
            {
                Texture2D texture = Resources.Load<Texture2D>(TEXTURES_PATH + "default/note_extap");
                this.gameObject.GetComponent<Renderer>().material.mainTexture = texture;
            }

            // 音無し時のデフォルトSE
            if (base.seId == 0)
            {
                base.seId = 10000;
            }
        }


        /// <summary>
        /// Update is called once per frame
        /// </summary>
        new void Update()
        {
            // 共通処理
            base.Update();

            // Miss判定
            if (this.GetAtTimeMS() < -CONST.DefApp.JUDGE_RANGE_TAP_A)
            {
                this.JudgeCallback(EJudgeType.Miss);
                this.DisableNote();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyPos"></param>
        /// <returns>入力を受け付けたかどうか</returns>
        public override bool KeyInputDown(int keyPos)
        {
            bool retval = false;
            EJudgeType judge = EJudgeType.None;

            // タイミング判定
            float absAtTime = Math.Abs(this.GetAtTimeMS());
            if (this.note_obj.NoteType == UBMS_serializer.ENoteType.Tap)
            {
                // Tapの判定
                if (absAtTime < CONST.DefApp.JUDGE_RANGE_TAP_JC)
                {
                    judge = EJudgeType.JusticeCritical;
                }
                else if (absAtTime < CONST.DefApp.JUDGE_RANGE_TAP_J)
                {
                    judge = EJudgeType.Justice;
                }
                else if (absAtTime < CONST.DefApp.JUDGE_RANGE_TAP_A)
                {
                    judge = EJudgeType.Attack;
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
                    base.DisableNote();
                }
            }

            return retval;
        }


        public override void KeyInputHold(bool[] keyHoldTable)
        {
            // No Action
        }


        //-------------------------------------------------------------------------------------
        // Member
        //-------------------------------------------------------------------------------------


    }
}