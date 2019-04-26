namespace CustomWarps.Framework.Models
{
	public class CustomWarp
	{
		public string WarpName { get; set; }

		public string MapName { get; set; }

		public int xCoordinate { get; set; }

		public int yCoordinate { get; set; }

		public bool IsGlobal { get; set; }

		public bool IsBuilding { get; set; }
	}
}
