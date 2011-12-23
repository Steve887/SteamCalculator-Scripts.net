using System;
using System.Collections.Generic;

namespace SteamCalculatorScripts.DAL.Entities
{
    public class SteamGame
    {
        public virtual int GameId { get; private set; }
        public virtual int AppId { get; set; }
        public virtual string Title { get; set; }
        public virtual DateTime? ReleaseDate { get; set; }
        public virtual DateTime? LastUpdate { get; set; }
        public virtual int Flags { get; set; }
        public virtual IList<SteamGamePrice> Price { get; set; }

        public SteamGame()
        {
            Price = new List<SteamGamePrice>();
        }

        public virtual void AddPrice(SteamGamePrice price)
        {
            price.Game = this;
            this.Price.Add(price);
        }
    }
}
