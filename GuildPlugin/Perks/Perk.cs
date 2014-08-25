//Copyright Au70 Galaxy <2013> (http://au70.net)
using System;
using System.Linq;
using fCraft;

namespace GuildPlugin {

    public abstract class Perk {
        public Guild guild;

        public bool Started;

        protected Perk( Guild Guild ) {
            guild = Guild;
            Started = false;
        }

        public abstract int LevelNeeded {
            get;
        }

        public virtual string Help {
            get { return "No Help Available"; }
        }

        public abstract string PerkName {
            get;
        }

        public SchedulerTask task;

        public abstract int TimesToRepeat {
            get;
        }

        /// <summary>
        /// In miliseconds
        /// </summary>
        public abstract int Delay {
            get;
        }

        public void StartPerk() {
            if ( !Started ) {
                Started = true;
                if ( TimesToRepeat == -1 ) {
                    task = Scheduler.NewTask( t => PerformAction() ).RunOnce( TimeSpan.FromMilliseconds( Delay ) );
                } else if ( TimesToRepeat == 0 ) {
                    task = Scheduler.NewTask( t => PerformAction() ).RunForever( TimeSpan.FromMilliseconds( Delay ) );
                } else {
                    task = Scheduler.NewTask( t => PerformAction() ).RunRepeating( TimeSpan.FromMilliseconds( Delay ), TimeSpan.FromMilliseconds( Delay ), TimesToRepeat );
                }
            }
        }

        public virtual void StopPerk() {
            if ( task != null ) {
                if ( !task.IsStopped ) {
                    task.Stop();
                    Started = false;
                }
            }
        }

        public override string ToString() {
            return PerkName;
        }

        public abstract void PerformAction();

        public static string GetHelpSection( Perk perk ) {
            return perk.Help +
                "\nThis perk requires level " + perk.LevelNeeded;
        }

        public static bool GuildHasPerk( Guild guild, Perk perk ) {
            return guild.AvailablePerks().Contains( perk );
        }
    }
}