using System;
using System.Linq;
using fCraft;

namespace GuildPlugin {

    public static class Cache {

        /// <summary>
        /// Array of all available guilds
        /// </summary>
        public static Guild[] Guilds = new Guild[] { };

        /// <summary>
        /// Array of all available turrets
        /// </summary>
        public static Turret[] Turrets = new Turret[] { };

        /// <summary>
        /// Cache containing player's kill scores
        /// </summary>
        public static String[] PlayerKillScores = new String[] { };

        //Function to format and return a player's score entry. Returns null if no score found
        public static String GetPlayerScoreEntry( PlayerInfo player ) {
            string toReturn = null;
            foreach ( String s in PlayerKillScores ) {
                if ( s.Contains( '$' ) ) {
                    if ( s.Split( '$' )[0] == player.Name ) {
                        toReturn = s;
                    }
                }
            }
            return toReturn;
        }

        /// <summary>
        /// returns the kill score of an entire guild
        /// </summary>
        /// <param name="g">Guild in question</param>
        /// <returns>number of kills</returns>
        public static int GetGuildKillsTotal( Guild g ) {
            int Score = 0;
            foreach ( String s in PlayerKillScores ) {
                string p = s.Split( '$' )[0];
                if ( g.Members.Contains( p ) ) {
                    PlayerInfo temp = PlayerDB.FindPlayerInfoExact( p );
                    if ( temp != null ) {
                        string t = GetPlayerScoreEntry( temp );
                        if ( t != null ) {
                            Score += int.Parse( t.Split( '$' )[1] );
                        }
                    }
                }
            }
            return Score;
        }

        public static String[] ForbiddenNames = new String[]{
            "au70galaxy",
            "au70 galaxy"
        };

        public static String[] ForbiddenCommands = new String[]{
            "title",
            "gun"
        };
    }
}