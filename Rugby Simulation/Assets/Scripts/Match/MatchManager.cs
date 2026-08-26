using System;
using UnityEngine;

//Tracks possession and manages ball events
public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance { get; private set; }
    public RuckManager GetActiveRuck()
    {
        return activeRuck;
    }

    public Team TeamA, TeamB;

    public Ball ball;
    public GameConfig config;
    public Team PossessionTeam { get; private set; }

    public event Action<Team> OnPossessionChanged;

    RuckManager activeRuck;
    LineoutManager activeLineout;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // Kickoff: give ball to first player on Team A (or whoever you want)
        if (ball == null || TeamA == null || TeamA.players.Count == 0) return;

        //ball.AttachTo(TeamA.players[0].transform);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); //Destroys duplicates (not itself) so there are no double updates/confliciting states etc (only 1 manager)
        }
        if (ball != null)
        {
            ball.OwnerChanged += OnBallOwnerChanged;
        }
    }
    public void RegisterRuck(RuckManager ruck)
    {
        activeRuck = ruck;
        ruck.OnRuckComplete += ClearActiveRuck;
        ruck.OnRuckComplete += ReEvaluateControllers;
    }
    public void SetConversionActive(bool active)
    {
        conversionActive = active;
    }

    bool conversionActive;

    public bool IsConversionActive()
    {
        return conversionActive;
    }

    private void ReEvaluateControllers()
    {
        // Re-trigger possession logic now the ruck is done
        OnPossessionChanged?.Invoke(PossessionTeam);
    }
    private void ClearActiveRuck()
    {
        activeRuck = null;
    }
    public void RegisterLineout(LineoutManager lineout)
    {
        activeLineout = lineout;
    }

    public void ClearActiveLineout()
    {
        activeLineout = null;
    }
    public bool IsLineoutActive()
    {
        return activeLineout != null;
    }

    private void Update()
    {
        activeRuck?.Tick(Time.deltaTime);
    }

    private void OnDestroy()
    {
        if (ball != null)
        {
            ball.OwnerChanged -= OnBallOwnerChanged;
        }
    }

    private void OnBallOwnerChanged(Transform oldHolder, Transform newHolder)
    {
        if (oldHolder != null)
        {
            var oldPlayer = oldHolder.GetComponentInParent<Player>();
            oldPlayer?.SetHasBall(false);  //if (oldPlayer != null) oldPlayer.SetHasBall(false);
        }
        if (newHolder != null)
        {
            var newPlayer = newHolder.GetComponentInParent<Player>();
            newPlayer?.SetHasBall(true); //if (newPlayer != null) newPlayer.SetHasBall(true);

            // Set possession to the player's team (requires player.Team to be set)
            if (newPlayer != null && newPlayer.team != null)
                SetPossession(newPlayer.team);
            else
                SetPossession(null); // if team not wired yet
        }
        else
        {
            // Ball is loose
            SetPossession(null);
        }
    }

    private void SetPossession(Team newTeam)
    {
        if (newTeam == PossessionTeam) return;
        PossessionTeam= newTeam;

        if (IsRuckActive() || IsLineoutActive()) return;

        OnPossessionChanged?.Invoke(PossessionTeam); //Invoke calls all the subscribed methods in order
    }
    
    public bool IsRuckActive()
    {
        return activeRuck != null;
    }
}
