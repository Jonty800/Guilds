//Copyright Au70 Galaxy <2013> (http://au70.net)

namespace GuildPlugin {

    internal class DeadSaverPerk : Perk {

        /// <summary>
        /// Construct this class, let the class know what guild it is working with
        /// </summary>
        /// <param name="g">The guild this perk is for</param>
        public DeadSaverPerk( Guild guild )
            : base( guild ) {
        }

        public override string Help {
            get { return "Spend less time dead"; }
        }

        /// <summary>
        /// The guild level needed for this perk to run
        /// </summary>
        public override int LevelNeeded {
            get { return 35; }
        }

        /// <summary>
        /// The name of the perk
        /// </summary>
        public override string PerkName {
            get { return "DeadSaver"; }
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

        public override void PerformAction() {
            guild.DeadSaver = 2;
        }

        public override void StopPerk() {
            guild.DeadSaver = 0;
        }
    }
}