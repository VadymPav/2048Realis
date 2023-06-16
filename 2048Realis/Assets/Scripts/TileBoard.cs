using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TileBoard : MonoBehaviour
{
    [Inject]
    private GameManager gameManager;
    [Inject]
    private TileGrid _grid;
    [Inject]
    private Tile tilePrefab;
    [Inject]
    private DiContainer _diContainer;
    public TileState[] tileStates;
    
    private List<Tile> _tiles;
    
    private Vector2 startPos;
    public int pixelDistToDetect = 20;
    private bool fingerDown;

    private void Awake()
    {
        _tiles = new List<Tile>(9);
    }

    public void ClearBoard()
    {
        foreach (var cell in _grid.Cells)
        {
            cell.Tile = null;
        }
        foreach (var tile in _tiles)
        {
            Destroy(tile.gameObject);
        }
        _tiles.Clear();
    }
    
    public void CreateTile()
    {
        GameObject tileTemp = _diContainer.InstantiatePrefab(tilePrefab, _grid.transform);
        Tile tile = tileTemp.GetComponent<Tile>();
        tile.SetState(tileStates[0], 2);
        tile.Spawn(_grid.GetRandomEmptyCell());
        _tiles.Add(tile);
    }

    private void Update()
    {
#if UNITY_Android || UNITY_IPHONE
        

        #region finger
        if(fingerDown == false && Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            // If so, we're going to set the startPos to the first touch's position, 
            startPos = Input.touches[0].position;
            // ... and set fingerDown to true to start checking the direction of the swipe.
            fingerDown = true;
        }
        
        if (fingerDown)
        {
            //Did we swipe up?
            if(Input.touches[0].position.y >= startPos.y + pixelDistToDetect)
            {
                fingerDown = false;
                //Move upwards
                MoveTiles(Vector2Int.up, 0, 1,1,1);
            }
            //Did we swipe down?
            else if(Input.touches[0].position.y <= startPos.y - pixelDistToDetect)
            {
                fingerDown = false;
                //Move downwards
                MoveTiles(Vector2Int.down, 0, 1,_grid.Height - 2,-1);
            }
            //Did we swipe left?
            else if(Input.touches[0].position.x <= startPos.x - pixelDistToDetect)
            {
                fingerDown = false;
                //Move left
                MoveTiles(Vector2Int.left, 1, 1,0,1);
            }
            //Did we swipe right?
            else if(Input.touches[0].position.x >= startPos.x + pixelDistToDetect)
            {
                fingerDown = false;
                //Move right
                MoveTiles(Vector2Int.right, _grid.Width - 2, -1,0,1);
            }
        }
        #endregion
#endif
        
        #region mouse
        if(!fingerDown && Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            fingerDown = true;
        }
        if(fingerDown && Input.GetMouseButtonUp(0))
        {
            //startPos will be reset
            fingerDown = false;
        }
        if (fingerDown)
        {
            //Did we swipe up?
            if(Input.mousePosition.y >= startPos.y + pixelDistToDetect)
            {
                fingerDown = false;
                //Move upwards
                MoveTiles(Vector2Int.up, 0, 1,1,1);
            }
            //Did we swipe down?
            else if(Input.mousePosition.y <= startPos.y - pixelDistToDetect)
            {
                fingerDown = false;
                //Move downwards
                MoveTiles(Vector2Int.down, 0, 1,_grid.Height - 2,-1);
                
            }
            //Did we swipe left?
            else if(Input.mousePosition.x <= startPos.x - pixelDistToDetect)
            {
                fingerDown = false;
                //Move left
                MoveTiles(Vector2Int.left, 1, 1,0,1);
                
            }
            //Did we swipe right?
            else if(Input.mousePosition.x >= startPos.x + pixelDistToDetect)
            {
                fingerDown = false;
                //Move right
                MoveTiles(Vector2Int.right, _grid.Width - 2, -1,0,1);
            }
        }
        

        #endregion
    }

    public void MoveTiles(Vector2Int direction, int startX, int incrementX, int startY, int incrementY)
    {
        bool changed = false;
        for (int x = startX; x>= 0 && x < _grid.Width; x += incrementX)
        {
            for (int y = startY; y >= 0 && y < _grid.Height; y += incrementY)
            {
                TileCell cell = _grid.GetCell(x, y);
                if (cell.Occupied)
                {
                    changed |= MoveTile(cell.Tile, direction);
                }
            }
        }
        if (changed) {
            StartCoroutine(WaitForChanges());
        }
    }

    private bool MoveTile(Tile tile, Vector2Int direction)
    {
        TileCell newCell = null;
        TileCell adjacent = _grid.GetAdjacentCell(tile.Cell, direction);

        while (adjacent != null)
        {
            if (adjacent.Occupied)
            {
                if (CanMerge(tile, adjacent.Tile))
                {
                    Merge(tile, adjacent.Tile);
                    return true;
                }
                break;
            }

            newCell = adjacent;
            adjacent = _grid.GetAdjacentCell(adjacent, direction);
        }

        if (newCell != null)
        {
            tile.MoveTo(newCell);
            return true;
        }
        return false;
    }

    private bool CanMerge(Tile a, Tile b)
    {
        return a.Number == b.Number && !b.Locked;
    }

    private void Merge(Tile a, Tile b)
    {
        _tiles.Remove(a);
        a.Merge(b.Cell);

        int index = Mathf.Clamp(IndexOf(b.State) + 1, 0, tileStates.Length - 1);
        int number = b.Number * 2;
        
        b.SetState(tileStates[index], number);
        
        gameManager.IncreaseScore(1);
    }

    private int IndexOf(TileState state)
    {
        for (int i = 0; i < tileStates.Length; i++)
        {
            if (state == tileStates[i])
                return i;
        }

        return -1;
    }

    private IEnumerator WaitForChanges()
    {
        yield return new WaitForSeconds(0.1f);
        foreach (var tile in _tiles)
        {
            tile.Locked = false;
        }
        if (_tiles.Count != _grid.Size)
            CreateTile();
        if (CheckForGameOver())
        {
            gameManager.GameOver();
        }
    }

    private bool CheckForGameOver()
    {
        if (_tiles.Count != _grid.Size)
        {
            return false;
        }

        foreach (var tile in _tiles)
        {
            TileCell up = _grid.GetAdjacentCell(tile.Cell, Vector2Int.up);
            TileCell down = _grid.GetAdjacentCell(tile.Cell, Vector2Int.down);
            TileCell left = _grid.GetAdjacentCell(tile.Cell, Vector2Int.left);
            TileCell right = _grid.GetAdjacentCell(tile.Cell, Vector2Int.right);

            if (up != null && CanMerge(tile, up.Tile))
                return false;
            if (down != null && CanMerge(tile, down.Tile))
                return false;
            if (left != null && CanMerge(tile, left.Tile))
                return false;
            if (right != null && CanMerge(tile, right.Tile))
                return false;
        }

        return true;
    }
}
