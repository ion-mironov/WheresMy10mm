using System.Reflection.PortableExecutable;
using System.Security.Cryptography;

namespace WheresMy10mm
	{
	class Program
		{
		static void Main(string[] args)
			{
			Game game = new Game();
			game.Start();
			}
		}

	class Game
		{
		private Player player;

		// Store a dictionary of location names to the actual Location objects.
		private Dictionary<string, Location> locations;

		// Store item descriptions by name
		private Dictionary<string, string> itemDescriptions;

		private Location homeGarage;

		public void Start()
			{
			Console.Title = "Where's My 10mm? - A Text-Based RPG";

			// Initialize player and world
			InitializeGame();

			Console.WriteLine($"Welcome to 'Where's My 10mm?', {player.Name}!\n");
			Console.WriteLine("Type 'help' for a list of commands.");
			LookAround();

			// Main game loop
			while (true)
				{
				Console.Write("\n>> ");
				string input = Console.ReadLine().Trim().ToLower();

				if (string.IsNullOrEmpty(input))
					continue;  // If player just hits Enter, skip.

				// Basic parser: break input by spaces
				string[] parts = input.Split(' ', 2);
				string command = parts[0];
				string argument = parts.Length > 1 ? parts[1] : "";

				// Create descriptions for items
				itemDescriptions = new Dictionary<string, string>
				{
					{ "10mm socket",  "A pristine 10mm socket. It's almost legendary, given how often they go missing." },
					{ "10mm wrench",  "A 10mm wrench. It's a rare find, and now you're hoping to find that elusive 10mm socket." },
					{ "crowbar",	  "A sturdy crowbar. Perfect for prying anything open." },
					{"flashlight",    "A flashlight. It flickers a bit, but it should work well enough to light up a dark room." }
				};

				// Evaluate the command
				switch (command)
					{
					case "help":
						ShowHelp();
						break;

					case "look":
						LookAround();
						break;

					case "move":
						MoveTo(argument);
						break;

					case "inventory":
						ShowInventory();
						break;

					case "take":
						Take(argument);
						break;

					case "examine":
						Examine(argument);
						break;

					case "use":
						UseItem(argument);
						break;

					case "quit":
						Console.WriteLine("Thanks for playing! Remember: keep an eye on that 10mm socket!");
						return; // Exit the game loop

					default:
						Console.WriteLine("Unknown command. Type 'help' for assistance.");
						break;
					}
				}
			}


		// ================================================================================================================ //
		// Set up the player and create a small world with locations
		private void InitializeGame()
			{
			Console.Write("Enter your name, aspiring mechanic: ");
			string nameInput = Console.ReadLine();
			if (string.IsNullOrWhiteSpace(nameInput))
				{
				nameInput = "Rookie"; // Default name if none provided
				}

			player = new Player(nameInput);


			// Locations
			Location bobsPartsStore = new Location(
				"Bob's Auto Parts Store",
				"You're at the local auto parts store. The clerk sighs, 'We haven't had a 10mm in stock for weeks...'"
			);

			Location dredgeTown = new Location(
				"Dredge Town",
					"A rustic town adorned with shops and tiny houses. Maybe one of the local stores will have a 10mm socket."
			);

			Location dredgeAutoParts = new Location(
				"Dredge Auto Parts",
					"A single fluorescent light gently flickers in the small abandoned building, casting its glow across what used to be the tools section. There's hardly anything left, except for one tool that catches your eye: a 10mm wrench. You can hardly believe it."
			);

			Location dredgeHardware = new Location(
				"Dredge Hardware",
					"A dilapidated building. Its sign is hanging by a single bolt, its windows are broken, and its door has wooden planks nailed across it. You peer inside to see that it's completely empty."
			);

			Location dredgeWarehouse = new Location(
				"Dredge Warehouse",
					"A large warehouse filled with crates. Near the entrance is an old man dressed in a dark cloak with its hood hiding most of his face. 'Welcome, traveler, I know why you're here. You seek the elusive 10mm socket. You won't find it here, but I know where you can look. Try the old factory on the outskirts of town.'"
			);

			Location grindstoneGarage = new Location(
				"Grindstone Garage",
				"You're in a cluttered garage. Tools lie everywhere, but oddly enough, every 10mm socket and wrench seems to be missing. Your boss jokes, 'Better go on a quest.', but you'll soon discover it's no joke at all."
			);

			// Save it so other methods (e.g., Take) can teleport the player here later for the finale
			homeGarage = grindstoneGarage;

			Location junkyard = new Location(
				"the Junkyard",
				"A sprawling junkyard full of rusted cars and hidden treasures. If you're lucky, maybe you'll find a 10mm socket somewhere in here."
			);

			Location wasteland = new Location(
				"the Wasteland",
				"A desolate area filled with only a few barren trees and several abandoned stores. You can tell that there's nothing of value or interest to be found here."
			);

			Location oldFactoryExterior = new Location(
				"Old Factory Exterior",
				"A crumbling factory. The walls are covered in graffiti, and the air is thick with dust. You can hear the faint sound of machinery in the distance. The door is rusted shut, but it looks like you can <<PRY>> it open."
			);

			Location oldFactoryLobby = new Location(
				"Old Factory Lobby",
				"You step inside the factory. The air is stale, and the floor is littered with debris. Tattered furniture and faded posters line the interior."
			);
			oldFactoryLobby.IsDoorLocked = true;

			Location oldFactoryWarehouseFloor = new Location(
				"Old Factory Warehouse Floor",
				"You step into the main warehouse. The floor is covered in dust, and there's a faint smell of oil in the air. Some old machines are still here, but they look like they haven't been used in years."
			);

			Location oldFactoryStorage = new Location(
				"Old Factory Storage",
				"A small storage room filled with old boxes and crates. You can see a few tools scattered around, but none are close to what you're looking for. You then notice a strange silhouette behind some boxes."
			);
			oldFactoryStorage.IsDark = true;  // Initially it's dark until the flashlight is used.

			Location storagePortal = new Location(
				"Old Factory Storage Portal",
				"You move the boxes to find that the strange silhouette appears to be a portal; there is a faint border of light around it and it appears to pulse rhythmically. You feel you must be seeing things; why, and how, does such a thing exist?"
			);

			Location alternateOldFactoryStorage = new Location(
				"Alternate Old Factory Storage",
				"You reach out to touch the portal and are greeted with a bright flash of light as a ripple flows out into the air. You then find yourself in a different version of the old factory. The air is thick with a strange energy, and the walls seem to shimmer with an otherworldly light. You've also noticed that the portal is now gone."
			);

			Location alternateOldFactoryWarehouseFloor = new Location(
				"Alternate Old Factory Warehouse Floor",
				"You step into the main warehouse. The floor is immaculate, and there's a strong smell of oil and electricity in the air. The machines are still here, but they look brand new. You also notice that the machines are glowing with a strange energy."
			);

			Location alternateOldFactoryMachineShop = new Location(
				"Alternate Old Factory Machine Shop",
				"You step into the machine shop. The machines are all in perfect condition, and they seem to be humming with energy. In the center of the room is a table with a machine that looks like it was being repaired but never finished. You notice that several bolts are loose. Tightening them with a <<WRENCH>> might do the trick."
			);

			Location alternateFactorySecretRoom = new Location(
				"Secret Room",
				"You step into the dark secret room. As soon as you enter, everything around you lights up, revealing a pedestal in the center of the room with an object on it. As you get closer, the light around you dims ever so slightly, as if allowing you to better focus on what the object is. You can hardly believe it; it's a 10mm socket!"
			);


			// Location connections: "north", "south", etc.
			bobsPartsStore.Exits.Add("west", grindstoneGarage);
			bobsPartsStore.Exits.Add("south", wasteland);

			dredgeTown.Exits.Add("east", junkyard);
			dredgeTown.Exits.Add("south", dredgeHardware);

			dredgeAutoParts.Exits.Add("east", dredgeWarehouse);
			dredgeAutoParts.Exits.Add("west", dredgeHardware);

			dredgeHardware.Exits.Add("north", dredgeTown);
			dredgeHardware.Exits.Add("east", dredgeAutoParts);

			dredgeWarehouse.Exits.Add("west", dredgeAutoParts);
			dredgeWarehouse.Exits.Add("south", oldFactoryExterior);

			grindstoneGarage.Exits.Add("east", bobsPartsStore);
			grindstoneGarage.Exits.Add("south", junkyard);

			junkyard.Exits.Add("north", grindstoneGarage);
			junkyard.Exits.Add("east", wasteland);
			junkyard.Exits.Add("west", dredgeTown);

			oldFactoryExterior.Exits.Add("north", dredgeWarehouse);
			oldFactoryExterior.Exits.Add("inside", oldFactoryLobby);
			
			oldFactoryLobby.Exits.Add("outside", oldFactoryExterior);
			oldFactoryLobby.Exits.Add("to warehouse", oldFactoryWarehouseFloor);

			oldFactoryWarehouseFloor.Exits.Add("to lobby", oldFactoryLobby);
			oldFactoryWarehouseFloor.Exits.Add("to storage", oldFactoryStorage);

			oldFactoryStorage.Exits.Add("to warehouse", oldFactoryWarehouseFloor);
			oldFactoryStorage.Exits.Add("boxes", storagePortal);

			storagePortal.Exits.Add("through portal", alternateOldFactoryStorage);

			alternateOldFactoryStorage.Exits.Add("to warehouse", alternateOldFactoryWarehouseFloor);

			alternateOldFactoryWarehouseFloor.Exits.Add("to machine shop", alternateOldFactoryMachineShop);

			alternateOldFactoryMachineShop.Exits.Add("to warehouse", alternateOldFactoryWarehouseFloor);

			wasteland.Exits.Add("north", bobsPartsStore);
			wasteland.Exits.Add("west", junkyard);


			// Location items
			dredgeAutoParts.Items.Add("wrench");
			oldFactoryExterior.Items.Add("crowbar");
			oldFactoryWarehouseFloor.Items.Add("flashlight");
			alternateFactorySecretRoom.Items.Add("10mm socket");


			// Store in dictionary for easy lookup
			locations = new Dictionary<string, Location>
			{
				{ "Bob's Auto Parts Store", bobsPartsStore },
				{ "Dredge Town", dredgeTown },
				{ "Dredge Auto Parts", dredgeAutoParts },
				{ "Dredge Hardware", dredgeHardware },
				{ "Dredge Warehouse", dredgeWarehouse },
				{ "Grindstone Garage", grindstoneGarage },
				{ "the Junkyard", junkyard },
				{ "the Wasteland", wasteland },
				{ "Old Factory Exterior", oldFactoryExterior },
				{ "Old Factory Lobby", oldFactoryLobby },
				{ "Old Factory Warehouse Floor", oldFactoryWarehouseFloor },
				{ "Old Factory Storage", oldFactoryStorage },
				{ "Old Factory Storage Portal", storagePortal },
				{ "Alternate Old Factory Storage", alternateOldFactoryStorage },
				{ "Alternate Old Factory Warehouse Floor", alternateOldFactoryWarehouseFloor },
				{ "Alternate Old Factory Machine Shop", alternateOldFactoryMachineShop },
				{ "Secret Room", alternateFactorySecretRoom },
			};

			// Player starts in the garage
			player.CurrentLocation = grindstoneGarage;
			}


		// === Help Commands =========================================================================================== //
		private void ShowHelp()
			{
			Console.WriteLine("\nAvailable Commands:");
			Console.WriteLine("  help				- Show this help message");
			Console.WriteLine("  look				- Look around your current location");
			Console.WriteLine("  move <place>			- Move to an adjacent location (e.g. 'move east')");
			Console.WriteLine("  inventory			- Check your inventory");
			Console.WriteLine("  take <item>			- Take an item in the room (e.g. 'take rusty wrench')");
			Console.WriteLine("  examine <item>		- Examine an item in your inventory or the room");
			Console.WriteLine("  use <item>			- Use an item in your inventory (e.g. 'use crowbar')");
			Console.WriteLine("  quit				- Exit the game");
			}


		// === Look Method =========================================================================================== //
		private void LookAround()
			{
			Location current = player.CurrentLocation;
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine($"\nYou're in {current.Name}.");
			Console.ResetColor();

			// If the room is dark, print a generic "can't see" message
			if (current.IsDark)
				{
				Console.WriteLine("It's pitch black in here. You can't see anything without a light.");
				return;
				}

			// Otherwise, show the normal description, items, exits, etc.
			Console.WriteLine(current.Description);

			// Show any items on the ground
			if (current.Items.Any())
				{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("\nItems you see here:");
				Console.ResetColor();
				foreach (string item in current.Items)
					{
					Console.WriteLine($" - {item}");
					}
				}
			else
				{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("\nNo items are visible here.");
				Console.ResetColor();
				}

			// Show exits
			if (current.Exits.Any())
				{
				Console.ForegroundColor = ConsoleColor.DarkYellow;
				Console.WriteLine("\nExits:");
				Console.ResetColor();
				foreach (var direction in current.Exits.Keys)
					{
					Console.WriteLine($" - {direction}");
					}
				}
			else
				{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("There are no exits from this location!");
				Console.ResetColor();
				}
			}


		// === Move Method =========================================================================================== //
		private void MoveTo(string direction)
			{
			if (string.IsNullOrEmpty(direction))
				{
				Console.WriteLine("Move where? Try 'move east', 'move west', etc.");
				return;
				}

			// Check if the direction is valid from this location
			if (player.CurrentLocation.Exits.ContainsKey(direction))
				{
				Location newLocation = player.CurrentLocation.Exits[direction];

				// Check if that location is locked
				if (newLocation.IsDoorLocked)
					{
					Console.WriteLine("The door is rusted shut. Maybe you can <<PRY>> it open.");
					return;
					}

				// Otherwise, move
				player.CurrentLocation = newLocation;
				Console.WriteLine($"\nYou move {direction} and arrive at {newLocation.Name}.");
				// Optionally auto 'look' after moving
				LookAround();
				}
			else
				{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("You can't go that way.");
				Console.ResetColor();
				}
			}


		// === Inventory =============================================================================================== //
		private void ShowInventory()
			{
			if (player.Inventory.Any())
				{
				Console.WriteLine("\nYou are carrying:");
				foreach (var item in player.Inventory)
					{
					Console.WriteLine($" - {item}");
					}
				}
			else
				{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("\nYou're not carrying any items.");
				Console.ResetColor();
				}
			}


		// === Take Method =========================================================================================== //
		private void Take(string argument)
			{
			// We expect something like "take wrench" => argument == "wrench"
			if (string.IsNullOrEmpty(argument))
				{
				Console.WriteLine("Take what?");
				return;
				}

			Location current = player.CurrentLocation;


			// ---------- Finale: grabbing the 10mm socket in the Secret Room ----------
			if (current.Name == "Secret Room" && argument == "10mm socket")
				{
				if (!current.Items.Contains("10mm socket"))
					{
					Console.WriteLine("Strangely, the socket is no longer on the pedestal.");
					return;
					}

				current.Items.Remove("10mm socket");
				player.Inventory.Add("10mm socket");

				Console.WriteLine("\nAs soon as you grasp the 10mm socket, the room blazes with light!");
				Console.WriteLine("A swirling vortex engulfs you and—before you can react—everything goes white…");

				// Remove exits for the epilogue scene
				homeGarage.Exits.Clear();

				// Teleport home
				player.CurrentLocation = homeGarage;

				// Show the new scene
				// ---- custom epilogue output ----
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.WriteLine("\nYou're in Grindstone Garage.");
				Console.ResetColor();

				Console.WriteLine(
					"You find yourself back where it all started—Grindstone Garage.\n" +
					"Tools are scattered just as you left them, but now you clutch the elusive 10mm socket.\n" +
					"You look around in disbelief, wondering if the journey was only a dream.\n");

				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("THE END");
				Console.ResetColor();
				return;
				}


			// Check if the room contains this item
			if (current.Items.Contains(argument))
				{
				// Remove from room, add to player inventory
				current.Items.Remove(argument);
				player.Inventory.Add(argument);
				Console.WriteLine($"You take the {argument} and put it in your toolbox.");
				}
			else
				{
				Console.WriteLine($"There's no '{argument}' here to take.");
				}
			}


		// === Examine Method ======================================================================================== //
		private void Examine(string argument)
			{
			if (string.IsNullOrEmpty(argument))
				{
				Console.WriteLine("Examine what?");
				return;
				}

			Location current = player.CurrentLocation;

			// Check if the item is in the player's inventory
			bool inInventory = player.Inventory.Contains(argument);
			// Or if it's on the ground in the current location
			bool inLocation = current.Items.Contains(argument);

			if (!inInventory && !inLocation)
				{
				Console.WriteLine($"You don't see '{argument}' here or in your inventory.");
				return;
				}

			// If there is a special description for this item, use it
			if (itemDescriptions.ContainsKey(argument))
				{
				Console.WriteLine(itemDescriptions[argument]);
				}
			else
				{
				// A fallback message if there isn't a description
				Console.WriteLine($"You inspect the {argument}. It looks unremarkable.");
				}
			}


		// === Use Method ============================================================================================ //
		private void UseItem(string argument)
			{
			if (string.IsNullOrWhiteSpace(argument))
				{
				Console.WriteLine("Use what?");
				return;
				}

			// Do we have the item?
			if (!player.Inventory.Contains(argument))
				{
				Console.WriteLine($"You don't have a {argument} in your inventory.");
				return;
				}

			// ---------- single‑word / single‑item handling ----------
			switch (argument)
				{
				case "flashlight":
					IlluminateCurrentRoom();
					break;

				case "wrench":
					if (player.CurrentLocation.Name == "Alternate Old Factory Machine Shop")
						FixTheMachine();
					else
						Console.WriteLine("There's nothing here that needs a wrench right now.");
					break;

				case "crowbar":
					if (player.CurrentLocation.Name == "Old Factory Exterior")
						AttemptToCrowbarDoor();
					else
						Console.WriteLine("You swing the crowbar aimlessly—nothing here needs prying.");
					break;

				default:
					Console.WriteLine($"You can’t figure out how to use the {argument} here.");
					break;
				}
			}


		// === Helper Method for flashlight usage ==================================================================== //
		private void IlluminateCurrentRoom()
			{
			Location current = player.CurrentLocation;

			if (!current.IsDark)
				{
				Console.WriteLine("It's already bright enough in here. The flashlight doesn't reveal anything more.");
				return;
				}

			// "Turn the lights on" for this room
			current.IsDark = false;

			Console.WriteLine("You switch on the flashlight. The darkness recedes, revealing the room around you.");

			// Optionally auto-look so the user sees new description immediately
			LookAround();
			}


		// === Helper Method for crowbar usage ======================================================================= //
		private void AttemptToCrowbarDoor()
			{
			Location oldFactoryLobby = locations["Old Factory Lobby"];

			if (!oldFactoryLobby.IsDoorLocked)
				{
				Console.WriteLine("The door is already unlocked. No need to pry it open.");
				return;
				}

			Console.WriteLine("You jam the crowbar into the rusted gap and pull with all your might...");
			Console.WriteLine("With a loud screech of metal, the door breaks free. You can now enter.");

			// Unlock it
			oldFactoryLobby.IsDoorLocked = false;
			}


		// === Helper Method for wrench usage =================================================================== //
		private void FixTheMachine()
			{
			Location machineShop = player.CurrentLocation;

			// If already fixed
			if (machineShop.IsMachineFixed)
				{
				Console.WriteLine("You've already tightened the bolts. The machine is running smoothly.");
				return;
				}

			machineShop.IsMachineFixed = true;

			Console.WriteLine("Tightening the bolts causes the machine to suddenly come to life, humming and churning as it powers on.");
			Console.WriteLine("A moment later, a door magically appears on the far wall.");

			// Create an exit that leads to a new location
			Location alternateFactorySecretRoom = locations["Secret Room"];

			// Name the direction "door"
			machineShop.Exits.Add("inside secret room", alternateFactorySecretRoom);

			// Auto "look" so the user sees the new exit
			LookAround();
			}


		// === Player class ========================================================================================== //
		class Player
			{
			public string Name { get; set; }
			public Location CurrentLocation { get; set; }
			public List<string> Inventory { get; set; }

			public Player(string name)
				{
				Name = name;
				Inventory = new List<string>();
				}
			}


		// === Location class ======================================================================================== //
		class Location
			{
			public string Name { get; set; }
			public string Description { get; set; }
			public List<string> Items { get; set; }
			public Dictionary<string, Location> Exits { get; set; }

			// Simple flag to indicate a blocked entry. Can be expanded as needed.
			public bool IsDoorLocked { get; set; }

			// If this is true, the player can't see anything unless they have a light source. (e.g., flashlight)
			public bool IsDark { get; set; }

			public bool IsMachineFixed { get; set; }

			public Location(string name, string description)
				{
				Name = name;
				Description = description;
				Items = new List<string>();
				Exits = new Dictionary<string, Location>();
				IsDoorLocked = false;		// Default to unlocked unless specified
				IsDark = false;				// Default
				IsMachineFixed = false;		// Default
				}
			}
		}
	}
