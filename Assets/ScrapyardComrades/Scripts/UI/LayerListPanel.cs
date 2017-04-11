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
        _entryX = ((RectTransform)this.LayerListEntryPrefab.transform).anchoredPosition.x;

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
            ((RectTransform)layerEntry.transform).anchoredPosition = new Vector2(_entryX, -this.Border - this.ElementHeight * i);
            layerEntry.transform.SetLocalZ(0);
            layerEntry.Title.text = sortedLayers[i];
            if (sortedLayers[i] == currentLayer)
                selection = i;
            _entryList.Add(layerEntry);
        }
        alignSelector(selection);
    }

    public void ChangeCurrentLayer(string currentLayer)
    {
        int selection = 0;
        for (int i = 0; i < _entryList.Count; ++i)
        {
            if (_entryList[i].Title.text == currentLayer)
                selection = i;
        }
        alignSelector(selection);
    }

    /**
     * Private
     */
    private List<LayerListEntry> _entryList;
    private float _entryX;

    private void alignSelector(int selection)
    {
        this.Selector.anchoredPosition = new Vector2(this.Selector.anchoredPosition.x, -this.Border - this.ElementHeight * selection - this.ElementHeight / 2 + this.Selector.sizeDelta.y / 2);
    }
}
