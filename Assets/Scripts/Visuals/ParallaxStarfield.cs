using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ParallaxStarfield : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Starfield")]
    [SerializeField, Range(128, 1024)] private int textureSize = 512;
    [SerializeField] private int[] starCounts = { 75, 42, 20 };
    [SerializeField] private float[] scrollSpeeds = { 0.12f, 0.28f, 0.52f };
    [SerializeField] private int randomSeed = 303;
    [SerializeField] private int baseSortingOrder = -1000;

    [Header("Deep Space Palette")]
    [SerializeField] private Color backgroundColor = new Color(0.012f, 0.008f, 0.055f, 1f);
    [SerializeField] private Color[] layerTints =
    {
        new Color(0.42f, 0.48f, 1f, 0.34f),
        new Color(0.70f, 0.55f, 1f, 0.58f),
        new Color(0.82f, 0.91f, 1f, 0.88f)
    };

    private readonly List<Texture2D> generatedTextures = new List<Texture2D>();
    private readonly List<Sprite> generatedSprites = new List<Sprite>();
    private readonly List<LayerTiles> layers = new List<LayerTiles>();
    private SpriteRenderer baseRenderer;
    private bool built;
    private float lastViewWidth;
    private float lastViewHeight;

    private sealed class LayerTiles
    {
        public Transform first;
        public Transform second;
        public float speed;
    }

    public Camera TargetCamera
    {
        get => targetCamera;
        set => targetCamera = value;
    }

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
        {
            Debug.LogError("[ParallaxStarfield] No camera assigned and no MainCamera found.", this);
            enabled = false;
            return;
        }

        if (!targetCamera.orthographic)
        {
            Debug.LogWarning("[ParallaxStarfield] This setup is designed for an orthographic 2D camera.", this);
        }

        BuildStarfield();
    }

    private void LateUpdate()
    {
        if (!built || targetCamera == null)
            return;

        float viewHeight = targetCamera.orthographicSize * 2f;
        float viewWidth = viewHeight * targetCamera.aspect;

        if (!Mathf.Approximately(viewWidth, lastViewWidth) ||
            !Mathf.Approximately(viewHeight, lastViewHeight))
        {
            ResizeToCamera(viewWidth, viewHeight);
        }

        Vector3 cameraPosition = targetCamera.transform.position;
        if (baseRenderer != null)
            baseRenderer.transform.position = new Vector3(cameraPosition.x, cameraPosition.y, 0f);

        foreach (LayerTiles layer in layers)
        {
            MoveTile(layer.first, layer.second, layer.speed, cameraPosition.y, viewHeight);
            MoveTile(layer.second, layer.first, layer.speed, cameraPosition.y, viewHeight);

            Vector3 firstPosition = layer.first.position;
            firstPosition.x = cameraPosition.x;
            firstPosition.z = 0f;
            layer.first.position = firstPosition;

            Vector3 secondPosition = layer.second.position;
            secondPosition.x = cameraPosition.x;
            secondPosition.z = 0f;
            layer.second.position = secondPosition;
        }
    }

    private void BuildStarfield()
    {
        if (built)
            return;

        int layerCount = Mathf.Min(3, Mathf.Min(starCounts.Length, scrollSpeeds.Length));
        if (layerCount == 0)
        {
            Debug.LogError("[ParallaxStarfield] Configure at least one star layer.", this);
            return;
        }

        Sprite baseSprite = CreateSolidSprite(Color.white);
        GameObject baseObject = new GameObject("DeepSpace_BaseColor");
        baseObject.transform.SetParent(transform, false);
        baseRenderer = baseObject.AddComponent<SpriteRenderer>();
        baseRenderer.sprite = baseSprite;
        baseRenderer.color = backgroundColor;
        baseRenderer.sortingOrder = baseSortingOrder;

        for (int i = 0; i < layerCount; i++)
        {
            Texture2D texture = CreateStarTexture(
                textureSize,
                Mathf.Max(1, starCounts[i]),
                randomSeed + (i * 97));

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f);

            generatedSprites.Add(sprite);

            Color tint = i < layerTints.Length ? layerTints[i] : Color.white;
            LayerTiles layer = new LayerTiles
            {
                first = CreateTile($"Stars_Layer_{i + 1}_A", sprite, tint, baseSortingOrder + 10 + i).transform,
                second = CreateTile($"Stars_Layer_{i + 1}_B", sprite, tint, baseSortingOrder + 10 + i).transform,
                speed = Mathf.Max(0f, scrollSpeeds[i])
            };

            layers.Add(layer);
        }

        built = true;
        ResizeToCamera(
            targetCamera.orthographicSize * 2f * targetCamera.aspect,
            targetCamera.orthographicSize * 2f);

        Vector3 cameraPosition = targetCamera.transform.position;
        for (int i = 0; i < layers.Count; i++)
        {
            layers[i].first.position = new Vector3(cameraPosition.x, cameraPosition.y, 0f);
            layers[i].second.position = new Vector3(
                cameraPosition.x,
                cameraPosition.y + lastViewHeight,
                0f);
        }
    }

    private GameObject CreateTile(string tileName, Sprite sprite, Color tint, int sortingOrder)
    {
        GameObject tile = new GameObject(tileName);
        tile.transform.SetParent(transform, false);
        SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = tint;
        renderer.sortingOrder = sortingOrder;
        return tile;
    }

    private void ResizeToCamera(float viewWidth, float viewHeight)
    {
        lastViewWidth = viewWidth;
        lastViewHeight = viewHeight;

        if (baseRenderer != null && baseRenderer.sprite != null)
        {
            Vector2 size = baseRenderer.sprite.bounds.size;
            baseRenderer.transform.localScale = new Vector3(
                viewWidth / size.x,
                viewHeight / size.y,
                1f);
        }

        foreach (LayerTiles layer in layers)
        {
            ResizeTile(layer.first, viewWidth, viewHeight);
            ResizeTile(layer.second, viewWidth, viewHeight);
        }
    }

    private static void ResizeTile(Transform tile, float viewWidth, float viewHeight)
    {
        if (tile == null)
            return;

        SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
        if (renderer == null || renderer.sprite == null)
            return;

        Vector2 size = renderer.sprite.bounds.size;
        tile.localScale = new Vector3(
            viewWidth / size.x,
            viewHeight / size.y,
            1f);
    }

    private static void MoveTile(
        Transform tile,
        Transform otherTile,
        float speed,
        float cameraY,
        float viewHeight)
    {
        Vector3 position = tile.position;
        position.y -= speed * Time.deltaTime;

        if (position.y <= cameraY - viewHeight)
            position.y = otherTile.position.y + viewHeight;

        tile.position = position;
    }

    private Sprite CreateSolidSprite(Color color)
    {
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.SetPixels(new[] { color, color, color, color });
        texture.Apply();
        generatedTextures.Add(texture);

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, 2f, 2f),
            new Vector2(0.5f, 0.5f),
            100f);
        generatedSprites.Add(sprite);
        return sprite;
    }

    private Texture2D CreateStarTexture(int size, int count, int seed)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Repeat;

        Color32[] pixels = new Color32[size * size];
        System.Random random = new System.Random(seed);

        for (int i = 0; i < count; i++)
        {
            int x = random.Next(2, size - 2);
            int y = random.Next(2, size - 2);
            int radius = random.Next(1, 4);
            byte alpha = (byte)random.Next(110, 256);
            DrawStar(pixels, size, x, y, radius, alpha);

           
            if (y < 5)
                DrawStar(pixels, size, x, size - 1 - y, radius, alpha);
            else if (y > size - 6)
                DrawStar(pixels, size, x, size - 1 - y, radius, alpha);
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, false);
        generatedTextures.Add(texture);
        return texture;
    }

    private static void DrawStar(
        Color32[] pixels,
        int size,
        int centerX,
        int centerY,
        int radius,
        byte alpha)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if ((x * x) + (y * y) > radius * radius)
                    continue;

                int px = centerX + x;
                int py = centerY + y;
                if (px < 0 || px >= size || py < 0 || py >= size)
                    continue;

                byte localAlpha = (byte)Mathf.Clamp(alpha - (Mathf.Abs(x) + Mathf.Abs(y)) * 35, 35, 255);
                pixels[(py * size) + px] = new Color32(255, 255, 255, localAlpha);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (Sprite sprite in generatedSprites)
        {
            if (sprite != null)
                Destroy(sprite);
        }

        foreach (Texture2D texture in generatedTextures)
        {
            if (texture != null)
                Destroy(texture);
        }
    }
}
