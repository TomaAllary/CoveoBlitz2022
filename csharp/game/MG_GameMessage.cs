using System.Collections.Generic;
using System.Linq;
using Blitz2022;

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
        }


        public Dictionary<string, MG_Team> getTeamsMapById
        {
            get { return this.teams.ToDictionary(team => team.id, team => team); }
        }
    }
}