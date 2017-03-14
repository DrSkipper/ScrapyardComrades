using UnityEngine;
using System.Collections.Generic;

public class LayerListPanel : VoBehavior
{
    public PooledObject LayerListEntryPrefab;
    public RectTransform Selector;
    public int Border;
    public int ElementHeight;

    public void ConfigureForLayers(List<string> sortedLayers, string currentLayer)
    {
        if (_entryList == null)
            _entryList = new List<LayerListEntry>();

        while (_entryList.Count > 0)
        {
            _entryList[_entryList.Count - 1].GetComponent<PooledObject>().Release();
            _entryList.RemoveAt(_entryList.Count - 1);
        }

        int selection = 0;
        for (int i = 0; i < sortedLayers.Count; ++i)
        {
            PooledObject layerListObject = this.LayerListEntryPrefab.Retain();
            LayerListEntry layerEntry = layerListObject.GetComponent<LayerListEntry>();
            layerEntry.transform.SetParent(this.transform, false);
            ((RectTransform)layerEntry.transform).anchoredPosition = new Vector2(((RectTransform)layerEntry.transform).anchoredPosition.x, -this.Border - this.ElementHeight * i);
            layerEntry.Title.text = sortedLayers[i];
            if (sortedLayers[i] == currentLayer)
                selection = i;
            _entryList.Add(layerEntry);
        }
        //TODO: Move Selector
    }

    public void ChangeCurrentLayer(string currentLayer)
    {
        int selection = 0;
        for (int i = 0; i < _entryList.Count; ++i)
        {
            if (_entryList[i].Title.text == currentLayer)
                selection = i;
        }
        //TODO: Move Selector
    }

    /**
     * Private
     */
    private List<LayerListEntry> _entryList;
}
