using System;
using System.Collections.Generic;
using System.Threading;
using fCraft;

namespace GuildPlugin {

    public class Bat {
        private Timer _timer;
        private bool _started;
        private Player _player;
        private const int Tick = 125;
        private object _objectLock = new object();

        public Bat( Player player ) {
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
                    if ( _player.PublicAuxStateObjects.ContainsKey( "bat" ) ) {
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

        // Contributed by Conrad "Redshift" Morgan
        public static IEnumerable<Vector3I> LineEnumerator( Vector3I a, Vector3I b ) {
            Vector3I pixel = a;
            Vector3I d = b - a;
            Vector3I inc = new Vector3I( Math.Sign( d.X ),
                                         Math.Sign( d.Y ),
                                         Math.Sign( d.Z ) );
            d = d.Abs();
            Vector3I d2 = d * 2;

            int x, y, z;
            if ( ( d.X >= d.Y ) && ( d.X >= d.Z ) ) {
                x = 0;
                y = 1;
                z = 2;
            } else if ( ( d.Y >= d.X ) && ( d.Y >= d.Z ) ) {
                x = 1;
                y = 2;
                z = 0;
            } else {
                x = 2;
                y = 0;
                z = 1;
            }

            int err1 = d2[y] - d[x];
            int err2 = d2[z] - d[x];
            for ( int i = 0; i < d[x]; i++ ) {
                yield return pixel;
                if ( err1 > 0 ) {
                    pixel[y] += inc[y];
                    err1 -= d2[x];
                }
                if ( err2 > 0 ) {
                    pixel[z] += inc[z];
                    err2 -= d2[x];
                }
                err1 += d2[y];
                err2 += d2[z];
                pixel[x] += inc[x];
            }

            yield return b;
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

                    if ( player.IsOnline ) {
                        if ( player.GunCache.Values.Count > 0 ) {
                            foreach ( Vector3I block in player.GunCache.Values ) {
                                if ( player.IsOnline ) {
                                    player.World.Players.Send( PacketWriter.MakeSetBlock( block.X, block.Y, block.Z, world.Map.GetBlock( block ) ) );
                                    Vector3I removed;
                                    player.GunCache.TryRemove( block.ToString(), out removed );
                                }
                            }
                        }
                    }

                    if ( player.IsOnline ) {
                        //4 is the distance betwen the player and the glass wall
                        Vector3I a = new Vector3I( ( int )( cphi * cksi * 3 - sphi * ( 0.5 + 1 ) - cphi * sksi * ( 0.5 + 1 ) ),
                              ( int )( sphi * cksi * 3 + cphi * ( 0.5 + 1 ) - sphi * sksi * ( 0.5 + 1 ) ),
                              ( int )( sksi * 3 + cksi * ( 0.5 + 1 ) ) );
                        Vector3I b = new Vector3I( ( int )( cphi * cksi * 6 - sphi * ( 0.5 + 1 ) - cphi * sksi * ( 0.5 + 1 ) ),
                              ( int )( sphi * cksi * 6 + cphi * ( 0.5 + 1 ) - sphi * sksi * ( 0.5 + 1 ) ),
                              ( int )( sksi * 6 + cksi * ( 0.5 + 1 ) ) );
                        Vector3I c = new Vector3I( p.X / 32, p.Y / 32, p.Z / 32 - 1 );
                        a += c;
                        b += c;
                        IEnumerable<Vector3I> blocks = LineEnumerator( a, b );
                        foreach ( Vector3I pos in blocks ) {
                            if ( world.Map.GetBlock( pos ) == Block.Air ) {
                                player.World.Players.Send( PacketWriter.MakeSetBlock( pos.X, pos.Y, pos.Z, Block.Log ) );
                                player.GunCache.TryAdd( pos.ToString(), pos );
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