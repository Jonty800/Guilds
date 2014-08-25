using System;
using System.Collections.Generic;
using System.Linq;
using fCraft;

namespace GuildPlugin {

    public class ExplodingBug {
        public Vector3I block;
        public Player player;
        public Guild guild;
        public World world;
        private Vector3I end;
        private SchedulerTask task;
        private SchedulerTask endTask;

        private bool Started = false;

        public ExplodingBug( Player p ) {
            player = p;
            world = player.World;
            guild = GuildManager.PlayersGuild( player.Info );
            end = player.Position.ToBlockCoords();
            block = new Vector3I( end.X, end.Y, end.Z - 1 );
            Started = true;
            task = Scheduler.NewBackgroundTask( t => StartAI() ).RunForever( TimeSpan.FromMilliseconds( 230 ) );
            endTask = Scheduler.NewTask( t => Stop() ).RunOnce( TimeSpan.FromSeconds( 25 ) );
            player.PublicAuxStateObjects.Add( "bug", this );
        }

        public void Stop() {
            Started = false;
            world.Players.Send( Packets.MakeSetBlock( block, Block.Air ) );
            task.Stop();
            player.PublicAuxStateObjects.Remove( "bug" );
        }

        public void StartAI() {
            if ( !Started )
                return;
            if ( !guild.PvP )
                Stop();
            world.Players.Send( Packets.MakeSetBlock( block, Block.Green ) );
            bool FoundPlayer = false;
            foreach ( Player p in world.Players ) {
                if ( p != player ) {
                    if ( p.Position.DistanceSquaredTo( block.ToPlayerCoords() ) <= 513 * 513 ) { //32 blocks
                        if ( !Events.IsInRangeOfSpawnpoint( p.World, p.Position ) ) {
                            end = p.Position.ToBlockCoords();
                            FoundPlayer = true;
                            break;
                        }
                    }
                }
            }
            if ( !FoundPlayer )
                return;
            IEnumerable<Vector3I> blocks = Bat.LineEnumerator( block, end );
            if ( blocks.ToArray().Length > 1 ) {
                Vector3I pos = blocks.ToArray()[1];
                foreach ( Player p in world.Players ) {
                    if ( p != player ) {
                        if ( GuildManager.PlayerHasGuild( p.Info ) ) {
                            if ( GuildManager.PlayersGuild( p.Info ) != guild ) {
                                if ( GuildManager.PlayersGuild( p.Info ).PvP ) {
                                    if ( player.World.Map.GetBlock( pos ) == Block.Air ) {
                                        world.Players.Send( Packets.MakeSetBlock( block, Block.Air ) );
                                        world.Players.Send( PacketWriter.MakeSetBlock( pos.X, pos.Y, pos.Z, Block.Green ) );
                                        block = pos;
                                        int Seconds = 6 - GuildManager.PlayersGuild( p.Info ).DeadSaver;
                                        double time = ( DateTime.UtcNow - p.LastTimeKilled ).TotalSeconds;
                                        if ( time > Seconds ) {
                                            if ( p.Position.DistanceSquaredTo( pos.ToPlayerCoords() ) <= 49 * 49 ) {
                                                Events.KillPlayer( player, p, KillContext.ExplodingBug );
                                                Explode();
                                                return;
                                            }
                                        }
                                    }
                                } else
                                    Explode();
                            }
                        }
                    }
                }
            }
        }

        private void Explode() {
            Stop();
        }
    }
}