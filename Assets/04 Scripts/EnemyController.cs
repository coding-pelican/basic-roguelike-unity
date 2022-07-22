using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node {
    public Node(bool _isWall, int _x, int _y) { isWall = _isWall; x = _x; y = _y; }

    public bool isWall;
    public Node ParentNode;

    // G : 시작으로부터 이동했던 거리, H : |가로|+|세로| 장애물 무시하여 목표까지의 거리, F : G + H
    public int x, y, G, H;
    public int F { get { return G + H; } }
}

public class EnemyController : MovingObject {
    public int playerDamage;
    public List<Node> FinalNodeList;
    public bool allowDiagonal, dontCrossCorner;

    private int _sizeX, _sizeY; // PathFinding Value
    private Node[,] _NodeArray;
    [SerializeField] private Vector2Int bottomLeft, topRight, startPos, targetPos; // PathFinding Value
    private Node _StartNode, _TargetNode, _CurNode;
    private List<Node> _OpenList, _ClosedList;
    //private Transform _target;
    private Animator _animator;
    private BoardManager _boardManager;
    private bool _skipMove;

    public bool SkipMove { get => _skipMove; set => _skipMove = value; }

    protected override void Start() {
        GameManager.instance.AddEnemyToList(this);
        _boardManager = GameManager.instance.BoardScript;
        _animator = GetComponent<Animator>();
        bottomLeft = new Vector2Int(0, 0);
        topRight = new Vector2Int(_boardManager.rows - 1, _boardManager.columns - 1);
        //_target = GameObject.FindGameObjectWithTag("Player").transform;
        base.Start();
    }

    protected override void AttemptMove<T>(int xDir, int yDir) {
        if (SkipMove) { // 움직임을 건너뛰어야 한다면
            SkipMove = false; // 다음 턴을 위해 false로 만들고 건너 뜀
            return;
        }
        base.AttemptMove<T>(xDir, yDir);
        // Enemy가 이미 움직였으므로 true로 변경
        SkipMove = true;
    }

    public void MoveEnemy() {
        int xDir = 0;
        int yDir = 0;
        //if (Mathf.Abs(_target.position.x - transform.position.x) < float.Epsilon) { // 둘의 x포지션이 같을 때
        //    yDir = _target.position.y > transform.position.y ? 1 : -1; // y축 상에서 타겟 쪽으로 한칸 움직여 줌
        //} else { // 둘의 x 포지션이 다르다면
        //    xDir = _target.position.x > transform.position.y ? 1 : -1; // x축 상에서 타겟 쪽으로 한 칸 움직여 줌
        //}
        PathFinding();
        if (FinalNodeList.Count != 0) {
            xDir = FinalNodeList[1].x - _StartNode.x;
            yDir = FinalNodeList[1].y - _StartNode.y;
        }
        AttemptMove<PlayerController>(xDir, yDir);
    }

    protected override void OnCantMove<T>(T component) {
        // component를 Player로 캐스트하여 저장
        PlayerController hitPlayer = component as PlayerController;
        // 받은 데미지만큼 음식포인트를 줄여줌
        hitPlayer.LoseFood(playerDamage);
        _animator.SetTrigger("enemyAttack");
    }

    public void PathFinding() {
        // NodeArray의 크기 정해주고, isWall, x, y 대입
        _sizeX = topRight.x - bottomLeft.x + 1;
        _sizeY = topRight.y - bottomLeft.y + 1;
        _NodeArray = new Node[_sizeX, _sizeY];

        // startPos의 x, y 대입
        startPos = new Vector2Int((int)transform.position.x, (int)transform.position.y);

        for (int i = 0; i < _sizeX; i++) {
            for (int j = 0; j < _sizeY; j++) {
                bool isWall = false;
                foreach (Collider2D col in Physics2D.OverlapCircleAll(new Vector2(i + bottomLeft.x, j + bottomLeft.y), 0.4f)) {
                    if (!(col.gameObject.CompareTag("Player") || col.gameObject.CompareTag("Enemy")) && col.gameObject.layer == LayerMask.NameToLayer("BlockingLayer")) isWall = true;
                    if (col.gameObject.CompareTag("Player")) targetPos = new Vector2Int((int)col.gameObject.transform.position.x, (int)col.gameObject.transform.position.y);
                }
                _NodeArray[i, j] = new Node(isWall, i + bottomLeft.x, j + bottomLeft.y);
            }
        }

        // 시작과 끝 노드, 열린리스트와 닫힌리스트, 마지막리스트 초기화
        _StartNode = _NodeArray[startPos.x - bottomLeft.x, startPos.y - bottomLeft.y];
        _TargetNode = _NodeArray[targetPos.x - bottomLeft.x, targetPos.y - bottomLeft.y];

        _OpenList = new List<Node>() { _StartNode };
        _ClosedList = new List<Node>();
        FinalNodeList = new List<Node>();

        while (_OpenList.Count > 0) {
            // 열린리스트 중 가장 F가 작고 F가 같다면 H가 작은 걸 현재노드로 하고 열린리스트에서 닫힌리스트로 옮기기
            _CurNode = _OpenList[0];
            for (int i = 1; i < _OpenList.Count; i++)
                if (_OpenList[i].F <= _CurNode.F && _OpenList[i].H < _CurNode.H) _CurNode = _OpenList[i];

            _OpenList.Remove(_CurNode);
            _ClosedList.Add(_CurNode);

            // 마지막
            if (_CurNode == _TargetNode) {
                Node TargetCurNode = _TargetNode;
                while (TargetCurNode != _StartNode) {
                    FinalNodeList.Add(TargetCurNode);
                    TargetCurNode = TargetCurNode.ParentNode;
                }
                FinalNodeList.Add(_StartNode);
                FinalNodeList.Reverse();

                //for (int i = 0; i < FinalNodeList.Count; i++) Debug.Log(i + "번째는 " + FinalNodeList[i].x + ", " + FinalNodeList[i].y);
                return;
            }

            // ↗↖↙↘
            if (allowDiagonal) {
                OpenListAdd(_CurNode.x + 1, _CurNode.y + 1);
                OpenListAdd(_CurNode.x - 1, _CurNode.y + 1);
                OpenListAdd(_CurNode.x - 1, _CurNode.y - 1);
                OpenListAdd(_CurNode.x + 1, _CurNode.y - 1);
            }

            // ↑ → ↓ ←
            OpenListAdd(_CurNode.x, _CurNode.y + 1);
            OpenListAdd(_CurNode.x + 1, _CurNode.y);
            OpenListAdd(_CurNode.x, _CurNode.y - 1);
            OpenListAdd(_CurNode.x - 1, _CurNode.y);
        }
    }

    void OpenListAdd(int checkX, int checkY) {
        // 상하좌우 범위를 벗어나지 않고, 벽이 아니면서, 닫힌리스트에 없다면
        if (checkX >= bottomLeft.x && checkX < topRight.x + 1 && checkY >= bottomLeft.y && checkY < topRight.y + 1 && !_NodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y].isWall && !_ClosedList.Contains(_NodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y])) {
            // 대각선 허용시, 벽 사이로 통과 안됨
            if (allowDiagonal) if (_NodeArray[_CurNode.x - bottomLeft.x, checkY - bottomLeft.y].isWall && _NodeArray[checkX - bottomLeft.x, _CurNode.y - bottomLeft.y].isWall) return;

            // 코너를 가로질러 가지 않을시, 이동 중에 수직수평 장애물이 있으면 안됨
            if (dontCrossCorner) if (_NodeArray[_CurNode.x - bottomLeft.x, checkY - bottomLeft.y].isWall || _NodeArray[checkX - bottomLeft.x, _CurNode.y - bottomLeft.y].isWall) return;

            // 이웃노드에 넣고, 직선은 10, 대각선은 14비용
            Node NeighborNode = _NodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y];
            int moveCost = _CurNode.G + (_CurNode.x - checkX == 0 || _CurNode.y - checkY == 0 ? 10 : 14);

            // 이동비용이 이웃노드G보다 작거나 또는 열린리스트에 이웃노드가 없다면 G, H, ParentNode를 설정 후 열린리스트에 추가
            if (moveCost < NeighborNode.G || !_OpenList.Contains(NeighborNode)) {
                NeighborNode.G = moveCost;
                NeighborNode.H = (Mathf.Abs(NeighborNode.x - _TargetNode.x) + Mathf.Abs(NeighborNode.y - _TargetNode.y)) * 10;
                NeighborNode.ParentNode = _CurNode;

                _OpenList.Add(NeighborNode);
            }
        }
    }

    void OnDrawGizmos() {
        if (FinalNodeList.Count != 0) for (int i = 0; i < FinalNodeList.Count - 1; i++)
                Gizmos.DrawLine(new Vector2(FinalNodeList[i].x, FinalNodeList[i].y), new Vector2(FinalNodeList[i + 1].x, FinalNodeList[i + 1].y));
    }
}
