using Godot;
using System;
using System.Collections.Generic;

public partial class ItemManager : Node
{
    [Export] private TileMapLayer _itemLayer;
	
	private Dictionary<Vector2I, (int sourceId, Vector2I atlasCoords)> _initialTiles
    = new();
	
	public override void _Ready()
	{
	    foreach (var cell in _itemLayer.GetUsedCells())
	    {
	        _initialTiles[cell] = (
	            _itemLayer.GetCellSourceId(cell),
	            _itemLayer.GetCellAtlasCoords(cell)
	        );
	    }
	}

    public void PlaceItem(Vector2I itemTile)
    {
        TileData itemData = _itemLayer.GetCellTileData(itemTile);

        if (itemData == null)
        {
            _itemLayer.SetCell(itemTile, 0, new Vector2I(3, 0));  // put new item in that space and return
            return;
        }

        if ((bool)itemData.GetCustomData("IsItem"))
        {
            int itemQuantity = (int)itemData.GetCustomData("ItemQuantity");

            switch (itemQuantity)
            {
                case 1 or 2:
                    Vector2I atlasCoords = _itemLayer.GetCellAtlasCoords(itemTile);
                    _itemLayer.SetCell(itemTile, 0, atlasCoords + new Vector2I(0, 1));
                    break;
                case 3:
                    return;
                default:
                    GD.PushError("Item quantity must range from 1 to 3");
                    break;
            }
        }
    }

    public void RemoveItem(Vector2I itemTile)
    {
        TileData itemData = _itemLayer.GetCellTileData(itemTile);

        if (itemData == null) return;  // can't remove an item that isn't there

        if ((bool)itemData.GetCustomData("IsItem"))
        {
            int itemQuantity = (int)itemData.GetCustomData("ItemQuantity");

            switch (itemQuantity)
            {
                case 1:
                    _itemLayer.EraseCell(itemTile); // if last item, remove the cell completely
                    break;
                case 2 or 3:
                    Vector2I atlasCoords = _itemLayer.GetCellAtlasCoords(itemTile);
                    _itemLayer.SetCell(itemTile, 0, atlasCoords - new Vector2I(0, 1));
                    break;
                default:
                    GD.PushError("Item quantity must range from 1 to 3");
                    break;
            }
        }
    }
	
	public void ResetItems()
	{
	    _itemLayer.Clear();

	    foreach (var kvp in _initialTiles)
	    {
	        var tile = kvp.Key;
	        var (sourceId, atlasCoords) = kvp.Value;

	        _itemLayer.SetCell(tile, sourceId, atlasCoords);
	    }
	}
}
