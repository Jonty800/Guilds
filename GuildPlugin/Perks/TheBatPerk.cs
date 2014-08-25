//Copyright Au70 Galaxy <2013> (http://au70.net)

namespace GuildPlugin {

    internal class TheBatPerk : Perk {

        public TheBatPerk( Guild guild )
            : base( guild ) {
        }

        public override string Help {
            get { return "Automatically applied: Your guild now has access to a bat! Use /Guild weapon Bat and kill some rivals!"; }
        }

        public override int LevelNeeded {
            get { return 10; }
        }

        public override string PerkName {
            get { return "TheBat"; }
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