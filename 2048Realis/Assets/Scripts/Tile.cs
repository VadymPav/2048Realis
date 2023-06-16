using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tile : MonoBehaviour
{
    public TileState State { get; private set; }
    public TileCell Cell { get; private set; }
    public int Number { get; private set; }
    
    public bool Locked { get; set; }

    private Image background;
    private TextMeshProUGUI text;

    private void Awake()
    {
        background = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetState(TileState state, int number)
    {
        this.State = state;
        this.Number = number;

        background.color = state.backgroundColor;
        text.color = state.textColor;
        text.text = number.ToString();
    }

    public void Spawn(TileCell cell)
    {
        if (this.Cell != null)
            this.Cell.Tile = null;
        this.Cell = cell;
        this.Cell.Tile = this;

        transform.position = cell.transform.position;
    }

    public void Merge(TileCell cell)
    {
        if (this.Cell != null)
            this.Cell.Tile = null;

        this.Cell = null;
        cell.Tile.Locked = true;
        Destroy(gameObject);
    }

    public void MoveTo(TileCell cell)
    {
        if (this.Cell != null)
            this.Cell.Tile = null;
        this.Cell = cell;
        this.Cell.Tile = this;

        transform.position = cell.transform.position;
    }
}
