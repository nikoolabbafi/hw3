using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public sealed class GameFlowOverlay : MonoBehaviour
{
    private static GameFlowOverlay instance;

    private Canvas canvas;
    private GameObject panel;
    private Text titleText;
    private Text messageText;
    private Button mainMenuButton;
    private Button quitButton;
    private EventSystem persistentEventSystem;
    private Font uiFont;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        EnsureInstance();
    }

    private static GameFlowOverlay EnsureInstance()
    {
        if (instance != null)
            return instance;

        instance = FindFirstObjectByType<GameFlowOverlay>();
        if (instance != null)
            return instance;

        GameObject root = new GameObject("GameFlowOverlay");
        instance = root.AddComponent<GameFlowOverlay>();
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        uiFont = LoadBuiltinFont();
        BuildCanvas();
        EnsurePersistentEventSystem();
        SceneManager.sceneLoaded += HandleSceneLoaded;
        HidePanel();
    }

    public static void ShowLevelComplete(int completedLevel, int nextLevel)
    {
        GameFlowOverlay overlay = EnsureInstance();
        overlay.ShowPanel(
            $"LEVEL {completedLevel} COMPLETE",
            $"LOADING LEVEL {nextLevel}...",
            showButtons: false);

        Time.timeScale = 0f;
    }

    public static void ShowVictory()
    {
        GameFlowOverlay overlay = EnsureInstance();
        overlay.ShowPanel(
            "VICTORY!",
            "Emperor Galaxy has been defeated. The galaxy is safe!",
            showButtons: true);

        Time.timeScale = 0f;
    }

    public static void Hide()
    {
        if (instance == null)
            return;

        instance.HidePanel();
    }

    private void BuildCanvas()
    {
        GameObject canvasObject = new GameObject(
            "GameFlowCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        canvasObject.transform.SetParent(transform, false);

        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        panel = CreateUIObject("OverlayPanel", canvasObject.transform);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        Stretch(panelRect);

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.015f, 0.012f, 0.055f, 0.94f);

        titleText = CreateText(
            "Title",
            panel.transform,
            64,
            FontStyle.Bold,
            new Vector2(0.5f, 0.64f),
            new Vector2(1200f, 140f));
        titleText.color = new Color(0.78f, 0.62f, 1f, 1f);

        messageText = CreateText(
            "Message",
            panel.transform,
            30,
            FontStyle.Normal,
            new Vector2(0.5f, 0.49f),
            new Vector2(1300f, 120f));
        messageText.color = Color.white;

        mainMenuButton = CreateButton(
            "MainMenuButton",
            "MAIN MENU",
            panel.transform,
            new Vector2(-175f, -155f));
        mainMenuButton.onClick.AddListener(GoToMainMenu);

        quitButton = CreateButton(
            "QuitButton",
            "QUIT",
            panel.transform,
            new Vector2(175f, -155f));
        quitButton.onClick.AddListener(QuitGame);
    }

    private void ShowPanel(string title, string message, bool showButtons)
    {
        if (panel == null)
            BuildCanvas();

        titleText.text = title;
        messageText.text = message;
        mainMenuButton.gameObject.SetActive(showButtons);
        quitButton.gameObject.SetActive(showButtons);
        panel.SetActive(true);
    }

    private void HidePanel()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void GoToMainMenu()
    {
        if (!Application.CanStreamedLevelBeLoaded("MainMenu"))
        {
            Debug.LogError(
                "[GameFlowOverlay] MainMenu scene is missing from Build Profiles / Scene List.");
            return;
        }

        Time.timeScale = 1f;
        HidePanel();
        LevelTransitionManager.LoadScene("MainMenu");
    }

    private void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void EnsurePersistentEventSystem()
    {
        EventSystem current = EventSystem.current;
        if (current != null)
        {
            persistentEventSystem = current;
            DontDestroyOnLoad(current.gameObject);
            return;
        }

        GameObject eventSystemObject = new GameObject(
            "PersistentEventSystem",
            typeof(EventSystem));
        eventSystemObject.transform.SetParent(transform, false);

        Type inputSystemModule = Type.GetType(
            "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");

        if (inputSystemModule != null)
            eventSystemObject.AddComponent(inputSystemModule);
        else
            eventSystemObject.AddComponent<StandaloneInputModule>();

        persistentEventSystem = eventSystemObject.GetComponent<EventSystem>();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HidePanel();

        
        EventSystem[] systems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        foreach (EventSystem system in systems)
        {
            if (system != null && system != persistentEventSystem)
                Destroy(system.gameObject);
        }
    }

    private Text CreateText(
        string objectName,
        Transform parent,
        int fontSize,
        FontStyle style,
        Vector2 anchor,
        Vector2 size)
    {
        GameObject textObject = CreateUIObject(objectName, parent);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;

        Text text = textObject.AddComponent<Text>();
        text.font = uiFont;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.color = Color.white;
        text.text = string.Empty;
        return text;
    }

    private Button CreateButton(
        string objectName,
        string label,
        Transform parent,
        Vector2 position)
    {
        GameObject buttonObject = CreateUIObject(objectName, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(300f, 76f);
        rect.anchoredPosition = position;

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.25f, 0.16f, 0.48f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        Text buttonText = CreateText(
            "Label",
            buttonObject.transform,
            24,
            FontStyle.Bold,
            new Vector2(0.5f, 0.5f),
            new Vector2(280f, 68f));
        buttonText.text = label;
        buttonText.raycastTarget = false;

        return button;
    }

    private static GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private static Font LoadBuiltinFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        return font;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;

        if (instance == this)
            instance = null;
    }
}
