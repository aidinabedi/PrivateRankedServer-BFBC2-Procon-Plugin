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

		private readonly HashSet<string> m_reservedPlayers;

		private bool m_isPluginEnabled;
		
		private int m_checkInterval = 0;

		public CPrivateMatchKicker()
		{
			m_reservedPlayers = new HashSet<string>();
		}

		public string GetPluginName()
		{
			return "Private Match Kicker";
		}

		public string GetPluginVersion()
		{
			return "1.0.0.0";
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

			// TODO

			return retval;
		}

		public void OnPluginLoaded(string hostName, string port, string proconVersion)
		{
			RegisterEvents(GetType().Name, "OnPlayerJoin", "OnReservedSlotsPlayerAdded", "OnReservedSlotsPlayerRemoved", "OnReservedSlotsList", "OnReservedSlotsCleared");
		}

		public void OnPluginEnable()
		{
			m_isPluginEnabled = true;

			ExecuteCommand("procon.protected.send", "reservedSlots.list");
		}

		public void OnPluginDisable()
		{
			m_isPluginEnabled = false;

			m_reservedPlayers.Clear();
		}

		public void SetPluginVariable(string variable, string value)
		{
			// TODO
		}

		public override void OnPlayerJoin(string soldierName)
		{
			try
			{
				if (!m_reservedPlayers.Contains(soldierName))
				{
					ExecuteCommand("procon.protected.send", "admin.kickPlayer", soldierName, "Sorry for the kick, dude! This is a private match!");
					ExecuteCommand("procon.protected.send", "admin.say", "Kicked '" + soldierName + "' because this is a private match!");
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
				m_reservedPlayers.Remove(soldierName);
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
				m_reservedPlayers.Clear();

				foreach (var soldierName in soldierNames)
				{
					m_reservedPlayers.Add(soldierName);
				}
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
				m_reservedPlayers.Clear();
			}
			catch (Exception e)
			{
				ExecuteCommand("procon.protected.pluginconsole.write", className + ".OnReservedSlotsCleared Exception: " + e.Message);
			}
		}
	}
}
