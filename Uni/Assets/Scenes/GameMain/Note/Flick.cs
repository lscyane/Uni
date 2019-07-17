using UnityEngine;
using System.Collections;
using System;

namespace CMS.Note
{
    class Flick : NoteBase
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
            Texture2D texture = Resources.Load<Texture2D>(TEXTURES_PATH + "default/note_flick");
            this.gameObject.GetComponent<Renderer>().material.mainTexture = texture;
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


        //-------------------------------------------------------------------------------------
        // Member
        //-------------------------------------------------------------------------------------


    }
}