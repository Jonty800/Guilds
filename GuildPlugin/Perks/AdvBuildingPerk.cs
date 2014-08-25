//Copyright Au70 Galaxy <2013> (http://au70.net)

namespace GuildPlugin {

    internal class AdvBuildingPerk : Perk {

        /// <summary>
        /// Construct this class, let the class know what guild it is working with
        /// </summary>
        /// <param name="g">The guild this perk is for</param>
        public AdvBuildingPerk( Guild guild )
            : base( guild ) {
        }

        public override string Help {
            get { return "XP gained from building is increased by an extra 25%"; }
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
            get { return "AdvBuilding"; }
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
            get { return 1000; }
        }

        private float addPer = 0;

        public override void PerformAction() {
            addPer = guild.BuildingModifier * 0.25F;
            guild.BuildingModifier += addPer;
        }

        public override void StopPerk() {
            if ( task != null ) {
                if ( !task.IsStopped ) {
                    task.Stop();
                    guild.BuildingModifier -= addPer;
                }
            }
        }
    }
}