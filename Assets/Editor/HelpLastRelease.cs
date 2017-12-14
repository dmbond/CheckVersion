using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using Debug = UnityEngine.Debug;

public class HelpLastRelease : EditorWindow {

	const string statsUrl = @"http://hwstats.unity3d.com/index.html";
	const string experimenalUrl = @"http://unity3d.com/experimental";
	const string roadmapUrl = @"http://unity3d.com/unity/roadmap";

	const string archiveUrl = @"http://unity3d.com/get-unity/download/archive";
	const string betaArchiveUrl = @"http://unity3d.com/unity/beta/archive";
	const string releaseUrl = @"http://beta.unity3d.com/download/{0}/download.html";

	const string searchUrl = @"http://unity3d.com/search";
	const string searchGitHubUrl = @"http://unitylist.com";

	const string assistantUrl = @"http://beta.unity3d.com/download/{0}/UnityDownloadAssistant-{1}.{2}";
	const string serverUrl = @"http://symbolserver.unity3d.com/";
	const string historyUrl = serverUrl + @"000Admin/history.txt";

	const string finalRN = @"http://unity3d.com/unity/whats-new/unity-";
	const string betaRN = @"http://unity3d.com/unity/beta/unity";
	const string patchRN = @"http://unity3d.com/unity/qa/patch-releases/";

	const string githubUrl = @"http://api.github.com/repos/dmbond/CheckVersion/releases/latest";

	static readonly string zipName = Application.platform == RuntimePlatform.WindowsEditor ? "7z" : "7za";
	const string baseName = "UnityYAMLMerge.ex";
	const string compressedName = baseName + "_";
	const string extractedName = baseName + "e";
	static string tempDir;

	static WWW wwwHistory, wwwList, wwwMerger, wwwAssistant;
	static WWW wwwGithub, wwwPackage;

	static SortedList<string, string> fullList;
	static SortedList<string, string> sortedList;
	static SortedList<string, string> currentList;

	static int idxSelectedInCurrent;
	static bool isAssistant;

	static HelpLastRelease window;
	const string wndTitle = "Unity Builds";
	const string scriptName = "HelpLastRelease";
	const string prefs = scriptName + ".";
	const string prefsCount = prefs + "count";
	// if you do not need autoupdate script from Github set to false
	static bool autoUpdate = true;

	static string filterString = "";
	static GUIStyle btnStyle;
	const float minWidth = 160f;
	static Vector2 scrollPos;

	const string rnTooltip = "Open Release Notes";
	const string assistTooltip = "Open Download Assistant";
	const string versionTooltip = "Open Download Page";

	static Dictionary<string, Color> colors = new Dictionary<string, Color>() {
		{ "5.5.", Color.magenta },
		{ "5.6.", Color.blue },
		{ "2017.1.", Color.cyan },
		{ "2017.2.", Color.green },
		{ "2017.3.", Color.yellow },
		{ "2018.1.", Color.red }
	};
	static float alphaBackForPersonal = 0.3f;

	#region Menu

	[MenuItem("Help/Links/Last Release...", false, 900)]
	static void Init() {
		window = GetWindow<HelpLastRelease>(wndTitle);
	}
	// ---

	[MenuItem("Help/Links/Search...", false, 920)]
	static void OpenSearch() {
		Application.OpenURL(searchUrl);
	}

	[MenuItem("Help/Links/Search GitHub...", false, 925)]
	static void OpenAwesome() {
		Application.OpenURL(searchGitHubUrl);
	}
	// ---

	[MenuItem("Help/Links/Archive...", false, 940)]
	static void OpenArchive() {
		Application.OpenURL(archiveUrl);
	}

	[MenuItem("Help/Links/Beta Archive...", false, 945)]
	static void OpenBetaArchive() {
		Application.OpenURL(betaArchiveUrl);
	}

	[MenuItem("Help/Links/Patch Archive...", false, 948)]
	static void OpenPatchArchive() {
		Application.OpenURL(patchRN);
	}
	// ---

	[MenuItem("Help/Links/Roadmap...", false, 960)]
	static void OpenRoadmap() {
		Application.OpenURL(roadmapUrl);
	}

	[MenuItem("Help/Links/Experimental...", false, 965)]
	static void OpenExperimental() {
		Application.OpenURL(experimenalUrl);
	}

	[MenuItem("Help/Links/Statistics...", false, 970)]
	static void OpenStatistics() {
		Application.OpenURL(statsUrl);
	}
	// ---

	[MenuItem("Help/Links/Update", false, 990)]
	static void UpdateFromGithub() {
		if (Application.internetReachability == NetworkReachability.NotReachable) {
			Debug.LogWarningFormat("Can't update {0} from Github, network is not reachable", scriptName);
		} else {
			DownloadGithub();
		}
	}

	#endregion

	void OnGUI() {
		if (fullList != null) {
			ListGUI();
		} else WaitGUI();
	}

	public static void ListGUI() {
		btnStyle = new GUIStyle(EditorStyles.miniButton);
		btnStyle.alignment = TextAnchor.MiddleLeft;
		SearchGUI();
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
		if (currentList == null) currentList = fullList;
		for (int i = currentList.Count - 1; i >= 0; i--) {
			GUILayout.BeginHorizontal();
			ColorGUI(i);
			if (GUILayout.Button(new GUIContent("RN", rnTooltip), btnStyle)) {
				OpenReleaseNotes(i);
			}
			if (GUILayout.Button(new GUIContent("A", assistTooltip), btnStyle)) {
				DownloadList(i, true);
			}
			if (GUILayout.Button(new GUIContent(currentList.Values[i], versionTooltip), btnStyle, GUILayout.MinWidth(minWidth))) {
				DownloadList(i);
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		EditorGUILayout.EndScrollView();
		GUILayout.FlexibleSpace();
		ProgressGUI(wwwAssistant, "Downloading Assistant");
		ProgressGUI(wwwHistory, "Downloading History");
		ProgressGUI(wwwList, "Downloading List");
		ProgressGUI(wwwMerger, "Downloading Merger");
	}

	static void ColorGUI(int i) {
		Color alpha = new Color(1f, 1f, 1f, alphaBackForPersonal);
		foreach (var k in colors.Keys) {
			bool isColored = currentList.Values[i].Contains(k);
			if (EditorGUIUtility.isProSkin) {
				GUI.contentColor = isColored ? colors[k] : Color.white;
			} else {
				GUI.backgroundColor = isColored ? colors[k] * alpha : Color.white * alpha;
			}
			if (isColored) break;
		}
	}

	void OnEnable() {
		tempDir = SetTempDir();
		DownloadHistory();
	}

	[InitializeOnLoadMethod]
	static void AutoUpdate() {
		if (autoUpdate && Application.internetReachability != NetworkReachability.NotReachable) {
			DownloadGithub();
		}
	}

	static string SetTempDir() {
		string result = string.Format("{0}/../Temp/{1}", Application.dataPath, scriptName);
		if (!Directory.Exists(result)) {
			Directory.CreateDirectory(result);
		}
		return result;
	}

	static void OpenReleaseNotes(int num) {
		string url = "", version = "";
		if (currentList.Values[num].Contains("a")) {
			Debug.LogWarningFormat("Release Notes for alpha version {0} are not available", currentList.Values[num]);
			EditorApplication.Beep();
			return;
		}
		if (currentList.Values[num].Contains("p")) {
			version = currentList.Values[num].Split(' ')[0];
			url = patchRN + version;
		}
		if (currentList.Values[num].Contains("f")) {
			version = currentList.Values[num].Split('f')[0];
			url = finalRN + version;
		}
		if (currentList.Values[num].Contains("b")) {
			version = currentList.Values[num].Split(' ')[0];
			url = betaRN + version;
		}
		if (!string.IsNullOrEmpty(url)) Application.OpenURL(url);
	}

	static void FillMenu(WWW history) {
		fullList = new SortedList<string, string>();
		string build;
		//0000000001,add,file,02/03/2015,13:13:44,"Unity","5.0.0b22","",
		string[] parts, releases = history.text.Split('\n');
		for (int i = 0; i < releases.Length; i++) {
			parts = releases[i].Split(',');
			DateTime dt;
			if (DateTime.TryParse(string.Format("{0} {1}", parts[3], parts[4]), out dt)) {
				build = string.Format("{0} ({1})", parts[6].Trim('\"'), dt.ToString("dd-MM-yyyy"));
				fullList.Add(parts[0], build);
			}
		}
		CheckNewVersion();
		if (window == null) {
			HelpLastRelease[] w = Resources.FindObjectsOfTypeAll<HelpLastRelease>();
			if (w != null && w.Length > 0) window = w[0];
		}
		if (window != null) window.Repaint();
	}

	static void CheckNewVersion() {
		int count = EditorPrefs.GetInt(prefsCount, 0);
		if (fullList.Count > count) {
			EditorApplication.Beep();
			string color = EditorGUIUtility.isProSkin ? "yellow" : "red";
			Debug.LogFormat("New version: <color={0}>{1}</color>", color,
				fullList.Values[fullList.Count - 1]);
			EditorPrefs.SetInt(prefsCount, fullList.Count);
			currentList = null;
		}
	}

	static void SearchVersion() {
		string path = Path.Combine(tempDir, extractedName);
		if (File.Exists(path)) {
			string[] lines;
			lines = File.ReadAllLines(path, Encoding.Unicode);
			FileUtil.DeleteFileOrDirectory(Path.GetDirectoryName(path));
			string version = currentList.Values[idxSelectedInCurrent].Split(' ')[0] + "_";
			for (int i = 0; i < lines.Length; i++) {
				if (lines[i].Contains(version)) {
					int pos = lines[i].IndexOf(version);
					string revision = lines[i].Substring(pos + version.Length, 12);
					DoWithRevision(revision);
					EditorPrefs.SetString(prefs + currentList.Keys[idxSelectedInCurrent], revision);
					break;
				}
			}
		}
	}

	private static void DoWithRevision(string revision) {
		if (!isAssistant) {
			Application.OpenURL(string.Format(releaseUrl, revision));
		} else {
			DownloadAssistant(revision);
		}
	}

	static void DownloadAssistant(string revision) {
		string version = currentList.Values[idxSelectedInCurrent].Split(' ')[0];
		string ext = Application.platform == RuntimePlatform.WindowsEditor ? "exe" : "dmg";
		string url = string.Format(assistantUrl, revision, version, ext);
		wwwAssistant = new WWW(url);
		EditorApplication.update += WaitAssistant;
	}

	static void DownloadHistory() {
		wwwHistory = new WWW(historyUrl);
		EditorApplication.update += WaitHistory;
	}

	static void DownloadList(int historyNum, bool assist = false) {
		idxSelectedInCurrent = historyNum;
		isAssistant = assist;
		string revision = EditorPrefs.GetString(prefs + currentList.Keys[idxSelectedInCurrent], "");
		if (!string.IsNullOrEmpty(revision)) {
			DoWithRevision(revision);
		} else {
			string listUrl = string.Format("{0}000Admin/{1}", serverUrl, currentList.Keys[historyNum]);
			wwwList = new WWW(listUrl);
			EditorApplication.update += WaitList;
		}
	}

	static void WaitList() {
		Wait(wwwList, WaitList, ParseList);
	}

	static void WaitHistory() {
		Wait(wwwHistory, WaitHistory, FillMenu);
	}

	static void WaitAssistant() {
		Wait(wwwAssistant, WaitAssistant, SaveAssistant);
	}

	static void SaveAssistant(WWW assistant) {
		if (!Directory.Exists(tempDir)) {
			Directory.CreateDirectory(tempDir);
		}
		string name = Path.GetFileName(assistant.url);
		string path = Path.Combine(tempDir, name);
		File.WriteAllBytes(path, assistant.bytes);
		if (Application.platform == RuntimePlatform.WindowsEditor) {
			Application.OpenURL(path);
		} else {
			StartAssistant(path);
		}
	}

	static void StartAssistant(string path) {
		string cmd = "hdiutil";
		string arg = string.Format("mount '{0}'", path);
		try {
			using (Process assist = new Process()) {
				assist.StartInfo.FileName = cmd;
				assist.StartInfo.Arguments = arg;
				assist.StartInfo.WorkingDirectory = Path.GetDirectoryName(path);
				assist.StartInfo.CreateNoWindow = true;
				assist.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				assist.Start();
			}
		} catch (Exception e) {
			Debug.LogErrorFormat("{0} {1}\n{2}", cmd, arg, e.Message);
		}
	}

	static void Wait(WWW www, EditorApplication.CallbackFunction caller, Action<WWW> action) {
		if (www != null && www.isDone) {
			EditorApplication.update -= caller;
			if (string.IsNullOrEmpty(www.error) && www.bytesDownloaded > 0) {
				//Debug.LogFormat("{0} kB: {1}", www.size/1024, www.url);
				if (action != null) action(www);
			} else Debug.LogWarningFormat("{0} {1}", www.url, www.error);
			www = null;
		} else {
			if (www == null) EditorApplication.update -= caller;
		}
	}

	static void ParseList(WWW list) {
		string[] files = list.text.Split('\n');
		string[] parts;
		for (int i = 0; i < files.Length; i++) {
			parts = files[i].Split(',');
			if (parts[0].Contains(extractedName)) {
				string mergerUrl = string.Format("{0}{1}/{2}", serverUrl, parts[0].Trim('\"').Replace('\\', '/'), compressedName);
				DownloadMerger(mergerUrl);
				break;
			}
		}
	}

	static void DownloadMerger(string mergerUrl) {
		wwwMerger = new WWW(mergerUrl);
		EditorApplication.update += WaitMerger;
	}

	static void WaitMerger() {
		Wait(wwwMerger, WaitMerger, SaveMerger);
	}

	static void SaveMerger(WWW merger) {
		if (!Directory.Exists(tempDir)) {
			Directory.CreateDirectory(tempDir);
		}
		string path = Path.Combine(tempDir, compressedName);
		File.WriteAllBytes(path, merger.bytes);
		ExtractMerger(path);
	}

	static void ExtractMerger(string path) {
		string zipPath = string.Format("{0}/Tools/{1}", EditorApplication.applicationContentsPath, zipName);
		string arg = string.Format("e -y \"{0}\"", path);
		try {
			using (Process unzip = new Process()) {
				unzip.StartInfo.FileName = zipPath;
				unzip.StartInfo.Arguments = arg;
				unzip.StartInfo.WorkingDirectory = Path.GetDirectoryName(path);
				unzip.StartInfo.CreateNoWindow = true;
				unzip.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				unzip.Start();
				unzip.WaitForExit();
				SearchVersion();
			}
		} catch (Exception e) {
			Debug.LogErrorFormat("{0} {1}\n{2}", zipPath, arg, e.Message);
		}
	}

	static void DownloadGithub() {
		wwwGithub = new WWW(githubUrl);
		EditorApplication.update += WaitGithub;
	}

	static void WaitGithub() {
		Wait(wwwGithub, WaitGithub, ParseGithub);
	}

	#pragma warning disable 0649
	[Serializable]
	class GithubRelease {
		public string created_at;
		public GithubAsset[] assets;
	}

	[Serializable]
	class GithubAsset {
		public string browser_download_url;
	}
	#pragma warning restore 0649

	static void ParseGithub(WWW github) {
		var release = JsonUtility.FromJson<GithubRelease>(github.text);
		string current = EditorPrefs.GetString(prefs + Application.productName, "1970-01-01T00:00:00Z");
		if (DateTime.Parse(release.created_at) > DateTime.Parse(current)) {
			DownloadPackage(release.assets[0].browser_download_url);
			EditorPrefs.SetString(prefs + Application.productName, release.created_at);
		}
	}

	static void DownloadPackage(string packageUrl) {
		wwwPackage = new WWW(packageUrl);
		EditorApplication.update += WaitPackage;
	}

	static void WaitPackage() {
		Wait(wwwPackage, WaitPackage, ImportPackage);
	}

	static void ImportPackage(WWW package) {
		tempDir = SetTempDir();
		string name = string.Format("{0}.unitypackage", scriptName);
		string path = Path.Combine(tempDir, name);
		File.WriteAllBytes(path, package.bytes);
		AssetDatabase.ImportPackage(path, false);
		Debug.LogFormat("{0} updated from Github", scriptName);
	}

	static void SearchGUI() {
		string s = string.Empty;
		GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
		s = GUILayout.TextField(filterString, GUI.skin.FindStyle("ToolbarSeachTextField"));
		if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton"))) {
			s = "";
			GUI.FocusControl(null);
		}
		GUILayout.EndHorizontal();
		if (s != filterString) {
			filterString = s;
			if (!string.IsNullOrEmpty(filterString)) {
				sortedList = new SortedList<string, string>();
				for (int i = fullList.Count - 1; i >= 0; i--) {
					if (fullList.Values[i].Contains(filterString)) {
						sortedList.Add(fullList.Keys[i], fullList.Values[i]);
					}
				}
				currentList = sortedList;
			} else currentList = fullList;
		}
	}

	static void ProgressGUI(WWW www, string text) {
		if (www != null && !www.isDone && string.IsNullOrEmpty(www.error)) {
			EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), www.progress, string.IsNullOrEmpty(www.error) ? text : www.error);
		}
	}

	void WaitGUI() {
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label("Wait...");
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

}
