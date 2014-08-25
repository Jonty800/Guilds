using System;
using System.Collections.Generic;
using System.Linq;
using fCraft;

namespace GuildPlugin {

    public class GuildGame {

        /// <summary>
        /// The 5min game timer to stop the game
        /// </summary>
        public SchedulerTask task;

        /// <summary>
        /// All players in game
        /// </summary>
        private static IEnumerable<Player> InGame = Server.Players.Where( p => p.PublicAuxStateObjects.ContainsKey( "GameEnabled" ) );

        /// <summary>
        /// The instance of this class
        /// </summary>
        private static GuildGame instance;

        public GuildGame() {
            instance = this;
        }

        /// <summary>
        /// The instance of this class
        /// </summary>
        /// <returns>GuildGame class or null if never instanced</returns>
        public static GuildGame GetInstance() {
            return instance;
        }

        /// <summary>
        /// Starts the game
        /// </summary>
        /// <param name="player">Player starting the game</param>
        public void Start( Player player ) {
            if ( instance != null ) {
                player.Message( "Cannot start GuildGame: A game is already in progress" );
                return;
            }
            task = Scheduler.NewTask( t => Stop( Player.Console ) ).RunOnce( TimeSpan.FromMinutes( 5 ) );

            foreach ( Player p in Server.Players ) {
                if ( GuildManager.PlayersGuild( p.Info ) != null ) {
                    //they can play the game, so add them
                    p.PublicAuxStateObjects.Add( "GameEnabled", true );
                }
            }
        }

        /// <summary>
        /// Stops the game
        /// </summary>
        /// <param name="player">Player stopping the game</param>
        public void Stop( Player player ) {
            if ( instance == null ) {
                player.Message( "Cannot stop GuildGame: A game is not in progress" );
                return;
            }
            task.Stop();
            foreach ( Player p1 in InGame ) {
                p1.PublicAuxStateObjects.Remove( "GameEnabled" );
            }
            instance = null;
        }
    }
}