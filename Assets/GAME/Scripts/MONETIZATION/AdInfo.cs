namespace GAME.Scripts.MONETIZATION
{
    public class AdInfo  {
        public string Placement;
        public AdsManager.EAdType AdType;
        public bool HasInternet;
        public string Availability;

        public AdInfo(string Placement, AdsManager.EAdType AdType, bool HasInternet = true, string Availability = "available") {
            this.HasInternet = HasInternet;
            this.Placement = Placement;
            this.AdType = AdType;
            this.Availability = Availability;
        }
    }
}