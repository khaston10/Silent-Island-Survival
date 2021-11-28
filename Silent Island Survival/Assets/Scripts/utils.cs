using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;

namespace utils
{
    
    public static class Globals
    {
        public static int test = 666;  // example global var
    }

    public static class Utils
    {

        public static string GetName(string sex)
        {
            // If sex is not specified then it will be assumed to be M as this is the default on the unit contoller script.

            string[] namesMale = {"James", "Robert", "John", "Michael", "William", "David", "Richard", "Joseph", "Thomas", "Charles", "Christopher", "Daniel", "Matthew", "Anthony", "Mark", "Donald", "Steven", 
                                    "Paul", "Andrew", "Joshua", "Kenneth", "Kevin", "Brian", "George", "Edward", "Ronald", "Timothy", "Jason", "Jeffrey", "Ryan", "Jacob", "Gary", "Nicholas", "Eric", "Jonathan", 
                                    "Stephen", "Larry", "Justin", "Scott", "Brandon", "Benjamin", "Samuel", "Gregory", "Frank", "Alexander", "Raymond", "Patrick", "Jack", "Dennis", "Jerry", "Tyler", "Aaron", "Jose", 
                                    "Adam", "Henry", "Nathan", "Douglas", "Zachary", "Peter", "Kyle", "Walter", "Ethan", "Jeremy", "Harold", "Keith", "Christian", "Roger", "Noah", "Gerald", "Carl", "Terry", "Sean", 
                                    "Austin", "Arthur", "Lawrence", "Jesse", "Dylan", "Bryan", "Joe", "Jordan", "Billy", "Bruce", "Albert", "Willie", "Gabriel", "Logan", "Alan", "Juan", "Wayne", "Roy", "Ralph", "Randy",
                                    "Eugene", "Vincent", "Russell", "Elijah", "Louis", "Bobby", "Philip", "Johnny"};

            string[] namesFemale = { "Mary", "Patricia", "Jennifer", "Linda", "Elizabeth", "Barbara", "Susan", "Jessica", "Sarah", "Karen", "Nancy", "Lisa", "Betty", "Margaret",
                                    "Sandra", "Ashley", "Kimberly", "Emily", "Donna", "Michelle", "Dorothy", "Carol", "Amanda", "Melissa", "Deborah", "Stephanie", "Rebecca", "Sharon", "Laura", "Cynthia",
                                    "Kathleen", "Amy", "Shirley", "Angela", "Helen", "Anna", "Brenda", "Pamela", "Nicole", "Emma", "Samantha", "Katherine", "Christine", "Debra", "Rachel", "Catherine", "Carolyn",
                                    "Janet", "Ruth", "Maria", "Heather", "Diane", "Virginia", "Julie", "Joyce", "Victoria", "Olivia", "Kelly", "Christina", "Lauren", "Joan", "Evelyn", "Judith", "Megan", "Cheryl",
                                    "Andrea", "Hannah", "Martha", "Jacqueline", "Frances", "Gloria", "Ann", "Teresa", "Kathryn", "Sara", "Janice", "Jean", "Alice", "Madison", "Doris", "Abigail", "Julia", "Judy",
                                    "Grace", "Denise", "Amber", "Marilyn", "Beverly", "Danielle", "Theresa", "Sophia", "Marie", "Diana", "Brittany", "Natalie", "Isabella", "Charlotte", "Rose", "Alexis", "Kayla" };



            if (sex == "F") return namesFemale[Random.Range(0, namesFemale.Length)];


            else return namesMale[Random.Range(0, namesMale.Length)];

        }

        public static string[] GenerateNewWorldMap(int inSize)
        {
            string[] Map = new string[inSize];

            Map = makeLand(inSize);
            Map = AddRocks(Map, 10);
            Map = AddTrees(Map, 10);
            Map = AddGraves(Map, 5);
            Map = AddWater(Map);
            Map = addRoads(Map);
            Map = addLootBoxes(Map);
            Map = addHouses(Map);
            Map = addFactories(Map);

            return Map;
        }

        #region Helper Functions For Map Generation

        //# ----- charset -----
        //# . water
        //# 0 land
        //# * rock
        //# ^ tree
        //# 1 abandoned house
        //# 2 abandoned factory
        //# 3 abandoned vehicle
        //# g Grave Yard
        //# & lootbox
        //# | road
        //# - road
        //# └ road
        //# ┘ road
        //# ┌ road
        //# ┐ road
        //# ┴ road
        //# ┬ road
        //# ├ road
        //# ┤ road

        private static string[] makeLand(int inSize)
        {
            string[] tempMap = new string[inSize];
            string mapline = "";

            // Add "0" * inSize to the line.
            for (int i = 0; i < inSize; i++)
            {
                mapline += "0";
            }

            // Add the string mapline to the map inSize times.
            for (int i = 0; i < inSize; i++)
            {
                tempMap[i] = mapline;
            }

            return tempMap;
        }

        private static string[] AddRocks(string[] inMap, int chanceOfRock)
        {
            string[] tempMap = new string[inMap.Length];

            for (int row = 0; row < inMap.Length; row++)
            {
                string tempLine = "";
                for (int tile = 0; tile < inMap[row].Length; tile++)
                {
                    // Random chance that we will add a rock here.
                    if (Random.Range(0, 100) < chanceOfRock)
                    {
                        tempLine += "*";
                    }
                    else
                    {
                        tempLine += inMap[row][tile];
                    }
                }
                tempMap[row] = tempLine;
            }

            return tempMap;
        }

        private static string[] AddTrees(string[] inMap, int chanceOfTree)
        {
            string[] tempMap = new string[inMap.Length];

            for (int row = 0; row < inMap.Length; row++)
            {
                string tempLine = "";
                for (int tile = 0; tile < inMap[row].Length; tile++)
                {
                    // Random chance that we will add a tree here.
                    if (Random.Range(0, 100) < chanceOfTree)
                    {
                        tempLine += "^";
                    }
                    else
                    {
                        tempLine += inMap[row][tile];
                    }
                }
                tempMap[row] = tempLine;
            }

            return tempMap;
        }

        private static string[] AddGraves(string[] inMap, int chanceOfGrave)
        {
            string[] tempMap = new string[inMap.Length];

            for (int row = 0; row < inMap.Length; row++)
            {
                string tempLine = "";
                for (int tile = 0; tile < inMap[row].Length; tile++)
                {
                    // Random chance that we will add a grave here.
                    if (Random.Range(0, 100) < chanceOfGrave)
                    {
                        tempLine += "g";
                    }
                    else
                    {
                        tempLine += inMap[row][tile];
                    }
                }
                tempMap[row] = tempLine;
            }

            return tempMap;
        }

        private static string[] transposeMap(string[] inMap)
        {
            string[] tempMap = new string[inMap.Length];

            for (int row = 0; row < inMap.Length; row++)
            {
                for (int col = 0; col < inMap[row].Length; col++)
                {
                    tempMap[row] += inMap[col][row];
                }
            }

            return tempMap;
        }

        private static string[] AddWaterLeftRight(string[] inMap, int inWaterBorder)
        {
            string[] tempMap = new string[inMap.Length];

            int leftTideLine = Random.Range(0, 2 * inWaterBorder);
            int rightTideLine = Random.Range(0, 2 * inWaterBorder);
            int delta = 4;
            int max = inWaterBorder * 8;

            for (int row = 0; row < inMap.Length; row++)
            {
                if (leftTideLine >= inWaterBorder)
                {
                    leftTideLine += Random.Range(-delta, delta);
                }
                else
                {
                    leftTideLine = inWaterBorder;
                }

                if (leftTideLine > max)
                {
                    leftTideLine = max;
                }

                if (rightTideLine >= inWaterBorder)
                {
                    rightTideLine += Random.Range(-delta, delta);
                }
                else
                {
                    rightTideLine = inWaterBorder;
                }

                if (rightTideLine > max)
                {
                    rightTideLine = max;
                }

                for (int col = 0; col < inMap[row].Length; col++)
                {
                    if (col < (inWaterBorder + leftTideLine) || col > (inMap[row].Length - (inWaterBorder + 1) - rightTideLine))
                    {
                        tempMap[row] += ".";
                    }
                    else
                    {
                        tempMap[row] += inMap[row][col];
                    }
                }


            }

            return tempMap;
        }

        private static string[] AddWater(string[] inMap)
        {
            string[] tempMap = new string[inMap.Length];

            int waterBoarder = 4;
            tempMap = AddWaterLeftRight(inMap, waterBoarder);
            tempMap = transposeMap(tempMap);
            tempMap = AddWaterLeftRight(tempMap, waterBoarder);
            tempMap = transposeMap(tempMap);

            return tempMap;
        }
        
        private static string[] AddIntersections(string[] inMap)
        {
            string[] tempMap = new string[inMap.Length];
            string intersections = "┴┬├┤";
            string nearbyTiles = "";
            bool intersectionNearby = false;
            bool waterNearby = false;
            int rBegin;
            int rEnd;
            int cBegin;
            int cEnd;


            for (int row = 0; row < inMap.Length; row++)
            {
                for (int col = 0; col < inMap.Length; col++)
                {
                    if (Random.Range(0, 100) <= 2) // Change this comparison to generate more or less
                    {
                        // Do not put an intersection right next to eachother.
                        nearbyTiles = "";

                        if (0 < row)
                        {
                            for (int i = 0; i < row; i++)
                            {
                                nearbyTiles += tempMap[i][col];
                            }
                        }

                        if (0 < col)
                        {
                            nearbyTiles += tempMap[row];
                        }


                        intersectionNearby = false;

                        for (int c = 0; c < nearbyTiles.Length; c++)
                        {
                            if (nearbyTiles[c] == intersections[0] || nearbyTiles[c] == intersections[1])
                            {
                                intersectionNearby = true;
                                break;
                            }
                        }

                        // Don"t put roads too close to water.
                        nearbyTiles = "";


                        // condition ? consequent : alternative
                        rBegin = (0 < row) ? row - 1 : row;
                        rEnd = (row < inMap[0].Length - 1) ? row + 1 : row;
                        cBegin = (0 < col) ? col - 1 : col;
                        cEnd = (col < inMap[0].Length - 1) ? col + 1 : col;

                        if (!intersectionNearby)
                        {
                            for (int r = rBegin; r < rEnd; r++)
                            {
                                for (int c = cBegin; c < cEnd; c++)
                                {
                                    nearbyTiles += inMap[r][c];
                                }
                            }
                        }

                        waterNearby = false;

                        for (int t = 0; t < nearbyTiles.Length; t++)
                        {
                            if (nearbyTiles[t].ToString() == ".")
                            {
                                waterNearby = true;
                                break;
                            }
                        }


                        if (intersectionNearby || waterNearby)
                        {
                            tempMap[row] += inMap[row][col];
                        }

                        else
                        {
                            tempMap[row] += intersections[Random.Range(0, 2)];
                        }
                    }

                    else
                    {
                        tempMap[row] += inMap[row][col];
                    }
                }
            }

            return tempMap;
        }

        private static string[] addRoads(string[] inMap)
        {
            string[] tempMap = new string[inMap.Length];
            tempMap = AddIntersections(inMap);
            tempMap = growFromIntersections(tempMap);
            return tempMap;
        }

        private static string[] growFromIntersections(string[] inMap)
        {
            string[] tempMap = new string[inMap.Length];

            //growToN = '|┌┐'
            //growToE = '-┘┐'
            //growToS = '|└┘'
            //growToW = '-└┌'
            tempMap = growE(inMap);
            tempMap = growS(tempMap);

            return tempMap;
        }

        private static string[] growE(string[] inMap)
        {
            string[] tempMap = new string[inMap.Length];
            string growToE = "-┘┐";
            string iAR = "┬┴├-┌└"; // Intersections and roads

            for (int row = 0; row < inMap.Length; row++)
            {
                for (int col = 0; col < inMap.Length; col++)
                {
                    // Check to see if the current position on the map is a location that can gorw.
                    if (0 < col && (tempMap[row][col - 1] == iAR[0] || tempMap[row][col - 1] == iAR[1] || tempMap[row][col - 1] == iAR[2] ||
                        tempMap[row][col - 1] == iAR[3] || tempMap[row][col - 1] == iAR[4] || tempMap[row][col - 1] == iAR[5])
                        && inMap[row][col] != '.')
                    {
                        if (Random.Range(0, 100) < 98) // Can change this value to increase/ decrease roads
                        {
                            if (0 < row && (tempMap[row - 1][col] == '-' || tempMap[row - 1][col] == '┬' || tempMap[row - 1][col] == '┤'
                                || tempMap[row - 1][col] == '├' || tempMap[row - 1][col] == '|' || tempMap[row - 1][col] == '┌'
                                || tempMap[row - 1][col] == '┐')) // if we can connect N, do it.
                            {
                                tempMap[row] += '┘';
                            }

                            else
                            {
                                if (Random.Range(0, 100) < 90) // generate more straight roads than not
                                {
                                    tempMap[row] += '-';
                                }

                                else
                                {
                                    tempMap[row] += growToE[Random.Range(0, growToE.Length)];
                                }
                            }
                        }

                        else
                        {
                            tempMap[row] += 'x';  // end this road with Kevin's impasse object
                        }
                    }
                    else
                    {
                        tempMap[row] += inMap[row][col];
                    }
                }
            }

            return tempMap;

        }

        private static string[] growS(string[] inMap)
        {
            string[] tempMap = new string[inMap.Length];
            string growToS = "|└┘";

            for (int row = 0; row < inMap.Length; row++)
            {
                for (int col = 0; col < inMap.Length; col++)
                {
                    if (5 < col && 0 < row)
                    {
                        if ((tempMap[row - 1][col] == '|' || tempMap[row - 1][col] == '┬' || tempMap[row - 1][col] == '┤' || tempMap[row - 1][col] == '├' || tempMap[row - 1][col] == '┐' || tempMap[row - 1][col] == '┌')
                        && tempMap[row][col - 5] != '|' && tempMap[row][col - 4] != '|' && tempMap[row][col - 3] != '|' && tempMap[row][col - 2] != '|' && tempMap[row][col - 1] != '|' && inMap[row][col] != '.')
                        {

                            if (Random.Range(0, 100) <= 98)
                            {

                                // connect to something from W if possible
                                if (0 < col && col < (inMap.Length - 2) && (tempMap[row][col - 1] == '-' || tempMap[row][col - 1] == '┌' ||
                                    tempMap[row][col - 1] == '└' || tempMap[row][col - 1] == '┬' || tempMap[row][col - 1] == '┴' ||
                                    tempMap[row][col - 1] == '├' || tempMap[row][col - 1] == '|') && inMap[row][col + 1] != '-'
                                    && inMap[row][col + 1] != '┘' && inMap[row][col + 1] != '┐' && inMap[row][col + 1] != '┬'
                                    && inMap[row][col + 1] != '┴' && inMap[row][col + 1] != '┤' && inMap[row][col + 1] != '|')
                                {
                                    tempMap[row] += '┘';
                                }
                                // connect to something from W if possible
                                else if (0 < col && col < (inMap.Length - 2) && (tempMap[row][col - 1] == '-' || tempMap[row][col - 1] == '┌' ||
                                    tempMap[row][col - 1] == '└' || tempMap[row][col - 1] == '┬' || tempMap[row][col - 1] == '┴' ||
                                    tempMap[row][col - 1] == '├' || tempMap[row][col - 1] == '|') && (inMap[row][col + 1] == '-'
                                    || inMap[row][col + 1] == '┘' || inMap[row][col + 1] == '┐' || inMap[row][col + 1] == '┬'
                                    || inMap[row][col + 1] == '┴' || inMap[row][col + 1] == '┤' || inMap[row][col + 1] != '|'))
                                {
                                    tempMap[row] += '┴';
                                }

                                // turn road toward E if something is there
                                else if (col < inMap[0].Length - 1 && (inMap[row][col + 1] == '-' || inMap[row][col + 1] == '┘' ||
                                    inMap[row][col + 1] == '┐' || inMap[row][col + 1] == '┬' || inMap[row][col + 1] == '┴' ||
                                    inMap[row][col + 1] == '┤'))
                                {
                                    tempMap[row] += '└';
                                }

                                else
                                {
                                    if (Random.Range(0, 100) <= 90) // generate more straight roads than not
                                    {
                                        tempMap[row] += '|';
                                    }

                                    else
                                    {
                                        tempMap[row] += growToS[Random.Range(0, growToS.Length - 1)];
                                    }
                                }
                            }

                            else
                            {
                                tempMap[row] += 'x';  // end this road with Kevin's impasse object
                            }


                        }

                        else
                        {
                            tempMap[row] += inMap[row][col];
                        }
                    }

                    else
                    {
                        tempMap[row] += inMap[row][col];
                    }

                }
            }

            return tempMap;
        }

        private static string[] addLootBoxes(string[] inMap)
        {
            string[] tempMap = new string[inMap.Length];

            for (int row = 0; row < inMap.Length; row++)
            {
                for (int tile = 0; tile < inMap.Length; tile++)
                {
                    // Don't disrupt roads with lootboxes
                    if (inMap[row][tile] == '0' && Random.Range(0, 100) <= 1) //change this comparison to generate more or less
                    {
                        tempMap[row] += '&';
                    }

                    else tempMap[row] += inMap[row][tile];

                }
            }

            return tempMap;
        }

        private static string[] addHouses(string[] inMap)
        {
            string[] tempMap = new string[inMap.Length];
            string site = "-------"; // Wherever this occurs on the map, we'll add bldgs.
            int indOfRoad = 0;
            string bldgs = "130";

            for (int row = 0; row < inMap.Length; row++)
            {
                if (0 < row && row < inMap.Length - 1 && inMap[row + 1].Contains(site))
                {
                    indOfRoad = inMap[row + 1].IndexOf(site);
                    tempMap[row] += inMap[row].Substring(0, indOfRoad);
       
                    //tempMap[row] += inMap[row];

                    // add houses, vehicles, and empty spaces
                    for (int i = 0; i < site.Length; i++)
                    {
                        // Don't build over roads
                        if (inMap[row + 1][indOfRoad + 1] != '┴' && inMap[row][indOfRoad + 1] != '┬'
                            && inMap[row][indOfRoad + 1] != '├' && inMap[row][indOfRoad + 1] != '┤'
                            && inMap[row][indOfRoad + 1] != '-' && inMap[row][indOfRoad + 1] != '|'
                            && inMap[row][indOfRoad + 1] != '┌' && inMap[row][indOfRoad + 1] != '└'
                            && inMap[row][indOfRoad + 1] != '┘' && inMap[row][indOfRoad + 1] != '┐')
                        {
                            tempMap[row] += bldgs[Random.Range(0, bldgs.Length -1) ];
                        }

                        else
                        {
                            tempMap[row] += inMap[row][indOfRoad + i];
                        }
                    }

                    tempMap[row] += inMap[row].Substring(indOfRoad + site.Length, inMap[row].Length - (indOfRoad + site.Length));
                        
                }

                else
                {
                    tempMap[row] += inMap[row];
                }
            }

            return tempMap;
        }

        private static string[] addFactories(string[] inMap)
        {
            string[] tempMap = new string[inMap.Length];
            int builtFactory = 0; // goes true when we place one. needed to get indexing right.

            for (int row = 0; row < inMap.Length; row++)
            {
                if (builtFactory == 2 || builtFactory == 1)
                {
                    builtFactory -= 1;
                    continue;
                }

                for (int col = 0; col < inMap.Length; col++)
                {
                    if (row < inMap.Length - 6 && col > 6 && inMap[row][col] == '|' && 
                        inMap[row + 1][col] == '|' && inMap[row + 2][col] == '|' && Random.Range(0, 100) <= 10)
                    {
                        tempMap[row] = inMap[row].Substring(0, col - 3) + "444" + inMap[row].Substring(col, inMap[row].Length - col);
                        tempMap[row + 1] += inMap[row + 1].Substring(0, col - 3) + "424" + inMap[row + 1].Substring(col, inMap[row + 1].Length - col);
                        tempMap[row + 2] += inMap[row + 2].Substring(0, col - 3) + "444" + inMap[row + 2].Substring(col, inMap[row + 2].Length - col);
                        builtFactory = 2;
                    }

                    else
                    {
                        tempMap[row] += inMap[row][col];
                    }

                    if (builtFactory == 2)
                    {
                        break; // done with this row.
                    }
                }
            }

            return tempMap;
        }

        #endregion
    }
}