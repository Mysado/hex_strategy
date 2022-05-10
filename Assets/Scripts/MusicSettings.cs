using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/MusicSettings")]
public class MusicSettings : ScriptableObject
{
    [SerializeField] private AudioClip defaultMusic;
    [SerializeField] private AudioClip battleMusic;
    [SerializeField] private AudioClip movementMusic;
    [SerializeField] private AudioClip victoryMusic;

    public AudioClip DefaultMusic => defaultMusic;
    public AudioClip BattleMusic => battleMusic;
    public AudioClip MovementMusic => movementMusic;
    public AudioClip VictoryMusic => victoryMusic;
}
