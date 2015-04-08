using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch8.API
{
    public enum RatingType { box, clock, cross, funny2, heart, information, palette, rainbow, tick, winner, wrench, zing };

    public class Rating
    {
        public RatingType Type { get; set; }
        public int Amount { get; set; }
        public Post Post { get; set; }
    }
}
