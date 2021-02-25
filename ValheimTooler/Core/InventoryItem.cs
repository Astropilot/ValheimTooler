using UnityEngine;

namespace ValheimTooler.Core
{
    class InventoryItem
    {
        public GameObject itemPrefab;
        public ItemDrop itemDrop;

        public InventoryItem(GameObject itemPrefab, ItemDrop itemDrop)
        {
            this.itemPrefab = itemPrefab;
            this.itemDrop = itemDrop;
        }
    }
}
