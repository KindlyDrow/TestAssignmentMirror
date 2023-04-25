using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : NetworkBehaviour
{
    public static Player LocalInstance { get; private set; }

    [SyncVar(hook = nameof(SetColor))]
    [SerializeField] private Color _playerColor;

    private Color _defaultColor;
    [SerializeField] private Color _damagedColor;
    [SerializeField] private MeshRenderer _bodyRenderer;
    private Material _mainMaterial;
    [SyncVar(hook = nameof(SetPlayerScore))]
    private int _playerScore;
    [SyncVar(hook = nameof(SetPlayerName))]
    private string _playerName;


    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _dashTime = 1f;
    [SerializeField] private float _dashPrepTime = 1f;
    [SerializeField] private float _dashForce = 10f;
    [SerializeField] private float _damagedTime = 3f;

    private bool isDashStarted = false;
    private bool isDashInProgress = false;
    private Dictionary<string, bool> DashedRecentlyDictionary;

    [SyncVar(hook = nameof(SetDamaged))]
    public bool _isDamaged  = false;

    private float triggerTime;

    private Rigidbody _playerRb;
    private CapsuleCollider _capsuleCollider;
    private bool isMove;

    private void Awake()
    {

        _mainMaterial = _bodyRenderer.material;
        _defaultColor = _mainMaterial.color;
        _playerRb = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
    }

    public override void OnStartLocalPlayer()
    {
        DashedRecentlyDictionary = new Dictionary<string, bool>();
        DashedRecentlyDictionary.Clear();
        if (isLocalPlayer)
        {
            LocalInstance = this;
        }
        GameInput.Instance.OnDashPerfomed += GameInput_OnDashPerfomed;
        _playerScore = 0;
        CmdSetPlayerScore(_playerScore);
        CmdSetPlayerName(($"Player{Random.Range(1, 9999)}"));
    }

    [Command(requiresAuthority = false)]
    private void CmdSetPlayerScore(int playerScore)
    {
        _playerScore = playerScore;
    }

    private void SetPlayerScore(int oldScore, int newScore)
    {
        _playerScore = newScore;
        
    }

    [Command(requiresAuthority = false)]
    private void CmdSetPlayerName(string playerName)
    {
        _playerName = playerName;
    }

    private void SetPlayerName(string oldValue, string newValue)
    {
        _playerName = newValue;
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
        if (!isDashStarted)
        {
            HandleMovement();
            HandleRotation();
        }
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
                foreach (string name in DashedRecentlyDictionary.Keys)
                {
                    if (collisionPlayer._playerName == name) return;
                }
                DashedRecentlyDictionary.Add(collisionPlayer._playerName, true);
                StartCoroutine(SetDashedRecentlyPlayerNameToFalse(collisionPlayer._playerName, _damagedTime));
                collisionPlayer.GetDashed();
                ChangePlayerScore(1);
            }
        }
    }

    IEnumerator SetDashedRecentlyPlayerNameToFalse(string name, float time)
    {
        yield return new WaitForSeconds(time);

        DashedRecentlyDictionary.Remove(name);
    }

    private void ChangePlayerScore(int score)
    {
        _playerScore += score;
        CmdSetPlayerScore(_playerScore);
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
            Vector3 direction = transform.TransformDirection(moveDir);
            _playerRb.velocity = (direction * Time.deltaTime * _moveSpeed);
            isMove = true;
        } else
        {
            isMove = false;
        }

    }

    private void HandleRotation()
    {
        Vector3 lookAtPosition = new Vector3(Camera.main.transform.position.x, transform.position.y, Camera.main.transform.position.z);

        transform.LookAt(lookAtPosition);
        transform.Rotate(0f, 180f, 0f);
    }

    private IEnumerator StartDash(float dashPrepTime, float dashTime, float dashForce)
    {
        yield return new WaitForSeconds(dashPrepTime / 10);

        isDashInProgress = true;

        //Vector3 forceDir = GetMouseVectorNormalized();

        _playerRb.AddForce(transform.forward * dashForce, ForceMode.Impulse);

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

    public bool GetPlayerMove()
    {
        return isMove;
    }

    private void OnDestroy()
    {
        Destroy(_mainMaterial);
    }
}
