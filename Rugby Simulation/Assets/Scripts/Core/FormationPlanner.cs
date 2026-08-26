using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FormationPlanner
{
    FormationSettings settings;

    //survives carrier changes (consistent throughout change)
    Dictionary<Player, PlayerSlotAssignment> stableAssignments = new Dictionary<Player, PlayerSlotAssignment>();

    //Tracks who was available last tick to detect change
    List<Player> lastAvailablePlayers = new List<Player>();

    public FormationPlanner(FormationSettings settings)
    {
        this.settings = settings;
    }

    public Dictionary<Player, Vector3> CalculateFormation(Transform carrier, List<Player> availablePlayers, Vector3 attackDirection)
    {
        if (!ValidateInputs(carrier, availablePlayers))
        {
            return new Dictionary<Player, Vector3>();
        }

        Vector3 normalisedAttackDirection = NormaliseAttackDirection(attackDirection);

        List<Player> nonCarrierPlayers = ExcludeCarrier(availablePlayers, carrier); //Exclude carrier before detection so passing never triggers reshuffle

        if (HasAvailablePlayersChanged(nonCarrierPlayers))
        {
            RebuildStableAssignments(nonCarrierPlayers);
            CacheAvailablePlayers(nonCarrierPlayers);
        }

        return BuildPositionsFromStableAssignments(carrier.position, normalisedAttackDirection, nonCarrierPlayers);
    }
    private List<Player> ExcludeCarrier(List<Player> players, Transform carrier)
    {
        List<Player> result = new List<Player>();

        foreach (Player player in players)
        {
            if (player.transform != carrier)
            {
                result.Add(player);
            }
        }

        return result;
    }
    private void RebuildStableAssignments(List<Player> availablePlayers)
    {
        Dictionary<Player, PlayerSlotAssignment> newAssignments = new Dictionary<Player, PlayerSlotAssignment>();

        int leftLane = 0;
        int rightLane = 0;

        availablePlayers.Sort(CompareByGroup);

        foreach (Player player in availablePlayers)
        {
            if (HasExistingAssignment(player))
            {
                // Keep their existing side - only update lane index
                PlayerSlotAssignment existing = stableAssignments[player];
                bool keepLeft = existing.isLeft;

                int lane;

                if (keepLeft)
                {
                    lane = leftLane;
                    leftLane++;
                }
                else
                {
                    lane = rightLane;
                    rightLane++;
                }
                newAssignments[player] = new PlayerSlotAssignment(lane, keepLeft);
            }
            else
            {
                // New player - assign to whichever side has fewer people
                bool assignLeft = leftLane <= rightLane;
                int lane;

                if (assignLeft)
                {
                    lane = leftLane;
                    leftLane++;
                }
                else
                {
                    lane = rightLane;
                    rightLane++;
                }
                newAssignments[player] = new PlayerSlotAssignment(lane, assignLeft);
            }
        }

        stableAssignments = newAssignments;
    }

    private int CompareByGroup(Player a, Player b)
    {
        int aScore;
        int bScore;

        if (a.playerGroup == PlayerGroup.Forward) aScore = 0;
        else aScore = 1;

        if (b.playerGroup == PlayerGroup.Forward) bScore = 0;
        else bScore = 1;

        return aScore.CompareTo(bScore);

    }
    
    // Converts lane assignments into positions in world (anchored around carrier)
    private Dictionary<Player, Vector3> BuildPositionsFromStableAssignments(Vector3 carrierPosition, Vector3 attackDirection, List<Player> availablePlayers)
    {
        Dictionary<Player, Vector3> positions = new Dictionary<Player, Vector3>();
        Vector3 lateral = CalculateLateralDirection(attackDirection);

        BuildPositionsRecursive(availablePlayers, 0, carrierPosition, attackDirection, lateral, positions);

        return positions;
    }

    private void BuildPositionsRecursive(List<Player> availablePlayers, int index, Vector3 carrierPosition, Vector3 attackDirection, Vector3 lateral, Dictionary<Player, Vector3> positions)
    {
        if (availablePlayers == null || index >= availablePlayers.Count)
        {
            return;
        }

        Player player = availablePlayers[index];

        if (player != null && stableAssignments.ContainsKey(player))
        {
            PlayerSlotAssignment assignment = stableAssignments[player];
            Vector3 slotPosition = CalculateSlotPosition(carrierPosition, attackDirection, lateral, assignment);
            positions[player] = slotPosition;
        }

        BuildPositionsRecursive(availablePlayers, index + 1, carrierPosition, attackDirection, lateral, positions);
    }
    //Calculates position in world of single slot
    private Vector3 CalculateSlotPosition(Vector3 carrierPosition, Vector3 attackDirection, Vector3 lateral, PlayerSlotAssignment assignment)
    {
        float depth = CalculateDepth(assignment.laneIndex);
        float width = CalculateWidth(assignment.laneIndex);
        float side;

        // Position behind carrier
        Vector3 position = carrierPosition - (attackDirection * depth);

        // Offset left or right
        if (assignment.isLeft)
        {
            side = -1f;
        }
        else
        {
            side = 1f;
        }

        Vector3 sideOffset = lateral * width * side;
        position += sideOffset;

        return position;
    }
    private bool HasAvailablePlayersChanged(List<Player> currentPlayers)
    {
        if (currentPlayers.Count != lastAvailablePlayers.Count)
        {
            return true;
        }

        HashSet<Player> currentSet = new HashSet<Player>(currentPlayers); // order doesn't matter so used to check if changes were made (players becoming unavailable)
        HashSet<Player> lastSet = new HashSet<Player>(lastAvailablePlayers); 

        return !currentSet.SetEquals(lastSet);
    }
    private void CacheAvailablePlayers(List<Player> players)
    {
        lastAvailablePlayers = new List<Player>(players);
    }
    private bool HasExistingAssignment(Player player)
    {
        return stableAssignments.ContainsKey(player);
    }

    private float CalculateDepth(int laneIndex)
    {
        return settings.firstLaneDepth + (laneIndex * settings.depthIncrement);
    }

    private float CalculateWidth(int laneIndex)
    {
        float baseWidth = (settings.forwardBaseWidth + settings.backBaseWidth) / 2f;
        return baseWidth + (laneIndex * settings.widthIncrement);
    }

    private Vector3 CalculateLateralDirection(Vector3 attackDirection)
    {
        return Vector3.Cross(attackDirection, Vector3.up).normalized;
    }

    private Vector3 NormaliseAttackDirection(Vector3 attackDirection)
    {
        Vector3 normalised = attackDirection;
        normalised.y = 0f;
        if (normalised.sqrMagnitude < 0.01f) normalised = Vector3.forward;
        return normalised.normalized;
    }

    private bool ValidateInputs(Transform carrier, List<Player> players)
    {
        if (carrier == null)
        {
            DebugLogNullCarrier();
            return false;
        }
        if (players == null || players.Count == 0)
        {
            return false;
        }
        return true;
    }
    private class PlayerSlotAssignment
    {
        public int laneIndex;
        public bool isLeft;

        public PlayerSlotAssignment(int lane, bool left)
        {
            laneIndex = lane;
            isLeft = left;
        }
    }
    private void DebugLogNullCarrier()
    {
        Debug.LogWarning("FormationPlanner: carrier is null, cannot calculate formation");
    }
    /* private List<FormationSlot> GenerateFormationSlots(Vector3 carrierPosition, Vector3 attackDirection, int forwardCount, int backCount)
     {
         List<FormationSlot> slots = new List<FormationSlot>();

         Vector3 lateralDirection = CalculateLateralDirection(attackDirection);

         int totalPlayers = forwardCount + backCount;
         int lanesToCreate = Mathf.Min(settings.numberOfLanes, totalPlayers);

         for (int laneIndex =  0;  laneIndex < lanesToCreate; laneIndex++)
         {
             CreateSlotsForLane(laneIndex, carrierPosition, attackDirection, lateralDirection, slots);
         }
         return slots;
     }
     private void CreateSlotsForLane(int laneIndex, Vector3 carrierPosition, Vector3 attackDirection, Vector3 LateralDirection, List<FormationSlot> slots)
     {
         float depth = CalculateLaneDepth(laneIndex);
         float width = CalculateLaneWidth(laneIndex);

         Vector3 laneCenter = CalculateLaneCenter(carrierPosition,attackDirection,depth);

         //Forwards in inside lanes (closer)
         PlayerGroup preferredGroup = DeterminePreferredGroup(laneIndex);

         Vector3 leftPosition = laneCenter - (LateralDirection * width);
         slots.Add(new FormationSlot(leftPosition, laneIndex, preferredGroup, isLeft: true));

         Vector3 rightPosition = laneCenter + (LateralDirection * width);
         slots.Add(new FormationSlot(rightPosition, laneIndex, preferredGroup, isLeft: false));
     }

     private Dictionary<Player, Vector3> AssignPlayersToSlots(List<Player> allPlayers, List<Player> forwards, List<Player> backs, List<FormationSlot> slots)
     {

         Dictionary<Player, Vector3> assignments = new Dictionary<Player, Vector3>();

         if (slots.Count == 0 || allPlayers.Count == 0) return assignments;
          // Sort slots by lane (closest first)
         List<FormationSlot> sortedSlots = SortSlotsByPriority(slots);

         // Assign forwards to forward-preferred slots first
         AssignGroupToPreferredSlots(forwards, sortedSlots, PlayerGroup.Forward, assignments);

         // Assign backs to back-preferred slots
         AssignGroupToPreferredSlots(backs, sortedSlots, PlayerGroup.Back, assignments);

         // Fill remaining slots with any available players
         FillRemainingSlots(allPlayers, sortedSlots, assignments);

         return assignments;
     }
     private void AssignGroupToPreferredSlots(List<Player> players, List<FormationSlot> slots, PlayerGroup prefferedGroup,  Dictionary<Player, Vector3> assignments)
     {
         foreach(Player player in players)
         {
             FormationSlot bestSlot = FindBestAvailableSlot(player, slots, assignments, prefferedGroup);

             if (bestSlot != null)
             {
                 assignments[player] = bestSlot.position;
                 lastAssignedLane[player] = bestSlot.laneIndex;
             }
         }
     }
     private void FillRemainingSlots(List<Player> allPlayers, List<FormationSlot> slots, Dictionary<Player, Vector3> assignments)
     {
         foreach (Player player in allPlayers)
         {
             if (assignments.ContainsKey(player)) continue;

             FormationSlot bestSlot = FindBestAvailableSlot(player, slots, assignments, null);

             if (bestSlot != null)
             {
                 assignments[player] = bestSlot.position;
                 lastAssignedLane[player] = bestSlot.laneIndex;
             }
         }
     }
     //Finds best slot for a player going from previously assigned lane to preferred group slot to closest available slot (high to low)
     private FormationSlot FindBestAvailableSlot(Player player, List<FormationSlot> slots, Dictionary<Player, Vector3> assignments, PlayerGroup? prefferedGroup) // ? means can be F,B or null
     {
         //Check where it was assigned last
         if (lastAssignedLane.ContainsKey(player))
         {
             int previousLane = lastAssignedLane[player];
             FormationSlot previousSlot = FindAvailableSlotInLane(previousLane, slots, assignments);
             if (previousSlot != null)
             {
                 return previousSlot;
             }
         }
         FormationSlot bestSlot = null;
         foreach(FormationSlot slot in slots)
         {
             if (IsSlotOccupied(slot, assignments))
             {
                 continue;
             }

             if (prefferedGroup.HasValue && slot.preferredGroup != prefferedGroup.Value)
             {
                 continue;
             }
             if (bestSlot == null)
             {
                 bestSlot = slot;
             }

         }
         if (bestSlot == null)
         {
             bestSlot = FindFirstAvailableSlot(slots, assignments);
         }
         return bestSlot;
     }
     private FormationSlot FindAvailableSlotInLane(int laneIndex, List<FormationSlot> slots, Dictionary<Player, Vector3> assignments)
     {
         foreach (FormationSlot slot in slots)
         {
             if (slot.laneIndex  == laneIndex && !IsSlotOccupied(slot, assignments))
             {
                 return slot;
             }
         }
         return null;
     }
     private FormationSlot FindFirstAvailableSlot(List<FormationSlot> slots, Dictionary<Player, Vector3> assignments)
     {
         foreach (FormationSlot slot in slots)
         {
             if (!IsSlotOccupied(slot, assignments))
             {
                 return slot;
             }
         }
         return null;
     }
     private bool IsSlotOccupied(FormationSlot slot, Dictionary<Player, Vector3> assignments)
     {
         foreach(Vector3 assignedPosition in assignments.Values)
         {
             if (Vector3.Distance(slot.position, assignedPosition) < 0.1f)
             {
                 return true;
             }
         }
         return false;
     }
     //sorts by lane index closest to carrier first
     private List<FormationSlot> SortSlotsByPriority(List<FormationSlot> slots)
     {
         return slots.OrderBy(slot => slot.laneIndex).ToList();
     }
     private List<Player> FilterPlayersByGroup(List<Player> players, PlayerGroup group)
     {
         List<Player> filtered = new List<Player>();

         foreach (Player player in players)
         {
             if(GetPlayerGroup(player) == group)
             {
                 filtered.Add(player);
             }
         }
         return filtered;
     }
     private PlayerGroup GetPlayerGroup(Player player) //This may not work
     {
         return player.playerGroup;
     }
     private float CalculateLaneDepth(int laneIndex)
     {
         return settings.firstLaneDepth + (laneIndex*settings.depthIncrement);
     }
     //Width increases for backs to create a nice shape
     private float CalculateLaneWidth(int laneIndex)
     {
         float baseWidth = (settings.forwardBaseWidth + settings.backBaseWidth) / 2f;
         return baseWidth * (1f + laneIndex + settings.widthIncrement);
     }
     private PlayerGroup DeterminePreferredGroup(int laneIndex)
     {
         if (!settings.forwardsInsideLanes)
         {
             if (laneIndex % 2 == 0)
             {
                 return PlayerGroup.Forward;
             }
             else
             {
                 return PlayerGroup.Back;
             }
         }
         else
         {
             // Inside lanes (0, 1) are forwards
             if (laneIndex < 2)
             {
                 return PlayerGroup.Forward;
             }
             else
             {
                 return PlayerGroup.Back;
             }
         }

     }
     private Vector3 CalculateLaneCenter(Vector3 carrierPosition, Vector3 attackDirection, float depth)
     {
         return carrierPosition - (attackDirection * depth);
     }

     //perp to attack direction

     private Vector3 NormaliseAttackDirection(Vector3 attackDirection)
     {
         Vector3 normalised = attackDirection;
         normalised.y = 0f;

         if(normalised.sqrMagnitude < 0.01f)
         {
             normalised = Vector3.forward;
         }
         return normalised.normalized;
     }
     private bool ValidateInputs(Transform carrier, List<Player> players, Vector3 attackDirection)
     {
         if (carrier == null)
         {
             DebugLogNullCarrier();
             return false;
         }

         if (players == null || players.Count == 0)
         {
             return false; // No players is valid state
         }

         return true;
     }
     private class FormationSlot
     {
         public Vector3 position;
         public int laneIndex;
         public PlayerGroup preferredGroup;
         public bool isLeft;

         public FormationSlot(Vector3 position, int laneIndex, PlayerGroup preferredGroup, bool isLeft)
         {
             this.position = position;
             this.laneIndex = laneIndex;
             this.preferredGroup = preferredGroup;
             this.isLeft = isLeft;
         }

     }*/

}