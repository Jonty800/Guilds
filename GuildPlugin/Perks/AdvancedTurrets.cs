//Copyright Au70 Galaxy <2013> (http://au70.net)

namespace GuildPlugin {

    internal class AdvancedTurretsPerk : Perk {

        public AdvancedTurretsPerk( Guild guild )
            : base( guild ) {
        }

        public override string Help {
            get { return "Automatically applied: Your turrets can now fire faster"; }
        }

        public override int LevelNeeded {
            get { return 35; }
        }

        public override string PerkName {
            get { return "AdvancedTurrets"; }
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