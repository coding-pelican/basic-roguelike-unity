using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node {
    public Node(bool _isWall, int _x, int _y) { isWall = _isWall; x = _x; y = _y; }

    public bool isWall;
    public Node ParentNode;

    // G : �������κ��� �̵��ߴ� �Ÿ�, H : |����|+|����| ��ֹ� �����Ͽ� ��ǥ������ �Ÿ�, F : G + H
    public int x, y, G, H;
    public int F { get { return G + H; } }
}

public class EnemyController : MovingObject {
    public int playerDamage;
    public List<Node> FinalNodeList;
    public bool allowDiagonal, dontCrossCorner;
    public AudioClip enemyAttack1;
    public AudioClip enemyAttack2;

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
        if (SkipMove) { // �������� �ǳʶپ�� �Ѵٸ�
            SkipMove = false; // ���� ���� ���� false�� ����� �ǳ� ��
            return;
        }
        base.AttemptMove<T>(xDir, yDir);
        // Enemy�� �̹� ���������Ƿ� true�� ����
        SkipMove = true;
        SoundManager.instance.RandomizeSfx(enemyAttack1, enemyAttack2);
    }

    public void MoveEnemy() {
        int xDir = 0;
        int yDir = 0;
        //if (Mathf.Abs(_target.position.x - transform.position.x) < float.Epsilon) { // ���� x�������� ���� ��
        //    yDir = _target.position.y > transform.position.y ? 1 : -1; // y�� �󿡼� Ÿ�� ������ ��ĭ ������ ��
        //} else { // ���� x �������� �ٸ��ٸ�
        //    xDir = _target.position.x > transform.position.y ? 1 : -1; // x�� �󿡼� Ÿ�� ������ �� ĭ ������ ��
        //}
        PathFinding();
        if (FinalNodeList.Count != 0) {
            xDir = FinalNodeList[1].x - _StartNode.x;
            yDir = FinalNodeList[1].y - _StartNode.y;
        }
        AttemptMove<PlayerController>(xDir, yDir);
    }

    protected override void OnCantMove<T>(T component) {
        // component�� Player�� ĳ��Ʈ�Ͽ� ����
        PlayerController hitPlayer = component as PlayerController;
        // ���� ��������ŭ ��������Ʈ�� �ٿ���
        hitPlayer.LoseFood(playerDamage);
        _animator.SetTrigger("enemyAttack");
    }

    public void PathFinding() {
        // NodeArray�� ũ�� �����ְ�, isWall, x, y ����
        _sizeX = topRight.x - bottomLeft.x + 1;
        _sizeY = topRight.y - bottomLeft.y + 1;
        _NodeArray = new Node[_sizeX, _sizeY];

        // startPos�� x, y ����
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

        // ���۰� �� ���, ��������Ʈ�� ��������Ʈ, ����������Ʈ �ʱ�ȭ
        _StartNode = _NodeArray[startPos.x - bottomLeft.x, startPos.y - bottomLeft.y];
        _TargetNode = _NodeArray[targetPos.x - bottomLeft.x, targetPos.y - bottomLeft.y];

        _OpenList = new List<Node>() { _StartNode };
        _ClosedList = new List<Node>();
        FinalNodeList = new List<Node>();

        while (_OpenList.Count > 0) {
            // ��������Ʈ �� ���� F�� �۰� F�� ���ٸ� H�� ���� �� ������� �ϰ� ��������Ʈ���� ��������Ʈ�� �ű��
            _CurNode = _OpenList[0];
            for (int i = 1; i < _OpenList.Count; i++)
                if (_OpenList[i].F <= _CurNode.F && _OpenList[i].H < _CurNode.H) _CurNode = _OpenList[i];

            _OpenList.Remove(_CurNode);
            _ClosedList.Add(_CurNode);

            // ������
            if (_CurNode == _TargetNode) {
                Node TargetCurNode = _TargetNode;
                while (TargetCurNode != _StartNode) {
                    FinalNodeList.Add(TargetCurNode);
                    TargetCurNode = TargetCurNode.ParentNode;
                }
                FinalNodeList.Add(_StartNode);
                FinalNodeList.Reverse();

                //for (int i = 0; i < FinalNodeList.Count; i++) Debug.Log(i + "��°�� " + FinalNodeList[i].x + ", " + FinalNodeList[i].y);
                return;
            }

            // �֢آע�
            if (allowDiagonal) {
                OpenListAdd(_CurNode.x + 1, _CurNode.y + 1);
                OpenListAdd(_CurNode.x - 1, _CurNode.y + 1);
                OpenListAdd(_CurNode.x - 1, _CurNode.y - 1);
                OpenListAdd(_CurNode.x + 1, _CurNode.y - 1);
            }

            // �� �� �� ��
            OpenListAdd(_CurNode.x, _CurNode.y + 1);
            OpenListAdd(_CurNode.x + 1, _CurNode.y);
            OpenListAdd(_CurNode.x, _CurNode.y - 1);
            OpenListAdd(_CurNode.x - 1, _CurNode.y);
        }
    }

    void OpenListAdd(int checkX, int checkY) {
        // �����¿� ������ ����� �ʰ�, ���� �ƴϸ鼭, ��������Ʈ�� ���ٸ�
        if (checkX >= bottomLeft.x && checkX < topRight.x + 1 && checkY >= bottomLeft.y && checkY < topRight.y + 1 && !_NodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y].isWall && !_ClosedList.Contains(_NodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y])) {
            // �밢�� ����, �� ���̷� ��� �ȵ�
            if (allowDiagonal) if (_NodeArray[_CurNode.x - bottomLeft.x, checkY - bottomLeft.y].isWall && _NodeArray[checkX - bottomLeft.x, _CurNode.y - bottomLeft.y].isWall) return;

            // �ڳʸ� �������� ���� ������, �̵� �߿� �������� ��ֹ��� ������ �ȵ�
            if (dontCrossCorner) if (_NodeArray[_CurNode.x - bottomLeft.x, checkY - bottomLeft.y].isWall || _NodeArray[checkX - bottomLeft.x, _CurNode.y - bottomLeft.y].isWall) return;

            // �̿���忡 �ְ�, ������ 10, �밢���� 14���
            Node NeighborNode = _NodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y];
            int moveCost = _CurNode.G + (_CurNode.x - checkX == 0 || _CurNode.y - checkY == 0 ? 10 : 14);

            // �̵������ �̿����G���� �۰ų� �Ǵ� ��������Ʈ�� �̿���尡 ���ٸ� G, H, ParentNode�� ���� �� ��������Ʈ�� �߰�
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
