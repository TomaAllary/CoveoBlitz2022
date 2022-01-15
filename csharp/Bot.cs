using System;
using System.Collections.Generic;
using System.Linq;
using static Blitz2022.Action;
using static Blitz2022.Map;

namespace Blitz2022
{
    public class Bot
    {
        public static string NAME = "MyBot C♭";
        private MG_GameMessage mG_GameMessage;

        public Bot()
        {
            // initialize some variables you will need throughout the game here
            Console.WriteLine("Initializing your super mega bot!");
        }

        /*
        * Here is where the magic happens, for now the moves are random. I bet you can do better ;)
        */
        public GameCommand nextMove(GameMessage gameMessage)
        {
            try {
                if(mG_GameMessage == null)
                {
                    mG_GameMessage = new MG_GameMessage(gameMessage);

                    int mapNb = 8;
                    //find map
                    //map #1
                    if(gameMessage.map.getTileTypeAt(new Position(0, 6)) == TileType.WALL
                        && gameMessage.map.getTileTypeAt(new Position(1, 6)) == TileType.WALL
                        && gameMessage.map.getTileTypeAt(new Position(2, 6)) == TileType.WALL
                        && gameMessage.map.getTileTypeAt(new Position(3, 6)) == TileType.WALL
                        && gameMessage.map.getTileTypeAt(new Position(0, 7)) == TileType.WALL
                        && gameMessage.map.getTileTypeAt(new Position(3,7)) == TileType.WALL
                        && gameMessage.map.getTileTypeAt(new Position(0, 8)) == TileType.WALL
                        && gameMessage.map.getTileTypeAt(new Position(1, 8)) == TileType.WALL
                        && gameMessage.map.getTileTypeAt(new Position(2, 8)) == TileType.WALL
                        && gameMessage.map.getTileTypeAt(new Position(3, 8)) == TileType.WALL
                        && gameMessage.map.getTileTypeAt(new Position(1,7)) == TileType.SPAWN
                        && gameMessage.map.getTileTypeAt(new Position(2,7)) == TileType.SPAWN)
                    {
                        mapNb = 1;

                        mG_GameMessage.considerWall = new Position[3];
                        mG_GameMessage.considerWall[0] = new Position(8,0);
                        mG_GameMessage.considerWall[1] = new Position(1,7);
                        mG_GameMessage.considerWall[2] = new Position(2,7);

                        mG_GameMessage.considerWall[3] = new Position(15,13);
                        mG_GameMessage.considerWall[4] = new Position(16,13);
                        mG_GameMessage.considerWall[5] = new Position(17,13);
                        mG_GameMessage.considerWall[6] = new Position(18,13);
                        mG_GameMessage.considerWall[7] = new Position(19,13);

                        /*Diamond d = mG_GameMessage.map.getDiamondByPos(new Position(8,0));
                        if (d != null)
                            d.isAcessible = false;*/
                    }


                    //map #2
                    int count = 4;
                    foreach(Diamond d in gameMessage.map.diamonds)
                    {
                        if( (d.position.x == 2 && d.position.y == 5) 
                            || (d.position.x == 2 && d.position.y == 9)
                            || (d.position.x == 2 && d.position.y == 12)
                            || (d.position.x == 11 && d.position.y == 9)){
                            count--;
                        }
                    }
                    if (count == 0)
                    {
                        mapNb = 2;
                        mG_GameMessage.considerWall = new Position[1];
                        mG_GameMessage.considerWall[0] = new Position(11, 9);

                        /*Diamond d = mG_GameMessage.map.getDiamondByPos(new Position(11,9));
                        if (d != null)
                            d.isAcessible = false;*/
                    }

                    //map #3
                    count = 2;
                    foreach (Diamond d in gameMessage.map.diamonds)
                    {
                        if ((d.position.x == 2 && d.position.y == 2)
                            || (d.position.x == 9 && d.position.y == 8)){
                            count--;
                        }
                    }
                    if (count == 0 && gameMessage.map.diamonds.Length == 2)
                    {
                        mapNb = 3;
                        mG_GameMessage.considerWall = new Position[1];
                        mG_GameMessage.considerWall[0] = new Position(2,2);

                        /*Diamond d = mG_GameMessage.map.getDiamondByPos(new Position(2,2));
                        if (d != null)
                            d.isAcessible = false;*/

                    }

                    //map #4


                    //map #5


                    //map#6


                    //map #7
                }
                else
                {
                    mG_GameMessage.UpdateValues(gameMessage);
                }


                Team myTeam = gameMessage.getTeamsMapById[gameMessage.teamId];

                var unitsByLifeStatus = myTeam.units.GroupBy(unit => unit.hasSpawned).ToDictionary(group => group.Key, group => group.ToList());
                List<Unit> deadUnits = unitsByLifeStatus.ContainsKey(false) ? unitsByLifeStatus[false] : new List<Unit>();
                List<Unit> aliveUnits = unitsByLifeStatus.ContainsKey(true) ? unitsByLifeStatus[true] : new List<Unit>();

                List<Action> actions = new List<Action>();
                actions.AddRange(deadUnits.Select(unit => new Action(UnitActionType.SPAWN, unit.id, findRandomSpawn(gameMessage, unit))).ToList<Action>());


                foreach (Unit u in aliveUnits)
                {
                    if (gameMessage.tick == gameMessage.totalTick - 2)
                    {
                        if (u.hasDiamond)
                        {
                            Position dropPos = DropD(u, gameMessage);
                            if(dropPos != null)
                                actions.Add(new Action(UnitActionType.DROP, u.id, dropPos));
                        }
                    }
                    else
                    {
                        if (u.hasDiamond)
                        {
                            //seuil todo: a changer
                            Position dropPos = DropD(u, gameMessage);
                            if (gameMessage.map.getDiamondById(u.diamondId).points > 30 && dropPos != null)
                            {
                                if (closestEnnemyDistance(gameMessage, u.position) < 4 )
                                    actions.Add(new Action(UnitActionType.DROP, u.id, dropPos));
                                else if (isVinable(gameMessage, u.position, 'x') || isVinable(gameMessage, u.position, 'y'))
                                {
                                    actions.Add(new Action(UnitActionType.MOVE, u.id, safestMove(u.position, gameMessage)));
                                }
                            }
                            else
                            {
                                int closestDistance = closestEnnemyDistance(gameMessage, u.position);
                                if (gameMessage.map.getDiamondById(u.diamondId).summonLevel < 5 && closestDistance - 1 > gameMessage.map.getDiamondById(u.diamondId).summonLevel)
                                {
                                    actions.Add(new Action(UnitActionType.SUMMON, u.id));
                                }
                                else
                                {
                                    List<Unit> ennemies = closestEnnemies(gameMessage, u.position);
                                    List<Unit> threats = new List<Unit>();
                                    foreach (Unit ennemy in ennemies)
                                    {
                                        if (!ennemy.hasDiamond)
                                            threats.Add(ennemy);
                                    }
                                    if (threats.Count > 0)
                                    {
                                        Position threat = threats[0].position;
                                        int xDist = Math.Abs(threat.x - u.position.x);
                                        int yDist = Math.Abs(threat.y - u.position.y);
                                        int forward = 1;
                                        if (xDist <= yDist)
                                        {
                                            if(xDist != 0)
                                                forward = -((threat.x - u.position.x) / xDist);
                                            if(gameMessage.map.getTileTypeAt(new Position(u.position.x + forward, u.position.y)) != TileType.SPAWN && gameMessage.map.getTileTypeAt(new Position(u.position.x + forward, u.position.y)) != TileType.WALL)
                                                actions.Add(new Action(UnitActionType.MOVE, u.id, new Position(u.position.x + forward, u.position.y)));
                                            else if (gameMessage.map.getTileTypeAt(new Position(u.position.x - forward, u.position.y)) != TileType.SPAWN && gameMessage.map.getTileTypeAt(new Position(u.position.x - forward, u.position.y)) != TileType.WALL && xDist == 0)
                                                actions.Add(new Action(UnitActionType.MOVE, u.id, new Position(u.position.x - forward, u.position.y)));
                                            else
                                            {
                                                forward = -((threat.y - u.position.y) / yDist);
                                                if (gameMessage.map.getTileTypeAt(new Position(u.position.x, u.position.y + forward)) != TileType.SPAWN && gameMessage.map.getTileTypeAt(new Position(u.position.x, u.position.y + forward)) != TileType.WALL)
                                                    actions.Add(new Action(UnitActionType.MOVE, u.id, new Position(u.position.x, u.position.y + forward)));
                                                else
                                                    actions.Add(new Action(UnitActionType.DROP, u.id, DropD(u, gameMessage)));
                                            }
                                                

                                        }
                                        else
                                        {
                                            if(yDist != 0)
                                                forward = -((threat.y - u.position.y) / yDist);
                                            if (gameMessage.map.getTileTypeAt(new Position(u.position.x, u.position.y + forward)) != TileType.SPAWN && gameMessage.map.getTileTypeAt(new Position(u.position.x, u.position.y + forward)) != TileType.WALL)
                                                actions.Add(new Action(UnitActionType.MOVE, u.id, new Position(u.position.x, u.position.y + forward)));
                                            else if (gameMessage.map.getTileTypeAt(new Position(u.position.x, u.position.y - forward)) != TileType.SPAWN && gameMessage.map.getTileTypeAt(new Position(u.position.x, u.position.y - forward)) != TileType.WALL && yDist == 0)
                                                    actions.Add(new Action(UnitActionType.MOVE, u.id, new Position(u.position.x, u.position.y - forward)));
                                            else
                                            {
                                                if (xDist != 0)
                                                    forward = -((threat.x - u.position.x) / xDist);
                                                if (gameMessage.map.getTileTypeAt(new Position(u.position.x + forward, u.position.y)) != TileType.SPAWN && gameMessage.map.getTileTypeAt(new Position(u.position.x + forward, u.position.y)) != TileType.WALL)
                                                    actions.Add(new Action(UnitActionType.MOVE, u.id, new Position(u.position.x + forward, u.position.y)));
                                                else
                                                    DropD(u, gameMessage);
                                            }
                                        }
                                    }

                                }
                            }
                        }
                        else
                        {
                            //kill if ennemy aside..
                            if (closestEnnemyDistance(gameMessage, u.position) == 1 && gameMessage.map.getTileTypeAt(u.position) != TileType.SPAWN)
                            {
                                List<Unit> attackableUnits = closestEnnemies(gameMessage, u.position);
                                List<Unit> hasDiamond = new List<Unit>();
                                List<Unit> actuallyAttackableUnits = new List<Unit>();
                                foreach(Unit au in attackableUnits)
                                {
                                    if (doWePlayBeforeThem(au.teamId, gameMessage))
                                    {
                                        actuallyAttackableUnits.Add(au);
                                        //actions.Add(new Action(UnitActionType.ATTACK, u.id, au.position));
                                    }                                      
                                }
                                if(actuallyAttackableUnits.Count() == 1 && gameMessage.map.getTileTypeAt(actuallyAttackableUnits.First().position) != TileType.SPAWN)
                                    actions.Add(new Action(UnitActionType.ATTACK, u.id, actuallyAttackableUnits.First().position));
                                else if(actuallyAttackableUnits.Count == 0)
                                {
                                    if(attackableUnits.Count() == 1 && gameMessage.map.getTileTypeAt(attackableUnits.First().position) != TileType.SPAWN)
                                        actions.Add(new Action(UnitActionType.ATTACK, u.id, attackableUnits.First().position));
                                    else
                                    {
                                        foreach(Unit au in attackableUnits)
                                        {
                                            if (au.hasDiamond)
                                                hasDiamond.Add(au);
                                        }
                                        //If many target have diamonds, we will attack the one whose has most points
                                        if(hasDiamond.Count() > 0 && gameMessage.map.getTileTypeAt(pickBestScore(hasDiamond, gameMessage).position) != TileType.SPAWN)
                                        {
                                            actions.Add(new Action(UnitActionType.ATTACK, u.id, pickBestScore(hasDiamond, gameMessage).position));
                                        }
                                    }
                                }
                                //If there is more than one possible target we are sure to kill
                                else
                                {
                                    foreach (Unit aau in actuallyAttackableUnits)
                                    {
                                        if (aau.hasDiamond)
                                            hasDiamond.Add(aau);
                                    }
                                    //If many target have diamonds, we will attack the one whose has most points
                                    if (hasDiamond.Count() > 0 && gameMessage.map.getTileTypeAt(pickBestScore(hasDiamond, gameMessage).position) != TileType.SPAWN)
                                    {
                                        actions.Add(new Action(UnitActionType.ATTACK, u.id, pickBestScore(hasDiamond, gameMessage).position));
                                    }
                                    //If none has a diamond, we still kill one of them.. for.. fun? 
                                    else if(gameMessage.map.getTileTypeAt(pickBestScore(actuallyAttackableUnits, gameMessage).position) != TileType.SPAWN)
                                    {
                                        actions.Add(new Action(UnitActionType.ATTACK, u.id, pickBestScore(actuallyAttackableUnits, gameMessage).position));
                                    }
                                }
                            }
                            /*
                             
                             PROBLEM TO FIX HERE IF MANY ACTIONS POSSIBLE WILL TRY TO DO THEM ALL
                             
                             */
                            else
                            {
                                //vine an ennemy with diamond if possible
                                Unit toVine = canVineSomeone(u, gameMessage);
                                if (toVine != null && gameMessage.map.getTileTypeAt(u.position) != TileType.SPAWN)
                                {
                                    if (toVine.hasDiamond || doWePlayBeforeThem(toVine.teamId, gameMessage, 1))
                                        actions.Add(new Action(UnitActionType.VINE, u.id, toVine.position));
                                }

                                else
                                {
                                    //Spot and move nearest diamond
                                    Diamond best = findBestNotTargetedDiamond(u.position);
                                    Position target = nearestWalkablePos(best.position);

                                    mG_GameMessage.map.getDiamondById(best.id).allyTargetingId = u.id;
                                    if (target.x == 1000)
                                        target = getRandomPosition(gameMessage.map.horizontalSize(), gameMessage.map.verticalSize());

                                    actions.Add(new Action(UnitActionType.MOVE, u.id, target));
                                }

                            }
                           
                        }
                    }
                }

                return new GameCommand(actions);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new GameCommand(new List<Action>());
            }
        }

        // Receives a list of units and return the one whose team has the best score (if more than one has the same score, will return the first on these)
        private Unit pickBestScore(List<Unit> units, GameMessage gm)
        {
            int teamScore = -1;
            Unit winner = null;

            foreach (Unit u in units)
            {
                if (u.hasSpawned)
                {
                    if (gm.getTeamsMapById[u.teamId].score > teamScore)
                    {
                        teamScore = gm.getTeamsMapById[u.teamId].score;
                        winner = u;
                    }
                }
            }
            return winner;
        }



        private Position DropD(Unit u, GameMessage gameMessage)
        {
            Position p1 = new Position(u.position.x, u.position.y + 1);
            Position p2 = new Position(u.position.x, u.position.y - 1);
            Position p3 = new Position(u.position.x + 1, u.position.y);
            Position p4 = new Position(u.position.x - 1, u.position.y);
            if (gameMessage.map.doesTileExists(p1))
            {
                if (gameMessage.map.getTileTypeAt(p1) == TileType.EMPTY && getUnitOnTile(gameMessage, p1) == null)
                {
                    return p1;
                }
            }
            if (gameMessage.map.doesTileExists(p2))
            {
                if (gameMessage.map.getTileTypeAt(p2) == TileType.EMPTY && getUnitOnTile(gameMessage, p2) == null)
                {
                    return p2;
                }
            }
            if (gameMessage.map.doesTileExists(p3))
            {
                if (gameMessage.map.getTileTypeAt(p3) == TileType.EMPTY && getUnitOnTile(gameMessage, p3) == null)
                {
                    return  p3;
                }
            }
            if (gameMessage.map.doesTileExists(p4))
            {
                if (gameMessage.map.getTileTypeAt(p4) == TileType.EMPTY && getUnitOnTile(gameMessage, p4) == null)
                {
                    return p4;
                }
            }

            return null;
        }

        private Position safestMove(Position from, GameMessage gameMessage)
        {
            Position p1 = new Position(from.x, from.y + 1);
            Position p2 = new Position(from.x, from.y - 1);
            Position p3 = new Position(from.x + 1, from.y);
            Position p4 = new Position(from.x - 1, from.y);
            int p1SafeDist = 0;
            int p2SafeDist = 0;
            int p3SafeDist = 0;
            int p4SafeDist = 0;
            Position bestPos = p1;
            int safestDist = 0;

            bool p1CanVine = false;
            bool p2CanVine = false;
            bool p3CanVine = false;
            bool p4CanVine = false;
            bool bestIsSafeFromVine = false;
            Position bestPosFromVine = p1;


            if (gameMessage.map.doesTileExists(p1))
            {
                if (gameMessage.map.getTileTypeAt(p1) == TileType.EMPTY && getUnitOnTile(gameMessage, p1) == null)
                {
                    p1SafeDist = Distance(from, closestEnnemies(gameMessage, p1)[0].position);

                    if (isVinable(gameMessage, p1, 'x') || isVinable(gameMessage, p1, 'y'))
                    {
                        p1CanVine = true;

                    }
                    else
                    {
                        bestIsSafeFromVine = true;
                        if (p1SafeDist > safestDist)
                        {
                            bestPosFromVine = p1;
                            safestDist = p1SafeDist;
                        }
                    }
                }
            }
            if (gameMessage.map.doesTileExists(p2))
            {
                if (gameMessage.map.getTileTypeAt(p2) == TileType.EMPTY && getUnitOnTile(gameMessage, p2) == null)
                {
                    p2SafeDist = Distance(from, closestEnnemies(gameMessage, p1)[0].position);

                    if (isVinable(gameMessage, p2, 'x') || isVinable(gameMessage, p2, 'y'))
                    {
                        p2CanVine = true;
                    }
                    else
                    {
                        bestIsSafeFromVine = true;
                        if (p2SafeDist > safestDist)
                        {
                            bestPosFromVine = p2;
                            safestDist = p2SafeDist;
                        }
                    }
                    

                }
            }
            if (gameMessage.map.doesTileExists(p3))
            {
                if (gameMessage.map.getTileTypeAt(p3) == TileType.EMPTY && getUnitOnTile(gameMessage, p3) == null)
                {
                    p3SafeDist = Distance(from, closestEnnemies(gameMessage, p1)[0].position);

                    if (isVinable(gameMessage, p3, 'x') || isVinable(gameMessage, p3, 'y'))
                    {
                        p3CanVine = true;
                    }
                    else
                    {
                        bestIsSafeFromVine = true;
                        if (p3SafeDist > safestDist)
                        {
                            bestPosFromVine = p3;
                            safestDist = p3SafeDist;
                        }
                    }

                }
            }
            if (gameMessage.map.doesTileExists(p4))
            {
                if (gameMessage.map.getTileTypeAt(p4) == TileType.EMPTY && getUnitOnTile(gameMessage, p4) == null)
                {
                    p4SafeDist = Distance(from, closestEnnemies(gameMessage, p1)[0].position);

                    if (isVinable(gameMessage, p4, 'x') || isVinable(gameMessage, p4, 'y'))
                    {
                        p4CanVine = true;
                    }
                    else
                    {
                        bestIsSafeFromVine = true;
                        if (p4SafeDist > safestDist)
                        {
                            bestPosFromVine = p4;
                            safestDist = p4SafeDist;
                        }
                    }
                }
            }

            if (bestIsSafeFromVine)
            {
                return bestPosFromVine;
            }
            else
            {
                if (p1CanVine)
                    return p1;
                if (p2CanVine)
                    return p2;
                if (p3CanVine)
                    return p3;
                else
                    return p4;
            }


            
        }

        private Position nearestWalkablePos(Position target)
        {
            if (mG_GameMessage.map.getTileTypeAt(target) == TileType.EMPTY && getUnitOnTile(mG_GameMessage, target) == null)
                return target;

            Position p1 = new Position(target.x, target.y + 1);
            Position p2 = new Position(target.x, target.y - 1);
            Position p3 = new Position(target.x + 1, target.y);
            Position p4 = new Position(target.x - 1, target.y);
            Position backup = p1;
            if (mG_GameMessage.map.doesTileExists(p1))
            {
                backup = p1;
                if (mG_GameMessage.map.getTileTypeAt(p1) == TileType.EMPTY && getUnitOnTile(mG_GameMessage, p1) == null)
                    return p1;
            }
            if (mG_GameMessage.map.doesTileExists(p2))
            {
                backup = p2;
                if (mG_GameMessage.map.getTileTypeAt(p2) == TileType.EMPTY && getUnitOnTile(mG_GameMessage, p2) == null)
                    return p2;
            }
            if (mG_GameMessage.map.doesTileExists(p3))
            {
                backup = p3;
                if (mG_GameMessage.map.getTileTypeAt(p3) == TileType.EMPTY && getUnitOnTile(mG_GameMessage, p3) == null)
                    return p3;
            }
            if (mG_GameMessage.map.doesTileExists(p4))
            {
                backup = p4;
                if (mG_GameMessage.map.getTileTypeAt(p4) == TileType.EMPTY && getUnitOnTile(mG_GameMessage, p4) == null)
                    return p4;
            }

            return backup;

        }

        private Unit getUnitOnTile(GameMessage gameMessage, Position tile)
        {
            foreach (Team t in gameMessage.teams)
            {
                foreach (Unit u in t.units)
                {
                    if (u.hasSpawned)
                    {
                        if (u.position.x == tile.x && u.position.y == tile.y)
                        {
                            return u;
                        }
                    }
                }
            }

            return null;
        }


        private MG_Unit getUnitOnTile(MG_GameMessage gameMessage, Position tile)
        {
            foreach (MG_Team t in gameMessage.teams)
            {
                foreach (MG_Unit u in t.units)
                {
                    if (u.hasSpawned)
                    {
                        if (u.position.x == tile.x && u.position.y == tile.y)
                        {
                            return u;
                        }
                    }
                }
            }

            return null;
        }
        private Unit canVineSomeone(Unit u, GameMessage gameMessage)
        {

            foreach (Team team in gameMessage.teams)
            {
                if (team.id != gameMessage.teamId)
                {
                    foreach (Unit ennemy in team.units)
                    {
                        if (ennemy.hasSpawned)
                        {
                            if (ennemy.position.x == u.position.x || ennemy.position.y == u.position.y)
                            {
                                if (ennemy.hasDiamond)
                                {
                                    //check if vines can go
                                    TileType[] obstacles = getTilesBetweenPos(gameMessage.map, u.position, ennemy.position);
                                    Boolean canVine = true;
                                    foreach (TileType tile in obstacles)
                                    {
                                        if (tile == TileType.SPAWN || tile == TileType.WALL) { canVine = false; break; }
                                    }
                                    if (canVine)
                                    {
                                        return ennemy;
                                    }
                                }
                            }
                        }
                        //for each ennemy
                       
                    }
                }
            }

            return null;
        }


        private Position findNearestDiamonds(Map map, Unit unit)
        {
            Position nearest = new Position(1000, 1000);
            foreach (Diamond diamonds in map.diamonds)
            {
                if (diamonds.ownerId == null && Distance(diamonds.position, unit.position) < Distance(nearest, unit.position))
                    nearest = diamonds.position;
            }
            return nearest;
        }

      
        private Diamond findBestNotTargetedDiamond(Position unit)
        {
            Diamond bestDiamond = mG_GameMessage.map.diamonds[0];
            float bestActualValue = 0;
            foreach (Diamond d in mG_GameMessage.map.diamonds)
            {
               // if (d.isAcessible)
                //{
                    if (d.ownerId == null)
                    {
                        float dValue = ValueOfDiamond(d, unit);
                        if (d.allyTargetingId == null && bestActualValue < dValue)
                        {
                            bestDiamond = d;
                            bestActualValue = dValue;
                        }
                    }
                    else
                    {
                        MG_Team t = mG_GameMessage.getTeamByUnitId(d.ownerId);
                        if (t != null && mG_GameMessage.teamId != t.id)
                        {
                            float dValue = ValueOfDiamond(d, unit);
                            if (d.allyTargetingId == null && bestActualValue < dValue)
                            {
                                bestDiamond = d;
                                bestActualValue = dValue;
                            }
                        }
                    }
                //}

            }

            return bestDiamond;
        }

        private float ValueOfDiamond(Diamond d, Position u)
        {
            //Heuristic pour la valeur ajoute d'une cible
            return (float)(d.points+1) / (float)Distance(d.position, u);
        }


        private Position findNearestDiamonds(Map map, Position position)
        {
            Position nearest = new Position(1000, 1000);
            foreach (Diamond diamonds in map.diamonds)
            {
                if (diamonds.ownerId == null && Distance(diamonds.position, position) < Distance(nearest, position))
                    nearest = diamonds.position;
            }
            return nearest;
        }

        private TileType[] getTilesBetweenPos(Map map, Position from, Position target)
        {
            TileType[] tiles = null;
            if(from.y == target.y)
            {
                int distance = Math.Abs(from.x - target.x);
                int Backward = (target.x - from.x) / distance; //give 1 or -1
                tiles = new TileType[distance - 1];
                for(int i = 1; i < tiles.Count(); i++)
                {
                    tiles[i] = map.getTileTypeAt(new Position(from.x + (i * Backward), from.y));
                }
            }
            else if(from.x == target.x)
            {
                int distance = Math.Abs(from.y - target.y);
                int Backward = (target.y - from.y) / distance; //give 1 or -1
                tiles = new TileType[distance - 1];
                for (int i = 1; i < tiles.Count(); i++)
                {
                    tiles[i] = map.getTileTypeAt(new Position(from.x, from.y + (i * Backward)));
                }
            }
            return tiles;
        }

        private int Distance(Position diamond, Position unit)
        {
            return Math.Abs((diamond.x - unit.x)) + Math.Abs((diamond.y - unit.y));
        }

        private Position findRandomSpawn(GameMessage gameMessage, Unit unit)
        {
            List<Position> spawns = new List<Position>();
            
            int x = 0;
            foreach (string[] tileX in gameMessage.map.tiles)
            {
                int y = 0;
                foreach (string tileY in tileX)
                {
                    var position = new Position(x, y);
                    //if (map.getTileTypeAt(position) == TileType.SPAWN && checkAroundForEMPTY(map, position))
                    if (gameMessage.map.getTileTypeAt(position) == TileType.SPAWN)
                    {
                        if(!mG_GameMessage.isConsideredWall(position))
                            spawns.Add(position);
                    }
                    y++;
                }
                x++;
            }

            spawns.Sort((i, n) => {
                return Distance(i, findBestNotTargetedDiamond(i).position).CompareTo(Distance(n, findBestNotTargetedDiamond(n).position));
            });
            
            //spawns = spawns.OrderBy(i => Distance(i, findNearestDiamonds(map, i)) <= 30);
   
            Position choosedSpawned = spawns.First();
            foreach (Position spawn in spawns)
            {
                choosedSpawned = spawn;
                bool validate = true;
                foreach (Team t in gameMessage.teams)
                {
                    foreach (Unit u in t.units)
                    {
                        if (u.hasSpawned)
                        {
                            if (u.position.x == choosedSpawned.x && u.position.y == choosedSpawned.y)
                            {
                                validate = false;
                                break;
                            }
                        }
                    }
                    if (!validate)
                    {
                        break;
                    }
                }
                if (validate)
                {
                    Diamond best = findBestNotTargetedDiamond(choosedSpawned);
                    best.allyTargetingId = unit.id;
                    return choosedSpawned;
                }

            }

            return choosedSpawned;
        }
        private bool checkAroundForEMPTY(Map map,Position position) 
        {
            bool retour = false;
            Position cmp = position;
            //regarde si la case à gauche est vide
            cmp.x = position.x-1;
            if (map.getTileTypeAt(cmp) == TileType.EMPTY) 
            { 
                retour = true;
            }
            //regarde si la case à droite est vide
            cmp.x = position.x + 1;
            if (map.getTileTypeAt(cmp) == TileType.EMPTY)
            {
                retour = true;
            }
            cmp.x = position.x;
            //regarde si la case en bas est vide
            cmp.y = position.y + 1;
            if (map.getTileTypeAt(cmp) == TileType.EMPTY)
            {
                retour = true;
            }
            //reagarde si la case en haut est vide
            cmp.y = position.y - 1;
            if (map.getTileTypeAt(cmp) == TileType.EMPTY)
            {
                retour = true;
            }
            return retour;
        }

        private Position getRandomPosition(int horizontalSize, int verticalSize)
        {
            Random rand = new Random();
            return new Position(rand.Next(horizontalSize), rand.Next(verticalSize));
        }

       private bool doWePlayBeforeThem (string otherTeam, GameMessage gm)
        {
            int test = gm.tick;
            Dictionary<int, string[]> dico = gm.teamPlayOrderings;
            string[] teamOrders = dico[gm.tick];
            if(Array.IndexOf(teamOrders, gm.teamId) < Array.IndexOf(teamOrders, otherTeam))
            {
                return true;
            }
                return false;
        }
        private bool doWePlayBeforeThem(string otherTeam, GameMessage gm, int inTurns)
        {
            int test = gm.tick + inTurns;
            Dictionary<int, string[]> dico = gm.teamPlayOrderings;
            string[] teamOrders = dico[gm.tick];
            if (Array.IndexOf(teamOrders, gm.teamId) < Array.IndexOf(teamOrders, otherTeam))
            {
                return true;
            }
            return false;
        }

        //Returns an int with the distance between player and closest ennemies
        private int closestEnnemyDistance(GameMessage gm, Position position)
        {
            int closest = 10000;
            foreach (Team t in gm.teams)
            {
                if (t.id != gm.teamId)
                {
                    foreach (Unit u in t.units)
                    {
                        if (u.hasSpawned)
                        {
                            int current = getAbsoluteDistance(u.position, position);
                            if (current < closest)
                                closest = current;
                        }

                    }
                }
            }
            return closest;
        }

        //Returns a table with all the closest ennemy units 
        private List<Unit> closestEnnemies(GameMessage gm, Position position)
        {
            int closest = 10000;
            List<Unit> units = new List<Unit>();
            foreach (Team t in gm.teams)
            {
                if (t.id != gm.teamId)
                {
                    foreach (Unit u in t.units)
                    {
                        if (u.hasSpawned)
                        {
                            int current = getAbsoluteDistance(u.position, position);
                            if (current == closest)
                            {
                                units.Add(u);
                            }
                            else if (current < closest)
                            {
                                closest = current;
                                units.Clear();
                                units.Add(u);
                            }
                        }

                    }
                }
            }
            return units;
        }

        private bool isVinable(GameMessage gm, Position unitPosition, char axis)
        {
            switch(axis)
            {
                case 'x':
                    foreach (Team t in gm.teams)
                    {
                        foreach (Unit u in t.units)
                        {
                            if (u.hasSpawned && !(u.position.x == unitPosition.x && u.position.y == unitPosition.y) && u.diamondId == null)
                            {
                                if (u.position.x == unitPosition.x)
                                {
                                    bool wallBetween = false;
                                    foreach (TileType tile in getTilesBetweenPos(gm.map, u.position, unitPosition))
                                    {
                                        if (tile != TileType.EMPTY)
                                        {
                                            wallBetween = true;
                                            break;
                                        }
                                    }
                                    if (!wallBetween)
                                        return true;
                                }
                            }
                        }
                    }
                    return false;
                case 'y':
                    foreach (Team t in gm.teams)
                    {
                        foreach (Unit u in t.units)
                        {
                            if (u.hasSpawned && !(u.position.x == unitPosition.x && u.position.y == unitPosition.y) && u.diamondId == null)
                            {
                                if (u.position.y == unitPosition.y)
                                {
                                    bool wallBetween = false;
                                    foreach (TileType tile in getTilesBetweenPos(gm.map, u.position, unitPosition))
                                    {
                                        if (tile != TileType.EMPTY)
                                        {
                                            wallBetween = true;
                                            break;
                                        }
                                    }
                                    if (!wallBetween)
                                        return true;
                                }
                            }
                        }
                    }
                    return false;
                default:
                    return false;
            }
        }

        private int getAbsoluteDistance(Position badGuyPos, Position goodGuyPos)
        {
            int x, y;
            x = Math.Abs(badGuyPos.x - goodGuyPos.x);
            y = Math.Abs(badGuyPos.y - goodGuyPos.y);
            return x + y;
        }
    }
}