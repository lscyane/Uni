using UnityEngine;
using System.Collections;
using System;

namespace CMS.Note
{
    class BGM : NoteBase
    {
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
            Texture2D texture = Resources.Load<Texture2D>(TEXTURES_PATH + "default/note_extap");
            this.gameObject.GetComponent<Renderer>().material.mainTexture = texture;


        }


        /// <summary>
        /// Update is called once per frame
        /// </summary>
        new void Update()
        {
            // 共通処理
            base.Update();

            if ((base.GetAtTime() <= 0) && (this.isAlive))
            {
                base.HitNote();
                base.DisableNote();
            }
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


        //-------------------------------------------------------------------------------------
        // Member
        //-------------------------------------------------------------------------------------

    }
}