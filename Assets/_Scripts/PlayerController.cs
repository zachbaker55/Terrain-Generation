using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour {

    //Components 
    [SerializeField] private Transform mouseOutline;
    private Rigidbody2D rigidBody;
    
    //Fields
    [SerializeField] private float _moveSpeed = 1;
    

    //Using variables
    private Tilemap tilemap;
    private Grid grid;
    private Vector2 _movement = Vector2.zero; 

    private void Awake() {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        GetInput();
        //TODO: fix | HandleMouseInputs();
    }

    private void FixedUpdate() {
        DoMove();
    }

    private void GetInput() {
        Vector2 movement = new Vector2();
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        _movement = movement;
    }

    private void HandleMouseInputs() {
        Vector3Int mouseTile = GetMouseTile();
        if (mouseTile != null) {
            //Debug.Log("Tile: " + tilemap.GetTile(mouseTile) + " @ " + mouseTile);
            mouseOutline.transform.position = new Vector3(grid.CellToWorld(mouseTile).x, grid.CellToWorld(mouseTile).y, mouseOutline.transform.position.z);
            //TODO: fix | if (Input.GetMouseButtonDown(0)) TileManager.Instance.Break(mouseTile); 
        } else {
            //Debug.Log("No tile");
        }
    }

    private void DoMove() { 
        if (_movement != Vector2.zero) { 
            rigidBody.velocity = _moveSpeed * new Vector2(_movement.x, _movement.y).normalized;
        } else {
            rigidBody.velocity = Vector2.zero;
        }
    }

public Vector3Int GetMouseTile() {
    if (tilemap == null) {
        //TODO: fix | tilemap = TileManager.Instance.Tiles;
        grid = tilemap.layoutGrid;
    }

    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    mouseWorldPos.z = 0f;

    float yOffsetPerZLayer = grid.cellSize.y; //should be 0.5 

    //offset ~halfway between side and top
    float visualBiasOffset = yOffsetPerZLayer * 0.75f;
    mouseWorldPos.y += visualBiasOffset;

    Vector3Int selectedCell = Vector3Int.zero;
    int maxZ = 10;

    for (int z = maxZ; z >= 0; z--) {
        Vector3 adjustedMousePos = mouseWorldPos;
        adjustedMousePos.y -= z * yOffsetPerZLayer;

        Vector3Int cellPos = grid.WorldToCell(adjustedMousePos);
        cellPos.z = z;

        if (tilemap.HasTile(cellPos)) {
            selectedCell = cellPos;
            break;
        }
    }

    return selectedCell;
}






}
