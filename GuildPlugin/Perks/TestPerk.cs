//Copyright Au70 Galaxy <2013> (http://au70.net)
using fCraft;

namespace GuildPlugin {

    internal class TestPerk : Perk {

        /// <summary>
        /// Construct this class, let the class know what guild it is working with
        /// </summary>
        /// <param name="g">The guild this perk is for</param>
        public TestPerk( Guild guild )
            : base( guild ) {
        }

        public override string Help {
            get { return "Test Perk"; }
        }

        /// <summary>
        /// The guild level needed for this perk to run
        /// </summary>
        public override int LevelNeeded {
            get { return 1; }
        }

        /// <summary>
        /// The name of the perk
        /// </summary>
        public override string PerkName {
            get { return "Test Perk"; }
        }

        /// <summary>
        /// Number of times to repeat what the perk does
        /// </summary>
        public override int TimesToRepeat {
            get { return 100; }
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
            Server.Players.Message( "TestPerk" );
        }
    }
}