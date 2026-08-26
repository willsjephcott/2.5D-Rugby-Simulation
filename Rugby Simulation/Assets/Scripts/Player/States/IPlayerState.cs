using UnityEngine;

//State interface patter for player FSMs
public interface IPlayerState //What does interface change? Question for future (NEA writeup)
{
    void Enter();
    void Tick();
    void FixedTick();
    void Exit();
}
