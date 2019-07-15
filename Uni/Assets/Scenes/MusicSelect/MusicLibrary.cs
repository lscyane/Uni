using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;


public class MusicLibrary
{
	public List<GroupFolder> group = new List<GroupFolder>();

	public MusicLibrary(string songsFolderPath)
	{
		// まずはグループフォルダ一覧を作成
		string[] dirs = System.IO.Directory.GetDirectories(Application.dataPath + songsFolderPath);
		foreach (string dir in dirs)
		{
			GroupFolder gf = new GroupFolder(dir);
			this.group.Add(gf);
		}
	}



	public class GroupFolder
	{
		public string Path { get; private set; }
		public string Name { get; private set; }
		public List<Music> musics = new List<Music>();

        /// <summary>
        /// グループ毎に曲ファイルを検索
        /// </summary>
        /// <param name="dir"></param>
		public GroupFolder(string dir)
		{
			this.Path = dir;
			this.Name = System.IO.Path.GetFileName(dir);

#if ENABLE_ZIP_MUSIC
            // zipファイル形式の曲ファイルを検索
            string[] files = System.IO.Directory.GetFiles(dir, "*.zip", System.IO.SearchOption.TopDirectoryOnly);
			foreach (string file in files)
			{
				Music[] m = Music.TryLoadZip(file);
				if (m.Length > 0)
				{
					this.musics.AddRange(m);
				}
			}
#endif
            // フォルダ展開状態の曲ファイルを検索
			string[] folders = System.IO.Directory.GetDirectories(dir);
			foreach (string folder in folders)
			{
				Music[] m = Music.TryLoadFolder(folder);
				if (m.Length > 0)
				{
					this.musics.AddRange(m);
				}
			}
		}

	}


	public class Music
	{
		public string Path;
		public string Name;

#if ENABLE_ZIP_MUSIC
		public static Music[] TryLoadZip(string path)
		{
			List<Music> retval = new List<Music>();
			using (ZipFile zip = ZipFile.Read(path))
			{
				zip.ExtractExistingFile = ExtractExistingFileAction.OverwriteSilently;
				foreach (ZipEntry entry in zip)
				{
					if (Regex.IsMatch(entry.FileName, CONST.DefApp.CMS_FILE_REGEX))
					{
						Music m = new Music();
						m.Path = path;              // zipファイルパスを格納
						m.Name = entry.FileName;    // TODO cmsx内記述のTITLE入れたい
						retval.Add(m);
					}
				}
			}
			return retval.ToArray();
		}
#endif


		public static Music[] TryLoadFolder(string path)
		{
			List<Music> retval = new List<Music>();
			string[] files = System.IO.Directory.GetFiles(path, "*", System.IO.SearchOption.TopDirectoryOnly);   // path パラメーターでは大文字と小文字が区別されません。 みたい
            foreach (string file in files)
			{
                if (Regex.IsMatch(file, CONST.DefApp.CMS_FILE_REGEX))
                {
                    Music m = new Music();
                    m.Path = file;          // 一応フルパスで入る
                    m.Name = System.IO.Path.GetFileName(file);  // TODO cmsx内記述のTITLE入れたい
                    retval.Add(m);
                }
			}
			return retval.ToArray();
		}
	}
}