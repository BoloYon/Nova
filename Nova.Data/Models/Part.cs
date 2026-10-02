namespace Nova.Data.Models
{
    public class Part
    {
        //Each new part has a unique ID
        public int Id { get; set; }
        //Part Properties
        public string PartName { get; set; } =  "";
        public string PrimaryPartNumber{ get; set; } = "";
        public string Description { get; set; } = "";
        public string UnitType { get; set; } = "";
        public decimal ShipWeight { get; set; }
        public decimal CubicFeet { get; set; }
        public bool DrawingOnFile { get; set; }
        public bool MoldAvailable { get; set; }
        public string Notes { get; set; } = "";

    }
}
