
namespace SteamCalculatorScripts.DAL.Entities
{
    public class SteamGamePrice
    {
        public virtual int PriceId { get; private set; }
        public virtual int GameId { get; set; }
        public virtual decimal Price { get; set; }
        public virtual string RegionCode { get; set; }
        public virtual SteamGame Game { get; set; }

        public SteamGamePrice()
        { }

        public SteamGamePrice(decimal price, string regionCode)
        {
            this.Price = price;
            this.RegionCode = regionCode;
        }
    }
}
