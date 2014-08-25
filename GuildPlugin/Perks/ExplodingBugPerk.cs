//Copyright Au70 Galaxy <2013> (http://au70.net)
using fCraft;

namespace GuildPlugin {

    internal class ExplodingBugPerk : Perk {
        private Player player;
        private ExplodingBug eb;

        public ExplodingBugPerk( Guild guild )
            : base( guild ) {
        }

        public override string Help {
            get { return "Automatically applied: Your guild now place down exploding bugs, which follow and kill nearby enemies"; }
        }

        public override int LevelNeeded {
            get { return 30; }
        }

        public override string PerkName {
            get { return "ExplodingBugPerk"; }
        }

        public override int TimesToRepeat {
            get { return -1; }
        }

        public override int Delay {
            get { return 100; }
        }

        public override void StopPerk() {
            if ( task != null ) {
                if ( !task.IsStopped ) {
                    task.Stop();
                    Started = false;
                }
                eb.Stop();
            }
        }

        public override void PerformAction() {
        }

        public void SpawnBug( Player p ) {
            this.player = p;
            eb = new ExplodingBug( p );
        }
    }
}