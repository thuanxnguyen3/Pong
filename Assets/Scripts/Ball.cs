
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [Header("Refs")]
    public Rigidbody2D rb2d;
    public SpriteRenderer spriteRenderer;
    public BallAudio ballAudio;
    public ParticleSystem collisionParticle;

    [Header("Config")]
    [Range(0f, 1f)]
    public float maxInitialAngle = 0.67f;
    [Tooltip("The maximum ball angle after colliding with a paddle")]
    public float maxCollisionAngle = 45f;
    [Space]
    public float moveSpeed = 1f;
    public float maxStartY = 4f;
    [SerializeField]
    private float speedMultiplier = 1.1f;

    private float startX = 0f;
    private float startY = 0f;

    public bool Player1Start = true;


    private void Start()
    {
        GameManager.instance.onReset += ResetBall;
        GameManager.instance.gameUI.onStartGame += ResetBall;
    }

    private void ResetBall()
    {
        ResetBallPosition();
        InitialPush();
    }

    private void InitialPush()
    {
        Vector2 dir;

        if (Player1Start)
        {
            dir = Vector2.left;
        } else
        {
            dir = Vector2.right;
        }

        dir.y = Random.Range(-maxInitialAngle, maxInitialAngle);
        rb2d.velocity = dir * moveSpeed;

        EmitParticle(32);
    }

    private void ResetBallPosition()
    {
        Vector2 position = new Vector2(startX, startY);
        transform.position = position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ScoreZone scoreZone = collision.GetComponent<ScoreZone>();
        if (scoreZone)
        {
            GameManager.instance.OnScoreZoneReached(scoreZone.id);
            if (scoreZone.id == 2)
            {
                Player1Start = true;
            } else if (scoreZone.id == 1)
            {
                Player1Start = false;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Paddle paddle = collision.collider.GetComponent<Paddle>();
        if (paddle)
        {
            ballAudio.PlayPaddleSound();
            rb2d.velocity *= speedMultiplier;
            EmitParticle(16);
            AdjustAngle(paddle, collision);
        }

        Wall wall = collision.collider.GetComponent<Wall>();
        if (wall)
        {
            ballAudio.PlayWallSound();
            EmitParticle(8);
        }

        AdjustSpriteRotation();
    }

    private void AdjustAngle(Paddle paddle, Collision2D collision)
    {
        // make the ball move at higher angle the further towards the edge of the paddle it hit
        Vector2 median = Vector2.zero;
        foreach (ContactPoint2D point in collision.contacts)
        {
            median += point.point;
        }
        median /= collision.contactCount;

        // calculate relative distance from center (between -1 and 1)
        float absoluteDistanceFromCenter = median.y - paddle.transform.position.y;
        float relativeDistanceFromCenter = absoluteDistanceFromCenter * 2 / paddle.GetHeight();

        // calculate rotation using quaternion
        int angleSign = paddle.IsLeftPaddle() ? 1 : -1;
        float angle = relativeDistanceFromCenter * maxCollisionAngle * angleSign;
        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
        Debug.DrawRay(median, Vector3.forward, Color.yellow, 1f);

        // calculate direction / velocity
        Vector2 dir = paddle.IsLeftPaddle() ? Vector2.right : Vector2.left;
        Vector2 velocity = rot * dir * rb2d.velocity.magnitude;
        rb2d.velocity = velocity;
        Debug.DrawRay(median, velocity, Color.green, 1f);
    }

    private void AdjustSpriteRotation()
    {
        spriteRenderer.flipY = rb2d.velocity.x < 0f;
    }

    private void EmitParticle(int amount)
    {
        collisionParticle.Emit(amount);
    }
}
