using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {
    private SpriteRenderer _spriteRenderer;

    public Sprite dmgSprite;
    public int hp = 4; // º®ÀÇ hp

    void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void DamageWall(int loss) {
        _spriteRenderer.sprite = dmgSprite;
        hp -= loss;
        if (hp <= 0) gameObject.SetActive(false);
    }
}
