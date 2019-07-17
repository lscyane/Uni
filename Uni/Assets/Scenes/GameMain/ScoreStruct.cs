using System;
using System.Collections;
using System.Collections.Generic;

using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;

namespace UBMS_serializer
{
    public enum ENoteType
    {
        Null = 0,

        Tap,
        ExTap,
        Air,
        AirAction,
        Fall,
        Flick,

        LongMk,
        LongNonMk,
        AirRide,

        reserve10,
        reserve11,
        reserve12,
        reserve13,
        reserve14,
        reserve15,

        BGM,
        BMP,
        BPM,
        STOP,
    }


    public enum EDifficultyType
    {
        BASIC,
        ADVANCED,
        EXPERT,
        MASTER,
        reserve4,
        reserve5,
        reserve6,
        reserve7,
        reserve8,
        reserve9,
        EDIT,
        DIVIDE,     // 割
        STOP,       // 止
        SHINE,      // 光
        TWIN,       // 両
        BOUNCE,     // 跳
        HALF,       // 半
        FLICK,      // 弾
        MAD,        // 狂
        RETURN,     // 戻
        REMODELING, // 改
        APRILFOOL,  // 嘘
        THEWORLD,   // 時
    }


    public class Time
    {
#if !DEBUG
        [XmlElement("B")]
#endif
        public UInt32 Bar;                      // 小節番号

#if !DEBUG
        [XmlElement("D")]
#endif
        public Byte Denominator;                // 分母

#if !DEBUG
        [XmlElement("N")]
#endif
        public Byte Numerator;                  // 分子

        // 以下、Slide用の情報。開始時と他のノートは無視する
#if !DEBUG
        [XmlElement("P")]    
#endif
        [DefaultValue(0)]                       // 値が DefaultValue の場合は、XML には出力しない
        public Byte Position;                   // 1～16

#if !DEBUG
        [XmlElement("T")]
#endif
        [DefaultValue(ENoteType.Null)]          // 値が DefaultValue の場合は、XML には出力しない
        public ENoteType NoteType;              // 今の所Null or Slide のみ、他はNullとして扱う


        /// <summary>
        /// BPMと拍位置からノートのタイミング[ms]を算出する
        /// </summary>
        /// <param name="bpm"></param>
        /// <returns>ノートのタイミング[ms]</returns>
        public float GetTiming_old(float bpm = 1)
        {
            uint bar = this.Bar;
            byte num = this.Numerator;
            byte den = this.Denominator;
            float time = 0;
            float barTime = 60000 * 4 / (float)bpm;
            if (num != 0)
            {
                time = bar * barTime + barTime * num / den;
            }
            else {
                time = bar * barTime;
            }
            return time;
        }


        /// <summary>
        /// ノートのタイミング(小節単位)を取得する
        /// </summary>
        /// <returns></returns>
        public float GetTiming()
        {
            float retval = 0;
            retval += this.Bar;
            retval += (float)this.Numerator / (float)this.Denominator;
            return retval;
        }


        #region 比較演算子のローバーロード
        public static bool operator < (Time t1, Time t2)
        {
            return (t1.GetTiming_old() < t2.GetTiming_old());
        }

        public static bool operator >(Time t1, Time t2)
        {
            return (t1.GetTiming_old() > t2.GetTiming_old());
        }

        public static bool operator <=(Time t1, Time t2)
        {
            return (t1.GetTiming_old() <= t2.GetTiming_old());
        }

        public static bool operator >=(Time t1, Time t2)
        {
            return (t1.GetTiming_old() >= t2.GetTiming_old());
        }

        public static bool operator ==(Time t1, Time t2)
        {
            return (t1.GetTiming_old() == t2.GetTiming_old());
        }

        public static bool operator !=(Time t1, Time t2)
        {
            return (t1.GetTiming_old() != t2.GetTiming_old());
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            Time p = obj as Time;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (Bar == p.Bar)
                && (Denominator == p.Denominator)
                && (Numerator == p.Numerator)
                && (Position == p.Position)
                && (NoteType == p.NoteType);
        }

        public override int GetHashCode()
        {
            return (int)Bar ^ Denominator ^ Numerator ^ Position ^ (int)NoteType;
        }
        #endregion
    }

    public class Note
    {
#if !DEBUG
        [XmlElement("N")]
#endif
        public ENoteType NoteType;

#if !DEBUG
        [XmlElement("P")]
#endif
        [DefaultValue(0)]                       // 値が DefaultValue の場合は、XML には出力しない
        public Byte Position;                   // 1～16

#if !DEBUG
        [XmlElement("W")]
#endif
        [DefaultValue(0)]                       // 値が DefaultValue の場合は、XML には出力しない
        public Byte Width;                      // 1～16

#if !DEBUG
        [XmlElement("V")]
#endif
        [DefaultValue(0)]
        public Decimal NoteValue;               // NoteType に対応した値 (Int値の場合は少数以下切り捨て)

#if !DEBUG
        [XmlElement("T")]
#endif
        public List<Time> Timing;               // [0]:開始(単ノートは1つのみ)    [1以降]:中間、又は終了
                                                // Airの時は、Count>1 でAirAction
                                                // Tap/ExTapの時は、同一PositionかつCount=2でHold、それ以外はSlideとして判定
                                                // Slideの時だけ、TimeクラスのPosとTypeを見る

        public Note()
        {
            this.Timing = new List<Time>();
        }


        public bool IsTapBase()
        {
            return ((this.NoteType == ENoteType.ExTap)
                 || (this.NoteType == ENoteType.Tap) );
        }


        public bool IsAirBase()
        {
            return ((this.NoteType == ENoteType.Air));
        }
    }


    public class ScoreStruct
    {
        private const Byte formatRev = 0;       // 0:開発用    1～:リリース版

        public String Title;                    // タイトル
        public String Artist;                   // アーティスト名
        public String Banner;                   // バナーファイル名
        public String NoteDesigner;             // 譜面作者名
        public EDifficultyType DifficultyType;  // 難易度種別
        public Byte Difficulty;                 // 難易度

        public String Background;               // 背景ファイル名(.png/.jpg/.mp4)
        public String SampleMusic;              // 曲サムネファイル名

        public Decimal BPM;
        public List<Note> Notes;

        public ScoreStruct()
        {
            this.Notes = new List<Note>();
        }
    }

    public class Package
    {
        public List<ScoreStruct> Scores;

        public SerializableDictionary<UInt16, String> WAVTable;     // TableIndex, ファイル名
        public SerializableDictionary<UInt16, String> BMPTable;     // TableIndex, ファイル名

        public Package()
        {
            this.Scores = new List<ScoreStruct>();
            this.WAVTable = new SerializableDictionary<ushort, string>();
            this.BMPTable = new SerializableDictionary<ushort, string>();
        }
    }


    public class Serializer
    {

        public static Package LoadFile(String path)
        {
            // XmlDeserialize
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Package));        // オブジェクトの型を指定する
            System.IO.StreamReader sr = new System.IO.StreamReader(path, new System.Text.UTF8Encoding(false));                      // 読み込むファイルを開く
            Package obj = serializer.Deserialize(sr) as Package;    // XMLファイルから読み込み、逆シリアル化する
            sr.Close();                                             // ファイルを閉じる
            return obj;
        }


        public static Boolean SaveFile(String path, Package pack)
        {
            Boolean retval = false;

            if (pack.Scores.Count != 0)
            {
                // XmlSerialize
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Package));    // オブジェクトの型を指定する
                System.IO.StreamWriter sw = new System.IO.StreamWriter(path, false, new System.Text.UTF8Encoding(false));           // 書き込むファイルを開く（UTF-8 BOM無し）
                serializer.Serialize(sw, pack);     //シリアル化し、XMLファイルに保存する
                sw.Close();                         //ファイルを閉じる
            }

            return retval;
        }
    }
}

#region // Utility
/// <summary>
/// XMLシリアル化ができるDictionaryクラス
/// </summary>
/// <typeparam name="TKey">キーの型</typeparam>
/// <typeparam name="TValue">値の型</typeparam>
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
{
    //nullを返す
    public System.Xml.Schema.XmlSchema GetSchema()
    {
        return null;
    }

    //XMLを読み込んで復元する
    public void ReadXml(XmlReader reader)
    {
        bool wasEmpty = reader.IsEmptyElement;
        reader.Read();
        if (wasEmpty)
            return;

        //XmlSerializerを用意する
        XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
        XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

        while (reader.NodeType != XmlNodeType.EndElement)
        {
            reader.ReadStartElement("KeyValuePair");

            //キーを逆シリアル化する
            reader.ReadStartElement("Key");
            TKey key = (TKey)keySerializer.Deserialize(reader);
            reader.ReadEndElement();

            //値を逆シリアル化する
            reader.ReadStartElement("Value");
            TValue val = (TValue)valueSerializer.Deserialize(reader);
            reader.ReadEndElement();

            reader.ReadEndElement();

            //コレクションに追加する
            this.Add(key, val);

            //次へ
            reader.MoveToContent();
        }

        reader.ReadEndElement();
    }

    //現在の内容をXMLに書き込む
    public void WriteXml(XmlWriter writer)
    {
        //XmlSerializerを用意する
        XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
        XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

        foreach (TKey key in this.Keys)
        {
            writer.WriteStartElement("KeyValuePair");

            //キーを書き込む
            writer.WriteStartElement("Key");
            keySerializer.Serialize(writer, key);
            writer.WriteEndElement();

            //値を書き込む
            writer.WriteStartElement("Value");
            valueSerializer.Serialize(writer, this[key]);
            writer.WriteEndElement();

            writer.WriteEndElement();
        }
    }
}
#endregion