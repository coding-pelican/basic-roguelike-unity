using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {
    public int hp = 4; // º®ÀÇ hp
    public Sprite dmgSprite;
    public AudioClip chopSound1;
    public AudioClip chopSound2;


    private SpriteRenderer _spriteRenderer;

    void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void DamageWall(int loss) {
        _spriteRenderer.sprite = dmgSprite;
        SoundManager.instance.RandomizeSfx(chopSound1, chopSound2);
        hp -= loss;
        if (hp <= 0) gameObject.SetActive(false);
    }
}
