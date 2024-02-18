namespace CustomWarps.Framework.Models {
    public class CustomWarp {
        public string WarpName { get; set; }

        public string MapName { get; set; }

        public int XCoordinate { get; set; }

        public int YCoordinate { get; set; }

        public bool IsGlobal { get; set; }

        public bool IsBuilding { get; set; }
    }
}
