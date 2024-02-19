using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] Tile tile;
    [SerializeField] GameObject[] points;
    [SerializeField] Transform[,] positions;
    [SerializeField] Sprite[] sprites;
    private Tile[,] grid;
    private int[,] numericGrid;

    // Start is called before the first frame update
    void Start()
    {
        InitBoard();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private bool IsValid(ref int[,] numericGrid, int row, int col, int number) {
        for (int i = 0; i < 9; ++i) {
            if (numericGrid[row, i] == number) {
                return false;
            }
        }
        
        for (int i = 0; i < 9; ++i) {
            if (numericGrid[i, col] == number) {
                return false;
            }
        }

        int corner_x = row / 3;
        int corner_y = col / 3;

        for (int i = 0; i < 3; ++i) 
            for (int j = 0; j < 3; ++j) 
                if (numericGrid[corner_x*3 + i, corner_y*3 + j] == number)
                    return false;

        return true;
    }

    private bool SudokuSolver(ref int[,] numericGrid, int row, int col) {
        if (col == 9) {
            if (row == 8) {
                return true;
            }
            col = 0;
            ++row;
        }

        if (numericGrid[row, col] > 0)
            return SudokuSolver(ref numericGrid, row, col + 1);

        for (int num = 1; num <= 9; ++num) {
            if (IsValid(ref numericGrid, row, col, num)) {
                numericGrid[row, col] = num;
                if (Finished())
                    return true;
                if (SudokuSolver(ref numericGrid, row, col + 1))
                    return true;
                numericGrid[row, col] = 0;
            }
        }
        
        return false;
    }
    private bool FillGrid(ref int[,] numericGrid, int row, int col) {
        if (col == 9) {
            if (row == 8) {
                return true;
            }
            col = 0;
            ++row;
        }

        if (numericGrid[row, col] > 0)
            return FillGrid(ref numericGrid, row, col + 1);

        List<int> numbers = new List<int>();
        for (int i = 1; i <= 9; ++i) {
            numbers.Add(i);
        }
        ShuffleList(numbers);

        for (int i = 0; i < numbers.Count; ++i) {
            if (IsValid(ref numericGrid, row, col, numbers[i])) {
                numericGrid[row, col] = numbers[i];
                if (Finished())
                    return true;
                if (FillGrid(ref numericGrid, row, col + 1))
                    return true;
                numericGrid[row, col] = 0;
            }
        }
        
        return false;
    }

    private void ShuffleList(List<int> list) {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private bool Finished() {
        for (int i = 0; i < 9; ++i) {
            for (int j = 0; j < 9; ++j) {
                if (numericGrid[i, j] > 0) 
                    return false;
            }
        }
        return true;
    }

    private void InitBoard() {
        int k = 0;
        grid = new Tile[9, 9];
        positions = new Transform[9, 9];
        numericGrid = new int[9, 9];
        for (int i = 0; i < 9; ++i) {
            for (int j = 0; j < 9; ++j) {
                positions[i, j] = points[k].transform;
                ++k;
            }
        }
        FillGrid(ref numericGrid, 0, 0);
        
        int attempts = Random.Range(17, 9*9 - 17);
        while (attempts > 0) {
            int row = Random.Range(0, 9);
            int col = Random.Range(0, 9);
            while (numericGrid[row, col] == 0) {
                row = Random.Range(0, 9);
                col = Random.Range(0, 9);
            }
            numericGrid[row, col] = 0;
            --attempts;
        }
        
        for (int i = 0; i < 9; ++i) {
            for (int j = 0; j < 9; ++j) {
                SpawnSingleNumber(i, j);
                grid[i, j].value = numericGrid[i, j];
                ApplySingleSprite(i, j);
            }
        }
    }

    private void SpawnSingleNumber(int x, int y) {
        grid[x, y] = Instantiate(tile, positions[x, y].transform);
    }

    private void ApplySingleSprite(int x, int y) {
        if (grid[x, y].value == 0) 
            grid[x, y].GetComponent<SpriteRenderer>().sprite = null;
        else 
            grid[x, y].GetComponent<SpriteRenderer>().sprite = sprites[grid[x, y].value - 1];
    }

    private void ApplyAllSprites() {
        if (SudokuSolver(ref numericGrid, 0, 0)) {
            for (int i = 0; i < 9; ++i) {
                for (int j = 0; j < 9; ++j) {
                    grid[i, j].value = numericGrid[i, j];
                    ApplySingleSprite(i, j);
                }
            }
        }
    }

    public void SolveButton() {
        ApplyAllSprites();
    }

    public void ResetButton() {
        SceneManager.LoadScene("SampleScene");
    }

    public void QuitButton() {
        Application.Quit();
    }
}
