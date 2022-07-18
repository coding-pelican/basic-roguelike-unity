using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MovingObject {
    public int wallDamage = 1;
    public int pointsPerFood = 5;
    public int pointsPerSoda = 10;
    public float restartLevelDelay = 1f;
    public TextMeshProUGUI foodText;

    private int _food;
    private Vector2 touchOrigin = -Vector2.one; // 모바일 터치 위치 저장용
    private Animator _animator;

    protected override void Start() {
        _animator = GetComponent<Animator>();
        _food = GameManager.instance.playerFoodPoints;
        foodText.text = "Food: " + _food;
        base.Start();
    }

    private void OnDisable() {
        GameManager.instance.playerFoodPoints = _food;
    }

    private void CheckIfGameOver() {
        if (_food <= 0)
            GameManager.instance.GameOver();
    }

    protected override void AttemptMove<T>(int xDir, int yDir) {
        if (GameManager.instance.Level > 27) {
            _food -= (int)Mathf.Log(GameManager.instance.Level, 3);
        } else if (GameManager.instance.Level > 16) {
            _food -= (int)Mathf.Log(GameManager.instance.Level, 4);
        } else if (GameManager.instance.Level > 2) {
            _food -= 1;
        }
        foodText.text = "Food: " + _food;
        base.AttemptMove<T>(xDir, yDir); // 부모 클래스의 AttemptMove 호출
        CheckIfGameOver();
        GameManager.instance.isPlayersTurn = false;
    }

    void Update() {
        if (!GameManager.instance.isPlayersTurn) return;
        // 1또는 -1로 가로, 세로 방향 저장용
        int horizontal = 0;
        int vertical = 0;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");
        // 가로로 움직였다면 세로는 0
        if (horizontal != 0) vertical = 0;
        // 어느 쪽으로든 움직이라는 명령이 있었다면
#else
        //터치입력이 여러번 있었다면
        if (Input.touchCount > 0) {
            Touch myTouch = Input.touches[0]; //첫 터치만 받아들임
            if (myTouch.phase == TouchPhase.Began) { //터치 페이즈가 시작되었으면
                touchOrigin = myTouch.position; // 터치 위치 저장
            } else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0) { // 터치페이즈가 끝났고 화면내에서 이루어졌다면
                Vector2 touchEnd = myTouch.position; // 터치 끝난 위치 저장
                                                     // 터치의 처음과 끝 차이 계산
                float x = touchEnd.x - touchOrigin.x;
                float y = touchEnd.y - touchOrigin.y;
                touchOrigin.x = -1; // 다시 음수로 설정
                                    // 유저의 터치는 완벽한 직선이 아니므로 어느쪽 움직임이
                                    // 더 컸나를 판정하여 해당방향으로 움직임
                if (Mathf.Abs(x) > Mathf.Abs(y)) { // x가 y보다 더 컸다면
                    horizontal = x > 0 ? 1 : -1; // x방향으로 1또는 -1 움직임
                } else {
                    vertical = y > 0 ? 1 : -1; //아니면 y축으로 움직임
                }
            }
        }
#endif
        if (horizontal != 0 || vertical != 0) AttemptMove<Wall>(horizontal, vertical); //플레이어는 벽과도 상호작용할 수 있으므로 제너릭에 Wall을 넘겨줌
    }

    protected override void OnCantMove<T>(T component) {
        // 인수로 받은 component를 Wall로 캐스팅
        Wall hitWall = component as Wall;
        // DamageWall 메소드로 데미지를 줌
        hitWall.DamageWall(wallDamage);
        _animator.SetTrigger("playerAttack");
    }

    private void Restart() {
        SceneManager.LoadScene(0);
    }

    public void LoseFood(int loss) {
        // 애니메이터 상태 변경
        _animator.SetTrigger("playerHit");
        _food -= loss; // 음식 포인트 차감
        foodText.text = "-" + loss + " Food: " + _food;
        CheckIfGameOver();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Exit")) {
            Invoke(nameof(Restart), restartLevelDelay);
            enabled = false;
        } else if (other.CompareTag("Food")) {
            _food += pointsPerFood;
            foodText.text = "+" + pointsPerFood + " Food:" + _food;
            other.gameObject.SetActive(false);
        } else if (other.CompareTag("Soda")) {
            _food += pointsPerSoda;
            foodText.text = "+" + pointsPerSoda + " Food:" + _food;
            other.gameObject.SetActive(false);
        }
    }
}
