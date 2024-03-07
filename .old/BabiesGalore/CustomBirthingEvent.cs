#if DEBUG
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Events;
using StardewValley.Menus;

namespace BabiesGalore {
  public class CustomBirthingEvent : FarmEvent, INetObject<NetFields> {
    private readonly IModHelper helper;
    private string babyName;
    private bool getBabyName;
    private bool isMale;
    private string message;
    private bool naming;
    private bool playedSound;
    private readonly string soundName = "";
    private int timer;

    public CustomBirthingEvent(IModHelper helper) {
      this.helper = helper;
      this.setUp();
    }

    public NetFields NetFields { get; } = new();

    public bool setUp() {
      Random random = new((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
      //NPC characterFromName = Game1.getCharacterFromName(Game1.player.spouse, true);
      Game1.player.CanMove = false;
      this.isMale = Game1.player.getNumberOfChildren() >= 2 ? random.NextDouble() < 0.5 : Game1.player.getNumberOfChildren() != 0 ? Game1.player.getChildren()[0].Gender == 1 : random.NextDouble() < 0.5;
      this.message = $"A {(this.isMale ? "male" : "female")} baby appeared from nowhere!";
      return false;
    }

    public bool tickUpdate(GameTime time) {
      Game1.player.CanMove = false;
      this.timer += time.ElapsedGameTime.Milliseconds;
      Game1.fadeToBlackAlpha = 1f;
      if (this.timer > 1500 && !this.playedSound && !this.getBabyName) {
        if (this.soundName != null && !this.soundName.Equals("")) {
          Game1.playSound(this.soundName);
          this.playedSound = true;
        }

        if (!this.playedSound && this.message != null && !Game1.dialogueUp && Game1.activeClickableMenu == null) {
          Game1.drawObjectDialogue(this.message);
          Game1.afterDialogues = this.afterMessage;
        }
      }
      else if (this.getBabyName) {
        Multiplayer multiplayer = this.helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

        if (!this.naming) {
          Game1.activeClickableMenu = new NamingMenu(this.returnBabyName, Game1.content.LoadString(this.isMale ? "Strings\\Events:BabyNamingTitle_Male" : "Strings\\Events:BabyNamingTitle_Female"), "");
          this.naming = true;
        }

        if (this.babyName != null && this.babyName != "" && this.babyName.Length > 0) {
          double num = (Game1.player.spouse != null && Game1.player.spouse.Equals("Maru") ? 0.5 : 0.0) + (Game1.player.hasDarkSkin() ? 0.5 : 0.0);
          bool isDarkSkinned = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed).NextDouble() < num;
          string babyName = this.babyName;
          DisposableList<NPC> allCharacters = Utility.getAllCharacters();
          bool flag;
          do {
            flag = false;
            foreach (Character character in allCharacters)
              if (character.Name.Equals((object)babyName)) {
                babyName += " ";
                flag = true;
                break;
              }
          } while (flag);

          Child baby = new(babyName, this.isMale, isDarkSkinned, Game1.player);
          baby.Age = 0;
          baby.Position = new Vector2(16f, 4f) * 64f + new Vector2(0.0f, -24f);
          Utility.getHomeOfFarmer(Game1.player).characters.Add(baby);
          Game1.playSound("smallSelect");
          if (Game1.player.spouse != null) {
            Game1.player.getSpouse().daysAfterLastBirth = 5;
            Game1.player.GetSpouseFriendship().NextBirthingDate = null;
          }

          if (Game1.player.getChildrenCount() == 2) {
            if (Game1.player.spouse != null) {
              Game1.player.getSpouse().shouldSayMarriageDialogue.Value = true;
              Game1.player.getSpouse().currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_SecondChild" + Game1.random.Next(1, 3), true));
            }

            Game1.getSteamAchievement("Achievement_FullHouse");
          }
          else if (Game1.player.spouse != null && Game1.player.getSpouse().isGaySpouse()) {
            Game1.player.getSpouse().currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_Adoption", true, this.babyName));
          }
          else if (Game1.player.spouse != null) {
            Game1.player.getSpouse().currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_FirstChild", true, this.babyName));
          }

          Game1.morningQueue.Enqueue(() => multiplayer.globalChatInfoMessage("Baby", Lexicon.capitalize(Game1.player.Name), Game1.player.spouse, Lexicon.getGenderedChildTerm(this.isMale), Lexicon.getPronoun(this.isMale), baby.displayName));
          if (Game1.keyboardDispatcher != null)
            Game1.keyboardDispatcher.Subscriber = null;
          Game1.player.Position = Utility.PointToVector2(Utility.getHomeOfFarmer(Game1.player).getBedSpot()) * 64f;
          Game1.globalFadeToClear();
          return true;
        }
      }

      return false;
    }

    public void draw(SpriteBatch b) { }

    public void makeChangesToLocation() { }

    public void drawAboveEverything(SpriteBatch b) { }

    public void returnBabyName(string name) {
      this.babyName = name;
      Game1.exitActiveMenu();
    }

    public void afterMessage() {
      this.getBabyName = true;
    }
  }
}
#endif