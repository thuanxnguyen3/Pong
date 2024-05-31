using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreText : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Animator animator;

    // highlight the score text
    public void Highlight()
    {
        animator.SetTrigger("highlight");
    }

    // set score text to value
    public void SetScore(int value)
    {
        text.text = value.ToString();
    }
}
