using FluentNHibernate.Mapping;
using SteamCalculatorScripts.DAL.Entities;

namespace SteamCalculatorScripts.DAL.Mappings
{
    public class SteamGamePriceMap : ClassMap<SteamGamePrice>
    {
        public SteamGamePriceMap()
        {
            Id(x => x.PriceId);
            Map(x => x.Price);
            Map(x => x.RegionCode);
            References(x => x.Game).Column("GameId");         
        }
    }
}
