using System;
using System.Collections.Generic;
using System.Linq;
using fCraft;

namespace GuildPlugin {

    public static class GuildManager {

        /// <summary>
        /// Gets the guild which the player is a part of
        /// </summary>
        /// <param name="player">Player in question</param>
        /// <returns>The guild belonging to the player (Null if none has been found)</returns>
        public static Guild PlayersGuild( PlayerInfo playerInfo ) {
            foreach ( Guild g in Cache.Guilds ) {
                if ( g.Members.Contains( playerInfo.Name ) ) {
                    return g;
                }
            }
            return null;
        }

        /// <summary>
        /// Is the player a member of a guild?
        /// </summary>
        /// <param name="player">Player in question</param>
        /// <returns>True if the player is in a guild</returns>
        public static bool PlayerHasGuild( PlayerInfo player ) {
            return PlayersGuild( player ) != null;
        }

        /// <summary>
        /// Searchs are returns a guild. If no guild is found, returns error message then null.
        /// If more than one guild is found, returns a message listing guilds then null
        /// </summary>
        /// <param name="player">Player searching</param>
        /// <param name="name">Name of the guild</param>
        public static Guild FindGuildOrPrintResults( Player player, string name ) {
            if ( player == null )
                throw new ArgumentNullException( "player" );
            if ( name == null )
                throw new ArgumentNullException( "name" );

            Guild guild = FindGuildExact( name );
            if ( guild == null ) {
                Guild[] guilds = FindGuilds( name );
                if ( guilds.Length == 0 ) {
                    player.Message( "No guilds found matching \"" + name + "\"" );
                    return null;
                } else if ( guilds.Length > 1 ) {
                    player.Message( "More than one guild matched: " + guilds.JoinToString() );
                    return null;
                }
                guild = guilds[0];
            }
            return guild;
        }

        public static Guild FindGuildExact( string name ) {
            if ( name == null )
                throw new ArgumentNullException( "name" );
            foreach ( Guild guild in Cache.Guilds ) {
                if ( guild.Name.ToLower() == name.ToLower() ) {
                    return guild;
                }
            }
            return null;
        }

        public static Guild[] FindGuilds( string Name ) {
            if ( Name == null )
                throw new ArgumentNullException( "Name" );
            List<Guild> result = new List<Guild>();
            int count = 0;
            Guild[] cache = Cache.Guilds;
            for ( int i = 0; i < cache.Length; i++ ) {
                if ( cache[i].Name.ToLower().Contains( Name.ToLower() ) ) {
                    result.Add( cache[i] );
                    count++;
                }
            }
            return result.ToArray();
        }

        public static Perk FindPerkOrPrintResults( Player player, string PerkName ) {
            List<Perk> temp = new List<Perk>();
            foreach ( Perk perk in PlayersGuild( player.Info ).Perks ) {
                if ( perk.PerkName.ToLower().Equals( PerkName.ToLower() ) ) {
                    return perk;
                }
                if ( perk.PerkName.ToLower().Contains( PerkName.ToLower() ) ) {
                    temp.Add( perk );
                }
            }
            if ( temp.ToArray().Length == 1 ) {
                return temp.ToArray()[0];
            }
            if ( temp.ToArray().Length > 1 ) {
                player.Message( "More than one perk matched: " + temp.JoinToString() );
                return null;
            }
            player.Message( "&WNo perks found matching \"" + PerkName + "\"" );
            return null;
        }

        public static void MessageProvisionalGuild( Player player ) {
            player.Message( "&WYour guild is provisional and will become active once it contains 4 members\n" +
                "Use /Guild Info to get a list of current members" );
        }

        /// <summary>
        /// Starts all running perks on the guild instance
        /// </summary>
        /// <param name="guild">Guild in question</param>
        public static void StartAllGuildPerks( Guild guild ) {
            lock ( guild.Perks ) {
                foreach ( Perk p in guild.AvailablePerks() ) {
                    if ( !p.Started )
                        p.StartPerk();
                }
            }
        }

        /// <summary>
        /// Stops all running perks on the guild instance
        /// </summary>
        /// <param name="guild">Guild in question</param>
        public static void StopAllGuildPerks( Guild guild ) {
            Perk[] perks = guild.AvailablePerks().ToArray();
            foreach ( Perk p in perks ) {
                p.StopPerk();
            }
        }

        /// <summary>
        /// Creates a guild
        /// </summary>
        /// <param name="Name">Name of the guild</param>
        /// <param name="Tag">Tag of the guild</param>
        /// <param name="GuildMaster">Creator of the guild (player)</param>
        public static void CreateGuild( String Name, Player GuildMaster ) {
            //Run some checks
            //==========================================================================
            foreach ( Guild g1 in Cache.Guilds ) {
                if ( g1.Name.Equals( Name, StringComparison.OrdinalIgnoreCase ) ) {
                    GuildMaster.Message( "&WError: Selected guild name already exists" );
                    return;
                }
                if ( CheckStringSimilarities( g1.Name, Name ) >= 80 ) {
                    GuildMaster.Message( "&WError: Selected name is over 80% similar to another guild name \"{0}\"", g1.Name );
                    return;
                }
            }
            if ( PlayerHasGuild( GuildMaster.Info ) ) {
                GuildMaster.Message( "&WError: You are already in a guild. You can leave your guild by using /Guild Quit" );
                return;
            }
            if ( Name.Length > 20 ) {
                GuildMaster.Message( "&WError: Guild name cannot exceed 20 characters" );
                return;
            }
            int i = 0;
            foreach ( Char c in Name.ToCharArray() ) {
                if ( c == ' ' )
                    i++;
            }
            if ( i > 2 ) {
                GuildMaster.Message( "&WError: Guild name contains too many spaces (More than 2)" );
                return;
            }
            Name = Color.StripColors( Name );
            //==========================================================================
            Guild g = new Guild( Name, null, GuildMaster.Name );
            g.Level = 0;
            g.XP = 0;
            g.PvP = false;
            AddGuildToList( g );
            GuildMaster.Message( "Your guild \"{0}\" has been created, but is provisional. " +
            "You need to add 3 more members before your guild becomes active.\nAs the guild creator, " +
            "you are now the guild master and have full control of the guild", Name );
        }

        public static string GetGuildPvPTagColor( Guild guild ) {
            if ( guild.PvP )
                return Color.Red;
            else
                return Color.Green;
        }

        public static bool IsPlayerAnAdmin( Player player, Guild g ) {
            return IsPlayerAnAdmin( player.Name, g );
        }

        public static bool IsPlayerAnAdmin( String Name, Guild g ) {
            if ( g.GuildMaster == Name )
                return true;
            if ( g.Admins == null )
                return false;
            if ( g.Admins.Contains( Name ) )
                return true;
            return false;
        }

        public static void AddGuildToList( Guild g ) {
            if ( g == null ) {
                throw new ArgumentNullException( "guild" );
            }
            List<Guild> gs = Cache.Guilds.ToList();
            gs.Add( g );
            Cache.Guilds = gs.ToArray();
        }

        public static int ParseKillContextXP( KillContext context ) {
            int XP = 0;
            switch ( context ) {
                case KillContext.Turret:
                case KillContext.ExplodingBug:
                    XP = 30;
                    break;

                case KillContext.Gun:
                    XP = 40;
                    break;

                case KillContext.Bat:
                case KillContext.Standard:
                    XP = 60;
                    break;
            }
            return XP;
        }

        /// <summary>
        /// Compute the distance between two strings in percentage
        /// </summary>
        private static int CheckStringSimilarities( string first, string second ) {
            if ( first == null ) {
                throw new ArgumentNullException( "first" );
            }
            if ( second == null ) {
                throw new ArgumentNullException( "second" );
            }

            int n = first.Length;
            int m = second.Length;
            var d = new int[n + 1, m + 1]; // matrix

            if ( n == 0 )
                return m;
            if ( m == 0 )
                return n;

            for ( int i = 0; i <= n; d[i, 0] = i++ ) { }

            for ( int j = 0; j <= m; d[0, j] = j++ ) { }

            for ( int i = 1; i <= n; i++ ) {
                for ( int j = 1; j <= m; j++ ) {
                    int cost = ( second.Substring( j - 1, 1 ) == first.Substring( i - 1, 1 ) ? 0 : 1 ); // cost
                    d[i, j] = Math.Min(
                        Math.Min(
                            d[i - 1, j] + 1,
                            d[i, j - 1] + 1 ),
                        d[i - 1, j - 1] + cost );
                }
            }
            int percentage = ( int )Math.Round( ( 1.0 - ( ( double )d[n, m] / ( double )System.Math.Max( n, m ) ) ) * 100.0, 2 );
            return percentage;
        }
    }
}