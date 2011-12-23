using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using SteamCalculatorScripts.DAL.Entities;

namespace SteamCalculatorScripts.DAL
{
    public static class SessionFactory
    {
        public static ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
                .Database(
                    MsSqlConfiguration.MsSql2008.ConnectionString(c=>c.FromConnectionStringWithKey("SteamTradesConnectionString"))
                ).Mappings(m=>m.FluentMappings.AddFromAssemblyOf<SteamGame>())
                .BuildSessionFactory();
        }
    }
}
