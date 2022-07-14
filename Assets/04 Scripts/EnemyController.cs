using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MovingObject {
    public int playerDamage;

    private Animator _animator;
    private Transform _target;
    private bool _skipMove;

    public bool SkipMove { get => _skipMove; set => _skipMove = value; }

    protected override void Start() {
        GameManager.instance.AddEnemyToList(this);
        _animator = GetComponent<Animator>();
        _target = GameObject.FindGameObjectWithTag("Player").transform;
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

        if (Mathf.Abs(_target.position.x - transform.position.x) < float.Epsilon) { // 둘의 x포지션이 같을 때
            yDir = _target.position.y > transform.position.y ? 1 : -1; // y축 상에서 타겟 쪽으로 한칸 움직여 줌
        } else { // 둘의 x 포지션이 다르다면
            xDir = _target.position.x > transform.position.y ? 1 : -1; // x축 상에서 타겟 쪽으로 한 칸 움직여 줌
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
}
