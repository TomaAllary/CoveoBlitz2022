using System.Collections.Generic;
using System.Linq;
using Blitz2022;
using static Blitz2022.Map;

namespace Blitz2022
{
    public class MG_GameConfig
    {
        public int pointsPerDiamond;
        public int maximumDiamondSummonLevel;
        public int initialDiamondSummonLevel;
    }

    public class MG_GameMessage
    {
        public int tick;

        public int totalTick;

        public string teamId;
        public List<MG_Team> teams;

        public Map map;

        public GameConfig gameConfig;

        public Dictionary<int, string[]> teamPlayOrderings;

        public Position[] considerWall;


        //only use to instantiate (override values)
        public MG_GameMessage(GameMessage original)
        {
            this.tick = original.tick;
            this.totalTick = original.totalTick;
            this.teamId = original.teamId;

            this.teams = new List<MG_Team>();
            foreach (Team team in original.teams)
            {
                this.teams.Add(new MG_Team(team));
            }

            this.map = original.map;
            this.gameConfig = original.gameConfig;
            this.teamPlayOrderings = original.teamPlayOrderings;
        }

        //Use to update state after each msg
        public void UpdateValues(GameMessage original)
        {
            //values to override
            this.tick = original.tick;
            this.totalTick = original.totalTick;
            this.teamId = original.teamId;
            this.teamPlayOrderings = original.teamPlayOrderings;

            Dictionary<string, Team> orginalTeams = original.getTeamsMapById;
            //values to updates
            foreach (MG_Team team in this.teams)
            {
                team.UpdateValues(orginalTeams[team.id]);
            }
            map.UpdateValues(original.map);

            //Refreash diamonds targets
            foreach(Diamond d in map.diamonds)
            {
                d.allyTargetingId = null;
            }
        }

        public MG_Team getTeamByUnitId(string id)
        {
            foreach (MG_Team team in this.teams)
            {
                foreach (MG_Unit unit in team.units)
                {
                    if (unit.id == id)
                        return team;
                }
            }

            return null;
        }


        public Dictionary<string, MG_Team> getTeamsMapById
        {
            get { return this.teams.ToDictionary(team => team.id, team => team); }
        }

        public bool isConsideredWall(Position p)
        {
            if(considerWall == null)
                return false;

            for(int i =0; i < considerWall.Length; i++)
            {
                if (considerWall[i].x == p.x && considerWall[i].y == p.y)
                    return true;
            }

            return false;
        }
    }
}