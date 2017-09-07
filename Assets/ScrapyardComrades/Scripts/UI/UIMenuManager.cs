using UnityEngine;

public class UIMenuManager : MonoBehaviour
{
    public Menu[] MenuStates;
    public PositionLerper MenuBounds;
    public UIMenuElement[] ElementVisuals;
    public PositionLerper SelectionIndicator;
    public string InitialStateName;
    public int Border = 4;

    public void Initialize()
    {
        configureForState(this.InitialStateName);
    }

    void FixedUpdate()
    {
        
    }

    /**
     * Private
     */
    private void configureForState(string state)
    {
        Menu menu = menuByStateName(state);

        for (int i = 0; i < this.ElementVisuals.Length; ++i)
        {
            if (i < menu.NumElements)
            {
                this.ElementVisuals[i].gameObject.SetActive(true);
                this.ElementVisuals[i].Configure(menu.Elements[i]);
            }
            else
            {
                this.ElementVisuals[i].gameObject.SetActive(false);
            }
        }

        this.MenuBounds.Destination = menu.transform;
        this.MenuBounds.BeginLerp();
    }

    private Menu menuByStateName(string stateName)
    {
        for (int i = 0; i < this.MenuStates.Length; ++i)
        {
            if (this.MenuStates[i].Name == stateName)
                return this.MenuStates[i];
        }
        return null;
    }
}
