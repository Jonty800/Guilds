//Copyright Au70 Galaxy <2013> (http://au70.net)

namespace GuildPlugin {

    internal class GuardianPerk : Perk {

        public GuardianPerk( Guild guild )
            : base( guild ) {
        }

        public override string Help {
            get { return "Can spawn a 5 minute guardian which follows you around killing nearby enemies (30 minute cooldown) using /Guild Guardian"; }
        }

        public override int LevelNeeded {
            get { return 20; }
        }

        public override string PerkName {
            get { return "Guardian"; }
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