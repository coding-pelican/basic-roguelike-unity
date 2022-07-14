using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MovingObject {
    private Animator _animator;
    private int _food;

    public int wallDamage = 1;
    public int pointsPerFood = 10;
    public int pointsPerSoda = 20;
    public float restartLevelDelay = 1f;
    public TextMeshProUGUI foodText;

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
        _food--;
        foodText.text = "Food: " + _food;
        base.AttemptMove<T>(xDir, yDir); // 부모 클래스의 AttemptMove 호출
        CheckIfGameOver();
        GameManager.instance.isPlayersTurn = false;
    }

    void Update() {
        if (!GameManager.instance.isPlayersTurn) return;
        // 1또는 -1로 가로, 세로 방향 저장용
        int horizontal = (int)Input.GetAxisRaw("Horizontal");
        int vertical = (int)Input.GetAxisRaw("Vertical");
        // 가로로 움직였다면 세로는 0
        if (horizontal != 0) vertical = 0;
        // 어느 쪽으로든 움직이라는 명령이 있었다면
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
