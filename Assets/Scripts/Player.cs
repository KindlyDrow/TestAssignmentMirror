using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public static Player LocalInstance { get; private set; }

    [SyncVar(hook = nameof(SetColor))]
    [SerializeField] private Color _playerColor;

    private Color _defaultColor;
    [SerializeField] private Color _damagedColor;
    [SerializeField] private MeshRenderer _bodyRenderer;
    private Material _mainMaterial;
    [SyncVar]
    private int _playerScore;
    [SyncVar]
    private string _playerName;


    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _dashTime = 1f;
    [SerializeField] private float _dashPrepTime = 1f;
    [SerializeField] private float _dashForce = 10f;
    [SerializeField] private float _damagedTime = 3f;

    private bool isDashStarted = false;
    private bool isDashInProgress = false;

    [SyncVar(hook = nameof(SetDamaged))]
    public bool _isDamaged  = false;

    private float triggerTime;

    private Rigidbody _playerRb;
    private CapsuleCollider _capsuleCollider;

    private void Awake()
    {
        if (isLocalPlayer) 
        { 
            LocalInstance = this; 
        }

        _mainMaterial = _bodyRenderer.material;
        _defaultColor = _mainMaterial.color;
        _playerRb = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
    }

    private void Start()
    {
        if (!isLocalPlayer) return;
        
        GameInput.Instance.OnDashPerfomed += GameInput_OnDashPerfomed;
        _playerScore = 0;
        _playerName = ($"Player{Random.Range(1, 9999)}");
        GameManager.Instance.CmdChangePlayersScore(_playerName, _playerScore);
    }

    private void GameInput_OnDashPerfomed()
    {
        if (!isLocalPlayer) return;
        if (!isDashStarted)
        {
            isDashStarted = true;
            StartCoroutine(StartDash(_dashPrepTime, _dashTime, _dashForce));
        }
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        if (!isDashStarted) HandleMovement();
    }

    // If wanna damaged do damag
    //private void OnTriggerEnter(Collider other)
    //{
    //    if (!isLocalPlayer) return;
    //    if (other.gameObject.CompareTag("Player") && isDashInProgress)
    //    {
    //        Player collisionPlayer = other.gameObject.GetComponent<Player>();
    //        if (!collisionPlayer._isDamaged)
    //        {
    //            Debug.Log("Damaged by damaged");
    //            collisionPlayer.GetDashed();
    //            ChangePlayerScore(1);
    //        }
    //    }
    //}

    private void OnCollisionEnter(Collision collision)
    {
        if (!isLocalPlayer) return;
        if (collision.gameObject.CompareTag("Player") && isDashInProgress)
        {
            Player collisionPlayer = collision.gameObject.GetComponent<Player>();
            if (!collisionPlayer._isDamaged)
            {
                collisionPlayer.GetDashed();
                ChangePlayerScore(1);
            }
        }

    }

    private void ChangePlayerScore(int score)
    {
        _playerScore += score;
        GameManager.Instance.CmdChangePlayersScore(_playerName, _playerScore);
    }

    public void GetDashed()
    {
        StartCoroutine(StartDamaged(_damagedTime));
    }

    private IEnumerator StartDamaged(float time)
    {
        CmdDamaged(true);

        yield return new WaitForSeconds(time);

        CmdDamaged(false);
    }

    [Command(requiresAuthority = false)]
    private void CmdDamaged(bool isDamaged)
    {
        if(isDamaged)
        {
            CmdChangeColor(_damagedColor);
        } else
        {
            CmdChangeColor(_defaultColor);
        }
        _isDamaged = isDamaged;
    }

    private void SetDamaged(bool oldValue, bool newValue)
    {
        _capsuleCollider.isTrigger = newValue;
        _isDamaged = newValue;
 
    }

    [Command(requiresAuthority = false)]
    private void CmdChangeColor(Color color)
    {
        _playerColor = color;
    }

    private void SetColor(Color oldColor, Color newColor)
    {
        _mainMaterial.color = newColor;
    }

    private void HandleMovement()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        if (moveDir != Vector3.zero) 
        { 
            _playerRb.velocity = (moveDir * Time.deltaTime * _moveSpeed); 
        }

    }

    private IEnumerator StartDash(float dashPrepTime, float dashTime, float dashForce)
    {
        yield return new WaitForSeconds(dashPrepTime / 10);

        isDashInProgress = true;

        Vector3 forceDir = GetMouseVectorNormalized();

        _playerRb.AddForce(forceDir * dashForce, ForceMode.Impulse);

        yield return new WaitForSeconds(dashTime / 10);

        _playerRb.velocity = Vector3.zero;
        isDashInProgress = false;
        isDashStarted = false;
    }

    private Vector3 GetMouseVectorNormalized()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.transform.position.y));
        mousePosition.y = transform.position.y;

        Vector3 forceDir = (mousePosition - transform.position).normalized;

        return forceDir;
    }

    private void OnDestroy()
    {
        Destroy(_mainMaterial);
    }
}
