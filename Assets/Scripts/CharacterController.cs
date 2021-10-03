using System;
using Enums;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

public class CharacterController : MonoBehaviour
{
    public GameObject player;
    public bool isGrounded = true;
    [SerializeField] private float jumpForce = 400.0f;
    [SerializeField] private float movingAcceleration = 5.0f;
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private GameSettings gameSettings;

    // public GravityManager gravity;
    

    private float _horizontalForce;

    private bool _isUnAligned = true;
    private bool _jumpAllowed;
    private float _jumpLock;
    private Orientation _oldOrientation;
    private Rigidbody2D _rg;
    private Vector2 _vel = new Vector2(0, 0);

    // Start is called before the first frame update
    private void Start()
    {
        _rg = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    private void Update()
    {
        // _rg.gravityScale = 1;
        // // _currentOrientation = gravity.GETCurrentOrientation();
        // _currentOrientation = Orientation.Down;
        // var grav = _currentOrientation switch
        // {
        //     Orientation.Up => Vector2.up,
        //     Orientation.Down => Vector2.down,
        //     Orientation.Left => Vector2.left,
        //     Orientation.Right => Vector2.right,
        //     _ => Vector2.zero
        // };
        // grav *= 5;
        //
        // Physics2D.gravity = grav;
        this.gameObject.transform.rotation = gameSettings.GravityOrientation switch
        {
            Orientation.Down => Quaternion.Euler(0, 0, 0),
            Orientation.Up => Quaternion.Euler(0, 0, 180f),
            Orientation.Left => Quaternion.Euler(0, 0, 270f),
            Orientation.Right => Quaternion.Euler(0, 0, 90f),
            _ => this.gameObject.transform.rotation
        };
/*   switch (_currentOrientation)
 {
      case Orientation.Up:
      case Orientation.Down:
          isGrounded = (Abs(_rg.velocity.y) < 0.05f);
          break;
      case Orientation.Left:
      case Orientation.Right:
          isGrounded = (Abs(_rg.velocity.x) < 0.05f);
          break;
      default: break;
  }*/

        _jumpLock -= Time.deltaTime;
        if (_jumpLock < 0)
        {
            _jumpLock = 0f;
            _jumpAllowed = true;
        }

        var horizontalInput = Input.GetAxis("Horizontal");
        var heightInput = (int) Input.GetAxis("Jump");
        if (!isGrounded)
        {
            // verticalSpeed += fallingAcceleration * Time.deltaTime;
            _horizontalForce = horizontalInput * 5f * movingAcceleration;

            var horizontalMovementDirection = gameSettings.GravityOrientation switch
            {
                Orientation.Up => Vector2.left,
                Orientation.Down => Vector2.right,
                Orientation.Left => Vector2.up,
                Orientation.Right => Vector2.down,
                _ => Vector2.zero
            };

            var currentVelocity = _rg.velocity;

            if (gameSettings.GravityOrientation == Orientation.Down || gameSettings.GravityOrientation == Orientation.Up)
            {
                if (Mathf.Abs(_rg.velocity.x) > 5 && currentVelocity.x * _horizontalForce > 0) _horizontalForce = 0;
            }
            else
            {
                if (Mathf.Abs(_rg.velocity.y) > 5 && currentVelocity.y * _horizontalForce > 0) _horizontalForce = 0;
            }

            horizontalMovementDirection *= _horizontalForce;
            _rg.AddForce(horizontalMovementDirection);
        }
        else
        {
            _horizontalForce = horizontalInput * movingAcceleration;


            if (_jumpAllowed && heightInput == 1)
            {
                switch (gameSettings.GravityOrientation)
                {
                    case Orientation.Up:
                        _rg.AddForce(new Vector2(0, -(jumpForce * _rg.mass)));
                        break;
                    case Orientation.Down:
                        _rg.AddForce(new Vector2(0, jumpForce * _rg.mass));
                        break;
                    case Orientation.Left:
                        _rg.AddForce(new Vector2(jumpForce * _rg.mass, 0));
                        break;
                    case Orientation.Right:
                        _rg.AddForce(new Vector2(-(jumpForce * _rg.mass), 0));
                        break;
                }


                _jumpLock = 0.1f;
                _jumpAllowed = false;
            }

            switch (gameSettings.GravityOrientation)
            {
                case Orientation.Up:
                    _vel = new Vector2(-_horizontalForce, _rg.velocity.y);
                    break;
                case Orientation.Down:
                    _vel = new Vector2(_horizontalForce, _rg.velocity.y);
                    break;
                case Orientation.Left:
                    _vel = new Vector2(_rg.velocity.x, _horizontalForce);
                    break;
                case Orientation.Right:
                    _vel = new Vector2(_rg.velocity.x, -_horizontalForce);
                    break;
            }

            _rg.velocity = _vel;
        }


        //  if (_isUnAligned) AlignPlayer();

        isGrounded = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hole")) Ejection();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Collidable")) isGrounded = true;
    }

    private void Ejection()
    {
        // a slightly more beautiful solution might be an option for Game över
        Time.timeScale = 0;
    }

    private static float Abs(float input)
    {
        return input < 0f ? -input : input;
    }

  
    private void AlignPlayer()
    {
        float targetAngle = 0;
        switch (gameSettings.GravityOrientation)
        {
            case Orientation.Up:
                targetAngle = 180;
                break;
            case Orientation.Down:
                targetAngle = 0;
                break;
            case Orientation.Left:
                targetAngle = 90;
                break;
            case Orientation.Right:
                targetAngle = 270;
                break;
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, targetAngle),
            turnSpeed * Time.deltaTime);
    }
}