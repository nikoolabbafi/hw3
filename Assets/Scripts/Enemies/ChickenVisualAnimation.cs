
using UnityEngine;

[DisallowMultipleComponent]
public class ChickenVisualAnimation : MonoBehaviour
{
    [Header("Visual Reference")]

    [SerializeField]
    private Transform visual;

    [Header("Animation Settings")]

    [SerializeField, Min(0f)]
    private float animationSpeed = 3f;

    [SerializeField, Min(0f)]
    private float rotationAmount = 5f;

    [SerializeField, Range(0f, 0.2f)]
    private float scaleAmount = 0.04f;

    private Vector3 originalScale;

    private Quaternion originalRotation;

    private float animationTime;

    private void Awake()
    {
        if (visual == null)
            return;

        originalScale = visual.localScale;

        originalRotation = visual.localRotation;
    }

    private void OnEnable()
    {
        animationTime = Random.Range(0f, 6f);
    }

    private void Update()
    {
        if (visual == null)
            return;

        animationTime +=
            Time.deltaTime * animationSpeed;

        float movement = Mathf.Sin(
            animationTime
        );

        float angle =
            movement * rotationAmount;

        visual.localRotation =
            originalRotation *
            Quaternion.Euler(
                0f,
                0f,
                angle
            );

        visual.localScale = new Vector3(
            originalScale.x *
            (1f + movement * scaleAmount),

            originalScale.y *
            (1f - movement * scaleAmount),

            originalScale.z
        );
    }

    private void OnDisable()
    {
        if (visual == null)
            return;

        visual.localScale = originalScale;

        visual.localRotation = originalRotation;
    }
}
