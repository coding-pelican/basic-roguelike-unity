using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[SerializeField]
public class Count {
    public int minimum;
    public int maximum;

    public Count(int minimum, int maximum) {
        this.minimum = minimum;
        this.maximum = maximum;
    }
}

public class BoardManager : MonoBehaviour {
    private Transform _boardHolder;
    private List<Vector3> _gridPositions = new List<Vector3>();

    public List<Vector3> GridPositions { get => _gridPositions; set => _gridPositions = value; }

    public int columns = 16;
    public int rows = 16;
    public Count wallCount = new Count(5, 19); // 레벨 당 벽의 하한,상한값
    public Count foodCount = new Count(1, 9); // 레벨 당 음식 하한,상한값

    public GameObject exit;
    public GameObject[] floorTiles;
    public GameObject[] outerWallTiles;
    public GameObject[] wallTiles;
    public GameObject[] foodTiles;
    public GameObject[] enemyTiles;

    void InitialiseList() {
        GridPositions.Clear();
        for (int x = 1; x < columns - 1; x++) {
            for (int y = 1; y < rows - 2; y++) {
                GridPositions.Add(new Vector3(x, y, 0f));
            }
        }
    }

    // outerWall과 floor 세팅
    void SetupBoard() {
        // 새 Board 오브젝트를 인스턴스화하고 그 트랜스폼을 보드홀더에 저장
        _boardHolder = new GameObject("Board").transform;
        for (int x = -1; x < columns + 1; x++) {
            for (int y = -1; y < rows + 1; y++) {
                // 바닥타일 8종 중 하나의 프리팹을 랜덤으로 고름
                GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];
                // 만약 위치가 테두리라면 외벽타일 중 골라 다시 저장
                if (x == -1 || x == columns || y == -1 || y == rows) {
                    toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
                }
                // 고른 프리팹을 현재 순회중인 위치에 인스턴스화하여 저장
                GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
                // 생성된 인스턴스의 트랜스폼을 boardHolder의 자식으로 둠
                instance.transform.SetParent(_boardHolder);
            }
        }
    }

    // gridPosition에서 랜덤으로 하나 고를 수 있게 함
    Vector3 RandomizePosition() {
        // gridPositon으 길이 내에서 랜덤으로 인덱스 형성
        int randomIndex = Random.Range(0, GridPositions.Count);
        // 랜덤으로 형성한 인덱스에 해당하는 gridPositons를 저장하는 변수선언
        Vector3 randomPosition = GridPositions[randomIndex];
        // randomIndex에 해당하는 gridPositions값 삭제하여 사용가능하게 함
        GridPositions.RemoveAt(randomIndex);
        return randomPosition;
    }

    // 주어진 상/하한 값 내에서 랜덤으로 지정한 타일을 생성해줌
    void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum) {
        // 랜덤으로 개수 선택
        int objectCount = Random.Range(minimum, maximum + 1);
        // 정한 개수만큼 오브젝트 생성
        for (int i = 0; i < objectCount; i++) {
            Vector3 randomPosition = RandomizePosition();
            // 주어진 타일 종류 내에서 골라 저장
            GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];
            // 고른 타일을 인스턴스화
            Instantiate(tileChoice, randomPosition, Quaternion.identity);
        }
    }

    public void SetupScene(int level) {
        SetupBoard();
        InitialiseList();
        LayoutObjectAtRandom(wallTiles, wallCount.minimum, wallCount.maximum);
        LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum);
        // 적의 수는 레벨에 따라 로그함수적으로 결정
        // 따라서 레벨 2에는 적 1, 4에는 적 2, 8에는 적 3
        int enemyCount = (int)Mathf.Log(level, 2f);
        LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);
        // 최종적으로 exit를 우상단에 생성
        Instantiate(exit, new Vector3(columns - 1, rows - 1, 0f), Quaternion.identity);
    }

}
