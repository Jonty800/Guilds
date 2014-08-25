using System;
using System.Collections.Generic;
using System.IO;
using fCraft;

namespace GuildPlugin {

    public static class GuildConfig {
        private static string Dir = "plugins/guilds";
        public static int NumberOfGuilds = Directory.GetFiles( Dir ).Length;

        public static void LoadAllGuilds() {
            foreach ( string f in Directory.GetFiles( Dir ) ) {
                Guild g = new Guild();
                try {
                    String[] lines = File.ReadAllLines( f );
                    foreach ( String set in lines ) {
                        String[] setting = set.Split( ':' );
                        if ( setting[0].Equals( "name" ) ) {
                            g.Name = setting[1];
                        }
                        if ( setting[0].Equals( "tag" ) ) {
                            g.Tag = setting[1];
                        }
                        if ( setting[0].Equals( "members" ) ) {
                            string[] names = setting[1].Split( ',' );
                            List<String> l = new List<String>();
                            foreach ( string n in names ) {
                                string nn = n;
                                nn = n.Replace( ",", "" ).Trim();
                                if ( !string.IsNullOrWhiteSpace( nn ) || !string.IsNullOrEmpty( nn ) ) {
                                    l.Add( nn );
                                }
                            }
                            g.Members = l.ToArray();
                        }
                        if ( setting[0].Equals( "admins" ) ) {
                            string[] names = setting[1].Split( ',' );
                            List<String> l = new List<String>();
                            foreach ( string n in names ) {
                                string nn = n;
                                nn = n.Replace( ",", "" ).Trim();
                                if (!string.IsNullOrWhiteSpace(nn) || !string.IsNullOrEmpty( nn ) ) {
                                    l.Add( nn );
                                }
                            }
                            g.Admins = l.ToArray();
                        }
                        if ( setting[0].Equals( "invitedmembers" ) ) {
                            string[] names = setting[1].Split( ',' );
                            List<String> l = new List<String>();
                            foreach ( string n in names ) {
                                string nn = n;
                                nn = n.Replace( ",", "" ).Trim();
                                if ( !string.IsNullOrWhiteSpace( nn ) || !string.IsNullOrEmpty( nn ) ) {
                                    l.Add( nn );
                                }
                            }
                            g.PlayersInvited = l.ToArray();
                        }
                        if ( setting[0].Equals( "guildmaster" ) ) {
                            g.GuildMaster = setting[1];
                        }
                        if ( setting[0].Equals( "open" ) ) {
                            g.Open = bool.Parse( setting[1] );
                        }
                        if ( setting[0].Equals( "level" ) ) {
                            g.Level = int.Parse( setting[1] );
                        }
                        if ( setting[0].Equals( "currentexp" ) ) {
                            g.XP = float.Parse( setting[1] );
                        }
                        if ( setting[0].Equals( "pvp" ) ) {
                            g.PvP = Boolean.Parse( setting[1] );
                        }
                        if ( g.Members == null )
                            g.Members = new string[] { };
                        if ( g.PlayersInvited == null )
                            g.PlayersInvited = new string[] { };
                        if ( g.Level == -1 )
                            g.Level = 0;
                        if ( g.XP == -1 )
                            g.XP = 0;
                    }
                } catch ( Exception e ) {
                    Logger.LogToConsole( "Failed to load guild options!: " + e.Message );
                }
                GuildManager.AddGuildToList( g );
            }
            String path = "plugins/PlayerScores.txt";
            try {
                if ( File.Exists( path ) ) {
                    Cache.PlayerKillScores = File.ReadAllLines( path );
                }
                Logger.LogToConsole( "Loaded player kill scores" );
            } catch ( Exception e ) {
                Logger.LogToConsole( "Failed to load playerscores!: " + e.Message );
            }
        }

        public static void RemoveGuildFile( Guild guild ) {
            try {
                string FileName = String.Format( "{0}/{1}.txt", Dir, guild.Name );
                if ( File.Exists( FileName ) ) {
                    File.Delete( FileName );
                }
            } catch ( Exception e ) {
                Logger.Log( LogType.Error, "Could not delete guild file name: " + e.Message );
            }
        }
    }
}