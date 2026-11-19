
using System.Collections.Generic;
using UnityEngine;

public class ChickenFormationController
{
    private readonly List<ChickenAI> members =
        new List<ChickenAI>();

    private readonly Camera targetCamera;
    private readonly float moveSpeed;
    private readonly float maxShift;
    private readonly float diveInterval;

    private float amplitude;
    private float movementStartTime;
    private float nextDiveTime;

    private int nextAttackerIndex;

    private bool started;

    private ChickenDiveAttack activeAttacker;

    public ChickenFormationController(
        Camera camera,
        float speed,
        float shift,
        float attackInterval)
    {
        targetCamera = camera;

        moveSpeed = Mathf.Max(0f, speed);

        maxShift = Mathf.Max(0f, shift);

        diveInterval = Mathf.Max(
            0.1f,
            attackInterval
        );
    }

    public void Register(ChickenAI chicken)
    {
        if (chicken != null &&
            !members.Contains(chicken))
        {
            members.Add(chicken);
        }
    }

    public Vector2 GetPosition(Vector2 slot)
    {
        if (!started || amplitude <= 0.001f)
            return slot;

        float elapsed = Mathf.Max(
            0f,
            Time.fixedTime - movementStartTime
        );

        float offset = Mathf.Sin(
            elapsed * moveSpeed / amplitude
        ) * amplitude;

        return slot + Vector2.right * offset;
    }

    public void Tick()
    {
        if (!started)
        {
            if (HasEnteringChickens())
                return;

            StartMovement();
        }

        UpdateDiveAttacks();
    }

    private bool HasEnteringChickens()
    {
        foreach (ChickenAI chicken in members)
        {
            if (chicken != null &&
                chicken.gameObject.activeInHierarchy &&
                chicken.IsEntering)
            {
                return true;
            }
        }

        return false;
    }

    private void StartMovement()
    {
        started = true;

        movementStartTime = Time.fixedTime;

        nextDiveTime = Time.time + diveInterval;

        CalculateAmplitude();
    }

    private void CalculateAmplitude()
    {
        if (targetCamera == null)
            return;

        float minX = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;

        float targetZ = 0f;
        bool foundMember = false;

        foreach (ChickenAI chicken in members)
        {
            if (chicken == null ||
                !chicken.gameObject.activeInHierarchy)
            {
                continue;
            }

            float x = chicken.FormationSlot.x;

            minX = Mathf.Min(minX, x);
            maxX = Mathf.Max(maxX, x);

            targetZ = chicken.transform.position.z;

            foundMember = true;
        }

        if (!foundMember)
            return;

        float distance = Mathf.Abs(
            targetCamera.transform.position.z - targetZ
        );

        float left = targetCamera.ViewportToWorldPoint(
            new Vector3(0f, 0.5f, distance)
        ).x + 0.8f;

        float right = targetCamera.ViewportToWorldPoint(
            new Vector3(1f, 0.5f, distance)
        ).x - 0.8f;

        float availableSpace = Mathf.Min(
            minX - left,
            right - maxX
        );

        amplitude = Mathf.Clamp(
            availableSpace,
            0f,
            maxShift
        );
    }

    private void UpdateDiveAttacks()
    {
        if (Time.time < nextDiveTime)
            return;

        if (activeAttacker != null &&
            activeAttacker.gameObject.activeInHierarchy &&
            activeAttacker.IsDiving)
        {
            return;
        }

        nextDiveTime = Time.time + diveInterval;

        PlayerHealth player =
            Object.FindFirstObjectByType<PlayerHealth>();

        if (player == null || player.IsDead)
            return;

        for (int i = 0; i < members.Count; i++)
        {
            int index =
                (nextAttackerIndex + i) % members.Count;

            ChickenAI chicken = members[index];

            if (chicken == null ||
                !chicken.gameObject.activeInHierarchy)
            {
                continue;
            }

            ChickenDiveAttack attacker =
                chicken.GetComponent<ChickenDiveAttack>();

            if (attacker == null)
                continue;

            if (attacker.TryStartDive(player.transform))
            {
                activeAttacker = attacker;

                nextAttackerIndex =
                    (index + 1) % members.Count;

                break;
            }
        }
    }
}
