using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fCraft;
using fCraft.Drawing;
using fCraft.Events;

namespace GuildPlugin {

    public static class Events {

        public static void Init() {
            Player.PlacingBlock += PlayerPlacedBlock;
            Player.Moved += OnPlayerMoved;
            Server.ShutdownBegan += Server_ShutdownBegan;
            Player.Connected += Player_Connected;
            Chat.Sent += Chat_Sent;
            DrawOperation.Ended += DrawOperation_Ended;
            CommandManager.CommandCalling += CommandCalling;
            PlayerInfo.FreezeChanged += new EventHandler<PlayerInfoFrozenChangedEventArgs>( PlayerInfo_FreezeChanged );
            WorldManager.WorldCreated += WorldCreated;
            Player.Disconnected += new EventHandler<PlayerDisconnectedEventArgs>( Player_Disconnected );
        }

        public static void DeInit() {
            Player.PlacingBlock -= PlayerPlacedBlock;
            Player.Moved -= OnPlayerMoved;
            Player.Connected -= Player_Connected;
            Chat.Sent -= Chat_Sent;
            DrawOperation.Ended -= DrawOperation_Ended;
            CommandManager.CommandCalling -= CommandCalling;
            PlayerInfo.FreezeChanged -= PlayerInfo_FreezeChanged;
        }

        public static void Player_Disconnected( object sender, PlayerDisconnectedEventArgs e ) {
            World world = e.Player.World;
            object o;
            if ( e.Player.PublicAuxStateObjects.TryGetValue( "bug", out o ) ) {
                ExplodingBug b = ( ExplodingBug )o;
                b.Stop();
            }
        }

        public static void WorldCreated( object sender, WorldCreatedEventArgs e ) {
            List<Turret> tur = Cache.Turrets.ToList();
            foreach ( Turret t in Cache.Turrets ) {
                if ( t.world.Name == e.World.Name ) {
                    tur.Remove( t );
                }
            }
            Cache.Turrets = tur.ToArray();
        }

        private static void CommandCalling( object sender, CommandCallingEventArgs e ) {
            if ( e.Player.World == null )
                return;
            Command cmd = e.Command;
            if ( cmd == null )
                return;
            if ( Cache.ForbiddenCommands.Contains( cmd.Name.ToLower() ) ) {
                e.Player.Message( "Use of this command is forbidden when running Au70 Guilds" );
                e.Cancel = true;
            }
        }

        public static bool IsInRangeOfSpawnpoint( World world, Position PlayerPos ) {
            try {
                int Xdistance = ( world.Map.Spawn.X / 32 ) - ( PlayerPos.X / 32 );
                int Ydistance = ( world.Map.Spawn.Y / 32 ) - ( PlayerPos.Y / 32 );
                int Zdistance = ( world.Map.Spawn.Z / 32 ) - ( PlayerPos.Z / 32 );

                if ( Xdistance <= 20 && Xdistance >= -20 ) {
                    if ( Ydistance <= 20 && Ydistance >= -20 ) {
                        if ( Zdistance <= 20 && Zdistance >= -20 ) {
                            return true;
                        }
                    }
                }
            } catch ( Exception ex ) {
                Logger.Log( LogType.Error, "GuildHandler.IsInRangeOfSpawnpoint: " + ex );
            }

            return false;
        }

        private static void PlayerInfo_FreezeChanged( object sender, PlayerInfoFrozenChangedEventArgs e ) {
            if ( !e.Unfreezing ) {
                Player p = e.Freezer;
                if ( GuildManager.PlayerHasGuild( p.Info ) ) {
                    Guild g = GuildManager.PlayersGuild( p.Info );
                    g.AddXP( ( int )Exp.Freeze );
                }
            }
        }

        private static void DrawOperation_Ended( object sender, DrawOperationEventArgs e ) {
            if ( e.DrawOp == null )
                return;
            Player p = e.DrawOp.Player;
            if ( GuildManager.PlayerHasGuild( p.Info ) ) {
                Guild g = GuildManager.PlayersGuild( p.Info );
                g.AddXP( ( int )Exp.DrawOp * g.BuildingModifier );
            }
        }

        private static void Chat_Sent( object sender, ChatSentEventArgs e ) {
            if ( e.Message == "$$FixGuilds" ) {
                foreach ( Guild g1 in Cache.Guilds ) {
                    if ( g1.Tag == "" )
                        g1.Tag = null;
                    if ( !string.IsNullOrEmpty( g1.Tag ) ) {
                        if ( g1.PvP ) {
                            foreach ( String s in g1.Members ) {
                                PlayerInfo i = PlayerDB.FindPlayerInfoExact( s );
                                if ( i != null ) {
                                    i.TitleName = GuildManager.GetGuildPvPTagColor(g1) + "[" + Color.StripColors( g1.Tag ) + "]";
                                }
                            }
                        } 
                    }
                }
                return;
            }
            if ( GuildManager.PlayerHasGuild( e.Player.Info ) ) {
                if ( e.Player.Info.MessagesWritten % 100 == 0 ) {
                    Guild g = GuildManager.PlayersGuild( e.Player.Info );
                    g.AddXP( ( int )Exp.OneHundredChat );
                }
            }
        }

        private static void Player_Connected( object sender, PlayerConnectedEventArgs e ) {
            if ( e.Player != null ) {
                if ( e.StartingWorld != null ) {
                    Scheduler.NewTask( t => SendPlayerConnectedMsg( e.Player ) ).RunOnce( TimeSpan.FromSeconds( 20 ) );
                }
            }
        }

        private static void SendPlayerConnectedMsg( Player player ) {
            if ( player != null ) {
                if ( player.IsOnline ) {
                    if ( GuildManager.PlayerHasGuild( player.Info ) ) {
                        Guild g = GuildManager.PlayersGuild( player.Info );
                        player.Message( "&HYour guild currently has {0} members and {1} XP",
                            g.Members.Length,
                            g.XP );
                        g.AddXP( ( int )Exp.Connected );
                    } else {
                        player.Message( "&HYou do not have a guild! Find one using /Guild List" );
                    }
                }
            }
        }

        private static void Server_ShutdownBegan( object sender, EventArgs e ) {
            DeInit();
            SaveGuilds();
        }

        public static void SaveGuilds() {
            lock ( Cache.Guilds ) {
                foreach ( Guild g in Cache.Guilds ) {
                    g.Save();
                }
            }
            SavePlayerScores();
        }

        public static void SavePlayerScores() {
            string FilePath = "plugins/";
            string FileFullPath = FilePath + "PlayerScores.txt";
            try {
                System.IO.File.WriteAllLines( FileFullPath, Cache.PlayerKillScores, Encoding.Default );
            } catch ( Exception e ) {
                Logger.Log( LogType.Error, "PlayerScoresSave: " + e.Message );
            }
        }

        public static void PlayerPlacedBlock( object sender, PlayerPlacingBlockEventArgs e ) {
            try {
                foreach ( Turret t in Cache.Turrets ) {
                    if ( e.Player.World == t.world ) {
                        if ( t.blocks.Contains( e.Coords ) ) {
                            e.Result = CanPlaceResult.Revert;
                        }
                    }
                }
                if ( GuildManager.PlayerHasGuild( e.Player.Info ) ) {
                    if ( e.Context == BlockChangeContext.Manual ) {
                        if ( e.NewBlock != Block.Air ) {
                            Guild g = GuildManager.PlayersGuild( e.Player.Info );
                            if ( !g.Provisional ) {
                                g.AddXP( ( int )Exp.BlockPlaced * g.BuildingModifier );
                            }
                        }
                    }
                }
            } catch ( Exception e1 ) {
                Logger.Log( LogType.Error, "GuildPlaceBlock: " + e1 );
            }
        }

        private static void OnPlayerMoved( object sender, PlayerMovedEventArgs e ) {
            try {
                if ( IsInRangeOfSpawnpoint( e.Player.World, e.NewPosition ) )
                    return;
                if ( !GuildManager.PlayerHasGuild( e.Player.Info ) )
                    return;
                if ( !GuildManager.PlayersGuild( e.Player.Info ).PvP )
                    return;
                if ( e.Player.World != null ) {
                    if ( e.NewPosition != null && e.OldPosition != null ) {
                        Vector3I oldPos = new Vector3I( e.OldPosition.X / 32, e.OldPosition.Y / 32, e.OldPosition.Z / 32 );
                        Vector3I newPos = new Vector3I( e.NewPosition.X / 32, e.NewPosition.Y / 32, e.NewPosition.Z / 32 );
                        //if they are inbounds, check zone (33*33) and kill if needed
                        if ( oldPos.X != newPos.X || oldPos.Y != newPos.Y || oldPos.Z != newPos.Z ) {
                            if ( e.Player.World.Map.InBounds( newPos ) ) {
                                if ( e.Player.PublicAuxStateObjects.ContainsKey( "dead" ) ) {
                                    e.Player.TeleportTo( e.OldPosition );
                                    return;
                                }
                                foreach ( Guild guild in Cache.Guilds ) {
                                    foreach ( string ps in guild.Members ) {
                                        PlayerInfo pi = PlayerDB.FindPlayerInfoExact( ps );
                                        if ( pi != null ) {
                                            Player p = pi.PlayerObject;
                                            if ( p != null ) {
                                                if ( p != e.Player ) {
                                                    if ( GuildManager.PlayersGuild( e.Player.Info ) != GuildManager.PlayersGuild( p.Info ) ) {
                                                        if ( GuildManager.PlayersGuild( p.Info ).PvP ) {
                                                            if ( p.World.Name == e.Player.World.Name ) {
                                                                if ( !IsInRangeOfSpawnpoint( p.World, p.Position ) ) {
                                                                    Vector3I pos = p.Position.ToBlockCoords();
                                                                    //check bat kills
                                                                    foreach ( Vector3I Block in p.GunCache.Values ) {
                                                                        if ( e.NewPosition.DistanceSquaredTo( Block.ToPlayerCoords() ) <= 49 * 49 ) {
                                                                            KillPlayer( p, e.Player, KillContext.Bat );
                                                                            return;
                                                                        }
                                                                    }
                                                                    foreach ( Vector3I Block in e.Player.GunCache.Values ) {
                                                                        if ( p.Position.DistanceSquaredTo( Block.ToPlayerCoords() ) <= 49 * 49 ) {
                                                                            KillPlayer( e.Player, p, KillContext.Bat );
                                                                            return;
                                                                        }
                                                                    }

                                                                    //check position kills
                                                                    if ( e.NewPosition.DistanceSquaredTo( pos.ToPlayerCoords() ) <= 33 * 33 ) {
                                                                        KillPlayer( e.Player, p, KillContext.Standard );
                                                                        return;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            } catch ( Exception ex ) {
                Logger.Log( LogType.SeriousError, ex.ToString() );
            }
        }

        public static void KillPlayer( Player player, Player target, KillContext context ) {
            try {
                if ( target.IsOnline ) {
                    SetPlayerDead( target );
                    Guild g1 = GuildManager.PlayersGuild( player.Info );
                    Guild g2 = GuildManager.PlayersGuild( target.Info );
                    int Seconds = 6 - GuildManager.PlayersGuild( target.Info ).DeadSaver;
                    double time = ( DateTime.UtcNow - target.LastTimeKilled ).TotalSeconds;
                    if ( time > Seconds ) {
                        string t = "";
                        switch ( context ) {
                            case KillContext.Standard:
                                break;

                            case KillContext.Turret:
                                t = "with a turret";
                                break;

                            case KillContext.Gun:
                                t = "with a gun";
                                break;

                            case KillContext.Bat:
                                t = "with a bat";
                                break;

                            case KillContext.ExplodingBug:
                                t = "with an exploding bug";
                                break;
                        }
                        if ( context == KillContext.Turret ) {
                            if ( player == target ) {
                                g2.SendGuildNotification( String.Format( "{0}&S obstructed the turret's lazer and was incinerated", player.ClassyName ) );
                            } else {
                                g2.SendGuildNotification( String.Format( "{0}&S has &4killed&s your guild member: {1}&S {2}", player.ClassyName, target.ClassyName, t ) );
                                g1.SendGuildNotification( String.Format( "{0}&S killed an enemy guild member: {1}&S {2}", player.ClassyName, target.ClassyName, t ) );
                            }
                        } else {
                            g2.SendGuildNotification( String.Format( "{0}&S has &4killed&s your guild member: {1}&S {2}", player.ClassyName, target.ClassyName, t ) );
                            g1.SendGuildNotification( String.Format( "{0}&S killed an enemy guild member: {1}&S {2}", player.ClassyName, target.ClassyName, t ) );
                        }
                        target.LastTimeKilled = DateTime.UtcNow;
                        g1.AddXP( GuildManager.ParseKillContextXP( context ) );

                        string PlayerKillScore = Cache.GetPlayerScoreEntry( player.Info );
                        if ( PlayerKillScore == null ) {
                            PlayerKillScore = player.Name + "$0";
                        }
                        int Score;
                        if ( int.TryParse( PlayerKillScore.Split( '$' )[1], out Score ) ) {
                            Score++;
                            string newString = player.Name + "$" + Score;
                            List<String> nl = Cache.PlayerKillScores.ToList();
                            nl.Remove( PlayerKillScore );
                            nl.Add( newString );
                            Cache.PlayerKillScores = nl.ToArray();
                        } else
                            Logger.Log( LogType.Error, "PlayerScoreAdd: Unable to increment" );
                    }
                }
            } catch ( Exception e ) {
                Logger.Log( LogType.Error, e.ToString() );
            }
        }

        private static void SetPlayerDead( Player player ) {
            try {
                if ( player.IsOnline ) {
                    player.Send( Packets.MakeTeleport( 255, new Position( player.World.Map.Spawn.X, player.World.Map.Spawn.Y, player.World.Map.Height - 1 ) ) );
                    StopWoMCrashBlocks( player );
                    player.Position = new Position( player.World.Map.Spawn.X, player.World.Map.Spawn.Y, player.World.Map.Spawn.Z + 1000 );
                    if ( !player.PublicAuxStateObjects.ContainsKey( "dead" ) ) {
                        player.PublicAuxStateObjects.Add( "dead", true );
                        int Seconds = 6 - GuildManager.PlayersGuild( player.Info ).DeadSaver;
                        player.Message( "You will respawn in {0} seconds", Seconds );
                        Scheduler.NewTask( t => SetPlayerNotDead( player ) ).RunOnce( TimeSpan.FromSeconds( Seconds ) );
                    }
                }
            } catch ( Exception e ) {
                Logger.Log( LogType.Error, e.ToString() );
            }
        }

        private static void StopWoMCrashBlocks( Player player ) {
            List<Vector3I> blocks = new List<Vector3I>();
            Vector3I spawn = player.World.Map.Spawn.ToBlockCoords();
            for ( int x = spawn.X - 2; x <= spawn.X + 2; x++ ) {
                for ( int y = spawn.Y - 2; y <= spawn.Y + 2; y++ ) {
                    for ( int z = player.World.Map.Height - 2; z <= player.World.Map.Height - 1; z++ ) {
                        if ( player.IsOnline ) {
                            if ( player.World != null ) {
                                if ( player.World.Map.GetBlock( x, y, z ) == Block.Air ) {
                                    player.Send( Packets.MakeSetBlock( x, y, z, Block.Glass ) );
                                    blocks.Add( new Vector3I( x, y, z ) );
                                }
                            }
                        }
                    }
                }
            }
            Scheduler.NewTask( t => ResetBlocks( blocks, player ) ).RunOnce( TimeSpan.FromSeconds( 6 ) );
        }

        private static void ResetBlocks( List<Vector3I> Blocks, Player player ) {
            foreach ( Vector3I b in Blocks ) {
                if ( player.IsOnline ) {
                    player.Send( Packets.MakeSetBlock( b, Block.Air ) );
                }
            }
        }

        private static void SetPlayerNotDead( Player player ) {
            try {
                if ( player != null ) {
                    if ( player.IsOnline ) {
                        if ( player.PublicAuxStateObjects.ContainsKey( "dead" ) ) {
                            player.PublicAuxStateObjects.Remove( "dead" );
                            player.TeleportTo( GetRandomPosition( player ) );
                        }
                    }
                }
            } catch ( Exception e ) {
                Logger.Log( LogType.Error, e.ToString() );
            }
        }

        private Position GetRandomPosition( Player player ) {
            World world = player.World;
			if(world == null)
				throw new NullReferenceException("World");
            int x = new Random().Next( world.Map.Width );
            int y = new Random().Next( world.Map.Length );
            int z1 = 0;
            for ( int z = world.Map.Height - 1; z > 0; z-- ) {
                if ( world != null ) {
                    if ( world.Map.GetBlock( x, y, z ) != Block.Air ) {
                        z1 = z + 3;
                        break;
                    }
                }
            }
            return new Position( x, y, z1 + 2 ).ToVector3I().ToPlayerCoords();
        }
    }
}