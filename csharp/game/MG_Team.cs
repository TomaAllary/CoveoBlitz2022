using System.Collections.Generic;
using static Blitz2022.Map;

namespace Blitz2022
{
    public class MG_Team
    {
        public string id;
        public string name;
        public int score;
        public List<MG_Unit> units;
        public List<string> errors;

        public MG_Team(Team original)
        {
            this.id = original.id;
            this.name = original.name;
            this.score = original.score;
            this.errors = original.errors;
            this.units = new List<MG_Unit>();
            foreach (Unit unit in original.units)
            {
                this.units.Add(new MG_Unit(unit));
            }
        }

        public void UpdateValues(Team team)
        {
            this.id = team.id;
            this.name = team.name;
            this.score = team.score;
            this.errors = team.errors;

            foreach(Unit unit in team.units)
            {
                units[getUnitIndexById(unit.id)].UpdateValues(unit);
            }
        }

        private int getUnitIndexById(string id)
        {
            for(int index = 0; index < units.Count; index++)
            {
                if(units[index].id == id)
                    return index;
            }
            return -1;

        }
    }

    public class MG_Unit
    {
        public string id;
        public string teamId;
        public Position position;
        public List<Position> path;
        public bool hasDiamond;
        public string diamondId;
        public bool hasSpawned;
        public bool isSummoning;
        public MG_UnitState lastState;

        public MG_Unit(Unit original)
        {
            this.id=original.id;
            this.teamId=original.teamId;

            this.position = original.position;
            this.path = original.path;
            this.hasDiamond = original.hasDiamond;
            this.diamondId = original.diamondId;
            this.hasSpawned = original.hasSpawned;
            this.isSummoning = original.isSummoning;

            lastState = new MG_UnitState(original.lastState);
        }

        public void UpdateValues(Unit original)
        {
            this.position = original.position;
            this.path = original.path;
            this.hasDiamond = original.hasDiamond;
            this.diamondId = original.diamondId;
            this.hasSpawned = original.hasSpawned;
            this.isSummoning = original.isSummoning;

            lastState.UpdateValues(original.lastState);
        }
    }

    public class MG_UnitState
    {
        public Position positionBefore;
        public string wasVinedBy;
        public string wasAttackedBy;

        public MG_UnitState(UnitState unitState)
        {
            this.positionBefore = unitState.positionBefore;
            this.wasVinedBy = unitState.wasVinedBy;
            this.wasAttackedBy = unitState.wasAttackedBy;
        }

        public void UpdateValues(UnitState unitState)
        {
            this.positionBefore = unitState.positionBefore;
            this.wasVinedBy = unitState.wasVinedBy;
            this.wasAttackedBy = unitState.wasAttackedBy;
        }
    }
}