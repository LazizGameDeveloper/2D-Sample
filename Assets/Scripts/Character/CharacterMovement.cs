using System;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class CharacterMovement : MonoBehaviour, IControlable
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _jumpHeight;
    [SerializeField] private float _jumpLiftingDuration;
    [SerializeField] private int _extraJumps = 1;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Transform _groundCheckTransform;
    [SerializeField] private float _groundCheckRange;

    private Rigidbody2D _rb;
    private Vector2 _moveVelocity;

    private int _jumpsRemained;
    private bool _grounded;
    private const float G = 9.81f;
    public float Acceleration { get { return (2 * _jumpHeight) / (_jumpLiftingDuration * _jumpLiftingDuration); } }

    private Animator _animator;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void FixedUpdate()
    {
        MoveInternal();
    }

    public void Move(Vector2 direction)
    {
        _animator.SetFloat("Speed", Mathf.Abs(direction.x));
        _moveVelocity = direction * _moveSpeed;
        Flip(direction.x);
    }

    private void Flip(float xDirection)
    {
        if (xDirection > 0)
            transform.localScale = new Vector3(-1f, 1f, 1f);
        else if(xDirection < 0)
            transform.localScale = new Vector3(1f, 1f, 1f);
    }

    private void MoveInternal()
    {
        _rb.velocity = new Vector2(_moveVelocity.x, _rb.velocity.y);
    }

    public void Jump()
    {
        if(_grounded || _jumpsRemained > 0)
        {
            _animator.SetTrigger("Jump");
            _grounded = false;
            _jumpsRemained--;
            float jumpSpeed = Mathf.Sqrt(2 * _jumpHeight * Acceleration);
            _rb.gravityScale = Acceleration / G;
            _rb.velocity = new Vector2(_rb.velocity.x, jumpSpeed);
        }
    }
    public void JumpStop(){}

    private void OnCollisionStay2D(Collision2D collision)
    {
        if ((_groundMask & (1 << collision.gameObject.layer)) != 0)
            _grounded = IsOnTheGround();
    }

    private bool IsOnTheGround()
    {
        bool isOnTheground = Physics2D.OverlapCircle(_groundCheckTransform.position, _groundCheckRange, _groundMask);
        _animator.SetBool("Grounded", isOnTheground);
        if (isOnTheground)
            _jumpsRemained = _extraJumps;
        return isOnTheground;
    }

    private void OnDrawGizmosSelected()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody2D>();
        }
        Renderer renderer = GetComponentInChildren<Renderer>();
        float yBound = renderer.bounds.extents.y;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(_rb.position, new Vector2(_rb.position.x, _rb.position.y + _jumpHeight + yBound));

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_groundCheckTransform.position, _groundCheckRange);
    }
}
