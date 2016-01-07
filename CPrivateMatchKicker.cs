using System;
using System.Collections.Generic;
using PRoCon.Core;
using PRoCon.Core.Players;
using PRoCon.Core.Plugin;

namespace PRoConEvents
{
	public class PrivateMatchKicker : PRoConPluginAPI, IPRoConPluginInterface
	{
		private static readonly string className = typeof(CPrivateMatchKicker).Name;

		private HashSet<string> reservedPlayers = null;
		private bool isPluginEnabled = false;
		
		private enumBoolYesNo ignoreCase = enumBoolYesNo.No;

		public string GetPluginName()
		{
			return "Private Match Kicker";
		}

		public string GetPluginVersion()
		{
			return "1.4.0.0";
		}

		public string GetPluginAuthor()
		{
			return "aidinabedi";
		}

		public string GetPluginWebsite()
		{
			return "";
		}

		public string GetPluginDescription()
		{
			return @"Kicks a player if they are not included in the reserved slots list (see Lists tab).";
		}

		public List<CPluginVariable> GetDisplayPluginVariables()
		{
			return GetPluginVariables();
		}

		public List<CPluginVariable> GetPluginVariables()
		{
			return new List<CPluginVariable>
			{
				new CPluginVariable("Ignore Case", typeof(enumBoolYesNo), ignoreCase),
				new CPluginVariable("Check Interval", typeof(int), checkInterval),
			};
		}

		public void OnPluginLoaded(string hostName, string port, string proconVersion)
		{
			RegisterEvents(className, "OnPlayerJoin", "OnListPlayers", "OnReservedSlotsPlayerAdded", "OnReservedSlotsPlayerRemoved", "OnReservedSlotsList", "OnReservedSlotsCleared");
		}

		public void OnPluginEnable()
		{
			isPluginEnabled = true;
			reservedPlayers = null;

			ExecuteCommand("procon.protected.send", "reservedSlots.list");

			ExecuteCommand("procon.protected.pluginconsole.write", "^b" + GetPluginName() + " ^2Enabled!" );
		}

		public void OnPluginDisable()
		{
			isPluginEnabled = false;
			reservedPlayers = null;

			ExecuteCommand("procon.protected.pluginconsole.write", "^b" + GetPluginName() + " ^1Disabled =(" );
		}

		public void SetPluginVariable(string variable, string value)
		{
			if (variable == "Ignore Case" && Enum.IsDefined(typeof(enumBoolYesNo), value))
			{
				ignoreCase = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), value);
				_UpdateIgnoreCase();
			}
		}

		private void _UpdateIgnoreCase() 
		{
			if (reservedPlayers != null && reservedPlayers.Comparer != _GetStringComparer())
			{
				reservedPlayers = new HashSet<string>(reservedPlayers, _GetStringComparer());
			}
		}

		public override void OnPlayerJoin(string soldierName)
		{
			try
			{
				if (!_IsPlayerReserved(soldierName))
				{
					_KickPlayer(soldierName);
				}
			}
			catch (Exception e)
			{
				ExecuteCommand("procon.protected.pluginconsole.write", className + ".OnPlayerJoin Exception: " + e.Message);
			}
		}

		public override void OnReservedSlotsPlayerAdded(string soldierName)
		{
			try
			{
				if (reservedPlayers == null)
				{
					reservedPlayers = new HashSet<string>(soldierNames, _GetStringComparer());
				}

				reservedPlayers.Add(soldierName);
			}
			catch (Exception e)
			{
				ExecuteCommand("procon.protected.pluginconsole.write", className + ".OnReservedSlotsPlayerAdded Exception: " + e.Message);
			}
		}

		public override void OnReservedSlotsPlayerRemoved(string soldierName)
		{
			try
			{
				if (reservedPlayers != null)
				{
					reservedPlayers.Remove(soldierName);

					_CheckAllPlayers();
				}
			}
			catch (Exception e)
			{
				ExecuteCommand("procon.protected.pluginconsole.write", className + ".OnReservedSlotsPlayerRemoved Exception: " + e.Message);
			}
		}

		public override void OnReservedSlotsList(List<string> soldierNames)
		{
			try
			{
				reservedPlayers = (soldierNames.Count > 0) ? new HashSet<string>(soldierNames, _GetStringComparer()) : null;

				_CheckAllPlayers();
			}
			catch (Exception e)
			{
				ExecuteCommand("procon.protected.pluginconsole.write", className + ".OnReservedSlotsList Exception: " + e.Message);
			}
		}

		public override void OnReservedSlotsCleared()
		{
			try
			{
				reservedPlayers = null;
			}
			catch (Exception e)
			{
				ExecuteCommand("procon.protected.pluginconsole.write", className + ".OnReservedSlotsCleared Exception: " + e.Message);
			}
		}

		public override void OnListPlayers(List<CPlayerInfo> players, CPlayerSubset subset)
		{
			try
			{
				foreach (var player in players)
				{
					var soldierName = player.SoldierName;

					if (!_IsPlayerReserved(soldierName))
					{
						_KickPlayer(soldierName);
					}
				}
			}
			catch (Exception e)
			{
				ExecuteCommand("procon.protected.pluginconsole.write", className + ".OnListPlayers Exception: " + e.Message);
			}
		}

		private void _CheckAllPlayers()
		{
			ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");
		}

		private bool _IsPlayerReserved(string soldierName)
		{
			if (reservedPlayers != null)
			{
				return reservedPlayers.Contains(soldierName);
			}

			return true; // If the reserved list is not loaded we assume everyone is ok
		}

		private StringComparer _GetStringComparer()
		{
			return (ignoreCase == enumBoolYesNo.Yes) ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
		}

		private void _KickPlayer(string soldierName)
		{
			ExecuteCommand("procon.protected.send", "admin.kickPlayer", soldierName, "Sorry for the kick, dude. This is a private match!");
			ExecuteCommand("procon.protected.send", "admin.say", "Kicked '" + soldierName + "' because this is a private match!");
		}
	}
}
