using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Events;
using StardewValley.Menus;
using System;
using System.Linq;

namespace BabyGenderInterface
{
    public class CustomBirthingEvent : FarmEvent, INetObject<NetFields>
    {
		private int timer;
		private string soundName;
		private string message;
		private string babyName;
		private bool playedSound;
		private bool isMale;
		private bool getBabyName;
		private TextBox babyNameBox;
		private ClickableTextureComponent okButton;
		private ClickableTextureComponent maleButton;
        private ClickableTextureComponent femaleButton;

        public NetFields NetFields { get; } = new NetFields();

        public CustomBirthingEvent()
        {
            this.babyNameBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), (Texture2D)null, Game1.smallFont, Game1.textColor)
            {
                X = Game1.graphics.GraphicsDevice.Viewport.Width / 2 - Game1.tileSize * 4,
                Y = Game1.graphics.GraphicsDevice.Viewport.Height / 2 + Game1.tileSize,
                Width = Game1.tileSize * 3
            };
            this.okButton = new ClickableTextureComponent(new Rectangle(this.babyNameBox.X + this.babyNameBox.Width + Game1.tileSize / 2, this.babyNameBox.Y - Game1.tileSize / 8, Game1.tileSize, Game1.tileSize), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false);
            this.maleButton = new ClickableTextureComponent("Male", new Rectangle(this.okButton.bounds.X + this.okButton.bounds.Width + Game1.tileSize / 2, this.okButton.bounds.Y, Game1.tileSize, Game1.tileSize), (string)null, "Male", Game1.mouseCursors, new Rectangle(128, 192, 16, 16), (float)Game1.pixelZoom, false);
            this.femaleButton = new ClickableTextureComponent("Female", new Rectangle(this.maleButton.bounds.X + this.maleButton.bounds.Width + Game1.tileSize / 4, this.maleButton.bounds.Y, Game1.tileSize, Game1.tileSize), (string)null, "Female", Game1.mouseCursors, new Rectangle(144, 192, 16, 16), (float)Game1.pixelZoom, false);
        }

        public bool setUp()
        {
            Random random = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
            this.isMale = Game1.player.getNumberOfChildren() != 0 ? Game1.player.getChildren()[0].Gender == 1 : random.NextDouble() < 0.5;
            Game1.player.CanMove = false;
            try
            {
                NPC spouse = Game1.player.getSpouse();
                if (spouse.isGaySpouse())
                    this.message = "The stork has brought you a beautiful baby";
                else
					this.message = "A beautiful baby has entered the world!";
			}
            catch (Exception ex)
            {
                Log.error($"Something went wrong! Details:\n{ex}");
            }
            return false;
        }

        public void afterMessage()
        {
            this.getBabyName = true;
			this.babyNameBox.SelectMe();
		}

		public void returnBabyName(string name)
		{
			this.babyName = name;
			Game1.exitActiveMenu();
		}

		public bool tickUpdate(GameTime time)
		{
			Game1.player.CanMove = false;
			this.timer += time.ElapsedGameTime.Milliseconds;
			Game1.fadeToBlackAlpha = 1f;
			if (this.timer > 1500 && !this.playedSound && !this.getBabyName)
			{
				if (this.soundName != null && !this.soundName.Equals(""))
				{
					Game1.playSound(this.soundName);
					this.playedSound = true;
				}
				if (!this.playedSound && this.message != null && (!Game1.dialogueUp && Game1.activeClickableMenu == null))
				{
					Game1.drawObjectDialogue(this.message);
					Game1.afterDialogues = new Game1.afterFadeFunction(this.afterMessage);
				}
			}
			else if (this.getBabyName)
			{
				int oldMouseX = Game1.getOldMouseX();
				int oldMouseY = Game1.getOldMouseY();
				if (Game1.oldMouseState.LeftButton == ButtonState.Pressed)
				{
					if (this.maleButton.containsPoint(oldMouseX, oldMouseY))
					{
						this.isMale = true;
						this.maleButton.scale -= 0.5f;
						this.maleButton.scale = Math.Max(3.5f, this.maleButton.scale);
					}
					if (this.femaleButton.containsPoint(oldMouseX, oldMouseY))
					{
						this.isMale = false;
						this.femaleButton.scale -= 0.5f;
						this.femaleButton.scale = Math.Max(3.5f, this.femaleButton.scale);
					}
					//if (!this.naming)
					//{
					//	Game1.activeClickableMenu = (IClickableMenu)new NamingMenu(new NamingMenu.doneNamingBehavior(this.returnBabyName), Game1.content.LoadString(this.isMale ? "Strings\\Events:BabyNamingTitle_Male" : "Strings\\Events:BabyNamingTitle_Female"), "");
					//	this.naming = true;
					//	this.babyNameBox.SelectMe();
					//}
					if (this.okButton.containsPoint(oldMouseX, oldMouseY) && this.babyNameBox != null && this.babyNameBox.Text != "" && this.babyNameBox.Text.Length > 0)
					{
						double num = (Game1.player.spouse.Equals("Maru") ? 0.5 : 0.0) + (Game1.player.hasDarkSkin() ? 0.5 : 0.0);
						bool isDarkSkinned = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed).NextDouble() < num;
						string babyName = this.babyNameBox.Text;
						foreach (Character allCharacter in Utility.getAllCharacters())
						{
							if (allCharacter.Name.Equals((object)babyName))
							{
								babyName += " ";
								break;
							}
						}
						Child child = new Child(babyName, this.isMale, isDarkSkinned, Game1.player);
						child.Age = 0;
						child.Position = new Vector2(16f, 4f) * 64f + new Vector2(0.0f, -24f);
						Utility.getHomeOfFarmer(Game1.player).characters.Add((NPC)child);
						Game1.playSound("smallSelect");
						Game1.player.getSpouse().daysAfterLastBirth = 5;
						Game1.player.GetSpouseFriendship().NextBirthingDate = (WorldDate)null;
						if (Game1.player.getChildrenCount() == 2)
						{
							NPC spouse = Game1.player.getSpouse();
							string s;
							if (Game1.random.NextDouble() >= 0.5)
							{
								if (Game1.player.getSpouse().Gender != 0)
									s = (Game1.content.LoadString("Data\\ExtraDialogue:NewChild_SecondChild2").Split('/')).Last<string>();
								else
									s = (Game1.content.LoadString("Data\\ExtraDialogue:NewChild_SecondChild2").Split('/')).First<string>();
							}
							else if (Game1.player.getSpouse().Gender != 0)
								s = (Game1.content.LoadString("Data\\ExtraDialogue:NewChild_SecondChild1").Split('/')).Last<string>();
							else
								s = (Game1.content.LoadString("Data\\ExtraDialogue:NewChild_SecondChild1").Split('/')).First<string>();
							spouse.setNewDialogue(s, false, false);
							Game1.getSteamAchievement("Achievement_FullHouse");
						}
						else if (Game1.player.getSpouse().isGaySpouse())
						{
							NPC spouse = Game1.player.getSpouse();
							string s;
							if (Game1.player.getSpouse().Gender != 0)
								s = (Game1.content.LoadString("Data\\ExtraDialogue:NewChild_Adoption", (object)this.babyName).Split('/')).Last<string>();
							else
								s = (Game1.content.LoadString("Data\\ExtraDialogue:NewChild_Adoption", (object)this.babyName).Split('/')).First<string>();
							spouse.setNewDialogue(s, false, false);
						}
						else
						{
							NPC spouse = Game1.player.getSpouse();
							string s;
							if (Game1.player.getSpouse().Gender != 0)
								s = (Game1.content.LoadString("Data\\ExtraDialogue:NewChild_FirstChild", (object)this.babyName).Split('/')).Last<string>();
							else
								s = (Game1.content.LoadString("Data\\ExtraDialogue:NewChild_FirstChild", (object)this.babyName).Split('/')).First<string>();
							spouse.setNewDialogue(s, false, false);
						}
						if (Game1.keyboardDispatcher != null)
							Game1.keyboardDispatcher.Subscriber = (IKeyboardSubscriber)null;
						Game1.player.Position = Utility.PointToVector2(Utility.getHomeOfFarmer(Game1.player).getBedSpot()) * 64f;
						Game1.globalFadeToClear((Game1.afterFadeFunction)null, 0.02f);
						return true;
					}
				}
				else
				{
					if (this.maleButton.containsPoint(oldMouseX, oldMouseY))
						this.maleButton.scale = Math.Min(this.maleButton.scale + 0.02f, this.maleButton.baseScale + 0.1f);
					else
						this.maleButton.scale = Math.Max(this.maleButton.scale - 0.02f, this.maleButton.baseScale);
					if (this.femaleButton.containsPoint(oldMouseX, oldMouseY))
						this.femaleButton.scale = Math.Min(this.femaleButton.scale + 0.02f, this.femaleButton.baseScale + 0.1f);
					else
						this.femaleButton.scale = Math.Max(this.femaleButton.scale - 0.02f, this.femaleButton.baseScale);
				}
			}
			return false;
		}

		public void draw(SpriteBatch b)
        {
        }

        public void makeChangesToLocation()
        {
        }

        public void drawAboveEverything(SpriteBatch b)
        {
            if (!this.getBabyName)
                return;
            string message = Game1.content.LoadString(this.isMale ? "Strings\\Events:BabyNamingTitle_Male" : "Strings\\Events:BabyNamingTitle_Female");
            Color black = Color.Black;
            Color white = Color.White;
            Viewport viewport = Game1.graphics.GraphicsDevice.Viewport;
            double num1 = (double)(viewport.Width / 2 - Game1.tileSize * 4);
            viewport = Game1.graphics.GraphicsDevice.Viewport;
            double num2 = (double)(viewport.Height / 2 - Game1.tileSize * 2);
            Vector2 position = new Vector2((float)num1, (float)num2);
            Game1.drawWithBorder(message, black, white, position);
            Game1.drawDialogueBox(this.babyNameBox.X - Game1.tileSize / 2, this.babyNameBox.Y - Game1.tileSize * 3 / 2, this.babyNameBox.Width + Game1.tileSize, this.babyNameBox.Height + Game1.tileSize * 2, false, true, (string)null, false);
            this.babyNameBox.Draw(b);
            this.okButton.draw(b);
            this.maleButton.draw(b);
            this.femaleButton.draw(b);
            if (this.isMale)
                b.Draw(Game1.mouseCursors, this.maleButton.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 34, -1, -1)), Color.White);
            else
                b.Draw(Game1.mouseCursors, this.femaleButton.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 34, -1, -1)), Color.White);
		}
    }
}