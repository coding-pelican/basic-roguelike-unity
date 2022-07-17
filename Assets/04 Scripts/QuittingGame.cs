using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuittingGame : MonoBehaviour {
    private int _clickCount = 0;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            _clickCount++;
            if (!IsInvoking("DoubleClick"))
                Invoke(nameof(DoubleClick), 1.0f);

        } else if (_clickCount == 2) {
            CancelInvoke(nameof(DoubleClick));
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(); // 어플리케이션 종료
#endif
        }
    }

    private void DoubleClick() {
        _clickCount = 0;
    }
}
