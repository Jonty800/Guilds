//Copyright Au70 Galaxy <2013> (http://au70.net)
using fCraft;

namespace GuildPlugin {

    internal class PvP_Perk : Perk {

        public PvP_Perk( Guild guild )
            : base( guild ) {
        }

        public override string Help {
            get { return "Your guild is now allowed to turn on PvP using /Guild PvP"; }
        }

        public override int LevelNeeded {
            get { return 5; }
        }

        public override string PerkName {
            get { return "PvP"; }
        }

        public override int TimesToRepeat {
            get { return -1; }
        }

        public override int Delay {
            get { return 100; }
        }

        public override void PerformAction() {
            guild.PvP = true;
            SetGuildtags();
        }

        public override void StopPerk() {
            guild.PvP = false;
            Started = false;
            SetGuildtags();
        }

        void SetGuildtags() {
            if ( !string.IsNullOrEmpty( guild.Tag ) ) {
                foreach ( string member in guild.Members ) {
                    PlayerInfo pi = PlayerDB.FindPlayerInfoExact( member );
                    if ( pi != null ) {
                        pi.TitleName = GuildManager.GetGuildPvPTagColor(guild) + "[" + Color.StripColors( guild.Tag ) + "]";
                    }
                }
            }
        }
    }
}