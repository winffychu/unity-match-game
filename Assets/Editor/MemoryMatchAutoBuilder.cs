using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MemoryMatch.Core;
using MemoryMatch.Gameplay;

public static class MemoryMatchAutoBuilder
{
    [MenuItem("Tools/Memory Match/Build Playable Scene")]
    public static void BuildPlayableScene()
    {
        EnsureFolders();
        CreateCardPrefab();

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
        var startButton = CreateButton("StartButton", mainMenu.transform, "开始挑战");
        Stretch(startButton.GetComponent<RectTransform>(), 0.35f, 0.48f, 0.65f, 0.58f);
        var quitButton = CreateButton("QuitButton", mainMenu.transform, "退出");
        Stretch(quitButton.GetComponent<RectTransform>(), 0.35f, 0.32f, 0.65f, 0.42f);

        var topBar = new GameObject("TopBar", typeof(RectTransform));
        topBar.transform.SetParent(gameUI.transform, false);
        Stretch(topBar.GetComponent<RectTransform>(), 0.04f, 0.88f, 0.96f, 0.98f);
        var levelText = CreateText("LevelText", topBar.transform, "第 1 关", 30, TextAlignmentOptions.Left);
        Stretch(levelText.rectTransform, 0f, 0f, 0.3f, 1f);
        var flipText = CreateText("FlipText", topBar.transform, "翻牌次数: 0 / 0", 28, TextAlignmentOptions.Left);
        Stretch(flipText.rectTransform, 0.3f, 0f, 0.75f, 1f);

        var pauseButton = CreateButton("PauseButton", gameUI.transform, "暂停");
        Stretch(pauseButton.GetComponent<RectTransform>(), 0.82f, 0.78f, 0.94f, 0.86f);
        var undoButton = CreateButton("UndoButton", gameUI.transform, "撤销翻牌");
        Stretch(undoButton.GetComponent<RectTransform>(), 0.78f, 0.66f, 0.94f, 0.74f);

        var board = new GameObject("CardBoard", typeof(RectTransform), typeof(Image), typeof(GridLayoutGroup));
        board.transform.SetParent(gameUI.transform, false);
        var boardRect = board.GetComponent<RectTransform>();
        Stretch(boardRect, 0.08f, 0.1f, 0.74f, 0.82f);
        board.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.08f);
        var grid = board.GetComponent<GridLayoutGroup>();
        grid.padding = new RectOffset(10, 10, 10, 10);
        grid.spacing = new Vector2(10, 10);
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;
        grid.cellSize = new Vector2(160f, 160f);

        var resultTitle = CreateText("ResultTitle", resultPanel.transform, "通关成功", 42, TextAlignmentOptions.Center);
        Stretch(resultTitle.rectTransform, 0.2f, 0.66f, 0.8f, 0.82f);
        var resultDetail = CreateText("ResultDetailText", resultPanel.transform, "已在 0 / 0 次翻牌内完成挑战", 28, TextAlignmentOptions.Center);
        Stretch(resultDetail.rectTransform, 0.2f, 0.46f, 0.8f, 0.58f);
        var nextLevelButton = CreateButton("NextLevelButton", resultPanel.transform, "下一关");
        Stretch(nextLevelButton.GetComponent<RectTransform>(), 0.18f, 0.18f, 0.38f, 0.3f);
        var restartButton = CreateButton("RestartButton", resultPanel.transform, "重新挑战");
        Stretch(restartButton.GetComponent<RectTransform>(), 0.41f, 0.18f, 0.61f, 0.3f);
        var menuButton = CreateButton("MenuButton", resultPanel.transform, "主菜单");
        Stretch(menuButton.GetComponent<RectTransform>(), 0.64f, 0.18f, 0.84f, 0.3f);

        var resumeButton = CreateButton("ResumeButton", pausePanel.transform, "继续");
        Stretch(resumeButton.GetComponent<RectTransform>(), 0.35f, 0.5f, 0.65f, 0.6f);
        var pauseRestartButton = CreateButton("PauseRestartButton", pausePanel.transform, "重新挑战");
        Stretch(pauseRestartButton.GetComponent<RectTransform>(), 0.35f, 0.36f, 0.65f, 0.46f);
        var pauseMenuButton = CreateButton("PauseMenuButton", pausePanel.transform, "返回菜单");
        Stretch(pauseMenuButton.GetComponent<RectTransform>(), 0.35f, 0.22f, 0.65f, 0.32f);

        var allDoneText = CreateText("AllDoneText", allDonePanel.transform, "全部关卡完成！", 44, TextAlignmentOptions.Center);
        Stretch(allDoneText.rectTransform, 0.2f, 0.56f, 0.8f, 0.74f);
        var allLevelsMenuButton = CreateButton("AllLevelsMenuButton", allDonePanel.transform, "返回主菜单");
        Stretch(allLevelsMenuButton.GetComponent<RectTransform>(), 0.34f, 0.28f, 0.66f, 0.4f);

        var sprites = CreateColorSprites();
        SetPrivateField(levelManager, "cardPrefab", AssetDatabase.LoadAssetAtPath<Card>("Assets/Prefabs/Card.prefab"));
        SetPrivateField(levelManager, "cardParent", board.transform);
        SetPrivateField(levelManager, "cardFrontSprites", sprites);

        SetPrivateField(uiManager, "mainMenuPanel", mainMenu);
        SetPrivateField(uiManager, "startButton", startButton.GetComponent<Button>());
        SetPrivateField(uiManager, "quitButton", quitButton.GetComponent<Button>());
        SetPrivateField(uiManager, "gameUIPanel", gameUI);
        SetPrivateField(uiManager, "levelText", levelText);
        SetPrivateField(uiManager, "flipText", flipText);
        SetPrivateField(uiManager, "undoButton", undoButton.GetComponent<Button>());
        SetPrivateField(uiManager, "resultPanel", resultPanel);
        SetPrivateField(uiManager, "resultTitle", resultTitle);
        SetPrivateField(uiManager, "resultDetailText", resultDetail);
        SetPrivateField(uiManager, "nextLevelButton", nextLevelButton.GetComponent<Button>());
        SetPrivateField(uiManager, "restartButton", restartButton.GetComponent<Button>());
        SetPrivateField(uiManager, "menuButton", menuButton.GetComponent<Button>());
        SetPrivateField(uiManager, "pausePanel", pausePanel);
        SetPrivateField(uiManager, "resumeButton", resumeButton.GetComponent<Button>());
        SetPrivateField(uiManager, "pauseRestartButton", pauseRestartButton.GetComponent<Button>());
        SetPrivateField(uiManager, "pauseMenuButton", pauseMenuButton.GetComponent<Button>());
        SetPrivateField(uiManager, "pauseButton", pauseButton.GetComponent<Button>());
        SetPrivateField(uiManager, "allLevelsCompletePanel", allDonePanel);
        SetPrivateField(uiManager, "allLevelsMenuButton", allLevelsMenuButton.GetComponent<Button>());

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

    static GameObject CreateButton(string name, Transform parent, string text)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = new Color(0.22f, 0.48f, 0.85f, 0.95f);
        var label = CreateText("Label", go.transform, text, 28, TextAlignmentOptions.Center);
        Stretch(label.rectTransform, 0f, 0f, 1f, 1f);
        var colors = go.GetComponent<Button>().colors;
        colors.normalColor = new Color(0.22f, 0.48f, 0.85f, 0.95f);
        colors.highlightedColor = new Color(0.32f, 0.58f, 0.95f, 1f);
        colors.pressedColor = new Color(0.18f, 0.38f, 0.7f, 1f);
        go.GetComponent<Button>().colors = colors;
        return go;
    }

    static void CreateCardPrefab()
    {
        var root = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Button), typeof(CanvasGroup), typeof(Card));
        var rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(180f, 180f);
        root.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);

        var front = new GameObject("Front", typeof(RectTransform), typeof(Image));
        front.transform.SetParent(root.transform, false);
        Stretch(front.GetComponent<RectTransform>(), 0.08f, 0.08f, 0.92f, 0.92f);

        var back = new GameObject("Back", typeof(RectTransform), typeof(Image));
        back.transform.SetParent(root.transform, false);
        Stretch(back.GetComponent<RectTransform>(), 0.08f, 0.08f, 0.92f, 0.92f);
        back.GetComponent<Image>().color = new Color(0.14f, 0.22f, 0.42f, 1f);

        var matched = new GameObject("MatchedEffect", typeof(RectTransform), typeof(Image));
        matched.transform.SetParent(root.transform, false);
        Stretch(matched.GetComponent<RectTransform>(), 0f, 0f, 1f, 1f);
        matched.GetComponent<Image>().color = new Color(0.35f, 0.85f, 0.45f, 0.28f);
        matched.SetActive(false);

        var card = root.GetComponent<Card>();
        SetPrivateField(card, "frontImage", front.GetComponent<Image>());
        SetPrivateField(card, "backImage", back.GetComponent<Image>());
        SetPrivateField(card, "matchedEffect", matched);

        PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Card.prefab");
        Object.DestroyImmediate(root);
    }

    static List<Sprite> CreateColorSprites()
    {
        var colors = new[]
        {
            new Color(0.91f, 0.34f, 0.34f),
            new Color(0.34f, 0.78f, 0.42f),
            new Color(0.26f, 0.58f, 0.93f),
            new Color(0.93f, 0.72f, 0.24f),
            new Color(0.69f, 0.38f, 0.91f),
            new Color(0.2f, 0.8f, 0.8f),
            new Color(0.95f, 0.55f, 0.2f),
            new Color(0.95f, 0.35f, 0.65f)
        };

        var sprites = new List<Sprite>();
        for (int i = 0; i < colors.Length; i++)
        {
            var tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            var pixels = new Color[128 * 128];
            for (int p = 0; p < pixels.Length; p++)
                pixels[p] = colors[i];
            tex.SetPixels(pixels);
            tex.Apply();

            var png = tex.EncodeToPNG();
            var path = $"Assets/Art/card_{i}.png";
            System.IO.File.WriteAllBytes(path, png);
            Object.DestroyImmediate(tex);
        }

        AssetDatabase.Refresh();

        for (int i = 0; i < colors.Length; i++)
        {
            var path = $"Assets/Art/card_{i}.png";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
                sprites.Add(sprite);
        }

        return sprites;
    }

    static void Stretch(RectTransform rect, float minX, float minY, float maxX, float maxY)
    {
        rect.anchorMin = new Vector2(minX, minY);
        rect.anchorMax = new Vector2(maxX, maxY);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static void SetPrivateField(object target, string fieldName, object value)
    {
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
        var field = target.GetType().GetField(fieldName, flags);
        if (field == null)
        {
            Debug.LogWarning($"Field not found: {target.GetType().Name}.{fieldName}");
            return;
        }
        field.SetValue(target, value);
    }
}
