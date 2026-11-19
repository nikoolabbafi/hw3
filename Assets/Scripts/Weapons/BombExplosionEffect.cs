
using UnityEngine;

[RequireComponent(typeof(PooledObject))]
[RequireComponent(typeof(SpriteRenderer))]
public class BombExplosionEffect : MonoBehaviour
{
    [Header("Explosion Animation")]

    [SerializeField, Min(0.05f)]
    private float duration = 0.45f;

    [SerializeField]
    private Vector3 endScale = new Vector3(2.4f, 2.4f, 1f);

    [SerializeField, Range(1f, 1.5f)]
    private float screenCoverageMargin = 1.08f;


    private SpriteRenderer spriteRenderer;
    private PooledObject pooledObject;

    private Vector3 startScale;
    private Color startColor;

    private float timer;


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        pooledObject = GetComponent<PooledObject>();

        startScale = transform.localScale;

        startColor = spriteRenderer.color;
    }


    private void OnEnable()
    {
        timer = 0f;

        transform.localScale = startScale;

        spriteRenderer.color = startColor;
    }



    public void ConfigureForCamera(Camera targetCamera)
    {
        if (targetCamera == null ||
            spriteRenderer == null ||
            spriteRenderer.sprite == null)
        {
            return;
        }


        float distance = Mathf.Abs(
            targetCamera.transform.position.z -
            transform.position.z
        );


        Vector3 bottomLeft =
            targetCamera.ViewportToWorldPoint(
                new Vector3(0f, 0f, distance)
            );

        Vector3 topRight =
            targetCamera.ViewportToWorldPoint(
                new Vector3(1f, 1f, distance)
            );


        float screenWidth =
            Mathf.Abs(topRight.x - bottomLeft.x);

        float screenHeight =
            Mathf.Abs(topRight.y - bottomLeft.y);


        Vector2 spriteSize =
            spriteRenderer.sprite.bounds.size;


        if (spriteSize.x <= 0f ||
            spriteSize.y <= 0f)
        {
            return;
        }


        float scaleX =
            screenWidth / spriteSize.x;

        float scaleY =
            screenHeight / spriteSize.y;


        float requiredScale =
            Mathf.Max(scaleX, scaleY) *
            screenCoverageMargin;


        endScale = new Vector3(
            requiredScale,
            requiredScale,
            startScale.z
        );
    }


    private void Update()
    {
        timer += Time.deltaTime;

        float progress = Mathf.Clamp01(
            timer / duration
        );


        transform.localScale = Vector3.Lerp(
            startScale,
            endScale,
            progress
        );


        Color color = startColor;

        color.a = Mathf.Lerp(
            startColor.a,
            0f,
            progress
        );

        spriteRenderer.color = color;


        if (timer >= duration)
        {
            pooledObject.ReturnToPool();
        }
    }


    private void OnValidate()
    {
        duration = Mathf.Max(0.05f, duration);

        screenCoverageMargin = Mathf.Clamp(
            screenCoverageMargin,
            1f,
            1.5f
        );
    }
}
