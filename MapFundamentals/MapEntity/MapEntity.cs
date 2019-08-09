using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEntity : MonoBehaviour {

    public HexCoordinates? Location
    {
        get
        {
            if (LocationIsNull) { return null; }
            Transform parent = transform.parent;
            if (parent != null)
            {
                MapEntity EntityParent = parent.GetComponentInParent<MapEntity>();
                if (EntityParent != null)
                {
                    if(!EntityParent.Location.HasValue) { return null; }
                    return LocalLocation + EntityParent.Location;
                }
            }
            return LocalLocation;
        }
        set
        {
            Transform parent = transform.parent;
            if (parent != null)
            {
                MapEntity EntityParent = parent.GetComponentInParent<MapEntity>();
                if (EntityParent != null)
                {
                    if (!EntityParent.Location.HasValue) { return; } //children of null entities should stay null themselves
                    LocalLocation = value.Value - EntityParent.Location.Value;
                    return;
                }
            }
            LocalLocation = value;
        }
    }

    public HexCoordinates? LocalLocation
    {
        get
        {
            if (LocationIsNull) { return null; } //pseudo null
            return _LocalLocation;
        }
        set
        {
            if (value.HasValue) { _LocalLocation = value.Value; LocationIsNull = false; }
            else { LocationIsNull = true; }
            TeleportToMyLocation();
        }
    }
    [SerializeField]
    private HexCoordinates _LocalLocation;
    [SerializeField]
    public bool LocationIsNull = false;


    public void TeleportToMyLocation()
    {
        if(Location.HasValue)
        {
            Vector3 Position = HexMetrics.TileLocation(HexCoordinates.ToOffsetCoordinates(Location.Value));
            MeshRenderer Mesh = GetComponent<MeshRenderer>();
            if(Mesh !=null)
            {
                Position = new Vector3(Position.x, Position.y + Mesh.bounds.extents.y, Position.z);
            }
            gameObject.transform.position = Position;
        }
        else { gameObject.transform.position = new Vector3(int.MinValue, int.MinValue, int.MinValue); }
    }

    private void Awake()
    {
        var BoardTracker = FindObjectsOfType<EntityMapping>();
        foreach(EntityMapping thing in BoardTracker)
        {
            thing.RegisterEntity(this);
        }
        _LocalLocation = new HexCoordinates(0,0,0);
    }

    private void OnDestroy()
    {
        var BoardTracker = FindObjectsOfType<EntityMapping>();
        foreach (EntityMapping thing in BoardTracker)
        {
            thing.UnRegisterEntity(this);
        }
    }

    private void OnValidate()
    {
        TeleportToMyLocation();
    }
}
