using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ScoreboardData
{
    public ScoreBoard data { get; set; }
    public class ScoreBoard
    {
        public List<ScoreboardEntry> scoreboard { get; set; }
        public class ScoreboardEntry
        {
            public PlayerDTO player { get; set; }
            public int kills { get; set; }
            public int deaths { get; set; }
            public float kdRatio { get; set; }
            public int points { get; set; }

            public class PlayerDTO
            {
                public string username { get; set; }
            }
        }
    }
}


