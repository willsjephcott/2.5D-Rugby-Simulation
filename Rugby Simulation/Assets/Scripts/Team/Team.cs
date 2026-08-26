using UnityEngine;
using System.Collections.Generic;

public class Team : MonoBehaviour
{
    public List<Player> players = new List<Player>();
    public Vector3 attackDirection = Vector3.forward;

    GameConfig config;
    private void Awake()
    {
        if (MatchManager.Instance != null)
        {
            config = MatchManager.Instance.config;
        }
        else
        {
            config = FindAnyObjectByType<GameConfig>();
        }
        // Auto-collect players
        var foundPlayers = GetComponentsInChildren<Player>();
        foreach (var p in foundPlayers)
        {
            if (p != null)
            {
                players.Add(p);
                p.team = this;
            }
        }
    }

    //Finds the best support player on a given side
    public Player FindBestPassTarget(Player carrier, bool leftSide)
    {
        if (carrier == null) return null;

        Player best = null;
        float bestDist = Mathf.Infinity;

        Vector3 carrierPos = carrier.transform.position;
        Vector3 atk = GetNormalizedAttackDirection();
        Vector3 leftVec = Vector3.Cross(atk, Vector3.up);
        Vector3 sideVec;

        if (leftSide)
        {
            sideVec = leftVec;
        }
        else
        {
            sideVec = -leftVec;
        }

        foreach (var p in players)
        {
            if (p == null || p == carrier) continue;

            Vector3 toPlayer = p.transform.position - carrierPos;

            // Must be behind or level (>0 = infront)
            float forwardDot = Vector3.Dot(toPlayer, atk);
            if (forwardDot > 0.01f) continue;

            // Must be on correct side (>0 = correct side)
            float sideDot = Vector3.Dot(toPlayer, sideVec);
            if (sideDot <= 0.01f) continue;

            float dist = toPlayer.sqrMagnitude;
            if (dist < bestDist)
            {
                bestDist = dist;
                best = p;
            }
        }
        return best;
    }

    //Calculates where the pass should land on ground if no player is suitable
    public Vector3 CalculateGroundPassTarget(Player carrier, bool leftSide)
    {
        if (carrier == null) return Vector3.zero;

        Vector3 atk = GetNormalizedAttackDirection();
        Vector3 leftVec = Vector3.Cross(Vector3.up, atk).normalized;
        Vector3 sideVec;

        if (leftSide)
        {
            sideVec = leftVec;
        }
        else
        {
            sideVec = -leftVec;
        }

        Vector3 target = carrier.transform.position - (atk * config.passBackwardDistance) + (sideVec * config.passLateralDistance);

        target.y = Mathf.Max(0f, target.y);
        return target;
    }

    private Vector3 GetNormalizedAttackDirection()
    {
        Vector3 atk = attackDirection;
        atk.y = 0f;
        if (atk.sqrMagnitude < 0.01f) atk = Vector3.forward;
        return atk.normalized;
    }
}
