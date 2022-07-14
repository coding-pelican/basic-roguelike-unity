using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MovingObject {
    private Animator _animator;
    private int _food;
    private Vector2 touchOrigin = -Vector2.one; // ����� ��ġ ��ġ �����

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
        base.AttemptMove<T>(xDir, yDir); // �θ� Ŭ������ AttemptMove ȣ��
        CheckIfGameOver();
        GameManager.instance.isPlayersTurn = false;
    }

    void Update() {
        if (!GameManager.instance.isPlayersTurn) return;
        // 1�Ǵ� -1�� ����, ���� ���� �����
        int horizontal = 0;
        int vertical = 0;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");
        // ���η� �������ٸ� ���δ� 0
        if (horizontal != 0) vertical = 0;
        // ��� �����ε� �����̶�� ������ �־��ٸ�
#else
        //��ġ�Է��� ������ �־��ٸ�
        if (Input.touchCount > 0) {
            Touch myTouch = Input.touches[0]; //ù ��ġ�� �޾Ƶ���
            if (myTouch.phase == TouchPhase.Began) { //��ġ ����� ���۵Ǿ�����
                touchOrigin = myTouch.position; // ��ġ ��ġ ����
            } else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0) { // ��ġ����� ������ ȭ�鳻���� �̷�����ٸ�
                Vector2 touchEnd = myTouch.position; // ��ġ ���� ��ġ ����
                                                     // ��ġ�� ó���� �� ���� ���
                float x = touchEnd.x - touchOrigin.x;
                float y = touchEnd.y - touchOrigin.y;
                touchOrigin.x = -1; // �ٽ� ������ ����
                                    // ������ ��ġ�� �Ϻ��� ������ �ƴϹǷ� ����� ��������
                                    // �� �ǳ��� �����Ͽ� �ش�������� ������
                if (Mathf.Abs(x) > Mathf.Abs(y)) { // x�� y���� �� �Ǵٸ�
                    horizontal = x > 0 ? 1 : -1; // x�������� 1�Ǵ� -1 ������
                } else {
                    vertical = y > 0 ? 1 : -1; //�ƴϸ� y������ ������
                }
            }
        }
#endif
        if (horizontal != 0 || vertical != 0) AttemptMove<Wall>(horizontal, vertical); //�÷��̾�� ������ ��ȣ�ۿ��� �� �����Ƿ� ���ʸ��� Wall�� �Ѱ���
    }

    protected override void OnCantMove<T>(T component) {
        // �μ��� ���� component�� Wall�� ĳ����
        Wall hitWall = component as Wall;
        // DamageWall �޼ҵ�� �������� ��
        hitWall.DamageWall(wallDamage);
        _animator.SetTrigger("playerAttack");
    }

    private void Restart() {
        SceneManager.LoadScene(0);
    }

    public void LoseFood(int loss) {
        // �ִϸ����� ���� ����
        _animator.SetTrigger("playerHit");
        _food -= loss; // ���� ����Ʈ ����
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