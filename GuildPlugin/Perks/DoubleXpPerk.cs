//Copyright Au70 Galaxy <2013> (http://au70.net)
using System;

namespace GuildPlugin {

    internal class DoubleXpPerk : Perk {

        /// <summary>
        /// Construct this class, let the class know what guild it is working with
        /// </summary>
        /// <param name="g">The guild this perk is for</param>
        public DoubleXpPerk( Guild guild )
            : base( guild ) {
        }

        public override string Help {
            get { return "During 7pm and 9pm every day ($Time), xp gained from building is doubled"; }
        }

        /// <summary>
        /// The guild level needed for this perk to run
        /// </summary>
        public override int LevelNeeded {
            get { return 5; }
        }

        private TimeSpan start = new TimeSpan( 19, 0, 0 ); //7 o'clock
        private TimeSpan end = new TimeSpan( 21, 0, 0 ); //9 o'clock
        private TimeSpan now = DateTime.Now.TimeOfDay;

        public bool CanRecieveMoreXP() {
            return ( ( now > start ) && ( now < end ) );
        }

        /// <summary>
        /// The name of the perk
        /// </summary>
        public override string PerkName {
            get { return "DoubleXp"; }
        }

        /// <summary>
        /// Number of times to repeat what the perk does
        /// </summary>
        public override int TimesToRepeat {
            get { return 0; }
        }

        /// <summary>
        /// Delay between each iteration of PerformAction (ms)
        /// </summary>
        public override int Delay {
            get { return 1000; }
        }

        /// <summary>
        /// What the perk actually does. Example: This will print "TestPerk" 100 times with a 1000ms delay between iterations
        /// If Continuous is set to false, this message will show once after the delay
        /// </summary>
        public override void PerformAction() {
            if ( CanRecieveMoreXP() )
                guild.BuildingModifier = 2;
            else
                guild.BuildingModifier = 1;
        }

        public override void StopPerk() {
            if ( task != null ) {
                if ( !task.IsStopped ) {
                    task.Stop();
                    guild.BuildingModifier = 1;
                }
            }
        }
    }
}