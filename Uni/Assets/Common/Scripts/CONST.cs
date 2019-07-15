using UnityEngine;

public class CONST
{
    public class DefApp
    {
        /// <summary> cms系ファイルを取得するための正規表現パターン </summary>
        public const string CMS_FILE_REGEX = @"\.(cmsx|cms|sus)$";

        /// <summary> ZIPから展開するファイルの正規表現パターン </summary>
        public const string ZIP_FILE_REGEX = @"\.(wav|wave|ogg|mp3|bme|cms|cmsx|sus)$";

        /// <summary> レーンの数 </summary>
        public const int LANE_NUM = 16;

        /// <summary> タッチ入力した時のレーザースプライトのサイズ </summary>
        public static readonly Vector2 LASER_SIZE = new Vector2(1f, 8f);

        /// <summary> ハイスピードの変更周期 </summary>
        public const float HIGH_SPEED_CHANGE_FREQ = 0.5f;

        /// <summary> 判定幅(Tap系) </summary>
        public const int JUDGE_RANGE_TAP_JC = 33;
        public const int JUDGE_RANGE_TAP_J = 66;
        public const int JUDGE_RANGE_TAP_A = 99;

        /// <summary> 判定幅(Air系) </summary>
        public const int JUDGE_RANGE_AIR_JC = 66;
        public const int JUDGE_RANGE_AIR_J = 99;
        public const int JUDGE_RANGE_AIR_A = 198;

        /// <summary> 判定幅(Hold/Slide) 判定時に手放してから経過した時間で判定 </summary>
        public const int JUDGE_RANGE_LONG_JC = JUDGE_RANGE_TAP_JC;
        public const int JUDGE_RANGE_LONG_J = JUDGE_RANGE_TAP_J;
        public const int JUDGE_RANGE_LONG_A = JUDGE_RANGE_TAP_A;

        /// <summary> 長押し系ノートの判定間隔 8=8分音符毎 </summary>
        public const int JUDGE_LONGNOTE_INTERVAL = 8;

        /// <summary> ゲーム内で生成するノート数の最大値 </summary>
        public const int MAX_NOTE_NUM = 200;

        /// <summary> 同時に表示する判定の最大スプライト数 </summary>
        public const int MAX_JUDGE_VIEW_NUM = 32;

        /// <summary> 強制的にノートを削除するタイミング(判定ライン基準[ms]) </summary>
        public const int NOTE_DELETE_TIMING = JUDGE_RANGE_AIR_A;

        /// <summary> 未指定の音量 </summary>
        public const float NOTE_DEFAULT_VOLUME = 1.0f;

        /// <summary> BGMノートの音量 </summary>
        public const float NOTE_BGM_VOLUME = 0.5f;


    }


    public class System
    {
        /// <summary> SEManagerのGameObject名 </summary>
        public const string SE_MANAGER_NAME = "SEManager";
    }
}
