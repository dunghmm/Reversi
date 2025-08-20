using UnityEngine;

public class Disc : MonoBehaviour
{
    [SerializeField]
    private Player up;


    private bool flipped = false;

    private Animator animator;

    // Start is called before the first frame update
    private void Start()
    {
        animator = GetComponent<Animator>();
        if (PlayerPrefs.GetFloat("AnimationSpeed") != 0)
        {
            animator.speed = PlayerPrefs.GetFloat("AnimationSpeed");
        }
    }

    public void Flip()
    {
        if (!flipped)
        {
            animator.Play("BlackToWhite");
            up = up.Opponent();
            flipped = true;
        }
        else
        {
            animator.Play("WhiteToBlack");
            up = up.Opponent();
            flipped = false;
        }
    }

    public void Twitch()
    {
        if (!flipped)
        {
            animator.Play("TwitchDisc");
        }
        else
        {
            animator.Play("TwitchDisc2");
        }
    }
}
