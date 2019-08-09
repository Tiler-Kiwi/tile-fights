using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// "Stitches" together entities and a tile list, to allow checking spaces for entities and other methods.
/// </summary>
    public class EntityMapping : MonoBehaviour
    {
    [SerializeField]
    List<MapEntity> ActiveEntity;
    [SerializeField]
    HashSet<MapEntity> EntityHash;
    [SerializeField]


    public List<MapEntity> EntityList
    {
        get
        {
            return new List<MapEntity>(ActiveEntity);
        }
        set { }
    }
    
    private void Awake()
    {
        RefreshEntityList();
    }

    public void RefreshEntityList() //slow
    {
        EntityHash = new HashSet<MapEntity>();
        ActiveEntity = FindObjectsOfType<MapEntity>().ToList();
        foreach(MapEntity waste in ActiveEntity)
        {
            EntityHash.Add(waste);
        }
    }

    internal void RegisterEntity(MapEntity mapEntity)
    {
        if (EntityHash.Add(mapEntity))
        {
            ActiveEntity.Add(mapEntity);
        }
    }

    internal void UnRegisterEntity(MapEntity mapEntity)
    {
        if(EntityHash.Remove(mapEntity))
        {
            ActiveEntity.Remove(mapEntity);
        }
    }

    public List<MapEntity> EntitysAtLocation(HexCoordinates coords)
    {
        List<MapEntity> ReturnVal = new List<MapEntity>();
        foreach(MapEntity i in ActiveEntity)
        {
            if(i.Location == coords)
            {
                ReturnVal.Add(i);
            }
        }
        return ReturnVal;
    }

    public List<MapEntity> EntitysAtLocation(List<HexCoordinates> coords)
    {
        /*
         * Could possibly do some sort of check for maximum / minumums to detect if object is first within range of parameters, then do more specific check
         * Possible use of recursion for increasingly precise checks.
         * But probably overkill for now.
         * possible target for optimization
         */
        List<MapEntity> ReturnVal = new List<MapEntity>();
        foreach(MapEntity i in ActiveEntity)
        {
            for(int c = 0; c<coords.Count; c++)
            {
                if(i.Location == coords[c])
                {
                    ReturnVal.Add(i);
                    break;
                }
            }
        }
        return ReturnVal;
    }

    public List<ExtraPath> ExtraPathsAtLocation(HexCoordinates coords)
    {
        List<MapEntity> LocalEnts = EntitysAtLocation(coords);
        List<ExtraPath> ReturnVal = new List<ExtraPath>();
        foreach(MapEntity oof in LocalEnts)
        {
            EntityMapPathModifier foo = oof.GetComponent<EntityMapPathModifier>();
            if(foo != null)
            {
                ReturnVal.AddRange(foo.Paths.ToList());
            }
        }
        return ReturnVal;
    }
}

