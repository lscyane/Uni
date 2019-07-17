using UnityEngine;
using System;
using System.Collections;
using CMS.Note;
using System.Collections.Generic;

namespace CMS
{
    public class CMSLoader
    {



        //-------------------------------------------------------------------------------------
        // Property
        //-------------------------------------------------------------------------------------
        float bpm;

        //-------------------------------------------------------------------------------------
        // Method
        //-------------------------------------------------------------------------------------

        /// <summary>
        /// コンストラクタ。譜面読み込み
        /// </summary>
        /// <param name="path">譜面ファイルのパス</param>
        public CMSLoader(string path, int scoreIndex = 0)
        {
        }

        public UBMS_serializer.Package LoadPackage(string path)
        { 
            UBMS_serializer.Package pack = null;

            // 種類別の展開読み込み処理(CMSX構造体形式に変換)
            string fullPath = System.IO.Path.GetFullPath(path);
            string file_ext = System.IO.Path.GetExtension(fullPath);
            switch (file_ext.ToLower())
            {
                case ".cms":
                    pack = this.loadCMS(fullPath);
                    break;

                case ".cmsx":
                    pack = this.loadCMSX(fullPath);
                    break;

                default:
                    throw new System.Exception("拡張子指定が不正です");
            }

            return pack;
        }


        /// <summary>
        /// 36進数文字列をdecimalに変換する
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private decimal decimal_ParseBase36(string str)
        {
            decimal retval = 0;
            str = str.ToLower();

            for (int digit = str.Length - 1; digit >= 0; digit--)
            {
                char c = str[digit];
                decimal val = 0;
                if (('a' <= c) && (c <= 'z'))
                {
                    val = c - 'a' + 10;
                }
                else
                {
                    val = c - '0';
                }

                retval += val * (decimal)System.Math.Pow(36, digit);
            }

            return retval;
        }


        /// <summary>
        /// CMS形式ファイルの解析・読み込み
        /// </summary>
        /// <param name="fullPath">ファイルの絶対パス</param>
        private UBMS_serializer.Package loadCMS(string fullPath)
        {
            var pack = new UBMS_serializer.Package();
            pack.Scores = new List<UBMS_serializer.ScoreStruct>();
            var score = new UBMS_serializer.ScoreStruct();
            var stopTable = new Dictionary<UInt16, string>();
            var bpmTable = new Dictionary<UInt16, string>();
            var chObjStack = new Dictionary<string, UBMS_serializer.Note>();

            // ※SJISはUnity非サポート
            System.IO.StreamReader sr = new System.IO.StreamReader(fullPath);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                string[] line_spt = line.Split(new char[] { ' ', '\t' });
                string val = (line_spt.Length >= 2 ? line.Remove(0, line.IndexOf(line_spt[1])) : "");   // ファイル名に空白がある時対策のために少し回りくどい事する

                // コメント処理
                if (line.IndexOf(";") >= 0)
                {
                    line = line.Remove(line.IndexOf(";"));
                }

                // コマンド行以外は無視
                if (line.IndexOf("#") != 0)
                {
                    continue;
                }

                // チャンネル文かどうかの判定
                int result = 0;
                if ((int.TryParse(line.Substring(1, 3), out result) && (line.IndexOf(':') == 6)))
                {
                    #region チャンネル文取得処理
                    string[] part = line.Split(':');
                    if (part.Length == 2)
                    {
                        // 小節番号(10進)
                        uint bar = uint.Parse(part[0].Substring(1, 3));
                        // チャンネル番号(16進)
                        int channel = int.Parse(part[0].Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                        // オブジェクト(36進)
                        string objectLine = part[1].Trim();

                        int size = objectLine.Length / 2;
                        for (int i = 0; i < size; ++i)
                        {
                            string noteValueStr = objectLine.Substring(i * 2, 2);
                            int noteObj = (int)this.decimal_ParseBase36(noteValueStr);

                            if (noteValueStr != "00")
                            {
                                // 共通データ格納
                                UBMS_serializer.Note note = new UBMS_serializer.Note();
                                UBMS_serializer.Time time = new UBMS_serializer.Time();
                                note.NoteValue = this.decimal_ParseBase36(noteValueStr);
                                time.Bar = bar;
                                time.Numerator = (byte)i;
                                time.Denominator = (byte)size;
                                note.Timing = new List<UBMS_serializer.Time>(new UBMS_serializer.Time[] { time });

                                // BGM
                                if (channel == 0x01)       
                                {
                                    note.NoteType = UBMS_serializer.ENoteType.BGM;
                                }
                                // #xxxの長さ(小節の短縮化、1 は 4/4 拍子に相当) [整数または小数を指定]
                                else if (channel == 0x02)   
                                {
                                    // TODO
                                    continue;
                                }
                                // BPM定義 (こっちはValueが直接 BPM 値として扱われるやつ)
                                else if (channel == 0x03)   
                                {
                                    note.NoteType = UBMS_serializer.ENoteType.BPM;
                                }
                                else if (channel == 0x05)
                                {
                                    // TODO ?
                                    // Extended Object  [#ExtChr] (BM98 のみ)
                                    // SEEK Object      [#SEEKxx n] (LR のみ)
                                    continue;
                                }
                                // exBPM
                                else if (channel == 0x08)   
                                {
                                    // [#BPMxx n | #EXBPMxx n] (実数)
                                    string bpm;
                                    note.NoteType = UBMS_serializer.ENoteType.BPM;
                                    if (bpmTable.TryGetValue((ushort)note.NoteValue, out bpm))
                                    {
                                        note.NoteValue = decimal.Parse(bpm);
                                    }
                                    else
                                    {
                                        continue;       // テーブルにヒットしなかったら無視
                                    }
                                }
                                // STOP
                                else if (channel == 0x09)   
                                {
                                    // [#STOPxx n] (1 は 192 分音符に相当)
                                    note.NoteType = UBMS_serializer.ENoteType.STOP;
                                    string stop;
                                    if (stopTable.TryGetValue((ushort)note.NoteValue, out stop))
                                    {
                                        note.NoteValue = decimal.Parse(stop);
                                    }
                                    else
                                    {
                                        continue;       // テーブルにヒットしなかったら無視
                                    }
                                }
                                // 表KeyCH
                                else if ((0x10 <= channel) && (channel <= 0x1F))
                                {
                                    byte type = byte.Parse(noteValueStr.Substring(0,1), System.Globalization.NumberStyles.HexNumber);
                                    byte width = (byte)this.decimal_ParseBase36(noteValueStr.Substring(1, 1));
                                    switch (type)
                                    {
                                        case 0x0: note.NoteType = UBMS_serializer.ENoteType.Tap; break;
                                        case 0x1: note.NoteType = UBMS_serializer.ENoteType.ExTap; break;
                                        case 0x2: note.NoteType = UBMS_serializer.ENoteType.Flick; break;
                                        case 0xA: note.NoteType = UBMS_serializer.ENoteType.LongMk; break;      // マーカーありHold系
                                        case 0xB: note.NoteType = UBMS_serializer.ENoteType.LongNonMk; break;   // マーカー無しHold系
                                    }
                                    time.Position = (byte)(channel - 0x10 + 1);
                                    note.Position = (byte)(channel - 0x10 + 1);
                                    note.Width = width;                             // ※Slideは移動先位置として扱う

                                    note.NoteValue = 0;
                                }
                                // 裏KeyCH
                                else if ((0x30 <= channel) && (channel <= 0x3F))
                                {
                                    byte type = byte.Parse(noteValueStr.Substring(0, 1), System.Globalization.NumberStyles.HexNumber);
                                    byte width = (byte)this.decimal_ParseBase36(noteValueStr.Substring(1, 1));
                                    switch (type)
                                    {
                                        case 0x0: note.NoteType = UBMS_serializer.ENoteType.Air; break;
                                        case 0x1: note.NoteType = UBMS_serializer.ENoteType.Fall; break;
                                        case 0xA: note.NoteType = UBMS_serializer.ENoteType.AirRide; break;
                                    }
                                    note.Position = (byte)(channel - 0x30 + 1);
                                    note.Width = width;

                                    note.NoteValue = 0;
                                }
                                // BGA関係のチャンネル
                                else if (((0x04 == channel))
                                    || ((0x06 <= channel) && (channel <= 0x07))
                                    || ((0x0A <= channel) && (channel <= 0x0E))
                                    || ((0xA1 <= channel) && (channel <= 0xA5))
                                ) {
                                    // とりあえずチャンネルをPositionに入れとく
                                    note.NoteType = UBMS_serializer.ENoteType.BMP;
                                    note.Position = (byte)channel;
                                }
                                // 指定以外はとりあえず無視する
                                else
                                {
                                    continue;
                                }

                                // ノート追加
                                score.Notes.Add(note);
                            }
                        }

                    }
                    #endregion
                }
                else
                {
                    #region ヘッダー文取得処理
                    string header = line_spt[0];
                    switch (header)
                    {
                        case "#ARTIST":
                            score.Artist = val;
                            break;

                        case "#BPM":
                            score.BPM = decimal.Parse(val);
                            break;

                        case "#TITLE":
                            score.Title = val;
                            break;

                        case "#PLAYLEVEL":
                            score.Difficulty = byte.Parse(val);
                            break;

                        case "#RANK":
                            score.Difficulty = byte.Parse(val);
                            break;

                        case "#STAGEFILE":
                            score.Background = val;
                            break;
                    }

                    // WAVxx
                    if (line.IndexOf("#WAV") >= 0)
                    {
                        // 識別番号を取得
                        UInt16 wavNum = (UInt16)this.decimal_ParseBase36(line.Substring(4, 2));

                        // すでに登録されているかをCheck
                        if (pack.WAVTable.ContainsKey(wavNum))
                        {
                            System.Diagnostics.Debugger.Break();
                            throw new System.FormatException();
                        }

                        pack.WAVTable.Add(wavNum, val);
                        continue;
                    }

                    // BMPxx
                    if (line.IndexOf("#BMP") >= 0)
                    {
                        // 識別番号を取得
                        UInt16 bmpNum = (UInt16)this.decimal_ParseBase36(line.Substring(4, 2));

                        // すでに登録されているかをCheck
                        if (pack.BMPTable.ContainsKey(bmpNum))
                        {
                            System.Diagnostics.Debugger.Break();
                            throw new System.FormatException();
                        }

                        pack.BMPTable.Add(bmpNum, val);
                        continue;
                    }

                    // STOPxx (STOPテーブル定義)
                    if (line.IndexOf("#STOP") >= 0)
                    {
                        // 識別番号を取得
                        UInt16 stopNum = (UInt16)this.decimal_ParseBase36(line.Substring(4, 2));

                        // すでに登録されているかをCheck
                        if (stopTable.ContainsKey(stopNum))
                        {
                            System.Diagnostics.Debugger.Break();
                            throw new System.FormatException();
                        }

                        stopTable.Add(stopNum, val);
                        continue;
                    }

                    // BPMxx (BPMテーブル定義 (0x08 ExBPM用)) (cmsxにテーブルは保存しない。生値を持つのでバッファ用のテーブルに置く)
                    if (line.IndexOf("#EXBPM") >= 0)
                    {
                        // 識別番号を取得
                        string lin = line.Remove(0, line.IndexOf("M") + 1).Trim();
                        UInt16 bpmNum = (UInt16)this.decimal_ParseBase36(lin);

                        // すでに登録されているかをCheck (BPM と EXBPM が衝突した場合は知らん、同時には使えないとする)
                        if (bpmTable.ContainsKey(bpmNum))
                        {
                            System.Diagnostics.Debugger.Break();
                            throw new System.FormatException();
                        }

                        bpmTable.Add(bpmNum, val);
                        continue;
                    }
                    #endregion
                }
            }

            // 時間順番にソート
            score.Notes.Sort((x, y) => (int)(x.Timing[0].GetTiming_old() - y.Timing[0].GetTiming_old()));

            // 仮置きしたLongNote系の結合
            for (int i = score.Notes.Count - 1; 0 < i; --i)
            {
                UBMS_serializer.Note nt = score.Notes[i];

                // Long定義があった時
                if ((nt.NoteType == UBMS_serializer.ENoteType.LongMk)
                 || (nt.NoteType == UBMS_serializer.ENoteType.LongNonMk)
                 ) {
                    // 繋がる根本のTap系ノートを探す
                    UBMS_serializer.Note baseNote = score.Notes.FindLast(x => { // 後ろから順に探す
                        return ((x.Timing[0] < nt.Timing[0])                    // 時間が早い方 
                             && (x.Position == nt.Position)                     // 同じ位置 (ここでのPsitionはチャンネルを示す)
                             && (x.IsTapBase())                                 // Tap系ノート
                        );
                    });

                    // 見つかった時
                    if (baseNote != null)
                    {
                        nt.Timing[0].Position = nt.Width;

                        baseNote.Timing.InsertRange(1, nt.Timing);
                        score.Notes.Remove(nt);
                    }










                }
            }

            pack.Scores.Add(score);
            return pack;
        }


        /// <summary>
        /// CMSX形式ファイルの解析・読み込み
        /// </summary>
        /// <param name="fullPath">ファイルの絶対パス</param>
        private UBMS_serializer.Package loadCMSX(string fullPath)
        {
            // CMSXは管理構造体をシリアライズしているだけなのでそのままデシリアライズすればOK
            UBMS_serializer.Package pack = UBMS_serializer.Serializer.LoadFile(fullPath);
            if (pack == null)
            {
                throw new System.Exception("ファイルの読み込みに失敗しました");
            }
            return pack;
        }


        //-------------------------------------------------------------------------------------
        // Member
        //-------------------------------------------------------------------------------------

    }
}