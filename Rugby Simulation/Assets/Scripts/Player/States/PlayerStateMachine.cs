using UnityEngine;

// Player FSM brain (Owns the logic and trnasitions
public class PlayerStateMachine : MonoBehaviour
{
    Player player;
    StateMachine stateMachine;

    public StateMachine SM
    {
        get { return stateMachine; }
    }


    public Vector2 moveInput { get; private set; }
    public bool sprintInput { get; private set; }

    public bool passRequested { get; private set; }
    public bool passLeftSide { get; private set; }
    public bool tackleRequested { get; private set; }

    public void Initialise(Player player)
    {
        this.player = player;
        stateMachine = new StateMachine();
    }

    private void Start()
    {
        stateMachine.SetState(new IdleState(this));
    }
    private void Update()
    {
        stateMachine.Tick();
    }
    private void FixedUpdate()
    {
        stateMachine.FixedTick();
    }

    //Called by TeamInputHandler
    public void SetInput(Vector2 move, bool sprint)
    {
        moveInput = move;
        sprintInput = sprint;
    }

    //Request a pass
    public void RequestPass(bool leftSide)
    {
        passRequested = true;
        passLeftSide = leftSide;
    }

    public void ClearPassRequest()
    {
        passRequested = false;
    }
    public void RequestTackle()
    {
        tackleRequested = true;
    }
    public void ClearTackleRequest()
    {
        tackleRequested = false;
    }
    public Player GetPlayer()
    {
        return player;
    }

    public Team GetTeam()
    {
        return player.team;
    }
}
