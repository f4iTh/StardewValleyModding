using CustomWarps.Framework.Models;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomWarps.Framework
{
	public class WarpHelper
	{
		private readonly IModHelper Helper;
		private readonly IMonitor Monitor;

		public Dictionary<string, CustomWarp> CustomWarps;

		private Dictionary<string, CustomWarp> LocalWarps;
		private Dictionary<string, CustomWarp> GlobalWarps;

		//private SortStyle OrderBy { get; set; } = SortStyle.Default;

		public WarpHelper(IModHelper Helper, IMonitor Monitor)
		{
			this.Helper = Helper;
			this.Monitor = Monitor;
			this.LocalWarps = this.LoadLocalCustomWarps();
			this.GlobalWarps = this.LoadGlobalCustomWarps();
			Dictionary<string, CustomWarp> dictionary = new Dictionary<string, CustomWarp>();
			foreach (KeyValuePair<string, CustomWarp> gWarp in this.GlobalWarps)
				if (!dictionary.ContainsKey(gWarp.Key))
					dictionary.Add(gWarp.Key, gWarp.Value);
			foreach (KeyValuePair<string, CustomWarp> lWarp in this.LocalWarps)
				if (!dictionary.ContainsKey(lWarp.Key))
					dictionary.Add(lWarp.Key, lWarp.Value);
			this.CustomWarps = dictionary;
			this.CustomWarps.OrderBy(n => n.Value.WarpName);
		}

		public bool HasKey(string Key)
		{
			return this.CustomWarps.ContainsKey(Key);
		}

		public CustomWarp GetWarp(string Key)
		{
			if (!this.CustomWarps.ContainsKey(Key))
				return null;
			return this.CustomWarps[Key];
		}

		public void Add(string name, CustomWarp warp, bool isGlobal)
		{
			if (isGlobal)
			{
				if (!this.GlobalWarps.ContainsKey(name))
					this.GlobalWarps.Add(name, warp);
				this.SaveWarps(isGlobal);
				return;
			}
			if (!this.LocalWarps.ContainsKey(name))
				this.LocalWarps.Add(name, warp);
			this.SaveWarps(isGlobal);
		}

		public bool BooleanAdd(string name, CustomWarp warp, bool isGlobal)
		{
			if (isGlobal)
			{
				if (!this.GlobalWarps.ContainsKey(name))
				{
					this.GlobalWarps.Add(name, warp);
					this.SaveWarps(isGlobal);
					return true;
				}
				return false;
			}
			if (!this.LocalWarps.ContainsKey(name))
			{
				this.LocalWarps.Add(name, warp);
				this.SaveWarps(isGlobal);
				return true;
			}
			return false;
		}

		public void Remove(string name, bool isGlobal)
		{
			if (!this.CustomWarps.ContainsKey(name))
				return;
			if (isGlobal)
				this.GlobalWarps.Remove(name);
			else
				this.LocalWarps.Remove(name);
			this.CustomWarps.Remove(name);
			this.SaveWarps(isGlobal);
		}

		public void SaveWarps(bool isGlobal)
		{
			if (isGlobal)
				this.Helper.Data.WriteJsonFile<Dictionary<string, CustomWarp>>($"warps/global.json", this.GlobalWarps);
			else
				this.Helper.Data.WriteJsonFile<Dictionary<string, CustomWarp>>($"warps/{Constants.SaveFolderName}.json", this.LocalWarps);
			this.Update();
		}

		//public void SwitchSort(SortStyle which)
		//{
		//	this.OrderBy = which;
		//	this.Sort();
		//}

		//public void Sort()
		//{
		//	switch (this.OrderBy)
		//	{
		//		case SortStyle.Default:
		//			break;
		//		case SortStyle.LocationName:
		//			this.CustomWarps.OrderBy(n => n.Value.MapName);
		//			break;
		//		case SortStyle.WarpName:
		//			this.CustomWarps.OrderBy(n => n.Value.WarpName);
		//			break;
		//	}
		//}

		private Dictionary<string, CustomWarp> LoadLocalCustomWarps()
		{
			return this.Helper.Data.ReadJsonFile<Dictionary<string, CustomWarp>>($"warps/{Constants.SaveFolderName}.json") ?? new Dictionary<string, CustomWarp>();
		}

		private Dictionary<string, CustomWarp> LoadGlobalCustomWarps()
		{
			return this.Helper.Data.ReadJsonFile<Dictionary<string, CustomWarp>>($"warps/global.json") ?? new Dictionary<string, CustomWarp>();
		}

		private void Update()
		{
			this.LocalWarps.Clear();
			this.GlobalWarps.Clear();
			this.CustomWarps.Clear();

			this.LocalWarps = this.LoadLocalCustomWarps();
			this.GlobalWarps = this.LoadGlobalCustomWarps();

			try
			{
				Dictionary<string, CustomWarp> dictionary = new Dictionary<string, CustomWarp>();
				foreach (KeyValuePair<string, CustomWarp> gWarp in this.GlobalWarps)
					if (!dictionary.ContainsKey(gWarp.Key))
						dictionary.Add(gWarp.Key, gWarp.Value);
				foreach (KeyValuePair<string, CustomWarp> lWarp in this.LocalWarps)
					if (!dictionary.ContainsKey(lWarp.Key))
						dictionary.Add(lWarp.Key, lWarp.Value);
				this.CustomWarps = dictionary;
			}
			catch (ArgumentException ae)
			{
				this.Monitor.Log($"Something went wrong!\n{ae}", LogLevel.Error);
			}
			//this.Sort();
		}

		//public enum SortStyle
		//{
		//	Default = 1,
		//	WarpName = 2,
		//	LocationName = 3
		//}
	}
}