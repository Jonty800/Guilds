using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using fCraft;

namespace GuildPlugin {

    public class Turret {
        public Vector3I[] blocks = new Vector3I[] { };
        public Player player;
        public Guild guild;
        public World world;
        private Vector3I end;
        private Vector3I start;
        private ConcurrentDictionary<string, Vector3I> fireBlocks = new ConcurrentDictionary<string, Vector3I>();
        private SchedulerTask task;
        private DateTime lastTimeShot;

        public Turret( Player p ) {
            player = p;
            world = p.World;
            guild = GuildManager.PlayersGuild( p.Info );
            end = player.Position.ToBlockCoords();
            Vector3I checkPos = new Vector3I( end.X, end.Y, end.Z - 2 );
            Block b = world.Map.GetBlock( checkPos );
            if ( Physics.BlockThrough( b ) ) {
                player.Message( "&WYou can only place a turret on the ground, not in the air" );
                return;
            }
            blocks = new Vector3I[] { new Vector3I( end ),
                new Vector3I( end.X, end.Y, end.Z - 1 ),
                new Vector3I( end.X, end.Y, end.Z + 1 ) };
            start = blocks[0];
            BuildTurret();
            task = Scheduler.NewBackgroundTask( t => StartAI() ).RunForever( TimeSpan.FromMilliseconds( 250 ) );
            AddTurretToList();
            lastTimeShot = DateTime.UtcNow;
        }

        public void AddTurretToList() {
            List<Turret> turrets = Cache.Turrets.ToList();
            turrets.Add( this );
            Cache.Turrets = turrets.ToArray();
        }

        public void RemoveTurretFromList() {
            List<Turret> turrets = Cache.Turrets.ToList();
            turrets.Remove( this );
            Cache.Turrets = turrets.ToArray();
        }

        public void BuildTurret() {
            if ( world != null ) {
                if ( world.IsLoaded ) {
                    world.Map.QueueUpdate( new BlockUpdate( null, new Vector3I( end.X, end.Y, end.Z - 1 ), Block.Log ) );
                    world.Map.QueueUpdate( new BlockUpdate( null, new Vector3I( end.X, end.Y, end.Z ), Block.DoubleStair ) );
                    world.Map.QueueUpdate( new BlockUpdate( null, new Vector3I( end.X, end.Y, end.Z + 1 ), Block.BrownMushroom ) );
                }
            }
        }

        public void Stop() {
            task.Stop();
            Vector3I removed;
            foreach ( Vector3I block in fireBlocks.Values ) {
                if ( world != null ) {
                    world.Players.Send( Packets.MakeSetBlock( block, Block.Air ) );
                }
                fireBlocks.TryRemove( block.ToString(), out removed );
            }
            //remove
            RemoveTurretFromList();
        }

        public bool CanHitPlayer( Vector3I[] blocks ) {
            foreach ( Vector3I pos in blocks ) {
                if ( this.blocks.Contains( pos ) )
                    return true;
                if ( world.Map.GetBlock( pos ) != Block.Air ) {
                    return false;
                }
            }
            return true;
        }

        public void StartAI() {
            if ( world == null ) { Stop(); }
            Vector3I removed;
            foreach ( Vector3I block in fireBlocks.Values ) {
                world.Players.Send( Packets.MakeSetBlock( block, Block.Air ) );
                fireBlocks.TryRemove( block.ToString(), out removed );
            }
            if ( guild.PvP ) {
                int Secs = 5; //* guild.TurretTimeSaver;
                if ( guild.Level > 35 )
                    Secs = 3;
                if ( ( DateTime.UtcNow - lastTimeShot ).TotalSeconds < Secs ) {
                    return; //wait until it can shoot
                }
                bool FoundPlayer = false;
                foreach ( Player p in world.Players ) {
                    if ( GuildManager.PlayerHasGuild( p.Info ) ) {
                        Guild playersGuild = GuildManager.PlayersGuild( p.Info );
                        if ( playersGuild != guild ) {
                            if ( playersGuild.PvP ) {
                                if ( p.Position.DistanceSquaredTo( start.ToPlayerCoords() ) <= 321 * 321 ) { //321 = 20 blocks
                                    if ( !Events.IsInRangeOfSpawnpoint( p.World, p.Position ) ) {
                                        end = p.Position.ToBlockCoords();
                                        FoundPlayer = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                if ( !FoundPlayer )
                    return;
                IEnumerable<Vector3I> blocks = Bat.LineEnumerator( start, end );
                if ( !CanHitPlayer( blocks.ToArray() ) )
                    return;
                if ( blocks.ToArray().Length <= 20 ) {
                    foreach ( Vector3I pos in blocks ) {
                        foreach ( Player p in world.Players ) {
                            System.Threading.Thread.Sleep( 100 );
                            if ( world.Map.GetBlock( pos ) == Block.Air ) {
                                world.Players.Send( PacketWriter.MakeSetBlock( pos.X, pos.Y, pos.Z, Block.Lava ) );
                                fireBlocks.TryAdd( pos.ToString(), pos );
                                int Seconds = 6 - GuildManager.PlayersGuild( p.Info ).DeadSaver;
                                double time = ( DateTime.UtcNow - p.LastTimeKilled ).TotalSeconds;
                                if ( time > Seconds ) {
                                    if ( p.Position.DistanceSquaredTo( pos.ToPlayerCoords() ) <= 49 * 49 ) {
                                        Events.KillPlayer( player, p, KillContext.Turret );
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