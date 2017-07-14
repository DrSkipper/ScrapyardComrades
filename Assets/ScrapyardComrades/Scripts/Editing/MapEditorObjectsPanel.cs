using UnityEngine;

public class MapEditorObjectsPanel : MonoBehaviour
{
    public ObjectParamEditPanel[] ParamPanels;
    public GameObject NoParametersPanel;

    public void ShowForLayer(MapEditorLayer layer)
    {
        _layer = layer as MapEditorObjectsLayer;
        _selectedPanel = 0;
        _parameters = null;
        _paramTypes = null;
        ObjectConfigurer configurer = null;

        if (_layer.CurrentPrefab != null)
        {
            if (_layer.CurrentPrefab is PooledObject)
                configurer = (_layer.CurrentPrefab as PooledObject).GetComponent<ObjectConfigurer>();
            else if (_layer.CurrentPrefab is GameObject)
                configurer = (_layer.CurrentPrefab as GameObject).GetComponent<ObjectConfigurer>();
            //TODO: else if Sprite, have a parameter for flipping x?
        }

        if (configurer != null)
        {
            _paramTypes = configurer.ParameterTypes;
            _parameters = _layer.CurrentObjectParams;

            ensureAllParamTypes();

            for (int i = 0; i < this.ParamPanels.Length; ++i)
            {
                if (_parameters != null && i < _paramTypes.Length)
                {
                    this.ParamPanels[i].gameObject.SetActive(true);
                    this.ParamPanels[i].ShowForParam(_parameters[i]);
                }
                else
                {
                    this.ParamPanels[i].gameObject.SetActive(false);
                }
            }
        }

        if (_parameters != null && _parameters.Length > 0)
        {
            this.NoParametersPanel.SetActive(false);
            selectPanel(_selectedPanel);
        }
        else
        {
            for (int i = 0; i < this.ParamPanels.Length; ++i)
            {
                this.ParamPanels[i].gameObject.SetActive(false);
            }
            this.NoParametersPanel.SetActive(true);
        }
    }

    void Update()
    {
        if (_parameters != null)
        {
            bool down = MapEditorInput.NavDown;
            bool up = MapEditorInput.NavUp;
            bool left = MapEditorInput.NavLeft;
            bool right = MapEditorInput.NavRight;

            if (_parameters.Length > 1 && (up || down))
            {
                deselectPanel(_selectedPanel);
                if (down)
                {
                    _selectedPanel = _selectedPanel == _parameters.Length - 1 ? 0 : _selectedPanel + 1;
                }
                else if (up)
                {
                    _selectedPanel = _selectedPanel == 0 ? _parameters.Length - 1 : _selectedPanel - 1;
                }
                selectPanel(_selectedPanel);
            }
            else if (left || right)
            {
                int oIndex = indexOfValue(_paramTypes[_selectedPanel].Options, _parameters[_selectedPanel].CurrentOption);
                if (right)
                {
                    oIndex = oIndex == _paramTypes[_selectedPanel].Options.Length - 1 ? 0 : oIndex + 1;
                }
                else if (left)
                {
                    oIndex = oIndex == 0 ? _paramTypes[_selectedPanel].Options.Length - 1 : oIndex - 1;
                }

                _parameters[_selectedPanel].CurrentOption = _paramTypes[_selectedPanel].Options[oIndex];
                _layer.CurrentObjectParams = _parameters;
                this.ParamPanels[_selectedPanel].ShowForParam(_parameters[_selectedPanel]);
            }
        }
    }

    /**
     * Private
     */
    private int _selectedPanel;
    private NewMapInfo.ObjectParam[] _parameters;
    private ObjectConfigurer.ObjectParamType[] _paramTypes;
    private MapEditorObjectsLayer _layer;

    private void deselectPanel(int i)
    {
        this.ParamPanels[i].SelectionIcon.SetActive(false);
    }

    private void selectPanel(int i)
    {
        this.ParamPanels[i].SelectionIcon.SetActive(true);
    }

    private void ensureAllParamTypes()
    {
        if (_parameters == null)
        {
            _parameters = new NewMapInfo.ObjectParam[_paramTypes.Length];
        }

        if (_parameters.Length != _paramTypes.Length)
        {
            NewMapInfo.ObjectParam[] oldParams = _parameters;
            _parameters = new NewMapInfo.ObjectParam[_paramTypes.Length];
            
            for (int i = 0; i < oldParams.Length; ++i)
            {
                _parameters[i] = oldParams[i];
            }
        }

        for (int i = 0; i < _paramTypes.Length; ++i)
        {
            int pIndex = findIndexForParamType(_paramTypes[i].Name);
            _parameters[pIndex].Name = _paramTypes[i].Name;
            if (indexOfValue(_paramTypes[i].Options, _parameters[pIndex].CurrentOption) < 0)
                _parameters[pIndex].CurrentOption = _paramTypes[i].Options[0];
        }
    }

    private int findIndexForParamType(string pName)
    {
        for (int i = 0; i < _parameters.Length; ++i)
        {
            if (_parameters[i].Name == pName)
                return i;
        }

        for (int i = 0; i < _parameters.Length; ++i)
        {
            if (_parameters[i].Name.IsEmpty())
                return i;
        }

        for (int i = 0; i < _parameters.Length; ++i)
        {
            bool found = false;
            for (int j = 0; j < _paramTypes.Length; ++j)
            {
                if (_paramTypes[j].Name == _parameters[i].Name)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                return i;
        }

        return _parameters.Length - 1;
    }

    private int indexOfValue(string[] options, string choice)
    {
        for (int i = 0; i < options.Length; ++i)
        {
            if (options[i] == choice)
                return i;
        }
        return -1;
    }
}
