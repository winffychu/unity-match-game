using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MemoryMatch.Core;
using MemoryMatch.Gameplay;

public static class MemoryMatchAutoBuilder
{
    [MenuItem("Tools/Memory Match/Build Playable Scene")]
    public static void BuildPlayableScene()
    {
        EnsureFolders();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MainScene";

        var canvas = CreateCanvas();
        CreateEventSystem();

        var gameManagerGO = new GameObject("GameManager");
        var gameManager = gameManagerGO.AddComponent<GameManager>();
        var levelManagerGO = new GameObject("LevelManager");
        var levelManager = levelManagerGO.AddComponent<LevelManager>();
        var uiManagerGO = new GameObject("UIManager");
        var uiManager = uiManagerGO.AddComponent<UIManager>();
        var audioManagerGO = new GameObject("AudioManager");
        var audioManager = audioManagerGO.AddComponent<AudioManager>();

        var bgm = audioManagerGO.AddComponent<AudioSource>();
        bgm.loop = true;
        var sfx = audioManagerGO.AddComponent<AudioSource>();
        var ui = audioManagerGO.AddComponent<AudioSource>();

        SetPrivateField(audioManager, "bgmSource", bgm);
        SetPrivateField(audioManager, "sfxSource", sfx);
        SetPrivateField(audioManager, "uiSource", ui);

        var mainMenu = CreatePanel("MainMenuPanel", canvas.transform, new Color(0.12f, 0.16f, 0.22f, 0.95f));
        var gameUI = CreatePanel("GameUIPanel", canvas.transform, new Color(0f, 0f, 0f, 0f));
        var resultPanel = CreatePanel("ResultPanel", canvas.transform, new Color(0.1f, 0.1f, 0.1f, 0.88f));
        var pausePanel = CreatePanel("PausePanel", canvas.transform, new Color(0.08f, 0.08f, 0.08f, 0.9f));
        var allDonePanel = CreatePanel("AllLevelsCompletePanel", canvas.transform, new Color(0.08f, 0.15f, 0.08f, 0.92f));
        resultPanel.SetActive(false);
        pausePanel.SetActive(false);
        allDonePanel.SetActive(false);

        var title = CreateText("Title", mainMenu.transform, "Memory Match", 48, TextAlignmentOptions.Center);
        Stretch(title.rectTransform, 0.2f, 0.68f, 0.8f, 0.88f);
        var startButton = CreateButton("StartButton", mainMenu.transform, "开始游戏");
        Stretch(startButton.GetComponent<RectTransform>(), 0.35f, 0.48f, 0.65f, 0.58f);
        var settingsButton = CreateButton("SettingsButton", mainMenu.transform, "设置(预留)");
        Stretch(settingsButton.GetComponent<RectTransform>(), 0.35f, 0.36f, 0.65f, 0.46f);
        var quitButton = CreateButton("QuitButton", mainMenu.transform, "退出");
        Stretch(quitButton.GetComponent<RectTransform>(), 0.35f, 0.24f, 0.65f, 0.34f);

        var topBar = new GameObject("TopBar", typeof(RectTransform));
        topBar.transform.SetParent(gameUI.transform, false);
        Stretch(topBar.GetComponent<RectTransform>(), 0.04f, 0.88f, 0.96f, 0.98f);
        var levelText = CreateText("LevelText", topBar.transform, "第 1 关", 30, TextAlignmentOptions.Left);
        Stretch(levelText.rectTransform, 0f, 0f, 0.25f, 1f);
        var flipText = CreateText("FlipText", topBar.transform, "翻牌: 0 / 0", 28, TextAlignmentOptions.Left);
        Stretch(flipText.rectTransform, 0.25f, 0f, 0.5f, 1f);
        var scoreText = CreateText("ScoreText", topBar.transform, "得分: 0", 28, TextAlignmentOptions.Left);
        Stretch(scoreText.rectTransform, 0.5f, 0f, 0.75f, 1f);
        var comboText = CreateText("ComboText", topBar.transform, "", 28, TextAlignmentOptions.Right);
        Stretch(comboText.rectTransform, 0.75f, 0f, 1f, 1f);

        var pauseButton = CreateButton("PauseButton", gameUI.transform, "暂停");
        Stretch(pauseButton.GetComponent<RectTransform>(), 0.82f, 0.78f, 0.94f, 0.86f);
        var undoButton = CreateButton("UndoButton", gameUI.transform, "撤销");
        Stretch(undoButton.GetComponent<RectTransform>(), 0.82f, 0.68f, 0.94f, 0.76f);
        var hintButton = CreateButton("HintButton", gameUI.transform, "提示(预留)");
        Stretch(hintButton.GetComponent<RectTransform>(), 0.78f, 0.58f, 0.94f, 0.66f);

        var board = new GameObject("CardBoard", typeof(RectTransform), typeof(Image), typeof(GridLayoutGroup));
        board.transform.SetParent(gameUI.transform, false);
        var boardRect = board.GetComponent<RectTransform>();
        Stretch(boardRect, 0.08f, 0.1f, 0.74f, 0.82f);
        board.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.08f);
        var grid = board.GetComponent<GridLayoutGroup>();
        grid.padding = new RectOffset(10, 10, 10, 10);
        grid.spacing = new Vector2(10, 10);
        grid.childAlignment = TextAnchor.MiddleCenter;

        var resultTitle = CreateText("ResultTitle", resultPanel.transform, "通关成功！", 42, TextAlignmentOptions.Center);
        Stretch(resultTitle.rectTransform, 0.2f, 0.66f, 0.8f, 0.82f);
        var resultScore = CreateText("ResultScoreText", resultPanel.transform, "得分: 0", 30, TextAlignmentOptions.Center);
        Stretch(resultScore.rectTransform, 0.25f, 0.5f, 0.75f, 0.62f);
        var resultCombo = CreateText("ResultComboText", resultPanel.transform, "最高连击: 0", 26, TextAlignmentOptions.Center);
        Stretch(resultCombo.rectTransform, 0.25f, 0.4f, 0.75f, 0.5f);
        var nextLevelButton = CreateButton("NextLevelButton", resultPanel.transform, "下一关");
        Stretch(nextLevelButton.GetComponent<RectTransform>(), 0.18f, 0.18f, 0.38f, 0.3f);
        var restartButton = CreateButton("RestartButton", resultPanel.transform, "重玩");
        Stretch(restartButton.GetComponent<RectTransform>(), 0.41f, 0.18f, 0.61f, 0.3f);
        var menuButton = CreateButton("MenuButton", resultPanel.transform, "主菜单");
        Stretch(menuButton.GetComponent<RectTransform>(), 0.64f, 0.18f, 0.84f, 0.3f);

        var resumeButton = CreateButton("ResumeButton", pausePanel.transform, "继续");
        Stretch(resumeButton.GetComponent<RectTransform>(), 0.35f, 0.5f, 0.65f, 0.6f);
        var pauseRestartButton = CreateButton("PauseRestartButton", pausePanel.transform, "重开本关");
        Stretch(pauseRestartButton.GetComponent<RectTransform>(), 0.35f, 0.36f, 0.65f, 0.46f);
        var pauseMenuButton = CreateButton("PauseMenuButton", pausePanel.transform, "返回菜单");
        Stretch(pauseMenuButton.GetComponent<RectTransform>(), 0.35f, 0.22f, 0.65f, 0.32f);

        var allDoneText = CreateText("AllDoneText", allDonePanel.transform, "全部关卡完成！", 44, TextAlignmentOptions.Center);
        Stretch(allDoneText.rectTransform, 0.2f, 0.56f, 0.8f, 0.74f);
        var allLevelsMenuButton = CreateButton("AllLevelsMenuButton", allDonePanel.transform, "返回主菜单");
        Stretch(allLevelsMenuButton.GetComponent<RectTransform>(), 0.34f, 0.28f, 0.66f, 0.4f);

        var comboEffect = CreateComboEffectPrefab();
        var cardPrefab = CreateCardPrefab();
        var sprites = CreateColorSprites();

        SetPrivateField(levelManager, "cardPrefab", AssetDatabase.LoadAssetAtPath<Card>("Assets/Prefabs/Card.prefab"));
        SetPrivateField(levelManager, "cardParent", board.transform);
        SetPrivateField(levelManager, "gridLayout", grid);
        SetPrivateField(levelManager, "cardBackSprite", sprites[0]);
        SetPrivateField(levelManager, "cardFrontSprites", sprites.GetRange(1, sprites.Count - 1));

        SetPrivateField(uiManager, "mainMenuPanel", mainMenu);
        SetPrivateField(uiManager, "startButton", startButton.GetComponent<Button>());
        SetPrivateField(uiManager, "settingsButton", settingsButton.GetComponent<Button>());
        SetPrivateField(uiManager, "quitButton", quitButton.GetComponent<Button>());
        SetPrivateField(uiManager, "gameUIPanel", gameUI);
        SetPrivateField(uiManager, "levelText", levelText);
        SetPrivateField(uiManager, "flipText", flipText);
        SetPrivateField(uiManager, "scoreText", scoreText);
        SetPrivateField(uiManager, "comboText", comboText);
        SetPrivateField(uiManager, "pauseButton", pauseButton.GetComponent<Button>());
        SetPrivateField(uiManager, "undoButton", undoButton.GetComponent<Button>());
        SetPrivateField(uiManager, "hintButton", hintButton.GetComponent<Button>());
        SetPrivateField(uiManager, "resultPanel", resultPanel);
        SetPrivateField(uiManager, "resultTitle", resultTitle);
        SetPrivateField(uiManager, "resultScoreText", resultScore);
        SetPrivateField(uiManager, "resultComboText", resultCombo);
        SetPrivateField(uiManager, "nextLevelButton", nextLevelButton.GetComponent<Button>());
        SetPrivateField(uiManager, "restartButton", restartButton.GetComponent<Button>());
        SetPrivateField(uiManager, "menuButton", menuButton.GetComponent<Button>());
        SetPrivateField(uiManager, "pausePanel", pausePanel);
        SetPrivateField(uiManager, "resumeButton", resumeButton.GetComponent<Button>());
        SetPrivateField(uiManager, "pauseRestartButton", pauseRestartButton.GetComponent<Button>());
        SetPrivateField(uiManager, "pauseMenuButton", pauseMenuButton.GetComponent<Button>());
        SetPrivateField(uiManager, "allLevelsCompletePanel", allDonePanel);
        SetPrivateField(uiManager, "allLevelsMenuButton", allLevelsMenuButton.GetComponent<Button>());
        SetPrivateField(uiManager, "comboEffectPrefab", comboEffect);
        SetPrivateField(uiManager, "comboEffectParent", gameUI.transform);

        SetPrivateField(gameManager, "levelManager", levelManager);
        SetPrivateField(gameManager, "uiManager", uiManager);
        SetPrivateField(gameManager, "audioManager", audioManager);

        gameUI.SetActive(false);
        resultPanel.SetActive(false);
        pausePanel.SetActive(false);
        allDonePanel.SetActive(false);

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainScene.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/MainScene.unity", true) };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity");
        Debug.Log("Memory Match scene built at Assets/Scenes/MainScene.unity");
    }

    static void EnsureFolders()
    {
        string[] folders = { "Assets/Scenes", "Assets/Prefabs", "Assets/Art", "Assets/Editor" };
        foreach (var folder in folders)
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder(System.IO.Path.GetDirectoryName(folder).Replace('\\', '/'), System.IO.Path.GetFileName(folder));
    }

    static Canvas CreateCanvas()
    {
        var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        return canvas;
    }

    static void CreateEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null) return;
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    static GameObject CreatePanel(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        go.transform.SetParent(parent, false);
        Stretch(go.GetComponent<RectTransform>(), 0f, 0f, 1f, 1f);
        go.GetComponent<Image>().color = color;
        return go;
    }

    static TMP_Text CreateText(string name, Transform parent, string text, int size, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        return tmp;
    }

    static GameObject CreateButton(string name, Transform parent, string label)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = new Color(0.2f, 0.45f, 0.85f, 0.95f);
        var text = CreateText("Label", go.transform, label, 28, TextAlignmentOptions.Center);
        Stretch(text.rectTransform, 0f, 0f, 1f, 1f);
        return go;
    }

    static GameObject CreateComboEffectPrefab()
    {
        var go = new GameObject("ComboEffect", typeof(RectTransform), typeof(CanvasGroup));
        var text = CreateText("ComboText", go.transform, "x2 连击!", 36, TextAlignmentOptions.Center);
        Stretch(text.rectTransform, 0f, 0f, 1f, 1f);
        PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/ComboEffect.prefab");
        Object.DestroyImmediate(go);
        return AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ComboEffect.prefab");
    }

    static GameObject CreateCardPrefab()
    {
        var root = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Card));
        var cardImage = root.GetComponent<Image>();
        cardImage.color = Color.white;
        var button = root.GetComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;

        var front = new GameObject("FrontImage", typeof(RectTransform), typeof(Image));
        front.transform.SetParent(root.transform, false);
        Stretch(front.GetComponent<RectTransform>(), 0.08f, 0.08f, 0.92f, 0.92f);
        var back = new GameObject("BackImage", typeof(RectTransform), typeof(Image));
        back.transform.SetParent(root.transform, false);
        Stretch(back.GetComponent<RectTransform>(), 0.08f, 0.08f, 0.92f, 0.92f);

        var card = root.GetComponent<Card>();
        SetPrivateField(card, "cardImage", cardImage);
        SetPrivateField(card, "frontImage", front.GetComponent<Image>());
        SetPrivateField(card, "backImage", back.GetComponent<Image>());
        PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Card.prefab");
        Object.DestroyImmediate(root);
        return AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Card.prefab");
    }

    static List<Sprite> CreateColorSprites()
    {
        var colors = new List<Color>
        {
            new Color(0.15f, 0.15f, 0.2f),
            new Color(0.91f, 0.3f, 0.24f),
            new Color(0.18f, 0.8f, 0.44f),
            new Color(0.2f, 0.6f, 0.86f),
            new Color(0.95f, 0.77f, 0.06f),
            new Color(0.61f, 0.35f, 0.71f),
            new Color(0.9f, 0.49f, 0.13f),
            new Color(0.1f, 0.74f, 0.61f),
            new Color(0.8f, 0.2f, 0.55f)
        };
        var sprites = new List<Sprite>();
        for (int i = 0; i < colors.Count; i++)
        {
            string path = $"Assets/Art/CardColor_{i}.png";
            System.IO.File.WriteAllBytes(path, MakeTexture(colors[i]).EncodeToPNG());
        }
        AssetDatabase.Refresh();
        for (int i = 0; i < colors.Count; i++)
        {
            string path = $"Assets/Art/CardColor_{i}.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
            sprites.Add(AssetDatabase.LoadAssetAtPath<Sprite>(path));
        }
        return sprites;
    }

    static Texture2D MakeTexture(Color color)
    {
        var tex = new Texture2D(256, 356, TextureFormat.RGBA32, false);
        var pixels = new Color[256 * 356];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    static void Stretch(RectTransform rt, float minX, float minY, float maxX, float maxY)
    {
        rt.anchorMin = new Vector2(minX, minY);
        rt.anchorMax = new Vector2(maxX, maxY);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void SetPrivateField(object target, string fieldName, object value)
    {
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
        var field = target.GetType().GetField(fieldName, flags);
        if (field != null) field.SetValue(target, value);
    }
}
