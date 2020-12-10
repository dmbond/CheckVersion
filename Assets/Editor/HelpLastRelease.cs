using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using Debug = UnityEngine.Debug;

public class HelpLastRelease : EditorWindow {

    #region Urls

    const string experimenalUrl = @"http://unity3d.com/experimental";
    const string roadmapUrl = @"http://unity3d.com/unity/roadmap";
    const string statusCloudUrl = @"http://status.cloud.unity3d.com";

    const string archiveUrl = @"http://unity3d.com/get-unity/download/archive";
    const string betaArchiveUrl = @"http://unity3d.com/unity/beta/archive";
    const string ltsArchiveUrl = @"http://unity3d.com/unity/qa/lts-releases";
    const string releaseUrlBeta = @"http://beta.unity3d.com/download/{0}/{1}";
    const string downloadHtml = @"download.html";

    const string searchUrl = @"http://unity3d.com/search";
    const string searchGoogleUrl = @"http://www.google.com/cse/publicurl?cx=000020748284628035790:axpeo4rho5e";
    const string searchGitHubUrl = @"http://unitylist.com";
    const string searchIssueUrl = @"http://issuetracker.unity3d.com";

    const string assistantUrl = @"http://beta.unity3d.com/download/{0}/UnityDownloadAssistant-{1}.{2}";
    const string torrentUrl = @"http://download.unity3d.com/download_unity/{0}/Unity.torrent";
    const string serverUrl = @"http://symbolserver.unity3d.com/";
    const string historyUrl = serverUrl + @"000Admin/history.txt";
    const string iniUrl = @"http://beta.unity3d.com/download/{0}/unity-{1}-{2}.ini";
    const string htmlUrl = @"http://beta.unity3d.com/download/{0}/download.html";
    const string jsonUrl = @"https://public-cdn.cloud.unity3d.com/hub/prod/releases-{0}.json";

    const string finalRN = @"http://unity3d.com/unity/whats-new/unity-";
    const string ltsRN = @"http://unity3d.com/unity/whatsnew/unity-";
    const string alphaRN = @"http://unity3d.com/unity/alpha/";
    const string betaRN = @"http://unity3d.com/unity/beta/";
    const string patchRN = @"http://unity3d.com/unity/qa/patch-releases/";
    const string newFinalRN = @"http://unity3d.com/unity/whats-new/";

    const string knowledgeBaseUrl = @"http://support.unity3d.com";
    const string customerServiceUrl = @"http://support.unity3d.com/hc/en-us/requests/new?ticket_form_id=65905";
    const string learnUrl = @"http://learn.unity3d.com/";
    const string faqUrl = @"http://unity3d.com/unity/faq";

    const string githubUTUrl = @"http://github.com/Unity-Technologies";
    const string bitbucketUTUrl = @"http://bitbucket.org/Unity-Technologies";
    const string newsUrl = @"http://t.me/s/unity_news";

    const string githubUrl = @"http://api.github.com/repos/dmbond/CheckVersion/releases/latest";

    const string unityHubUrl = @"unityhub://{0}/{1}";

    static Dictionary<string, string> unlisted = new Dictionary<string, string>() {
        { "Builtin Shaders", "builtin_shaders-{0}.zip" },
        { "Nintendo Switch Support", "switch/UnitySetup-Nintendo-Switch-Support-for-Editor-{0}.exe" },
        { "Mac Documentation Installer", "MacDocumentationInstaller/Documentation-{0}.pkg" },
        { "Windows Documentation Installer", "WindowsDocumentationInstaller/UnityDocumentationSetup-{0}.exe" },
        { "Misc PDB Internal", "misc-pdb-internal.zip" },
        { "Android Symbol Files", "AndroidSymbolFiles.zip" },
        { "Unity Remote Android", "UnityRemote-Android-{0}.apk" },
        { "Unity Remote iOS", "UnityRemote-iOS-{0}.zip" },
        { "Unity Remote Project", "UnityRemoteProject-{0}.zip" },
        { "Mono Develop Win", "MonoDevelop-{0}.zip" },
        { "Mono Develop OSX", "MonoDevelop.app.tar-{0}.gz" },
        { "Zipped For TeamCity", "ZippedForTeamCity.tar.gz" },
        { "Cache Server", "CacheServer-{0}.zip" },
        { "Build Info", "buildInfo" },
        { "Win INI", "unity-{0}-win.ini" },
        { "OSX INI", "unity-{0}-osx.ini" },
        { "Linux INI", "unity-{0}-linux.ini" }
    };

    private static Dictionary<string, string[]> android = new Dictionary<string, string[]>() {
            {"Android SDK & NDK Tools (26.1.1)", new string[] {
                "http://dl.google.com/android/repository/sdk-tools-windows-4333796.zip",
                "http://dl.google.com/android/repository/sdk-tools-darwin-4333796.zip",
                "http://dl.google.com/android/repository/sdk-tools-linux-4333796.zip"
            }},
            {"Android SDK Platform Tools (28.0.1)", new string[] {
                "http://dl.google.com/android/repository/platform-tools_r28.0.1-windows.zip",
                "http://dl.google.com/android/repository/platform-tools_r28.0.1-darwin.zip",
                "http://dl.google.com/android/repository/platform-tools_r28.0.1-linux.zip"
            }},
            {"Android SDK Build Tools (28.0.3)", new string[] {
                "http://dl.google.com/android/repository/build-tools_r28.0.3-windows.zip",
                "http://dl.google.com/android/repository/build-tools_r28.0.3-macosx.zip",
                "http://dl.google.com/android/repository/build-tools_r28.0.3-linux.zip"
            }},
            {"Android SDK Platforms (28)", new string[] {
                "http://dl.google.com/android/repository/platform-28_r06.zip",
                "http://dl.google.com/android/repository/platform-28_r06.zip",
                "http://dl.google.com/android/repository/platform-28_r06.zip"
            }},
            {"Android NDK (r16b)", new string[] {
                "http://dl.google.com/android/repository/android-ndk-r16b-windows-x86_64.zip",
                "http://dl.google.com/android/repository/android-ndk-r16b-darwin-x86_64.zip",
                "http://dl.google.com/android/repository/android-ndk-r19-linux-x86_64.zip"
            }},
            {"Android NDK (r19)", new string[] {
                "http://dl.google.com/android/repository/android-ndk-r19-windows-x86_64.zip",
                "http://dl.google.com/android/repository/android-ndk-r19-darwin-x86_64.zip",
                "http://dl.google.com/android/repository/android-ndk-r19-linux-x86_64.zip"
            }},
            {"OpenJDK (8u172-b11)", new string[] {
                "http://download.unity3d.com/download_unity/open-jdk/open-jdk-win-x64/jdk8u172-b11_4be8440cc514099cfe1b50cbc74128f6955cd90fd5afe15ea7be60f832de67b4.zip",
                "http://download.unity3d.com/download_unity/open-jdk/open-jdk-mac-x64/jdk8u172-b11_4be8440cc514099cfe1b50cbc74128f6955cd90fd5afe15ea7be60f832de67b4.zip",
                "http://download.unity3d.com/download_unity/open-jdk/open-jdk-linux-x64/jdk8u172-b11_4be8440cc514099cfe1b50cbc74128f6955cd90fd5afe15ea7be60f832de67b4.zip"
            }}
    };

    #endregion

    #region Vars

    #pragma warning disable 0414, 0649, 1635, 0618

    [Serializable]
    class GithubRelease {
        public string created_at;
        public GithubAsset[] assets;
    }

    static GithubRelease release = null;

    [Serializable]
    class GithubAsset {
        public string browser_download_url;
    }

    [Serializable]
    class JsonOS {
        public JsonRelease[] official;
        public JsonRelease[] beta;
    }

    [Serializable]
    class JsonRelease {
        public string version;
        public bool lts;
        public string downloadUrl;
        public JsonModule[] modules;
    }

    [Serializable]
    class JsonModule {
        public string name;
        public string description;
        public string downloadUrl;
    }

    #pragma warning restore 0649, 1635

    static readonly string zipName = Application.platform == RuntimePlatform.WindowsEditor ? "7z" : "7za";
    static string tempDir;
    const string torrentFile = "Unity.torrent";
    const string shaderCompiler = "UnityShaderCompiler.ex";
    const string yamlMerge = "UnityYAMLMerge.ex";
    const string searchInRN = @"Changeset:</span>";
    static char[] splitInRN =  {' ', '\r', '\n', '\t' };
    static bool iniWinDownloaded = false;

    static WWW wwwHistory, wwwList, wwwCompressed, wwwAssistant;
    static WWW wwwGithub, wwwPackage;
    static WWW wwwIniWin, wwwIniOSX, wwwIniLinux;
    static WWW wwwJsonWin, wwwJsonOSX, wwwJsonLinux;
    static WWW wwwReleaseNotes, wwwTorrent, wwwHtml;

    static SortedList<string, string> fullList, sortedList, currentList, officialList;

    static int idxSelectedInCurrent = -1;
    static bool officialShow = false;
    static string selectedVersion;
    static string selectedRevision;
    static Action ReleaseCallback;

    static int idxOS = -1;
    static readonly string[] titlesOS = { "Win", "OSX" };
    static readonly string[] titlesOSLinux = { "Win", "OSX", "Linux" };
    static Dictionary<string, Dictionary<string, string>> dictIniWin, dictIniOSX, dictIniLinux;
    static Dictionary<int, JsonRelease> dictJsonWin, dictJsonOSX, dictJsonLinux;
    static JsonOS jsonWin, jsonOSX, jsonLinux;
    static bool hasLinux, hasReleaseNotes, hasTorrent;

    static HelpLastRelease window;
    const string wndTitle = "Releases";
    const string scriptName = "HelpLastRelease";
    const string prefs = scriptName + ".";
    const string prefsCount = prefs + "count";
    const string prefsLog = prefs + "log";
    const string prefsFoldoutDefault = prefs + "foldoutDefault";
    const string prefsFoldoutAndroid = prefs + "foldoutAndroid";
    const string prefsFoldoutOther = prefs + "foldoutOther";
    static bool hasUpdate = false;

    static string filterString = string.Empty;
    static string universalDT = "yyyy-MM-ddTHH:mm:ssZ";
    static string nullDT = "1970-01-01T00:00:00Z";
    static string srcDT = "MM/dd/yyyy HH:mm:ss";
    static string listDT = "dd-MM-yyyy";
    
    const string rnTooltip = "Open Release Notes";
    const string torrentTooltip = "Open Torrent";
    const string assistTooltip = "Open Download Assistant";
    const string versionTooltip = "Open Download Page";
    const string defaultSection = "default";
    const string otherSection = "other";
    const string androidSection = "android";
    const string infoTooltip = "Show more info";
    const string updateTooltip = "Update from Github";
    const string editorTooltip = "Unity Editor for building your games";
    const string hubTooltip = "Installation via Unity Hub";
    const string logTooltip = "Enable/Disable URL Logging";
    const string logButton = "Log";
    const string releasesButton = "Official";
    const string releasesTooltip = "Switch to Official List";
    const string officialButton = "Releases";
    const string officialTooltip = "Switch to Releases List";
    const string updateButton = "Update";
    const string openInHubButton = "Open in Hub";
    const string releaseNotesButton = "Release Notes";
    const string torrentButton = "Torrent";

    static readonly Dictionary<string, Color> colors = new Dictionary<string, Color>() {
        { "2017.4.", new Color(0f, 1f, 1f, 1f) },
        { "2018.4.", new Color(0.5f, 1f, 0.5f, 1f) },
        { "2019.4.", new Color(0f, 1f, 0f, 1f) },
        { "2020.1.", new Color(1f, 1f, 0f, 1f) },
        { "2020.2.", new Color(1f, 0f, 0f, 1f) },
        { "2020.3.", new Color(0f, 1f, 0f, 1f) },
        { "2021.1.", new Color(1f, 0.4f, 0.4f, 1f) },
        { "2021.2.", new Color(1f, 0f, 0f, 1f) },
    };
    static Color oldColor = Color.white;
    static Color currentColor = Color.black;
    static float alphaBackForPersonal = 0.3f;
    static Color alpha = new Color(1f, 1f, 1f, alphaBackForPersonal);

    static int repeatRN = 0;
    static bool log = false;
    static bool isDebug = false;

    static Vector2 minSizeWindow = new Vector2(620f, 100f);
    static GUIStyle btnStyle;
    static Vector2 scrollPosReleases;
    static Vector2 scrollPosInfo;
    static bool foldoutDefault, foldoutOther, foldoutAndroid;

    static string baseName;
    static string CompressedName {
        get { return baseName + "_"; }
    }
    static string ExtractedName {
        get { return baseName + "e"; }
    }

    #endregion

    #region Menu

    [MenuItem("Help/Links/Releases...", false, 010)]
    static void ShowReleases() {
        officialShow = false;
        ShowInit();
        SortList(string.Empty);
    }

    [MenuItem("Help/Links/Check for Updates...", false, 015)]
    static void ShowUpdates() {
        officialShow = false;
        ShowInit();
        int index = Application.unityVersion.LastIndexOf('.');
        string filter = Application.unityVersion.Substring(0, index + 1);
        SortList(filter);
    }

    [MenuItem("Help/Links/Official...", false, 020)]
    static void ShowOfficial() {
        officialShow = true;
        ShowInit();
        int counter = 0;
        officialList = new SortedList<string, string>();
        dictJsonWin = new Dictionary<int, JsonRelease>();
        dictJsonOSX = new Dictionary<int, JsonRelease>();
        dictJsonLinux = new Dictionary<int, JsonRelease>();
        for (int i = 0; i < jsonWin.official.Length; i++) {
            officialList.Add(counter.ToString(), jsonWin.official[i].version);
            dictJsonWin.Add(counter, jsonWin.official[i]);
            dictJsonOSX.Add(counter, jsonOSX.official[i]);
            dictJsonLinux.Add(counter, jsonLinux.official[i]);
            counter++;
        }
        for (int i = 0; i < jsonWin.beta.Length; i++) {
            officialList.Add(counter.ToString(), jsonWin.beta[i].version);
            dictJsonWin.Add(counter, jsonWin.beta[i]);
            dictJsonOSX.Add(counter, jsonOSX.beta[i]);
            dictJsonLinux.Add(counter, jsonLinux.beta[i]);
            counter++;
        }
    }

    static void ShowInit() {
        ClearGUI();
        window = GetWindow<HelpLastRelease>(wndTitle);
        window.minSize = minSizeWindow;
    }
    // ---

    #region Services

    [MenuItem("Help/Links/Search...", false, 100)]
    static void OpenSearch() {
        Application.OpenURL(searchUrl);
    }

    [MenuItem("Help/Links/Search Google...", false, 100)]
    static void OpenSearchGoogle() {
        Application.OpenURL(searchGoogleUrl);
    }

    [MenuItem("Help/Links/Search GitHub...", false, 105)]
    static void OpenSearchGitHub() {
        Application.OpenURL(searchGitHubUrl);
    }

    [MenuItem("Help/Links/Search Issue...", false, 110)]
    static void OpenSearchIssue() {
        Application.OpenURL(searchIssueUrl);
    }
    // ---

    [MenuItem("Help/Links/Archive...", false, 200)]
    static void OpenArchive() {
        Application.OpenURL(archiveUrl);
    }

    [MenuItem("Help/Links/LTS Archive...", false, 205)]
    static void OpenLTSArchive() {
        Application.OpenURL(ltsArchiveUrl);
    }

    [MenuItem("Help/Links/Beta Archive...", false, 205)]
    static void OpenBetaArchive() {
        Application.OpenURL(betaArchiveUrl);
    }

    [MenuItem("Help/Links/Patch Archive...", false, 210)]
    static void OpenPatchArchive() {
        Application.OpenURL(patchRN);
    }
    // ---

    [MenuItem("Help/Links/Knowledge Base...", false, 705)]
    static void OpenKnowledgeBase() {
        Application.OpenURL(knowledgeBaseUrl);
    }

    [MenuItem("Help/Links/Customer Service...", false, 707)]
    static void OpenCustomerService() {
        Application.OpenURL(customerServiceUrl);
    }

    [MenuItem("Help/Links/Learn...", false, 710)]
    static void OpenLiveTraining() {
        Application.OpenURL(learnUrl);
    }

    [MenuItem("Help/Links/FAQ...", false, 715)]
    static void OpenFaq() {
        Application.OpenURL(faqUrl);
    }
    // ---

    [MenuItem("Help/Links/Roadmap...", false, 800)]
    static void OpenRoadmap() {
        Application.OpenURL(roadmapUrl);
    }

    [MenuItem("Help/Links/Experimental...", false, 805)]
    static void OpenExperimental() {
        Application.OpenURL(experimenalUrl);
    }

    [MenuItem("Help/Links/Status Cloud...", false, 810)]
    static void OpenStatusCloud() {
        Application.OpenURL(statusCloudUrl);
    }
    // ---

    [MenuItem("Help/Links/Github UT...", false, 830)]
    static void OpenGithubUT() {
        Application.OpenURL(githubUTUrl);
    }

    [MenuItem("Help/Links/Bitbucket UT...", false, 835)]
    static void OpenBitbucketUT() {
        Application.OpenURL(bitbucketUTUrl);
    }

    [MenuItem("Help/Links/News...", false, 840)]
    static void OpenNews() {
        Application.OpenURL(newsUrl);
    }
    // ---

    /*
    [MenuItem("Help/Links/Clear prefs...", false, 860)]
    static void Clear() {
        //EditorPrefs.SetInt(prefsCount, 0);
        EditorPrefs.SetString(prefs + Application.productName, nullDT);
    }
    */

    #endregion

    #endregion

    #region GUI

    void OnGUI() {
        if (fullList != null || officialList != null) {
            GUILayout.BeginHorizontal();
            ListGUI();
            InfoGUI();
            GUILayout.EndHorizontal();
        } else {
            WaitGUI();
        }
    }

    void LogGUI() {
        if (EditorGUIUtility.isProSkin) {
            GUI.contentColor = log ? Color.green : Color.red;
        } else {
            GUI.backgroundColor = log ? Color.green : Color.red * alpha;
        }
        if (GUILayout.Button(new GUIContent(logButton, logTooltip), btnStyle)) {
            log = !log;
            EditorPrefs.SetBool(prefsLog, log);
            Debug.LogFormat("URL Logging: {0}", log);
        }
    }

    void SwitchGUI() {
        GUILayout.Space(5f);
        if (EditorGUIUtility.isProSkin) {
            GUI.contentColor = oldColor;
        } else {
            GUI.backgroundColor = oldColor * alpha;
        }
        btnStyle.alignment = TextAnchor.MiddleCenter;
        if (GUILayout.Button(new GUIContent(officialShow ? officialButton : releasesButton,
            officialShow ? officialTooltip : releasesTooltip), btnStyle)) {
            OnEnable();
            officialShow = !officialShow;
            if (officialShow) {
                ShowOfficial();
            } else {
                ShowReleases();
            }
        }
        GUILayout.Space(5f);
    }

    void ListGUI() {
        btnStyle = new GUIStyle(EditorStyles.miniButton);
        GUILayout.BeginVertical(GUILayout.MaxWidth(210));
        SearchVersionGUI();
        scrollPosReleases = EditorGUILayout.BeginScrollView(scrollPosReleases, false, true);
        if (currentList == null) currentList = officialShow ? officialList : fullList;
        for (int i = currentList.Count - 1; i >= 0; i--) {
            GUILayout.BeginHorizontal();
            ColorGUI(i);
            btnStyle.alignment = TextAnchor.MiddleCenter;
            #if UNITY_5_5_OR_NEWER
            if (Application.platform != RuntimePlatform.LinuxEditor)
            #endif
            if (GUILayout.Button(new GUIContent("A", assistTooltip), btnStyle, GUILayout.MaxWidth(23f))) {
                DownloadList(i, DownloadAssistant);
            }
            btnStyle.alignment = TextAnchor.MiddleLeft;
            if (GUILayout.Button(new GUIContent(currentList.Values[i], infoTooltip), btnStyle, GUILayout.MaxWidth(160f))) {
                DownloadList(i, UpdateInfo);
            }
            GUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        BottomButtonsGUI();
        GUILayout.EndVertical();
    }

    void BottomButtonsGUI() {
        GUILayout.Space(5f);
        GUILayout.BeginHorizontal();
        SwitchGUI();
        LogGUI();
        UpdateGUI();
        GUILayout.EndHorizontal();
        GUILayout.Space(5f);
    }

    void UpdateGUI() {
        if (hasUpdate) {
            GUILayout.Space(5f);
            if (EditorGUIUtility.isProSkin) {
                GUI.contentColor = Color.green;
            } else {
                GUI.backgroundColor = Color.green * alpha;
            }
            btnStyle.alignment = TextAnchor.MiddleCenter;
            if (GUILayout.Button(new GUIContent(updateButton, updateTooltip), btnStyle)) {
                if (release != null) {
                    hasUpdate = false;
                   DownloadPackage(release.assets[0].browser_download_url);
                }
            }
            GUILayout.Space(5f);
        }
    }

    void InfoGUI() {
        if (idxSelectedInCurrent == -1 && (string.IsNullOrEmpty(selectedVersion) || string.IsNullOrEmpty(selectedRevision))) return;
        if (EditorGUIUtility.isProSkin) {
            GUI.contentColor = oldColor;
        } else {
            GUI.backgroundColor = oldColor * alpha;
        }
        GUILayout.BeginVertical(GUILayout.Width(410));
        GUILayout.Space(5f);
        TopInfoButtons();

        if (officialShow && !IsRevision(filterString)) {
            OfficialInfo();
        } else {
            if (iniWinDownloaded) ReleaseInfo();
        }
        GUILayout.EndVertical();
    }

    void ReleaseInfo() {
        Dictionary<string, Dictionary<string, string>> dict = null;
        if (!string.IsNullOrEmpty(selectedRevision)) {
            var titles = hasLinux ? titlesOSLinux : titlesOS;
            int newIdxOS = GUILayout.SelectionGrid(idxOS, titles, hasLinux ? 3 : 2, btnStyle);
            switch (newIdxOS) {
                case 0:
                    dict = dictIniWin;
                    if (newIdxOS != idxOS) URLOpenAndLog("Ini " + titles[newIdxOS], wwwIniWin.url, true);
                    break;
                case 1:
                    dict = dictIniOSX;
                    if (newIdxOS != idxOS) URLOpenAndLog("Ini " + titles[newIdxOS], wwwIniOSX.url, true);
                    break;
                case 2:
                    dict = dictIniLinux;
                    if (newIdxOS != idxOS) URLOpenAndLog("Ini " + titles[newIdxOS], wwwIniLinux.url, true);
                    break;
            }

            idxOS = newIdxOS;
        }

        if (dict != null) {
            scrollPosInfo = EditorGUILayout.BeginScrollView(scrollPosInfo, false, false);
            btnStyle.alignment = TextAnchor.MiddleLeft;

            bool foldDefault = EditorGUILayout.Foldout(foldoutDefault, defaultSection);
            if (foldDefault != foldoutDefault) {
                foldoutDefault = foldDefault;
                EditorPrefs.SetBool(prefsFoldoutDefault, foldoutDefault);
            }
            if (foldoutDefault) {
                foreach (var key in dict.Keys) {
                    if (GUILayout.Button(
                        new GUIContent(dict[key]["title"], "Download " + dict[key]["description"]), btnStyle)) {
                        var url = dict[key]["url"].StartsWith("http")
                            ? dict[key]["url"]
                            : string.Format(releaseUrlBeta, selectedRevision, dict[key]["url"]);
                        URLOpenAndLog(key, url);
                    }
                }
            }
            bool foldAndroid = EditorGUILayout.Foldout(foldoutAndroid, androidSection);
            if (foldAndroid != foldoutAndroid) {
                foldoutAndroid = foldAndroid;
                EditorPrefs.SetBool(prefsFoldoutAndroid, foldoutAndroid);
            }
            if (foldoutAndroid) {
                foreach (var key in android.Keys) {
                    if (GUILayout.Button(
                        new GUIContent(key, "Download " + key), btnStyle)) {
                        var url = android[key][idxOS];
                        URLOpenAndLog(key, url);
                    }
                }
            }
            bool foldOther = EditorGUILayout.Foldout(foldoutOther, otherSection);
            if (foldOther != foldoutOther) {
                foldoutOther = foldOther;
                EditorPrefs.SetBool(prefsFoldoutOther, foldoutOther);
            }
            if (foldoutOther) {
                foreach (var key in unlisted.Keys) {
                    if (GUILayout.Button(
                        new GUIContent(key, "Download " + key), btnStyle)) {
                        var url = string.Format(releaseUrlBeta, selectedRevision,
                            string.Format(unlisted[key], selectedVersion, selectedRevision));
                        URLOpenAndLog(key, url);
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }

    void OfficialInfo() {
        idxOS = GUILayout.SelectionGrid(idxOS, titlesOSLinux, 3, btnStyle);
        JsonRelease release = null;
        switch (idxOS) {
            case 0:
                release = dictJsonWin != null ? dictJsonWin[idxSelectedInCurrent] : null;
                break;
            case 1:
                release = dictJsonOSX != null ? dictJsonOSX[idxSelectedInCurrent] : null;
                break;
            case 2:
                release = dictJsonLinux != null ? dictJsonLinux[idxSelectedInCurrent] : null;
                break;
        }
        if (release != null) {
            GUILayout.BeginVertical();
            GUILayout.Space(5f);
            btnStyle.alignment = TextAnchor.MiddleLeft;
            if (GUILayout.Button(
                new GUIContent(string.Format("Unity {0}", release.version), editorTooltip), btnStyle)) {
                URLOpenAndLog("Download Url", release.downloadUrl);
            }
            if (release.modules != null) {
                for (int i = 0; i < release.modules.Length; i++) {
                    if (GUILayout.Button(
                        new GUIContent(release.modules[i].name, release.modules[i].description), btnStyle)) {
                        URLOpenAndLog(release.modules[i].name, release.modules[i].downloadUrl);
                    }
                }
            }
            GUILayout.EndVertical();
        }
    }

    void TopInfoButtons() {
        GUILayout.BeginHorizontal();
        btnStyle.alignment = TextAnchor.MiddleCenter;
        if (!string.IsNullOrEmpty(selectedRevision) &&
            GUILayout.Button(new GUIContent(string.Format("{0} ({1})", selectedVersion, selectedRevision), versionTooltip), btnStyle)) {
            URLOpenAndLog("Release", string.Format(releaseUrlBeta, selectedRevision, downloadHtml));
        }
        if (!string.IsNullOrEmpty(selectedRevision) &&
            GUILayout.Button(new GUIContent(openInHubButton, hubTooltip), btnStyle)) {
            URLOpenAndLog(openInHubButton, string.Format(unityHubUrl, selectedVersion, selectedRevision));
        }
        if (hasReleaseNotes && GUILayout.Button(
            new GUIContent(releaseNotesButton, rnTooltip), btnStyle)) {
            URLOpenAndLog(releaseNotesButton, wwwReleaseNotes.url);
        }
        if (hasTorrent && GUILayout.Button(
            new GUIContent(torrentButton, torrentTooltip), btnStyle)) {
            URLOpenAndLog(torrentButton, wwwTorrent.url, true);
            StartTorrent();
        }
        GUILayout.EndHorizontal();
    }

    void ColorGUI(int i) {
        foreach (var k in colors.Keys) {
            bool isColored = currentList.Values[i].Contains(k);
            if (EditorGUIUtility.isProSkin) {
                GUI.contentColor = isColored ? colors[k] : oldColor;
            } else {
                GUI.backgroundColor = isColored ? colors[k] * alpha : oldColor * alpha;
            }
            if (isColored) break;
        }
    }

    static void SearchVersionGUI() {
        string s = string.Empty;
        GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
        s = GUILayout.TextField(filterString, GUI.skin.FindStyle("ToolbarSeachTextField"));
        // hack for old versions of Unity
        // which do not process the clipboard
#if UNITY_5
        if (string.IsNullOrEmpty(s)) {
            Event current = Event.current;
            if ((current.control && current.keyCode == KeyCode.V) ||
                (current.shift && current.keyCode == KeyCode.Insert) ||
                (current.command && current.keyCode == KeyCode.V)) {
                if (IsRevision(EditorGUIUtility.systemCopyBuffer)) {
                    s = EditorGUIUtility.systemCopyBuffer;
                }
            }
        }
#endif
        if (GUILayout.Button(string.Empty, GUI.skin.FindStyle("ToolbarSeachCancelButton"))) {
            s = string.Empty;
            GUI.FocusControl(null);
        }
        GUILayout.EndHorizontal();
        if (s != filterString) {
            if (IsRevision(s)) {
                filterString = s;
                DownloadHtml(s);
            } else {
                SortList(s);
            }
        }
    }

    static bool IsRevision(string revision) {
        return (revision.Length == 10 || revision.Length == 12) && !revision.Contains(".");
    }

    static void FillMenu(WWW history) {
        URLOpenAndLog("History", historyUrl, true);
        fullList = new SortedList<string, string>();
        string build;
        //0000000001,add,file,02/03/2015,13:13:44,"Unity","5.0.0b22","",
        string[] parts, releases = history.text.Split('\n');
        for (int i = 0; i < releases.Length; i++) {
            parts = releases[i].Split(',');
            DateTime dt = DateTime.ParseExact(string.Format("{0} {1}", parts[3], parts[4]), srcDT, CultureInfo.InvariantCulture);
            build = string.Format("{0} ({1})", parts[6].Trim('\"'), dt.ToString(listDT));
            fullList.Add(parts[0], build);
        }
        CheckNewVersion();
        if (!string.IsNullOrEmpty(filterString)) SortList(filterString);
        if (window == null) {
           HelpLastRelease[] w = Resources.FindObjectsOfTypeAll<HelpLastRelease>();
            if (w != null && w.Length > 0) window = w[0];
        }
        if (window != null) window.Repaint();
    }

    static void SortList(string filter) {
        var tempList = officialShow ? officialList : fullList;
        if (!string.IsNullOrEmpty(filter) && tempList != null) {
            sortedList = new SortedList<string, string>();
            for (int i = tempList.Count - 1; i >= 0; i--) {
                if (tempList.Values[i].Contains(filter)) {
                    sortedList.Add(tempList.Keys[i], tempList.Values[i]);
                }
            }
            currentList = sortedList;
        } else currentList = tempList;
        filterString = filter;
    }

    void WaitGUI() {
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Wait...");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
    }

    static void ClearGUI() {
        currentList = null;
        idxSelectedInCurrent = -1;
        selectedVersion = string.Empty;
        selectedRevision = string.Empty;
        filterString = string.Empty;
    }

    static void URLOpenAndLog(string title, string url, bool notOpen = false) {
        if (notOpen == false) Application.OpenURL(url);
        if (log) Debug.LogFormat("{0}: <b>{1}</b>", title, url);
    }

    #endregion

    #region Window

    void OnEnable() {
        tempDir = SetTempDir();
        log = EditorPrefs.GetBool(prefsLog, false);
        foldoutDefault = EditorPrefs.GetBool(prefsFoldoutDefault, true);
        foldoutOther = EditorPrefs.GetBool(prefsFoldoutOther, true);
        foldoutAndroid = EditorPrefs.GetBool(prefsFoldoutAndroid, true);
        if (Application.internetReachability != NetworkReachability.NotReachable) {
            DownloadHistory();
        }
    }

    [InitializeOnLoadMethod]
    static void AutoUpdate() {
        colors.Add(Application.unityVersion, currentColor);
        if (Application.internetReachability != NetworkReachability.NotReachable) {
            DownloadGithub();
            UpdateJsonInfo();
        }
    }

    #endregion

    #region Download

    static void UpdateInfo() {
        idxOS = Application.platform == RuntimePlatform.WindowsEditor ? 0 : 1;
        window.Repaint();
        if (!string.IsNullOrEmpty(selectedRevision)) {
            if (!officialShow || IsRevision(filterString)) {
                DownloadIniWin(selectedRevision, selectedVersion);
                URLOpenAndLog("Ini Win", wwwIniWin.url, true);
                DownloadIniOSX(selectedRevision, selectedVersion);
                DownloadIniLinux(selectedRevision, selectedVersion);
            }
            DownloadTorrent(selectedRevision, selectedVersion);
        }
    }
    
    static void UpdateJsonInfo() {
        DownloadJsonWin();
        DownloadJsonOSX();
        DownloadJsonLinux();
    }

    static void DownloadAssistant() {
        UpdateInfo();
        string ext = Application.platform == RuntimePlatform.WindowsEditor ? "exe" : "dmg";
        string url = string.Format(assistantUrl, selectedRevision, selectedVersion, ext);
        URLOpenAndLog("Assistant", url, true);
        wwwAssistant = new WWW(url);
        EditorApplication.update += WaitAssistant;
    }

    static void DownloadHistory() {
        wwwHistory = new WWW(historyUrl);
        EditorApplication.update += WaitHistory;
    }

    static void DownloadList(int historyNum, Action callback) {
        hasTorrent = false;
        hasReleaseNotes = false;
        idxSelectedInCurrent = historyNum;
        repeatRN = 0;
        if (IsRevision(filterString)) filterString = string.Empty;
        selectedVersion = currentList.Values[idxSelectedInCurrent].Split(' ')[0];
        DownloadReleaseNotes(VersionToReleaseNotesUrl(selectedVersion));
        if (officialShow) {
            selectedRevision = dictJsonWin[idxSelectedInCurrent].downloadUrl.Split('/')[4];
            if (callback != null) callback();
        } else {
            selectedRevision = isDebug ? "" : EditorPrefs.GetString(prefs + currentList.Keys[idxSelectedInCurrent], "");
            if (!string.IsNullOrEmpty(selectedRevision)) {
                if (callback != null) callback();
            } else {
                ReleaseCallback = callback;
                string listUrl = string.Format("{0}000Admin/{1}", serverUrl, currentList.Keys[idxSelectedInCurrent]);
                URLOpenAndLog("List", listUrl, true);
                wwwList = new WWW(listUrl);
                EditorApplication.update += WaitList;
            }
        }
    }

    static void DownloadInfo(string revision, string version, Action callback) {
        hasTorrent = false;
        hasReleaseNotes = false;
        repeatRN = 0;
        selectedVersion = version;
        selectedRevision = revision;
        //DownloadReleaseNotes(VersionToReleaseNotesUrl(selectedVersion));
        if (callback != null) callback();
    }

    static string VersionToReleaseNotesUrl(string version, int repeat = 0) {
        string url = null;
        string versionDigits;
        if (version.Contains("a")) {
            versionDigits = version.Split(' ')[0];
            url = alphaRN + versionDigits;
        }
        if (version.Contains("p")) {
            versionDigits = version.Split(' ')[0];
            url = patchRN + versionDigits;
        }
        if (version.Contains("f")) {
            versionDigits = version.Split('f')[0];
            var parts = versionDigits.Split('.');
            // RC
            if (parts[2].StartsWith("0")) {
                // old releases
                if (versionDigits.StartsWith("5.3") || versionDigits.StartsWith("5.2") ||
                    versionDigits.StartsWith("5.1") || versionDigits.StartsWith("5.0")) {
                    url = finalRN + versionDigits.Substring(0, 3);
                } else {
                    if (repeat == 0) {
                        url = betaRN + version;
                    } else {
                        url = finalRN + versionDigits;
                    }
                }
            } else {
                // LTS or new Final
                switch (repeat) {
                case 0:
                        url = finalRN + versionDigits;
                        break;
                    case 1:
                        url = ltsRN + versionDigits;
                        break;
                    case 2:
                        url = newFinalRN + versionDigits;
                        break;
                }
            }
        }
        if (version.Contains("b")) {
            versionDigits = version.Split(' ')[0];
            if (repeat == 0) {
                url = betaRN + versionDigits;
            } else {
                url = string.Format("{0}unity{1}", betaRN, versionDigits);
            }
        }
        return url;
    }

    static void DownloadReleaseNotes(string url) {
        hasReleaseNotes = false;
        URLOpenAndLog("Release Notes", url, true);
        wwwReleaseNotes = new WWW(url);
        EditorApplication.update += WaitReleaseNotes;
    }

    static void DownloadTorrent(string revision, string version) {
        hasTorrent = false;
        if (version.Contains("f")) {
            string url = string.Format(torrentUrl, revision);
            wwwTorrent = new WWW(url);
            EditorApplication.update += WaitTorrent;
        }
    }

    static void DownloadIniWin(string revision, string version) {
        dictIniWin = null;
        iniWinDownloaded = false;
        string url = string.Format(iniUrl, revision, version, "win");
        wwwIniWin = new WWW(url);
        EditorApplication.update += WaitIniWin;
    }

    static void DownloadIniOSX(string revision, string version) {
        dictIniOSX = null;
        string url = string.Format(iniUrl, revision, version, "osx");
        wwwIniOSX = new WWW(url);
        EditorApplication.update += WaitIniOSX;
    }

    static void DownloadIniLinux(string revision, string version) {
        dictIniLinux = null;
        hasLinux = false;
        string url = string.Format(iniUrl, revision, version, "linux");
        wwwIniLinux = new WWW(url);
        EditorApplication.update += WaitIniLinux;
    }

    static void DownloadCompressed(string compressedUrl) {
        URLOpenAndLog("Compressed", compressedUrl, true);
        wwwCompressed = new WWW(compressedUrl);
        EditorApplication.update += WaitCompressed;
    }

    static void DownloadPackage(string packageUrl) {
        URLOpenAndLog("Package", packageUrl, true);
        wwwPackage = new WWW(packageUrl);
        EditorApplication.update += WaitPackage;
    }

    static void DownloadGithub() {
        URLOpenAndLog("Github", githubUrl, true);
        wwwGithub = new WWW(githubUrl);
        EditorApplication.update += WaitGithub;
    }

    static void DownloadJsonWin() {
        jsonWin = null;
        string url = string.Format(jsonUrl, "win32");
        URLOpenAndLog("Json Win", url, true);
        wwwJsonWin = new WWW(url);
        EditorApplication.update += WaitJsonWin;
    }

    static void DownloadJsonOSX() {
        jsonOSX = null;
        string url = string.Format(jsonUrl, "darwin");
        URLOpenAndLog("Json OSX", url, true);
        wwwJsonOSX = new WWW(url);
        EditorApplication.update += WaitJsonOSX;
    }

    static void DownloadJsonLinux() {
        jsonLinux = null;
        string url = string.Format(jsonUrl, "linux");
        URLOpenAndLog("Json Linux", url, true);
        wwwJsonLinux = new WWW(url);
        EditorApplication.update += WaitJsonLinux;
    }

    static void DownloadHtml(string revision) {
        filterString = revision;
        string url = string.Format(htmlUrl, revision);
        URLOpenAndLog("Download html", url, true);
        wwwHtml = new WWW(url);
        EditorApplication.update += WaitHtml;
    }

    #endregion

    #region Wait

    static void WaitList() {
        Wait(wwwList, WaitList, ParseList);
    }

    static void WaitHistory() {
        Wait(wwwHistory, WaitHistory, FillMenu);
    }

    static void WaitAssistant() {
        Wait(wwwAssistant, WaitAssistant, SaveAssistant);
    }

    static void WaitReleaseNotes() {
        Wait(wwwReleaseNotes, WaitReleaseNotes, ParseReleaseNotes);
    }

    static void WaitTorrent() {
        Wait(wwwTorrent, WaitTorrent, ProcessTorrent);
    }

    static void WaitIniWin() {
        Wait(wwwIniWin, WaitIniWin, ParseIniWin);
    }

    static void WaitIniOSX() {
        Wait(wwwIniOSX, WaitIniOSX, ParseIniOSX);
    }

    static void WaitIniLinux() {
        Wait(wwwIniLinux, WaitIniLinux, ParseIniLinux);
    }

    static void WaitCompressed() {
        Wait(wwwCompressed, WaitCompressed, SaveCompressed);
    }

    static void WaitGithub() {
        Wait(wwwGithub, WaitGithub, ParseGithub);
    }

    static void WaitPackage() {
        Wait(wwwPackage, WaitPackage, ImportPackage);
    }

    static void WaitJsonWin() {
        Wait(wwwJsonWin, WaitJsonWin, ParseJsonWin);
    }

    static void WaitJsonOSX() {
        Wait(wwwJsonOSX, WaitJsonOSX, ParseJsonOSX);
    }

    static void WaitJsonLinux() {
        Wait(wwwJsonLinux, WaitJsonLinux, ParseJsonLinux);
    }

    static void WaitHtml() {
        Wait(wwwHtml, WaitHtml, ParseHtml);
    }

    static void Wait(WWW www, EditorApplication.CallbackFunction caller, Action<WWW> action) {
        if (www != null && www.isDone) {
            EditorApplication.update -= caller;
            if (string.IsNullOrEmpty(www.error) && www.bytesDownloaded > 0) {
                //if (isDebug) Debug.LogWarningFormat("{0} kB: {1}", www.size/1024, www.url);
                if (action != null) action(www);
            } else {
                if (www.error.StartsWith("403") || www.error.StartsWith("404")) {
                    if (action != null) action(www);
                }
                if (isDebug) Debug.LogWarningFormat("{0} <b>{1}</b>", www.error, www.url);
            }
        }
    }

    #endregion

    #region Actions after download

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

    static void ParseList(WWW list) {
        string[] files = list.text.Split('\n');
        string[] parts;
        string comppressedUrl = null;
        for (int i = 0; i < files.Length; i++) {
            parts = files[i].Split(',');
            // for 2021 (and later?) UnityYAMLMerge returns incorrect information
            // and a larger UnityShaderCompiler should be used
            baseName = selectedVersion.StartsWith("2021") ? shaderCompiler : yamlMerge;
            if (parts[0].Contains(ExtractedName)) {
                comppressedUrl = string.Format("{0}{1}/{2}", serverUrl, parts[0].Trim('\"').Replace('\\', '/'), CompressedName);
                DownloadCompressed(comppressedUrl);
                break;
            }
        }
        if (string.IsNullOrEmpty(comppressedUrl) && ReleaseCallback != null) ReleaseCallback();
    }

    static void ParseIniWin(WWW ini) {
        if (string.IsNullOrEmpty(ini.error)) iniWinDownloaded = true;
        ParseIni(ini, out dictIniWin);
    }

    static void ParseIniOSX(WWW ini) {
        ParseIni(ini, out dictIniOSX);
    }

    static void ParseIniLinux(WWW ini) {
        hasLinux = wwwIniLinux != null && string.IsNullOrEmpty(wwwIniLinux.error);
        if (hasLinux) {
            ParseIni(ini, out dictIniLinux);
            #if UNITY_5_5_OR_NEWER
            if (Application.platform == RuntimePlatform.LinuxEditor) {
                idxOS = 2;
                window.Repaint();
            }
            #endif
        }
    }

    static void ParseIni(WWW ini, out Dictionary<string, Dictionary<string, string>> dictIni) {
        string[] lines = ini.text.Split('\n');
        string section = null;
        Dictionary<string, string> dict = null;
        dictIni = new Dictionary<string, Dictionary<string, string>>();
        for (int i = 0; i < lines.Length; i++) {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            if (line.StartsWith("[") && line.EndsWith("]")) {
                if (section != null && dict != null) {
                    dictIni.Add(section, dict);
                }
                section = line.Substring(1, line.Length - 2);
                dict = new Dictionary<string, string>();
            } else {
                if (dict == null) continue;
                var parts = line.Split('=');
                if (parts.Length > 1) {
                    dict.Add(parts[0].Trim(), parts[1].Trim());
                }
            }
        }
        if (section != null && dict != null) {
            dictIni.Add(section, dict);
        }
        window.Repaint();
    }

    static void ParseReleaseNotes(WWW www) {
        bool err403 = www.text.Contains("403</h1>") || (!string.IsNullOrEmpty(www.error) && www.error.StartsWith("403"));
        bool err404 = www.text.Contains("404</h1>") || (!string.IsNullOrEmpty(www.error) && www.error.StartsWith("404"));
        if (!string.IsNullOrEmpty(www.error) || err403 || err404) {
            if ((selectedVersion.Contains("f") || selectedVersion.Contains("b")) && repeatRN < 2) {
                repeatRN++;
                string url = VersionToReleaseNotesUrl(selectedVersion, repeatRN);
                DownloadReleaseNotes(url);
                return;
            } else {
                wwwReleaseNotes = null;
            }
        }
        hasReleaseNotes = wwwReleaseNotes != null && string.IsNullOrEmpty(wwwReleaseNotes.error) && !err403 && !err404;
        if (hasReleaseNotes) {
            int idx = www.text.IndexOf(searchInRN);
            if (idx != -1) {
                string part = www.text.Substring(idx + searchInRN.Length, 31);
                string[] parts = part.Split(splitInRN, StringSplitOptions.RemoveEmptyEntries);
                string foundRevision = parts[0];
                if (isDebug)  Debug.LogFormat("ParseReleaseNotes: <b>{0}</b> found=<b>{1}</b>", selectedVersion, foundRevision);
            }
            window.Repaint();
        }
    }

    static void ProcessTorrent(WWW www) {
        if (!string.IsNullOrEmpty(www.error) || www.text.Contains("403</h1>") || www.text.Contains("404</h1>")) {
            wwwTorrent = null;
        }
        hasTorrent = wwwTorrent != null && string.IsNullOrEmpty(wwwTorrent.error);
        if (hasTorrent) {
            SaveTorrent(wwwTorrent);
            window.Repaint();
        }
    }

    static void SaveTorrent(WWW torrent) {
        if (!Directory.Exists(tempDir)) {
            Directory.CreateDirectory(tempDir);
        }
        string path = Path.Combine(tempDir, torrentFile);
        File.WriteAllBytes(path, torrent.bytes);
    }

    static void StartTorrent() {
        string path = Path.Combine(tempDir, torrentFile);
        if (File.Exists(path)) {
            Application.OpenURL(path);
        } else {
            Application.OpenURL(wwwTorrent.url);
        }
    }

    static void SaveCompressed(WWW compressed) {
        if (!Directory.Exists(tempDir)) {
            Directory.CreateDirectory(tempDir);
        }
        string path = Path.Combine(tempDir, CompressedName);
        File.WriteAllBytes(path, compressed.bytes);
        ExtractCompressed(path);
    }

    static void ExtractCompressed(string path) {
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

    static void ParseGithub(WWW github) {
        release = JsonUtility.FromJson<GithubRelease>(github.text);
        if (string.IsNullOrEmpty(release.created_at)) return;
        string current = EditorPrefs.GetString(prefs + Application.productName, nullDT);
        if (DateTime.ParseExact(release.created_at, universalDT, CultureInfo.InvariantCulture) > 
            DateTime.ParseExact(current, universalDT, CultureInfo.InvariantCulture)) {
            hasUpdate = true;
            if (window != null) window.Repaint();
        }
    }

    static void ImportPackage(WWW package) {
        tempDir = SetTempDir();
        string name = string.Format("{0}.unitypackage", scriptName);
        string path = Path.Combine(tempDir, name);
        File.WriteAllBytes(path, package.bytes);
        AssetDatabase.ImportPackage(path, false);
        EditorPrefs.SetString(prefs + Application.productName, release.created_at);
        Debug.LogFormat("{0} updated from Github", scriptName);
    }

    static string SetTempDir() {
        string result = string.Format("{0}/../Temp/{1}", Application.dataPath, scriptName);
        if (!Directory.Exists(result)) {
            Directory.CreateDirectory(result);
        }
        return result;
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
        string path = Path.Combine(tempDir, ExtractedName);
        if (File.Exists(path)) {
            string search = selectedVersion + "_";
            using (FileStream fs = File.OpenRead(path)) {
                byte[] b = new byte[3 * 1024 * 1024]; // 3 MB
                if (fs.Read(b,0,b.Length) > 0) {
                    string s = new UnicodeEncoding().GetString(b);
                    int idxVersion = s.IndexOf(search);
                    if (idxVersion > 0) {
                        string foundRevision = s.Substring(idxVersion + search.Length, 12);
                        if (foundRevision != selectedRevision) {
                            if (!string.IsNullOrEmpty(selectedRevision))
                                if (isDebug) Debug.LogFormat("SearchVersion: {0} != {1}", selectedRevision, foundRevision);
                            selectedRevision = foundRevision;
                            EditorPrefs.SetString(prefs + currentList.Keys[idxSelectedInCurrent], selectedRevision);
                            if (ReleaseCallback != null) ReleaseCallback();
                            window.Repaint();
                        }
                    }
                }
            }
            if (!isDebug) {
                FileUtil.DeleteFileOrDirectory(Path.GetDirectoryName(path));
            }
        } else {
            Debug.LogErrorFormat("Not found: {0}", path);
        }
    }

    static void ParseJsonWin(WWW json) {
        ParseJson(json, out jsonWin);
    }

    static void ParseJsonOSX(WWW json) {
        ParseJson(json, out jsonOSX);
    }

    static void ParseJsonLinux(WWW json) {
        ParseJson(json, out jsonLinux);
    }

    static void ParseJson(WWW jsonWWW, out JsonOS jsonOS) {
        jsonOS = JsonUtility.FromJson<JsonOS>(jsonWWW.text);
    }

    static void ParseHtml(WWW html) {
        string[] strings = html.text.Split('\n');
        for (int i = 0; i < strings.Length; i++) {
            if (strings[i].Contains("<title>") || strings[i].Contains("<h2>") || strings[i].Contains("<h3>")) {
                string[] parts = strings[i].TrimStart(' ').Split(' ');
                if (parts.Length > 1) {
                    selectedRevision = filterString;
                    selectedVersion = parts[1];
                    DownloadInfo(selectedRevision, selectedVersion, UpdateInfo);
                    break;
                }
            }
        }
    }

    #endregion

    #pragma warning restore 0414, 0618
}