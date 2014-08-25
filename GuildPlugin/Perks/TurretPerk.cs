//Copyright Au70 Galaxy <2013> (http://au70.net)
using fCraft;

namespace GuildPlugin {

    internal class TurretPerk : Perk {
        public Player player;
        public Turret turret;

        public TurretPerk( Guild guild )
            : base( guild ) {
        }

        public void AddTurret( Player p ) {
            this.player = p;
            turret = new Turret( p );
        }

        public override string Help {
            get { return "Automatically applied: Your guild now place down turrets, which will fire lazers at nearby enemies"; }
        }

        public override int LevelNeeded {
            get { return 25; }
        }

        public override string PerkName {
            get { return "TurretPerk"; }
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
            }
            if ( turret != null ) {
                turret.Stop();
            }
        }

        public override void PerformAction() {
        }

        public void CreateTurret() {
        }
    }
}