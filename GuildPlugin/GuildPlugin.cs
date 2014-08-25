using System;
using System.Collections.Generic;
using System.Linq;
using fCraft;

namespace GuildPlugin {

    public class Init : Plugin {

        /// <summary>
        /// The saving task
        /// </summary>
        public SchedulerTask task;

        public void Initialize() {
            if ( !System.IO.Directory.Exists( "plugins/guilds" ) )
                System.IO.Directory.CreateDirectory( "plugins/guilds" );

            Logger.Log( LogType.ConsoleOutput, "Starting Guild Plugin Version " + Version );
            Server.Started += new EventHandler( Server_Initialized );
        }

        public string Name {
            get {
                return "GuildPlugin";
            }
            set {
                Name = value;
            }
        }

        public string Version {
            get {
                return "1.2";
            }
            set {
                Version = value;
            }
        }

        private void Server_Initialized( object sender, EventArgs e ) {
            task = Scheduler.NewTask( t => Events.SaveGuilds() ).RunForever( TimeSpan.FromMinutes( 1 ), TimeSpan.FromMinutes( 1 ) );
            Logger.Log( LogType.ConsoleOutput, "Loading " + GuildConfig.NumberOfGuilds + " guilds" );
            GuildConfig.LoadAllGuilds();
            Logger.Log( LogType.ConsoleOutput, "Finished loading guilds" );
            Logger.Log( LogType.ConsoleOutput, "Starting guild perks" );
            Events.Init();
            CommandManager.RegisterCustomCommand( CdGuild );
            CommandManager.RegisterCustomCommand( CdGuildChat );
            CommandManager.RegisterCustomCommand( CdHiScores );
        }

        private static readonly CommandDescriptor CdHiScores = new CommandDescriptor {
            Name = "HiScores",
            Aliases = new[] { "HiScore" },
            Category = CommandCategory.Fun,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.Chat },
            Help = "Shows a list of the top 10 player killers",
            Usage = "/HiScores [GuildName](Optional)",
            Handler = HiScoreHandler
        };

        private static void HiScoreHandler( Player player, Command cmd ) {
            string Param = cmd.NextAll();
            if ( string.IsNullOrEmpty( Param ) ) {
                string[] a = Cache.PlayerKillScores;
                if ( a.Length == 0 ) {
                    player.Message( "Top 10 killers on Au70: None available" );
                    return;
                }
                IOrderedEnumerable<string> b = a.OrderByDescending( p => {
                    if ( p != null )
                        return int.Parse( p.Split( '$' )[1] );
                    else
                        return float.MinValue;
                } );
                List<string> d = new List<string>();
                foreach ( string c in b ) {
                    string temp = c.Replace( "$", ": " );
                    d.Add( temp );
                }
                player.Message( "Top 10 killers on Au70: " + d.Take( Math.Max( 10, d.Count ) ).JoinToString() );
            } else {
                Guild g = GuildManager.FindGuildOrPrintResults( player, Param );
                if ( g == null )
                    return;
                player.Message( "This guild has killed a total of " + Cache.GetGuildKillsTotal( g ) + " enemy guild members" );
            }
        }

        private static readonly CommandDescriptor CdGuildChat = new CommandDescriptor {
            Name = "GuildChat",
            Aliases = new[] { "Gc" },
            Category = CommandCategory.Fun,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.Chat },
            Help = "Sends a message to your guild",
            Usage = "/GuildChat [Message]",
            Handler = GuildChatHandler
        };

        private static void GuildChatHandler( Player player, Command cmd ) {
            Guild guild = GuildManager.PlayersGuild( player.Info );
            if ( guild == null ) {
                player.Message( "You do not have a guild" );
                return;
            }
            string msg = cmd.NextAll();
            if ( string.IsNullOrEmpty( msg ) ) {
                player.Message( "&WMessage cannot be null: /Gc [message to send]" );
                return;
            }
            guild.SendGuildMessage( player, msg );
            return;
        }

        private static readonly CommandDescriptor CdGuild = new CommandDescriptor {
            Name = "Guild",
            Category = CommandCategory.Fun,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.Chat },
            Help = "Command to control your guild or use guild features",
            Usage = "/Guild [Create | List | Info | Find | Join | Invite | Perks | Xp | Who | SlapAll | Tag | Quit | Message | Bat | Turret | Bug | Open ]",
            Handler = GuildHandler
        };

        private static void GuildHandler( Player player, Command cmd ) {
            string Param = cmd.Next();
            if ( Param == null ) {
                CdGuild.PrintUsage( player );
                return;
            }

            Param = Param.ToLower();

            PlayerInfo target; //used to store a targetplayer in the switch case

            //this is for stuff which someone without a guild CAN do
            switch ( Param ) {
                case "create":
                    string Name = cmd.NextAll();
                    if ( string.IsNullOrEmpty( Name ) ) {
                        player.Message( "&WGuild name cannot be null: &H/Guild create Name" );
                        return;
                    }
                    if ( Cache.ForbiddenNames.Contains( Name.ToLower() ) ) {
                        player.Message( "&WName is forbidden: You cannot create a guild with this name" );
                        return;
                    }
                    GuildManager.CreateGuild( Name, player );
                    return;

                case "who":
                case "player":
                    string pp = cmd.Next();
                    if ( string.IsNullOrEmpty( pp ) ) {
                        player.Message( "&WPlayer name cannot be null: &h/Guild who playername" );
                        return;
                    }
                    target = PlayerDB.FindPlayerInfoOrPrintMatches( player, pp );
                    if ( target == null )
                        return;
                    player.Message( "{0}&s belongs to {1}", target.Name,
                        GuildManager.PlayersGuild( target ) == null ?
                        "no guild" :
                        "guild \"" + GuildManager.PlayersGuild( target ).Name + "\"" );
                    return;

                case "list":
                    string pro = cmd.Next();
                    if ( pro != null ) {
                        if ( pro.ToLower() == "provisional" ) {
                            player.Message( "Available provisional guilds:" );
                            player.Message( Cache.Guilds.Where( gg => gg.Provisional ).JoinToString() );
                            return;
                        }
                    }
                    player.Message( "Available guilds:" );
                    player.Message( Cache.Guilds.Where( gg => !gg.Provisional ).JoinToString() );
                    return;

                case "info":
                    string gstring = cmd.NextAll();
                    if ( string.IsNullOrEmpty( gstring ) ) {
                        if ( GuildManager.PlayerHasGuild( player.Info ) ) {
                            gstring = GuildManager.PlayersGuild( player.Info ).Name;
                        } else {
                            player.Message( "Guild name cannot be null: &H/Guild Info [GuildName]" );
                            return;
                        }
                    }
                    Guild gui = GuildManager.FindGuildOrPrintResults( player, gstring );
                    if ( gui == null ) {
                        return;
                    }
                    string[] Members = gui.Members.Where( p => gui.GuildMaster != p && !gui.Admins.Contains( p ) ).ToArray();
                    player.Message( "&hGuild \"{0}&h\" ({1}&h): &S\n" +
                        "{2}" +
                        "Guildmaster: {3}&S\n" +
                        "Admins: {4}\n" +
                        "Members: {5}\n" +
                        "Level: {6}\n" +
                        "PvP: {7}",
                        gui.Name,
                        !string.IsNullOrEmpty( gui.Tag ) ? gui.Tag : "No Tag",
                        gui.Open ? "This guild is open for anyone to join\n" : "",
                        gui.GuildMaster,
                        gui.Admins == null || gui.Admins.Length == 0 ? "(No Admins)" : gui.Admins.JoinToString(),
                        Members == null || Members.Length == 0 ? "(No Members)" : Members.JoinToString(),
                        gui.Level,
                        gui.PvP ? "&COn" : "&AOff" );
                    return;

                case "find":
                case "search":
                    string searchterm = cmd.NextAll(); {
                        if ( string.IsNullOrEmpty( searchterm ) ) {
                            player.Message( "&WSearch term cannot be null: &H/Guild search [search term]" );
                            return;
                        }
                        Guild[] FoundGuilds = GuildManager.FindGuilds( searchterm );
                        player.Message( "Guilds found matching {0}: {1}", searchterm, FoundGuilds.JoinToString() );
                    }
                    return;

                case "join":
                    if ( GuildManager.PlayerHasGuild( player.Info ) ) {
                        player.Message( "&HYou already have guild" );
                        return;
                    }
                    string s = cmd.NextAll();
                    if ( string.IsNullOrEmpty( s ) ) {
                        player.Message( "&WGuild to join cannot be null: &H/Guild join [GuildName]" );
                        return;
                    }
                    Guild g = GuildManager.FindGuildOrPrintResults( player, s );
                    if ( g == null )
                        return;
                    g.AcceptInvitationAndJoinGuild( player );
                    return;
            }
            if ( !GuildManager.PlayerHasGuild( player.Info ) ) {
                player.Message( "&HYou do not have a guild. Ask a guildmaster or guild admin to join their guild" );
                return;
            }
            //This is for stuff you NEED a guild for (provisional or not)
            Guild guild = GuildManager.PlayersGuild( player.Info );
            switch ( Param ) {
                case "open":
                    if ( !GuildManager.IsPlayerAnAdmin( player, guild ) ) {
                        player.Message( "&WYou need to be a guildmaster or admin to use this command" );
                        return;
                    }
                    string OpenParam = cmd.Next();
                    if ( string.IsNullOrEmpty( OpenParam ) ) {
                        player.Message( "&WCannot parse toggle: &H/Guild Open [True/False]" );
                        return;
                    }
                    bool OpenStatus;
                    if ( !bool.TryParse( OpenParam, out OpenStatus ) ) {
                        player.Message( "&WCannot parse toggle: &H/Guild Open [True/False]" );
                        return;
                    }
                    if ( guild.Open == OpenStatus )
                        player.Message( "Guild invitation status not changed, currently {0}",
                            OpenStatus == true ? "open" : "closed" );
                    else if ( guild.Open == true )
                        player.Message( "Guild invitation status changed to \"closed\"" );
                    else
                        player.Message( "Guild invitation status changed to \"open for anyone\"" );

                    guild.Open = OpenStatus;
                    return;

                case "pvp":
                    if ( !GuildManager.IsPlayerAnAdmin( player, guild ) ) {
                        player.Message( "&WYou need to be a guildmaster or admin to use this command" );
                        return;
                    }
                    PvP_Perk prk = ( PvP_Perk )GuildManager.FindPerkOrPrintResults( player, "pvp" );
                    if ( prk == null )
                        return;
                    Guild g = guild;
                    if ( g.Level < prk.LevelNeeded ) {
                        player.Message( "You cannot use this perk: Your guild needs level {0}", prk.LevelNeeded );
                        return;
                    }
                    if ( g.PvP ) {
                        prk.StopPerk();
                        guild.SendGuildNotification( "&HGuild PvP has been " + Color.Green + "disabled" );
                    } else {
                        if ( string.IsNullOrEmpty( g.Tag ) ) {
                            player.Message( "&WGuild tag must be set before pvp can be enabled" );
                            return;
                        }
                        prk.StartPerk();
                        guild.SendGuildNotification( "&HGuild PvP has been &Wenabled" );
                    }
                    return;

                case "invite":
                    if ( !GuildManager.IsPlayerAnAdmin( player, guild ) ) {
                        player.Message( "&WYou need to be a guildmaster or admin to use this command" );
                        return;
                    }
                    string playerstr = cmd.Next();
                    if ( String.IsNullOrEmpty( playerstr ) ) {
                        player.Message( "You need to use a name for someone to join your guild: &H/Guild invite PlayerName" );
                        return;
                    }
                    target = PlayerDB.FindPlayerInfoOrPrintMatches( player, playerstr );
                    if ( target == null )
                        return;
                    List<String> lst = guild.PlayersInvited.ToList();
                    if ( GuildManager.PlayerHasGuild( target ) ) {
                        player.Message( "&WThis player is already in a guild! ({0})", GuildManager.PlayersGuild(target));
                        return;
                    }
                    if ( lst.Contains( target.Name ) ) {
                        player.Message( "&WThis player has already been invited to your guild" );
                        return;
                    }
                    lst.Add( target.Name );
                    guild.PlayersInvited = lst.ToArray();
                    player.Message( "Player {0} has been invited to your guild", target.Name );
                    if ( target.IsOnline ) {
                        target.PlayerObject.Message( "&H{0} has invited you to join their guild {1}! Join now, using /Guild Join {1}", player.Name, guild.Name );
                    }
                    return;

                case "exp":
                case "xp":
                case "experience":
                    player.Message( "&HCurrent XP: {0}. Xp needed for next level ({1}): {2}",
                        guild.XP,
                        guild.Level + 1,
                        guild.XPNeededForNextLevel() );
                    return;

                case "leave":
                case "quit":
                    if ( !cmd.IsConfirmed ) {
                        player.Confirm( cmd, "Remove yourself from {0}&S?", GuildManager.PlayersGuild( player.Info ).Name );
                        return;
                    }
                    guild.RemoveMember( player.Info );
                    return;

                case "message":
                case "msg":
                    string msg = cmd.NextAll();
                    if ( string.IsNullOrEmpty( msg ) ) {
                        player.Message( "&WMessage cannot be null: &H/Guild message [message to send]" );
                        return;
                    }
                    guild.SendGuildMessage( player, msg );
                    return;
            }
            if ( guild.Provisional ) {
                GuildManager.MessageProvisionalGuild( player );
                return;
            }
            //this is for stuff a provisional guild cannot do
            switch ( Param ) {
                case "admin":
                case "admins":
                    string s1 = cmd.Next();
                    if ( string.IsNullOrEmpty( s1 ) ) {
                        player.Message( "Guild admins: {0}",
                            guild.Admins.Length == 0 ?
                            "&WThis guild has no admins!" :
                            "(Guildmaster)" + guild.GuildMaster + ", " + guild.Admins.JoinToString() );

                        if ( player.Name == guild.GuildMaster ) {
                            player.Message( "&HYou can add admins using &H/Guild admin [add/remove] [playerName]" );
                        }
                        return;
                    }
                    if ( player.Name != guild.GuildMaster ) {
                        player.Message( "&WOnly the guildmaster ({0}) can manage admins", guild.GuildMaster );
                        return;
                    }
                    string s2 = cmd.Next();
                    if ( string.IsNullOrEmpty( s2 ) ) {
                        player.Message( "Player name cannot be null" );
                        return;
                    }
                    target = PlayerDB.FindPlayerInfoOrPrintMatches( player, s2 );
                    if ( target == null )
                        return;
                    string Name = target.Name;
                    if ( !guild.Members.Contains( Name ) ) {
                        player.Message( "This player is not a member of your guild" );
                        return;
                    }
                    if ( s1.ToLower() == "add" ) {
                        List<string> lst = guild.Admins.ToList();
                        if ( Name == player.Name ) {
                            player.Message( "&WCannot add yourself" );
                            return;
                        }
                        if ( lst.Contains( Name ) ) {
                            player.Message( "{0} is already an admin", Name );
                            return;
                        }
                        lst.Add( Name );
                        guild.Admins = lst.ToArray();
                        player.Message( "Added {0} as an admin", Name );
                        if ( target.IsOnline ) {
                            target.PlayerObject.Message( "&WYou are now an admin of {0}", guild.Name );
                        }
                    }
                    if ( s1.ToLower() == "remove" ) {
                        List<string> lst = guild.Admins.ToList();
                        if ( Name == player.Name ) {
                            player.Message( "&WCannot remove yourself" );
                            return;
                        }
                        if ( !lst.Contains( Name ) ) {
                            player.Message( "&W{0} is not an admin", Name );
                            return;
                        }
                        lst.Remove( Name );
                        guild.Admins = lst.ToArray();
                        player.Message( "Removed {0} as an admin", Name );
                        if ( target.IsOnline ) {
                            target.PlayerObject.Message( "&WYou are no longer an admin of {0}", guild.Name );
                        }
                    }
                    return;

                case "kick":
                    if ( !GuildManager.IsPlayerAnAdmin( player, guild ) ) {
                        player.Message( "&WYou need to be an Admin+ of the guild to use this command" );
                        return;
                    }
                    string TargetName = cmd.Next();
                    if ( string.IsNullOrEmpty( TargetName ) ) {
                        player.Message( "&WPlayer name cannot be null" );
                        return;
                    }
                    target = PlayerDB.FindPlayerInfoOrPrintMatches( player, TargetName );
                    if ( target == null )
                        return;
                    if ( target.Name == guild.GuildMaster ) {
                        player.Message( "&WUnable to kick this player" );
                        return;
                    }
                    if ( !guild.Members.Contains( target.Name ) ) {
                        player.Message( "&WPlayer is not a member of your guild" );
                        return;
                    }
                    guild.RemoveMember( target );
                    guild.SendGuildNotification( target.Name + " was kicked from the guild by " + player.Name );
                    return;

                case "bug":
                case "eb":
                case "explodingbug":
                    ExplodingBugPerk p1 = ( ExplodingBugPerk )GuildManager.FindPerkOrPrintResults( player, "explodingbug" );
                    if ( p1 == null )
                        return;
                    if ( guild.Level < p1.LevelNeeded ) {
                        player.Message( "You cannot use this perk: Your guild needs level {0}", p1.LevelNeeded );
                        return;
                    }
                    string bParam = cmd.Next();
                    if ( string.IsNullOrEmpty( bParam ) ) {
                        object o;
                        if ( player.PublicAuxStateObjects.TryGetValue( "bug", out o ) ) {
                            player.Message( "&WYou already have an exploding bug alive" );
                            return;
                        }
                        if ( Events.IsInRangeOfSpawnpoint( player.World, player.Position ) ) {
                            player.Message( "&WCannot place a bug this close to a spawn point" );
                            return;
                        }
                        p1.SpawnBug( player );
                    } else if ( bParam.ToLower() == "stop" || bParam.ToLower() == "remove" ) {
                        object o;
                        if ( player.PublicAuxStateObjects.TryGetValue( "bug", out o ) ) {
                            ExplodingBug b = ( ExplodingBug )o;
                            b.Stop();
                        } else {
                            player.Message( "&WYou do not have an exploding bug to remove" );
                        }
                    }
                    return;

                case "turret":
                    string tParam = cmd.Next();
                    TurretPerk p2 = ( TurretPerk )GuildManager.FindPerkOrPrintResults( player, "turretperk" );
                    if ( p2 == null )
                        return;
                    if ( guild.Level < p2.LevelNeeded ) {
                        player.Message( "You cannot use this perk: Your guild needs level {0}", p2.LevelNeeded );
                        return;
                    }
                    if ( string.IsNullOrEmpty( tParam ) ) {
                        if ( p2.turret != null ) {
                            player.Message( "&WA turret in your guild already exists: {0} - {1}", p2.turret.world.Name, p2.turret.blocks[0].ToString() );
                            return;
                        }
                        if ( Events.IsInRangeOfSpawnpoint( player.World, player.Position ) ) {
                            player.Message( "&WCannot place a turret this close to a spawn point" );
                            return;
                        }
                        p2.AddTurret( player );
                        return;
                    } else if ( tParam.ToLower() == "stop" || tParam.ToLower() == "remove" ) {
                        if ( p2.turret == null ) {
                            player.Message( "&WA turret in your guild does not exist" );
                            return;
                        }
                        p2.StopPerk();
                        p2.turret = null;
                        player.Message( "Stopping turret..." );
                    }
                    return;

                case "tag":
                    if ( !GuildManager.PlayerHasGuild( player.Info ) ) {
                        player.Message( "You are not in a guild. Create one, or ask someone to join theirs" );
                        return;
                    }
                    if ( !GuildManager.IsPlayerAnAdmin( player, guild ) ) {
                        player.Message( "&WYou need to be a guildmaster or admin to use this command" );
                        return;
                    }
                    string Tag = cmd.NextAll();
                    if ( string.IsNullOrEmpty( Tag ) ) {
                        if ( guild.PvP ) {
                            if ( !cmd.IsConfirmed ) {
                                player.Confirm( cmd, "&WTag cannot be removed when PVP is enabled\n&SDisable PVP?" );
                                return;
                            }
                            guild.PvP = false;
                            guild.SendGuildNotification( "&HGuild PvP has been disabled" );
                            guild.ChangeTagName( player, Tag );
                            return;
                        }
                    }
                    guild.ChangeTagName( player, Tag );
                    return;

                case "bat":
                    TheBatPerk tbp = ( TheBatPerk )GuildManager.FindPerkOrPrintResults( player, "TheBat" );
                    if ( guild.Level < tbp.LevelNeeded ) {
                        player.Message( "Your guild needs to be level {0} to use this perk (Current level: {1})",
                            tbp.LevelNeeded,
                            guild.Level );
                        return;
                    }
                    if ( player.PublicAuxStateObjects.ContainsKey( "bat" ) ) {
                        player.Message( "Bat off" );
                        player.PublicAuxStateObjects.Remove( "bat" );
                    } else {
                        player.Message( "Bat on" );
                        player.PublicAuxStateObjects.Add( "bat", true );
                        Bat b = new Bat( player );
                        b.Start();
                    }
                    return;

                case "reroll":
                    if ( string.IsNullOrEmpty( guild.GuildMaster ) ) {
                        Random rand = new Random();
                        player.Message( "Attempting to reroll guildmaster" );
                        if ( guild.Admins.Length > 0 ) {
                            guild.GuildMaster = guild.Admins[rand.Next(0, guild.Admins.Length )];
                        } else {
                            guild.GuildMaster = guild.Members[rand.Next(0, guild.Members.Length )];
                        }
                        if ( string.IsNullOrEmpty( guild.GuildMaster ) )
                            guild.GuildMaster = player.Name;
                        player.Message( "New guildmaster is: " + guild.GuildMaster );
                    }
                    return;

                case "slapall":
                    if ( !GuildManager.IsPlayerAnAdmin( player, guild ) ) {
                        player.Message( "&WYou need to be a guildmaster or admin to use this command" );
                        return;
                    }
                    SlapAllPerk p = ( SlapAllPerk )GuildManager.FindPerkOrPrintResults( player, "SlapAllPerk" );
                    if ( !Perk.GuildHasPerk( guild, p ) ) {
                        player.Message( "&WYou are unable to use this command: You need the \"SlapAll\" perk" );
                        return;
                    }
                    DateTime now = DateTime.UtcNow;
                    DateTime yesterday = now.AddDays( -1 );
                    if ( p.start != null ) {
                        if ( p.start > yesterday && p.start <= now ) {
                            string op = cmd.NextAll();
                            if ( string.IsNullOrEmpty( op ) ) {
                                player.Message( "&WGuild cannot be null: &H/Guild SlapAll [GuildName]" );
                                return;
                            }
                            Guild gd = GuildManager.FindGuildOrPrintResults( player, op );
                            if ( gd == null )
                                return;
                            int i = 0;
                            foreach ( string s in gd.Members ) {
                                PlayerInfo pla = PlayerDB.FindPlayerInfoExact( s );
                                if ( pla != null ) {
                                    Player pl = pla.PlayerObject;
                                    if ( pl != null ) {
                                        i++;
                                        pl.TeleportTo( new Position( pl.Position.X, pl.Position.Y, pl.World.Map.Height * 32 ) );
                                        pl.Message( "You were slapped sky high from {0}'s guild", guild.GuildMaster );
                                    }
                                }
                            }
                            player.Message( "You slapped {0} members of guild {1} sky high", i, gd.Name );
                        } else
                            player.Message( "&WUnable to slap another guild: The perk is still on cooldown since it was last used (24 hour cooldown)" );
                    }
                    return;

                case "perk":
                case "perks":
                    string str = cmd.NextAll();
                    if ( string.IsNullOrEmpty( str ) ) {
                        player.Message( "Perks your guild currently has: {0}",
                            guild.AvailablePerks().JoinToString() );
                        player.Message( "&HUse /Guild Perk [Name] for info on that perk" );
                    } else {
                        if ( str.ToLower() == "all" ) {
                            player.Message( "All perks: {0}",
                            guild.Perks.JoinToString() );
                            player.Message( "&HUse /Guild Perk [Name] for info on that perk" );
                            return;
                        }
                        Perk perk = GuildManager.FindPerkOrPrintResults( player, str );
                        if ( perk == null )
                            return;
                        player.Message( "Help info on Perk {0}: ", perk.PerkName );
                        player.Message( Perk.GetHelpSection( perk ) );
                    }
                    return;

                default:
                    player.Message( "Unknown param for /Guild: Try {0}",
                        "/Guild [Create | List | Info | Find | Join | Invite | Perks | Xp | SlapAll | Tag | Quit | Message | Bat | Turret | Bug ]" );
                    return;
            }
        }
    }
}