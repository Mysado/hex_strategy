using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private static readonly int Moving = Animator.StringToHash("moving");
    private static readonly int Fight1 = Animator.StringToHash("fight");
    private static readonly int Death = Animator.StringToHash("death");

    public void Movement(bool state)
    {
        animator.SetBool(Moving, state);
    }
    
    public void Fight()
    {
        animator.SetTrigger(Fight1);
    }
    
    public void SetDeath()
    {
        animator.SetTrigger(Death);
    }
}
