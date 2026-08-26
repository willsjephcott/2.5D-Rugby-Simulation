using UnityEngine;
using System.Collections.Generic;

public class LineoutJumpQueue
{
    Queue<Vector3> slotPositions = new Queue<Vector3>();
    Queue<Player> assignedPlayers = new Queue<Player>();

    //returns the number of slot positions
    public int Count { get { return slotPositions.Count; } }
    public void Enqueue(Player player, Vector3 slotPosition)
    {
        assignedPlayers.Enqueue(player);
        slotPositions.Enqueue(slotPosition);
    }

    public bool Dequeue(out Player player, out Vector3 position)
    {
        if (assignedPlayers.Count == 0)
        {
            player = null;
            position = Vector3.zero;
            return false;
        }

        player = assignedPlayers.Dequeue();
        position = slotPositions.Dequeue();
        return true;
    }
    public Vector3 GetSlotPosition(int index)
    {
        Vector3[] positions = slotPositions.ToArray();
        if (index < 0 || index >= positions.Length) return Vector3.zero;
        return positions[index];
    }

    public Player GetPlayer(int index)
    {
        Player[] players = assignedPlayers.ToArray();
        if (index < 0 || index >= players.Length) return null;
        return players[index];
    }

    public void Clear()
    {
        slotPositions.Clear();
        assignedPlayers.Clear();
    }
}
