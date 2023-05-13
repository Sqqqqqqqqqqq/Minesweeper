using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Game : MonoBehaviour
{
    public int width;
    public int height;
    public int mineCount;
    private Board board;
    private Cell[,] state;
    private bool gameOver;
    private bool firstMove;
    public TMP_Text minesText;
    public TMP_Text timerText;
    public float timeStart = 0;
    private int flaggedCnt = 0;
    private bool panelClicked = false;
    private bool panelResume = true;
    private bool resume = true;
    private bool restartPanelOn = false;
    [SerializeField] private TMP_Text time;
    
    private void Awake()
    {
        board = GetComponentInChildren<Board>();
        
        width = DataHolder.width;
        height = DataHolder.height;
        mineCount = DataHolder.mines; 

        if (height <= width) 
        {
            Camera.main.orthographicSize = (height + width) / 4 + System.Math.Max(height / width, width / height);
        } else 
        {
            Camera.main.orthographicSize = height / 2 + 2f;
        }
    }
    
    private void Start()
    {
        NewGame();
    }

    private void SetTime() 
    {
        time.text = (Mathf.Round(timeStart * 100f) / 100f).ToString();
    }

    public void NewGame() 
    {
        state = new Cell[width, height];
        gameOver = false;
        firstMove = false;
        minesText.text = "MINES: " + mineCount;
        timerText.text = "TIME: 0,00";
        Time.timeScale = 1;
        flaggedCnt = 0;
        timeStart = 0;
        
        GenerateCells();

        Camera.main.transform.position = new Vector3(width / 2.5f, height / 2f, -10f);
        board.Draw(state);
    }

    private void GenerateCells() 
    {
        for (int x = 0; x < width; ++x) 
        {
            for (int y = 0; y < height; ++y) 
            {
                Cell cell = new Cell();
                cell.position = new Vector3Int(x, y, 100);
                cell.number = 0;
                cell.type = Cell.Type.Empty;
                state[x, y] = cell;
            }
        }
    }

    private void GenerateMines(int cellPositionX, int cellPositionY) 
    {
        for (int i = 0; i < mineCount; ++i) 
        {
            int x = Random.Range(1, width - 1);
            int y = Random.Range(1, height - 1);
            while (state[x, y].type == Cell.Type.Mine || (System.Math.Abs(x - cellPositionX) + System.Math.Abs(y - cellPositionY)) < 4)
            {
                ++x;
                if (x >= width - 1) {
                    x = 1;
                    ++y;
                    if (y >= height - 1) {
                        y = 1;
                    }
                }
            }
            state[x, y].type = Cell.Type.Mine;
        }
    }

    private void GenerateNumbers() 
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (state[x, y].type == Cell.Type.Empty) {
                    Cell cell = state[x, y];
                    cell.number = CountMines(x, y);
                    if (cell.number > 0) 
                    {
                        cell.type = Cell.Type.Number;
                    }
                    state[x, y] = cell;
                }       
            }       
        }
    }

    private int CountMines(int cellX, int cellY) 
    {
        int count = 0;

        for (int adjacentX = -1; adjacentX <= 1; ++adjacentX)        {
            
            for (int adjacentY = -1; adjacentY <= 1; ++adjacentY)
            {
                int x = cellX + adjacentX;
                int y = cellY + adjacentY;

                if (!isValid(x, y)) {
                    continue;
                }

                if (GetCell(x, y).type == Cell.Type.Mine) 
                {
                    ++count;
                }
            }
        }
        return count;
    }

    private int CountFlags(int cellX, int cellY) 
    {
        int count = 0;

        for (int adjacentX = -1; adjacentX <= 1; ++adjacentX)        {
            
            for (int adjacentY = -1; adjacentY <= 1; ++adjacentY)
            {
                int x = cellX + adjacentX;
                int y = cellY + adjacentY;

                if (!isValid(x, y)) {
                    continue;
                }

                if (GetCell(x, y).flagged) 
                {
                    ++count;
                }
            }
        }
        return count;
    }

    public void RestartButton() 
    {
        panelClicked = true;
        restartPanelOn = false;
        panelResume = true;
    }

    public void ResumeButton() 
    {
        panelResume = true;
    }

    private void Update()
    {
        if (panelClicked) 
        {
            NewGame();
            panelClicked = false;
        } 
        else if (Input.GetKeyDown(KeyCode.Escape) && panelResume && !restartPanelOn) 
        {
            panelResume = false;
            resume = false;
            restartPanelOn = false;
            Camera.main.GetComponent<UIManager>().PauseOn();
        } 
        else if (Input.GetKeyDown(KeyCode.Escape) && !panelResume) 
        {
            panelResume = true;
            resume = true;
            Camera.main.GetComponent<UIManager>().PauseOff();
        } 
        else if (panelResume && !resume) 
        {
            resume = true;
        }
        else if (!gameOver && panelResume && resume) 
        {
            if (firstMove) 
            {
                timeStart += Time.deltaTime;
                timerText.text = "TIME: " + System.Math.Round(timeStart, 2).ToString();
            }

            if (Input.GetMouseButtonUp(1)) 
            {
                Flag();
            } 
            else if (Input.GetMouseButtonUp(0)) 
            {
                Reveal();
            }
        }
    }

    private void Reveal() 
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);

        if (!firstMove && isValid(cellPosition.x, cellPosition.y)) 
        {
            GenerateMines(cellPosition.x, cellPosition.y);
            GenerateNumbers();
            OpenBoundCells();
            board.Draw(state);
            firstMove = !firstMove;
        }

        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.flagged) 
        {
            return; 
        }

        switch (cell.type) {
            case Cell.Type.Mine: 
                Explode(cell);
                break;
            case Cell.Type.Empty: 
                Flood(cell);
                break;
            case Cell.Type.Number:
                if (cell.revealed) 
                {
                    bool isOpened = OpenNumberTiles(cell);
                    if (!isOpened) 
                    {
                        cell.revealed = true;
                        state[cellPosition.x, cellPosition.y] = cell;
                    }
                } 
                else 
                {
                    cell.revealed = true;
                    state[cellPosition.x, cellPosition.y] = cell;
                }
                break;
            default: 
                cell.revealed = true;
                state[cellPosition.x, cellPosition.y] = cell;
                break;
        }
        ChechWinCondition();
        board.Draw(state);
    }

    private void OpenBoundCells() 
    {
        for (int x = 0; x < width; ++x) 
        {
            for (int y = 0; y < height; ++y)
            {
                if (x == 0 || x == width - 1 || y == height - 1 || y == 0) {
                    if (state[x, y].type == Cell.Type.Empty) 
                    {
                        Flood(state[x, y]);
                    } 
                    else 
                    {
                        state[x, y].revealed = true;
                    }
                }
            }
        }
    }

    private bool OpenNumberTiles(Cell cell) 
    {
        if (CountFlags(cell.position.x, cell.position.y) == cell.number) {
            for (int adjacentX = -1; adjacentX <= 1; ++adjacentX)   
            {
                for (int adjacentY = -1; adjacentY <= 1; ++adjacentY)
                {
                    int x = cell.position.x + adjacentX;
                    int y = cell.position.y + adjacentY;

                    if (!isValid(x, y)) {
                        continue;
                    }

                    if (!GetCell(x, y).flagged) 
                    {
                        Cell opening = GetCell(x, y);
                        if (opening.type == Cell.Type.Mine) 
                        {
                            Explode(opening);
                        } 
                        else if (opening.type == Cell.Type.Empty) {
                            Flood(opening);
                        } 
                        else
                        {
                            opening.revealed = true;
                            state[x, y] = opening;
                        }
                    }
                }
            }
            return true;
        }
        return false;
    }

    private void ChechWinCondition() 
    {
        if (gameOver) {
            return;
        }
        for (int x = 0; x < width; ++x) 
        {
            for (int y = 0; y < height; ++y)
            {
                Cell cell = state[x, y];
                if (cell.type != Cell.Type.Mine && !cell.revealed) 
                {
                    return;
                }
            }
        }
        Camera.main.GetComponent<UIManager>().DelayWon();
        SetTime();
        minesText.text = "MINES: " + 0;
        gameOver = true;

        for (int x = 0; x < width; ++x) 
        {
            for (int y = 0; y < height; ++y)
            {
                Cell cell = state[x, y];

                if (cell.type == Cell.Type.Mine) 
                {
                    cell.flagged = true;
                    state[x, y] = cell;
                }
            }
        }
    }

    private void Explode(Cell cell) 
    {
        gameOver = true;

        cell.revealed = true;
        cell.exploded = true;

        state[cell.position.x, cell.position.y] = cell;
        board.Draw(state);

        for (int x = 0; x < width; ++x) 
        {
            for (int y = 0; y < height; ++y)
            {
                cell = state[x, y];

                if (cell.type == Cell.Type.Mine) 
                {
                    cell.revealed = true;
                    state[x, y] = cell;
                }
            }
        }
        restartPanelOn = true;
        Camera.main.GetComponent<UIManager>().DelayLose();
    }

    private void Flood(Cell cell) 
    {
        if (cell.revealed || cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid) 
        {
            return;
        }

        cell.revealed = true;
        state[cell.position.x, cell.position.y] = cell;

        if (cell.type == Cell.Type.Empty)
        {
            Flood(GetCell(cell.position.x + 1, cell.position.y));
            Flood(GetCell(cell.position.x - 1, cell.position.y));
            Flood(GetCell(cell.position.x, cell.position.y + 1));
            Flood(GetCell(cell.position.x, cell.position.y - 1));
            Flood(GetCell(cell.position.x + 1, cell.position.y + 1));
            Flood(GetCell(cell.position.x - 1, cell.position.y - 1));
            Flood(GetCell(cell.position.x - 1, cell.position.y + 1));
            Flood(GetCell(cell.position.x + 1, cell.position.y - 1));
        }
    }

    private void Flag() 
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);
        
        if (cell.type == Cell.Type.Invalid || cell.revealed) 
        {
            return; 
        }
        flaggedCnt += (cell.flagged) ? -1 : 1;
        minesText.text = "MINES: " + (mineCount - flaggedCnt);
        cell.flagged = !cell.flagged;
        state[cellPosition.x, cellPosition.y] = cell;

        board.Draw(state);
    }

    private Cell GetCell(int x, int y) 
    {
        if (isValid(x, y)) 
        {
            return state[x, y];
        } 
        else 
        {
            return new Cell();
        }
    }

    private bool isValid(int x, int y) 
    {
        return !(x < 0 || y < 0 || x >= width || y >= height);      
    }
}
