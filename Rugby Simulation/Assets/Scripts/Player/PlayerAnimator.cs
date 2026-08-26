using UnityEngine;

// Translates states into animations
public class PlayerAnimator : MonoBehaviour
{
    private Player player;

    public void Initialise(Player player)
    {
        this.player = player;
    }

    public void UpdateMovement(bool isMoving)
    {
        if (player?.anim == null) return;
        player.anim.SetBool("IsMoving", isMoving);
    }

    public void UpdateBallState(bool hasBall)
    {
        if (player?.anim == null) return;
        player.anim.SetBool("HasBall", hasBall);

    }
    public void TriggerPass()
    {
        if (player?.anim == null) return;
        player.anim.SetTrigger("Pass");
    }
}
