using UnityEngine;
using UnityEngine.UI;

public class MapEditorLightingPanel : MonoBehaviour
{
    public Image ColorPreview;
    public Text DistanceValueText;
    public Text RangeValueText;
    public Text IntensityValueText;
    public Text SpotAngleValueText;
    public Text RotXValueText;
    public Text RotYValueText;
    public GameObject TypeValueObject;
    public GameObject AffectsForegroundObject;
    public GameObject[] AffectsParallaxObjects;
    [HideInInspector]
    public Color[] ValidLightColors;
    public Vector3[] AffectsParallaxConfigurations;

    public void ShowForLayer(MapEditorLayer layer)
    {
        _layer = layer as MapEditorLightingLayer;
        _currentColorIndex = findCurrentColorIndex();
        _currentAffectParallaxIndex = findCurrentAffectParallaxIndex();
        updateVisual();
    }

    void Update()
    {
        if (MapEditorInput.CyclePrev)
        {
            --_currentColorIndex;
            if (_currentColorIndex < 0)
                _currentColorIndex = this.ValidLightColors.Length - 1;
            Color c = this.ValidLightColors[_currentColorIndex];
            _layer.CurrentProperties.r = c.r;
            _layer.CurrentProperties.g = c.g;
            _layer.CurrentProperties.b = c.b;
            updateVisual();
        }
        else if (MapEditorInput.CycleNext)
        {
            ++_currentColorIndex;
            if (_currentColorIndex >= this.ValidLightColors.Length)
                _currentColorIndex = 0;
            Color c = this.ValidLightColors[_currentColorIndex];
            _layer.CurrentProperties.r = c.r;
            _layer.CurrentProperties.g = c.g;
            _layer.CurrentProperties.b = c.b;
            updateVisual();
        }
        else if (MapEditorInput.Action)
        {
            _layer.CurrentProperties.light_type = _layer.CurrentProperties.light_type == (int)LightType.Spot ? (int)LightType.Point : (int)LightType.Spot;
            updateVisual();
        }
        else if (MapEditorInput.NavDown)
        {
            _layer.CurrentProperties.distance = Mathf.Max(0, _layer.CurrentProperties.distance - INCREMENT);
            updateVisual();
        }
        else if (MapEditorInput.NavUp)
        {
            _layer.CurrentProperties.distance = Mathf.Min(500, _layer.CurrentProperties.distance + INCREMENT);
            updateVisual();
        }
        else if (MapEditorInput.NavLeft)
        {
            _layer.CurrentProperties.intensity = Mathf.Max(0, _layer.CurrentProperties.intensity - SMALL_INCREMENT);
            updateVisual();
        }
        else if (MapEditorInput.NavRight)
        {
            _layer.CurrentProperties.intensity = Mathf.Min(25, _layer.CurrentProperties.intensity + SMALL_INCREMENT);
            updateVisual();
        }
        else if (MapEditorInput.ResizeDown)
        {
            _layer.CurrentProperties.range = Mathf.Max(0, _layer.CurrentProperties.range - INCREMENT);
            updateVisual();
        }
        else if (MapEditorInput.ResizeUp)
        {
            _layer.CurrentProperties.range = Mathf.Min(1000, _layer.CurrentProperties.range + INCREMENT);
            updateVisual();
        }
        else if (MapEditorInput.ResizeLeft)
        {
            _layer.CurrentProperties.spot_angle = Mathf.Max(-90, _layer.CurrentProperties.spot_angle - MID_INCREMENT);
            updateVisual();
        }
        else if (MapEditorInput.ResizeRight)
        {
            _layer.CurrentProperties.spot_angle = Mathf.Min(90, _layer.CurrentProperties.spot_angle + MID_INCREMENT);
            updateVisual();
        }
        else if (MapEditorInput.Cancel)
        {
            _layer.CurrentProperties.affects_foreground = !_layer.CurrentProperties.affects_foreground;
            updateVisual();
        }
        else if (MapEditorInput.CycleNextAlt)
        {
            _currentAffectParallaxIndex = _currentAffectParallaxIndex < this.AffectsParallaxConfigurations.Length - 1 ? _currentAffectParallaxIndex + 1 : 0;
            updateAffectsParallax();
            updateVisual();
        }
        else if (MapEditorInput.CyclePrevAlt)
        {
            _currentAffectParallaxIndex = _currentAffectParallaxIndex > 0 ? _currentAffectParallaxIndex - 1 : this.AffectsParallaxConfigurations.Length - 1;
            updateAffectsParallax();
            updateVisual();
        }
    }

    /**
     * Private
     */
    private MapEditorLightingLayer _layer;
    private int _currentColorIndex;
    private int _currentAffectParallaxIndex;

    private const int INCREMENT = 10;
    private const int MID_INCREMENT = 5;
    private const int SMALL_INCREMENT = 1;

    private int findCurrentAffectParallaxIndex()
    {
        for (int i = 0; i < this.AffectsParallaxConfigurations.Length; ++i)
        {
            Vector3 config = this.AffectsParallaxConfigurations[i];
            if ((Mathf.RoundToInt(config.x) != 0) == _layer.CurrentProperties.AffectsParallax(0) && (Mathf.RoundToInt(config.y) != 0) == _layer.CurrentProperties.AffectsParallax(1) && (Mathf.RoundToInt(config.z) != 0) == _layer.CurrentProperties.AffectsParallax(2))
            {
                return i;
            }
        }
        return 0;
    }

    private void updateAffectsParallax()
    {
        Vector3 config = this.AffectsParallaxConfigurations[_currentAffectParallaxIndex];
        _layer.CurrentProperties.SetAffectsParallax(2, Mathf.RoundToInt(config.z) != 0, 3);
        _layer.CurrentProperties.SetAffectsParallax(1, Mathf.RoundToInt(config.y) != 0, 3);
        _layer.CurrentProperties.SetAffectsParallax(0, Mathf.RoundToInt(config.x) != 0, 3);
    }

    private void updateVisual()
    {
        this.ColorPreview.color = this.ValidLightColors[_currentColorIndex];
        this.TypeValueObject.SetActive(_layer.CurrentProperties.light_type == (int)LightType.Spot);

        this.DistanceValueText.text = _layer.CurrentProperties.distance.ToString();
        this.RangeValueText.text = _layer.CurrentProperties.range.ToString();
        this.IntensityValueText.text = _layer.CurrentProperties.intensity.ToString();
        this.SpotAngleValueText.text = _layer.CurrentProperties.spot_angle.ToString();
        this.RotXValueText.text = _layer.CurrentProperties.rot_x.ToString("0.0");
        this.RotYValueText.text = _layer.CurrentProperties.rot_y.ToString("0.0");

        this.AffectsForegroundObject.SetActive(_layer.CurrentProperties.affects_foreground);
        for (int i = 0; i < this.AffectsParallaxObjects.Length; ++i)
        {
            this.AffectsParallaxObjects[i].SetActive(_layer.CurrentProperties.AffectsParallax(i));
        }
    }

    private int findCurrentColorIndex()
    {
        for (int i = 0; i < this.ValidLightColors.Length; ++i)
        {
            Color c = this.ValidLightColors[i];
            if (Mathf.Approximately(c.r, _layer.CurrentProperties.r) && Mathf.Approximately(c.g, _layer.CurrentProperties.g) && Mathf.Approximately(c.b, _layer.CurrentProperties.b))
                return i;
        }
        return 0;
    }
}
