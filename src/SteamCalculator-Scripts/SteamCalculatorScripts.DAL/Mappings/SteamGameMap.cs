using FluentNHibernate.Mapping;
using SteamCalculatorScripts.DAL.Entities;

namespace SteamCalculatorScripts.DAL.Mappings
{
    public class SteamGameMap : ClassMap<SteamGame>
    {
        public SteamGameMap()
        {
            Id(x => x.GameId).GeneratedBy.Identity();
            Map(x => x.AppId);
            Map(x => x.Title);
            Map(x => x.ReleaseDate).Default(null).Nullable();
            Map(x => x.LastUpdate).Default(null).Nullable();
            Map(x => x.Flags);
            HasMany(x => x.Price).KeyColumn("GameId").Inverse().Cascade.All();
        }
    }
}
