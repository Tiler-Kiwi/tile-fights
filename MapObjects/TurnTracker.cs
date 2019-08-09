using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TurnTracker : MonoBehaviour
{
    public int TurnCount;
    public int TurnTaker;
    public int FactionCount
    {
        get
        {
            if(FactionList == null) { return 0; }
            return FactionList.Count;
        }
    }
    public List<Faction> FactionList;

    private void Start()
    {
        TurnCount = 0;
        TurnTaker = 0;
        if (FactionList == null || FactionList.Count == 0)
        {
            FactionList = FindFactionsWithMapEntites();
        }
    }

    public static List<Faction> FindFactionsWithMapEntites()
    {
        List<Faction> ReturnList = new List<Faction>();
        foreach (MapEntity entity in FindObjectsOfType<MapEntity>())
        {
            MapEntityFaction EntityFaction = entity.GetComponent<MapEntityFaction>();
            if (EntityFaction != null)
            {
                Faction FactionObject = EntityFaction.FactionObject;
                if (!ReturnList.Contains(FactionObject))
                {
                    ReturnList.Add(FactionObject);
                }
            }
        }
        return ReturnList;
    }

    public EventHandler<TurnChanged> TurnChangedEvent;
    public void OnTurnChanged(TurnChanged e)
    {
        EventHandler<TurnChanged> handler = TurnChangedEvent;
        if (handler != null)
        {
            handler(this, e);
        }
    }
    public int GetCurrentTurn()
    {
        return TurnCount;
    }

    public void AdvanceTurn()
    {
        TurnChanged e = new TurnChanged();
        e.PriorActiveFaction = FactionList[TurnTaker];
        TurnTaker = TurnTaker++;
        if(TurnTaker >= FactionCount) { TurnTaker = 0; }
 
        if (TurnTaker == 0)
        {
            TurnCount++;
            e.TurnCountIncrerased = true;
        }

        e.ActiveFaction = FactionList[TurnTaker];
        e.CurrentTurnCount = TurnCount;
        OnTurnChanged(e);

        /* handle with ActorManagement or some such thing
        List<Actor> ActorList = ActorsOnBoard.Keys.ToList();
        for (int i = 0; i < ActorList.Count; i++)
        {
            if (ActorsOnBoard[ActorList[i]].Item2 == TurnTaker)
            {
                ActorList[i].StartTurn();
            }
            else if (ActorsOnBoard[ActorList[i]].Item2 == TurnTaker.Previous())
            {
                ActorList[i].EndTurn();
            }
        }
        */
    }
}
