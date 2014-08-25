using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using fCraft;

namespace GuildPlugin {

    public class Guild {

        /// <summary>
        /// The name of the guild
        /// </summary>
        public string Name;

        /// <summary>
        /// The guild's tag (Like on LoL)
        /// </summary>
        public string Tag;

        /// <summary>
        /// Members in the guild
        /// </summary>
        public string[] Members;

        /// <summary>
        /// Admins in the guild
        /// </summary>
        public string[] Admins;

        /// <summary>
        /// Is the guild provisional? (Under 4 members)
        /// </summary>
        public bool Provisional {
            get { return this.Members.Length < 4; }
        }

        /// <summary>
        /// Players who have been invited to the guild
        /// </summary>
        public string[] PlayersInvited;

        /// <summary>
        /// The name of the guild master (Player name)
        /// </summary>
        public string GuildMaster;

        /// <summary>
        /// The guilds current rank
        /// </summary>
        public int Level;

        /// <summary>
        /// If the guild has PvP enabled
        /// </summary>
        public bool PvP;

        /// <summary>
        /// Whether the guild is open to invites or not
        /// </summary>
        public bool Open;

        #region XP

        public float? XP;

        public void AddXP( float XpToAdd ) {
            if ( !XP.HasValue )
                XP = 0;
            if ( Provisional )
                return;
            XP += XpToAdd;
            if ( XP >= XPNeededForNextLevel() ) {
                ChangeGuildLevel( Level + 1 );
            }
        }

        public int XPNeededForNextLevel() {
            double total = 0;
            for ( int i = 1; i < Level + 1; i++ ) {
                total += Math.Floor( i + 300 * Math.Pow( 2, i / 7.0 ) );
            }
            int a = 0;
            int b = 0;
            if ( Members != null )
                a = Members.Length;
            foreach ( string PlayerString in Members ) {
                if ( PlayerDB.FindPlayerInfoExact( PlayerString ) != null ) {
                    if ( PlayerDB.FindPlayerInfoExact( PlayerString ).Can( Permission.ReadStaffChat ) ) {
                        b++;
                    }
                }
            }
            return ( int )Math.Floor( total / 4 ) * (( ( a + 1 ) / 2) + b + 1 );
        }

        #endregion XP

        public IEnumerable<Perk> AvailablePerks() {
            return Perks.Where( perk => perk.LevelNeeded <= Level );
        }

        public Perk[] Perks;

        public void FillPerks() {
            Perks = new Perk[]{
               new ActivePerk(this),
               new DoubleXpPerk(this),
               new PvP_Perk(this),
               new SlapAllPerk(this),
               new TheBatPerk(this),
               new DeadSaverPerk(this),
               new TurretPerk(this),
               new ExplodingBugPerk(this),
               new AdvancedTurretsPerk(this)
            };
        }

        public int DeadSaver;

        public float BuildingModifier;

        /// <summary>
        /// Creates a new guild through a command
        /// </summary>
        /// <param name="Name">Name of the guild</param>
        /// <param name="Tag">Tag of the guild</param>
        /// <param name="GuildMaster">The guild creator</param>
        public Guild( String Name, String Tag, String GuildMaster ) {
            this.Name = Name;
            this.Tag = Tag;
            this.GuildMaster = GuildMaster;
            Members = new string[] { GuildMaster };
            Admins = new string[] { };
            if ( Members == null )
                Members = new string[] { };
            if ( PlayersInvited == null )
                PlayersInvited = new string[] { };
            if ( Level == -1 )
                Level = 0;
            if ( XP == -1 )
                XP = 0;
            DeadSaver = 0;
            BuildingModifier = 1;
            FillPerks();
            GuildManager.StartAllGuildPerks( this );
        }

        public Guild() {
            DeadSaver = 0;
            BuildingModifier = 1;
            FillPerks();
            GuildManager.StartAllGuildPerks( this );
        }

        /// <summary>
        /// Sends a message to the guild
        /// </summary>
        /// <param name="OtherGuild">The guild in which the message is coming from [CanBeNull]</param>
        /// <param name="player">Player sending the message</param>
        /// <param name="Message">The message being sent</param>
        public void SendGuildMessage( Player player, String Message ) {
            try {
                string ToSend = "&P[Guild]" + player.ClassyName + "&P: " + Message;
                foreach ( string s in Members ) {
                    PlayerInfo p = PlayerDB.FindPlayerInfoExact( s );
                    if ( p != null ) {
                        if ( p.PlayerObject != null ) {
                            p.PlayerObject.Message( ToSend );
                        }
                    }
                }
            } catch { }
        }

        public void SendGuildNotification( string Message ) {
            try {
                foreach ( string s in Members ) {
                    PlayerInfo p = PlayerDB.FindPlayerInfoExact( s );
                    if ( p != null ) {
                        Player p1 = p.PlayerObject;
                        if ( p1 != null ) {
                            p1.Message( Message );
                        }
                    }
                }
            } catch { }
        }

        /// <summary>
        /// Changes the guilds name (Must be from guildmaster)
        /// </summary>
        /// <param name="player">Player changing the name</param>
        public void ChangeGuildName( Player player, string s ) {
            if ( string.IsNullOrEmpty( s ) ) {
                player.Message( "Name cannot be null or empty" );
                return;
            }
            if ( GuildManager.PlayersGuild( player.Info ) == this ) {
                if ( player.Name != GuildMaster ) {
                    player.Message( "&WCannot change guilds name: You are not the guildmaster" );
                    return;
                }
            }
            if ( File.Exists( "plugins/guilds/" + Name + ".txt" ) ) {
                File.Delete( "plugins/guilds/" + Name + ".txt" );
            }
            Name = s; //now name has changed, new file will be generated using this name
        }

        public void ChangeTagName( Player player, string tag ) {
            if ( string.IsNullOrEmpty( tag ) ) {
                foreach ( string s in Members ) {
                    PlayerInfo pi = PlayerDB.FindPlayerInfoExact( s );
                    if ( pi != null ) {
                        pi.TitleName = null;
                    }
                }
                player.Message( "Guild tag removed" );
                Tag = tag;
                return;
            }
            foreach ( Guild g1 in Cache.Guilds ) {
                if ( g1.Tag.Equals( tag, StringComparison.OrdinalIgnoreCase ) ) {
                    player.Message( "&WError: Selected tag name already exists" );
                    return;
                }
            }
            if ( tag.Length > 6 ) {
                player.Message( "&WError: Guild tag cannot exceed 6 characters" );
                return;
            }
            if ( tag.Contains( " " ) ) {
                player.Message( "&WError: Guild tag contains too many spaces (More than 0)" );
                return;
            }
            tag = Color.StripColors( tag );
            player.Message( "Tag changed from {0} to {1}", string.IsNullOrEmpty( Tag ) ? "'No Tag'" : Tag, tag );
            Tag = tag;
            if ( PvP ) {
                foreach ( string s in Members ) {
                    PlayerInfo pi = PlayerDB.FindPlayerInfoExact( s );
                    if ( pi != null ) {
                        pi.TitleName = Color.Red + "[" + Color.StripColors( Tag ) + "]";
                    }
                }
            } else {
                foreach ( string s in Members ) {
                    PlayerInfo pi = PlayerDB.FindPlayerInfoExact( s );
                    if ( pi != null ) {
                        pi.TitleName = Color.Green + "[" + Color.StripColors( Tag ) + "]";
                    }
                }
            }
        }

        /// <summary>
        /// Changes the guilds tag (Must be from guildmaster)
        /// </summary>
        /// <param name="player">Player changing the tag</param>
        public void ChangeGuildTag( Player player ) {
            if ( GuildManager.PlayersGuild( player.Info ) == this ) {
                if ( player.Name != GuildMaster ) {
                    player.Message( "&WCannot change guilds tag: You are not the guildmaster" );
                    return;
                }
            }
        }

        public void ChangeGuildLevel( int NewLevel ) {
            Level = NewLevel;
            SendGuildNotification( "&HYour guild has now reached level " + NewLevel + "!" );
            foreach ( Perk perk in AvailablePerks() ) {
                try {
                    if ( perk.LevelNeeded == Level ) {
                        if ( !perk.Started )
                            perk.StartPerk();
                    }
                } catch { }
            }
        }

        public void RemoveMember( PlayerInfo player ) {
            String S = player.Name;
            //remove member
            bool RemovedPlayer = false;
            List<string> m = Members.ToList();
            if ( m.Contains( S ) ) {
                m.Remove( S );
                Members = m.ToArray();
                RemovedPlayer = true;
            }

            //remove admin
            List<string> lst = Admins.ToList();
            if ( lst.Contains( S ) ) {
                lst.Remove( S );
                Admins = lst.ToArray();
                RemovedPlayer = true;
            }

            if ( !RemovedPlayer ) {
                throw new Exception( "Unable to make player leave guild: You were not found as a member" );
            }

            //remove and stop turrets
            List<Turret> tlst = Cache.Turrets.ToList();
            foreach ( Turret t in Cache.Turrets ) {
                if ( t.player.Name == player.Name ) {
                    t.Stop();
                    tlst.Remove( t );
                }
            }
            Cache.Turrets = tlst.ToArray();
            if ( player.IsOnline ) {
                player.PlayerObject.Message( "&HYou have been removed from your guild" );
            }
            SendGuildNotification( String.Format( "{0}&H has left your guild", player.ClassyName ) );
            player.TitleName = null;

            //remove exploding bug
            if ( player.IsOnline ) {
                Object o;
                if ( player.PlayerObject.PublicAuxStateObjects.TryGetValue( "bug", out o ) ) {
                    ExplodingBug bug = ( ExplodingBug )o;
                    bug.Stop();
                }
            }

            if ( Members.Length != 0 ) {
                if ( player.Name == GuildMaster ) {
                    if ( Admins.Length > 0 ) {
                        GuildMaster = Admins[new Random().Next( Admins.Length )]; //assign new guildmaster
                    } else {
                        GuildMaster = Members[new Random().Next( Members.Length )];
                    }
                    PlayerInfo pi = PlayerDB.FindPlayerInfoExact( GuildMaster );
                    if ( pi != null ) {
                        if ( pi.IsOnline ) {
                            pi.PlayerObject.Message( "&WYou are now the guildmaster of " + Name );
                        }
                    }
                }
            }
            if ( Members.Length == 0 ) {
                List<Guild> g = Cache.Guilds.ToList();
                g.Remove( this );
                Cache.Guilds = g.ToArray();
                foreach ( Perk perk in Perks ) { perk.StopPerk(); }
                GuildConfig.RemoveGuildFile( this );
            }
        }

        /// <summary>
        /// Adds the player to the guild
        /// </summary>
        /// <param name="player">Player in question</param>
        public void AcceptInvitationAndJoinGuild( Player player ) {
            if ( !Open ) {
                if ( !PlayersInvited.Contains( player.Name ) ) {
                    player.Message( "&WYou have not been invited to this guild. Ask a member for an invite!" );
                    return;
                }
            }
            player.Message( "&HYou have joined " + Name );
            SendGuildNotification( String.Format( "{0}&S has joined your guild!", player.ClassyName ) );
            List<string> m = Members.ToList();
            m.Add( player.Name );
            Members = m.ToArray();
            if ( Level == 0 ) {
                if ( Members.Length >= 4 ) {
                    ChangeGuildLevel( 1 );
                }
            }
            //remove from invited
            List<string> i = PlayersInvited.ToList();
            i.Remove( player.Name );
            PlayersInvited = i.ToArray();

            if ( PvP ) {
                player.Info.TitleName = Color.Red + "[" + Color.StripColors( Tag ) + "]";
            }
        }

        public override string ToString() {
            return this.Name;
        }

        #region Saving

        private static string FilePath = "plugins/Guilds/";

        public void Save() {
            string FileFullPath = FilePath + Name + ".txt";
            string[] contents = new string[]{
                "name:" + Name,
                "tag:" + String.Format(string.IsNullOrEmpty(Tag) == true ? "" : Tag),
                "guildmaster:" + GuildMaster,
                "members:" + StringJoin(Members),
                "admins:" + StringJoin(Admins),
                "invitedmembers:" + StringJoin(PlayersInvited),
                "open:" + Open,
                "level:" + Level,
                "currentexp:" + XP,
                "pvp:" + PvP.ToString()
            };
            try {
                File.WriteAllLines( FileFullPath, contents, Encoding.Default );
            } catch ( Exception e ) {
                Logger.Log( LogType.Error, "GuildFileSave: " + e.Message );
            }
        }

        private string StringJoin( string[] collection ) {
            if ( collection == null )
                return "";
            if ( collection.Length == 1 )
                return collection[0];
            try { return collection.JoinToString(); } catch { return ""; }
        }

        #endregion Saving
    }
}