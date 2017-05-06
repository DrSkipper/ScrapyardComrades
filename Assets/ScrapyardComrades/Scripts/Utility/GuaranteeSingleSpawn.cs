using UnityEngine;

public class GuaranteeSingleSpawn : MonoBehaviour
{
    public bool MarkedForDestruction { get { return _markedForDestruction; } }

    void Awake()
    {
        string n = this.name;
        this.name = this.name + "_check";
        if (GameObject.Find(n) != null)
        {
            _markedForDestruction = true;
            this.gameObject.SetActive(false);
            Destroy(this.gameObject);
        }
        else
        {
            this.name = n;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private bool _markedForDestruction;
}
