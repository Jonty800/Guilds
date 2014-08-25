//Copyright Au70 Galaxy <2013> (http://au70.net)

namespace GuildPlugin {

    internal class ActivePerk : Perk {

        public ActivePerk( Guild guild )
            : base( guild ) {
        }

        public override string Help {
            get { return "Automatically applied: Your guild is now active and can be controlled using /Guild"; }
        }

        public override int LevelNeeded {
            get { return 1; }
        }

        public override string PerkName {
            get { return "Active"; }
        }

        public override int TimesToRepeat {
            get { return -1; }
        }

        public override int Delay {
            get { return 100; }
        }

        public override void PerformAction() {
        }
    }
}