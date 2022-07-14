using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour {
    private BoxCollider2D _boxCollider;
    private Rigidbody2D _rb2D;
    private float _inverseMoveTime; // 이동 계산을 효과적으로 하기 위한 변수

    public float InverseMoveTime { get => _inverseMoveTime; set => _inverseMoveTime = value; }

    public LayerMask blockingLayer;
    public float moveTime = 0.1f;

    protected virtual void Start() {
        _boxCollider = GetComponent<BoxCollider2D>();
        _rb2D = GetComponent<Rigidbody2D>();
        InverseMoveTime = 1f / moveTime; // 미리 역수로 계산을 해두어서 계산시 나누기가 아닌 곱하기가 가능하게 함
    }

    protected IEnumerator SmoothMovement(Vector3 end) {
        // 남은 거리의 노름 계산, 제곱으로 계산하는 편이 계산이 용이하여 제곱으로 사용
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
        while (sqrRemainingDistance > float.Epsilon) { // 0에 아주 근접한 값(엡실론)보다 큰 경우 루틴이 돌아감
            Vector3 newPosition = Vector3.MoveTowards(_rb2D.position, end, InverseMoveTime * Time.deltaTime); // 시간에 비례하여 목적지로 향하는 새 위치 계산
            _rb2D.MovePosition(newPosition); // 계산된 새 위치로 이동
            sqrRemainingDistance = (transform.position - end).sqrMagnitude; // 남은 거리 재계산
            yield return null; // 남은 거리가 0에 근접할 때까지 루프
        }
    }

    protected abstract void OnCantMove<T>(T component)
    where T : Component;

    protected bool Move(int xDir, int yDir, out RaycastHit2D hit) {
        Vector2 start = transform.position;
        Vector2 end = start + new Vector2(xDir, yDir);
        _boxCollider.enabled = false; // Raycast 계산시 본인의 콜라이더가 맞게 되는 것을 피함
        hit = Physics2D.Linecast(start, end, blockingLayer); // start에서 end로 라인을 캐스트 함
        _boxCollider.enabled = true;
        if (hit.transform == null) { // 캐스팅 된 라인에 걸리는 것이 없어 움직일 수 있다면 코루틴 시작
            StartCoroutine(SmoothMovement(end));
            return true;

        }
        return false; // 걸리는 것이 있으면 움직일 수 없음
    }

    protected virtual void AttemptMove<T>(int xDir, int yDir) where T : Component {
        // Move가 호출되었을때 Linecast가 때리게 되는 것을 저장할 변수
        bool canMove = Move(xDir, yDir, out RaycastHit2D hit);
        if (hit.transform == null) return; // 걸리는 것이 없으면 코드를 마침
        T hitComponent = hit.transform.GetComponent<T>(); // hit된 것의 컴포넌트 레퍼런스를 얻어 옴
        if (!canMove && hitComponent != null) OnCantMove(hitComponent);
    }

}
