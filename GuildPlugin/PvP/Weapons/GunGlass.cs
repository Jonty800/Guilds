using System;
using System.Threading;
using fCraft;

namespace GuildPlugin {

    public class GunGlass {
        private Timer _timer;
        private bool _started;
        private Player _player;
        private const int Tick = 125;
        private object _objectLock = new object();

        public GunGlass( Player player ) {
            _player = player;
            _started = false;
            _timer = new Timer( callback, null, Timeout.Infinite, Timeout.Infinite );
        }

        public void Start() {
            lock ( _objectLock ) {
                if ( !_started ) {
                    _started = true;
                    _timer.Change( 0, Timeout.Infinite );
                }
            }
        }

        public void Stop() {
            lock ( _objectLock ) {
                _started = false;
                _timer.Change( Timeout.Infinite, Timeout.Infinite );
            }
        }

        private void callback( object state ) {
            try {
                if ( _player.IsOnline && _player != null ) {
                    if ( _player.GunMode ) {
                        GunMove( _player );
                    } else {
                        Stop();
                    }
                } else {
                    Stop();
                }
            } catch ( Exception e ) {
                Logger.Log( LogType.Error, "GunGlassTimer: " + e );
            }

            lock ( _objectLock ) {
                if ( _started )
                    _timer.Change( Tick, Timeout.Infinite );
            }
        }

        public static void GunMove( Player player ) {
            World world = player.World;
            if ( null == world )
                return;
            try {
                if ( null == world.Map )
                    return;
                if ( player.IsOnline ) {
                    Position p = player.Position;
                    double ksi = 2.0 * Math.PI * ( -player.Position.L ) / 256.0;
                    double phi = 2.0 * Math.PI * ( player.Position.R - 64 ) / 256.0;
                    double sphi = Math.Sin( phi );
                    double cphi = Math.Cos( phi );
                    double sksi = Math.Sin( ksi );
                    double cksi = Math.Cos( ksi );

                    if ( player.GunCache.Values.Count > 0 ) {
                        foreach ( Vector3I block in player.GunCache.Values ) {
                            if ( player.IsOnline ) {
                                player.Send( PacketWriter.MakeSetBlock( block.X, block.Y, block.Z, world.Map.GetBlock( block ) ) );
                                Vector3I removed;
                                player.GunCache.TryRemove( block.ToString(), out removed );
                            }
                        }
                    }

                    for ( int y = -1; y < 2; ++y ) {
                        for ( int z = -1; z < 2; ++z ) {
                            if ( player.IsOnline ) {
                                //4 is the distance betwen the player and the glass wall
                                Vector3I glassBlockPos = new Vector3I( ( int )( cphi * cksi * 4 - sphi * ( 0.5 + y ) - cphi * sksi * ( 0.5 + z ) ),
                                      ( int )( sphi * cksi * 4 + cphi * ( 0.5 + y ) - sphi * sksi * ( 0.5 + z ) ),
                                      ( int )( sksi * 4 + cksi * ( 0.5 + z ) ) );
                                glassBlockPos += p.ToBlockCoords();
                                if ( world.Map.GetBlock( glassBlockPos ) == Block.Air ) {
                                    player.Send( PacketWriter.MakeSetBlock( glassBlockPos.X, glassBlockPos.Y, glassBlockPos.Z, Block.Glass ) );
                                    player.GunCache.TryAdd( glassBlockPos.ToString(), glassBlockPos );
                                }
                            }
                        }
                    }
                }
            } catch ( Exception ex ) {
                Logger.Log( LogType.SeriousError, "GunGlass: " + ex );
            }
        }
    }
}