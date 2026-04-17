using Godot;
using System;

public partial class ItemManager : Node
{
    [Export] private TileMapLayer _itemLayer;

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
}
