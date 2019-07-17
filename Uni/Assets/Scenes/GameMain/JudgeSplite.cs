using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeSplite
{
    GameObject[] JudgeSprite = new GameObject[CONST.DefApp.MAX_JUDGE_VIEW_NUM];

    public JudgeSplite()
    {
        // リソースをロードしてインスタンス化(仮)
        for (int i = 0; i < JudgeSprite.Length; i++)
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
            this.JudgeSprite[i] = go;
        }
    }


    public void Request(CMS.Note.EJudgeType jt, int position)
    {

    }


    public void Draw()
    {

    }
}


class JudgeViewQueue
{
    CMS.Note.EJudgeType type;
    int position;
}