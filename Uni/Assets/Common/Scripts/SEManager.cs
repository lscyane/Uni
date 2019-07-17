using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SEManager : MonoBehaviour
{
    //-------------------------------------------------------------------------------------
    // Initialize for inspector （インスペクタで初期化するオブジェクト）
    //-------------------------------------------------------------------------------------


    //-------------------------------------------------------------------------------------
    // Public member & property
    //-------------------------------------------------------------------------------------



    //-------------------------------------------------------------------------------------
    // Method
    //-------------------------------------------------------------------------------------
    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start()
    {
        // 発声チャンネルをコンポーネントに追加
        this.audioSource = this.gameObject.AddComponent<AudioSource>();

        // 問題があった時のおまじない
        // -> ガベージコレクションされないように指定らしい。多分発声が少ない場合などにガベコレで履きだされると発声時にラグが出る？
        //GameObject.DontDestroyOnLoad(this.gameObject);

        loadSystemAudio(10000, "Audio/HandClap");
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="dirPath"></param>
    /// <param name="sdict"></param>
    public void SetSETable(string dirPath, SerializableDictionary<ushort, string> sdict)
    {
        // Dictionalyに登録
        foreach (System.Collections.Generic.KeyValuePair<ushort,string> pair in sdict)
        {
            string filePath = System.IO.Path.Combine(dirPath, pair.Value);
            this.SetSE(pair.Key, filePath);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="se_id"></param>
    /// <param name="filePath"></param>
    public void SetSE(ushort se_id, string filePath)
    {
        // ファイル存在チェック
        if (!System.IO.File.Exists(filePath))
        {
            Debug.LogWarning("[BMS Load] " + filePath + " の読み込みに失敗。ファイルが見つかりません。");
            return; // 見つからなければあきらめる
        }

        // コルーチンを実行  
        StartCoroutine(loadAudio(se_id, filePath));
    }


    private IEnumerator loadAudio(ushort se_id, string filePath)
    {
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file:///" + filePath, GetAudioType(filePath));
        yield return www.SendWebRequest();

        // 読み込んだファイルからAudioClipを取り出す
        AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
        while (clip.loadState == AudioDataLoadState.Loading)
        {
            yield return null;
        }
        if (clip.loadState != AudioDataLoadState.Loaded)
        {
            // どうも一部のoggで読み込みエラーになるっぽい？
            // TODO #37 AudioClipで一部のoggが読み込みエラーになる問題の対策

            // TODO Message Boxの作成
            //MessageBox.Show("AudioClipのLoadに失敗しました\n" + filePath, "OK", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            throw new System.Exception("AudioClipのLoadに失敗しました\n" + filePath);
        }

        // Dictionalyに登録
        if (!this.clipDict.ContainsKey(se_id))
        {
            this.clipDict.Add(se_id, clip);
        }

        yield break;
    }


    private AudioType GetAudioType(string file)
    {
        AudioType retval;
        string extention = System.IO.Path.GetExtension(file);
        switch (extention.ToLower())
        {
            case ".wav": retval = AudioType.WAV; break;
            case ".ogg": retval = AudioType.OGGVORBIS; break;
            default:retval = AudioType.UNKNOWN; break;
        }
        return retval;
    }


    private void loadSystemAudio(ulong se_id, string filePath)
    {
        // 読み込んだファイルからAudioClipを取り出す
        AudioClip clip = Resources.Load<AudioClip>(filePath);

        //clip.loadType = AudioClipLoadType.DecompressOnLoad;

        if (clip.loadState != AudioDataLoadState.Loaded)
        {
            // どうも一部のoggで読み込みエラーになるっぽい？
            // TODO #37 AudioClipで一部のoggが読み込みエラーになる問題の対策

            // TODO Message Boxの作成
            //MessageBox.Show("AudioClipのLoadに失敗しました\n" + filePath, "OK", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            throw new System.Exception("AudioClipのLoadに失敗しました\n" + filePath);
        }

        // Dictionalyに登録
        if (!this.clipDict.ContainsKey(se_id))
        {
            this.clipDict.Add(se_id, clip);
        }

        return;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="se_id"></param>
    public void PlaySE(ushort se_id, float vol = 1f)
    {
        if (this.clipDict.ContainsKey(se_id))
        {
            this.audioSource.PlayOneShot(this.clipDict[se_id], vol);
        }
    }


    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update () {
	
	}


    //-------------------------------------------------------------------------------------
    // Private member
    //-------------------------------------------------------------------------------------
    private AudioSource audioSource;
    private System.Collections.Generic.Dictionary<ulong, AudioClip> clipDict = new System.Collections.Generic.Dictionary<ulong, AudioClip>();
}


