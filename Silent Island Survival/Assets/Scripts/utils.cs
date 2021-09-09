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

        public static string GetName()
        {
            string[] names = { "James", "Mary", "Robert", "Patricia", "John", "Jennifer", "Michael", "Linda", "William", "Elizabeth", "David", "Barbara", "Richard",
                               "Susan", "Joseph", "Jessica", "Thomas", "Sarah", "Charles", "Karen", "Christopher", "Nancy", "Daniel", "Lisa", "Matthew", "Betty",
                               "Anthony", "Margaret", "Mark", "Sandra", "Donald", "Ashley", "Steven", "Kimberly", "Paul", "Emily", "Andrew", "Donna", "Joshua",
                               "Michelle", "Kenneth", "Dorothy", "Kevin", "Carol", "Brian", "Amanda", "George", "Melissa", "Edward", "Deborah", "Ronald", "Stephanie",
                               "Timothy", "Rebecca", "Jason", "Sharon", "Jeffrey", "Laura", "Ryan", "Cynthia", "Jacob", "Kathleen", "Gary", "Amy", "Nicholas", "Shirley",
                               "Eric", "Angela", "Jonathan", "Helen", "Stephen", "Anna", "Larry", "Brenda", "Justin", "Pamela", "Scott", "Nicole", "Brandon", "Emma",
                               "Benjamin", "Samantha", "Samuel", "Katherine", "Gregory", "Christine", "Frank", "Debra", "Alexander", "Rachel", "Raymond", "Catherine",
                               "Patrick", "Carolyn", "Jack", "Janet", "Dennis", "Ruth", "Jerry", "Maria", "Tyler", "Heather", "Aaron", "Diane", "Jose", "Virginia",
                               "Adam", "Julie", "Henry", "Joyce", "Nathan", "Victoria", "Douglas", "Olivia", "Zachary", "Kelly", "Peter", "Christina", "Kyle", "Lauren",
                               "Walter", "Joan", "Ethan", "Evelyn", "Jeremy", "Judith", "Harold", "Megan", "Keith", "Cheryl", "Christian", "Andrea", "Roger", "Hannah",
                               "Noah", "Martha", "Gerald", "Jacqueline", "Carl", "Frances", "Terry", "Gloria", "Sean", "Ann", "Austin", "Teresa", "Arthur", "Kathryn",
                               "Lawrence", "Sara", "Jesse", "Janice", "Dylan", "Jean", "Bryan", "Alice", "Joe", "Madison", "Jordan", "Doris", "Billy", "Abigail", "Bruce",
                               "Julia", "Albert", "Judy", "Willie", "Grace", "Gabriel", "Denise", "Logan", "Amber", "Alan", "Marilyn", "Juan", "Beverly", "Wayne", "Danielle",
                               "Roy", "Theresa", "Ralph", "Sophia", "Randy", "Marie", "Eugene", "Diana", "Vincent", "Brittany", "Russell", "Natalie", "Elijah", "Isabella",
                               "Louis", "Charlotte", "Bobby", "Rose", "Philip", "Alexis", "Johnny", "Kayla" };
            return names[Random.Range(0,names.Length)];
        }

        public static string[] GenerateNewWorldMap(int inSize)
        {
            string[] Map = new string[inSize];

            Map = makeLand(inSize);
            Map = AddRocks(Map, 10);
            Map = AddTrees(Map, 10);
            Map = AddWater(Map);

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

        private static string[] transposeMap(string[] inMap)
        {
            string[] tempMap = new string[inMap.Length];

            for(int row = 0; row < inMap.Length; row++)
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
        #endregion
        
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
                            for(int i = 0; i < row; row++)
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

                        // Don't put roads too close to water.
                        nearbyTiles = "";

                        // condition ? consequent : alternative
                        rBegin =  (0 < row) ? row - 1 : row;
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

    }


}