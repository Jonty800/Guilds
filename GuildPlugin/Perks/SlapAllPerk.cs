//Copyright Au70 Galaxy <2013> (http://au70.net)
using System;

namespace GuildPlugin {

    internal class SlapAllPerk : Perk {

        /// <summary>
        /// Construct this class, let the class know what guild it is working with
        /// </summary>
        /// <param name="g">The guild this perk is for</param>
        public SlapAllPerk( Guild guild )
            : base( guild ) {
            start = new DateTime();
        }

        public override string Help {
            get { return "Gives the GuildMaster the ability to slap all members of another guild, once per day"; }
        }

        /// <summary>
        /// The guild level needed for this perk to run
        /// </summary>
        public override int LevelNeeded {
            get { return 15; }
        }

        public DateTime start;

        /// <summary>
        /// The name of the perk
        /// </summary>
        public override string PerkName {
            get { return "SlapAllPerk"; }
        }

        /// <summary>
        /// Number of times to repeat what the perk does
        /// </summary>
        public override int TimesToRepeat {
            get { return -1; }
        }

        /// <summary>
        /// Delay between each iteration of PerformAction (ms)
        /// </summary>
        public override int Delay {
            get { return 100; }
        }

        /// <summary>
        /// What the perk actually does. Example: This will print "TestPerk" 100 times with a 1000ms delay between iterations
        /// If Continuous is set to false, this message will show once after the delay
        /// </summary>
        public override void PerformAction() {
        }
    }
}