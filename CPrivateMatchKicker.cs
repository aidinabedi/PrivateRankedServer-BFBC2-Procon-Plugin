using System;
using System.Collections.Generic;
using PRoCon.Core;
using PRoCon.Core.Players;
using PRoCon.Core.Plugin;

namespace PRoConEvents
{
	public class CPrivateMatchKicker : PRoConPluginAPI, IPRoConPluginInterface
	{
		private static readonly string className = typeof(CPrivateMatchKicker).Name;

		private readonly List<string> m_reservedPlayers;

		private bool m_isPluginEnabled;
		
		private enumBoolYesNo m_ignoreCase = enumBoolYesNo.No;
		
		private int m_checkInterval;

		public string GetPluginName()
		{
			return "Private Match Kicker";
		}

		public string GetPluginVersion()
		{
			return "1.3.0.0";
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

		// Lists all of the plugin variables.
		public List<CPluginVariable> GetPluginVariables()
		{
			var retval = new List<CPluginVariable>();

			retval.Add(new CPluginVariable("Ignore Case", typeof(enumBoolYesNo), m_ignoreCase));
			retval.Add(new CPluginVariable("Check Interval", typeof(int), m_checkInterval));

			return retval;
		}

		public void OnPluginLoaded(string hostName, string port, string proconVersion)
		{
			RegisterEvents(className, "OnPlayerJoin", "OnListPlayers", "OnReservedSlotsPlayerAdded", "OnReservedSlotsPlayerRemoved", "OnReservedSlotsList", "OnReservedSlotsCleared");
		}

		public void OnPluginEnable()
		{
			m_isPluginEnabled = true;
			m_reservedPlayers = null;

			ExecuteCommand("procon.protected.send", "reservedSlots.list");

			ExecuteCommand("procon.protected.pluginconsole.write", "^b" + GetPluginName() + " ^2Enabled!" );

			_UpdateCheckInterval();
		}

		public void OnPluginDisable()
		{
			m_isPluginEnabled = false;
			m_reservedPlayers = null;

			ExecuteCommand("procon.protected.tasks.remove", className);

			ExecuteCommand("procon.protected.pluginconsole.write", "^b" + GetPluginName() + " ^1Disabled =(" );
		}

		public void SetPluginVariable(string variable, string value)
		{
			if (variable == "Check Interval")
			{
				int.TryParse(value, out m_checkInterval);
				_UpdateCheckInterval();
			}
			else if (variable == "Ignore Case" && Enum.IsDefined(typeof(enumBoolYesNo), value))
			{
				m_ignoreCase = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), value);
			}
		}

		private void _UpdateCheckInterval() 
		{
			ExecuteCommand("procon.protected.tasks.remove", className);

			if (m_isPluginEnabled && m_checkInterval > 0)
			{
				ExecuteCommand("procon.protected.tasks.add", className, "0", m_checkInterval.ToString(), "-1", "procon.protected.send", "admin.listPlayers", "all");
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
				if (m_reservedPlayers == null)
				{
					m_reservedPlayers = new List<string>(soldierNames);
				}

				m_reservedPlayers.Add(soldierName);
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
				if (m_reservedPlayers != null)
				{
					m_reservedPlayers.Remove(soldierName);

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
				m_reservedPlayers = (soldierNames.Count > 0) ? new List<string>(soldierNames) : null;

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
				m_reservedPlayers = null;
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
			if (m_reservedPlayers != null)
			{
				var comparer = (m_ignoreCase == enumBoolYesNo.Yes) ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
				return m_reservedPlayers.Contains(soldierName, comparer);
			}

			return true; // If the list is not loaded we assume everyone is ok
		}

		private void _KickPlayer(string soldierName)
		{
			ExecuteCommand("procon.protected.send", "admin.kickPlayer", soldierName, "Sorry for the kick, dude! This is a private match!");
			ExecuteCommand("procon.protected.send", "admin.say", "Kicked '" + soldierName + "' because this is a private match!");
		}
	}
}
