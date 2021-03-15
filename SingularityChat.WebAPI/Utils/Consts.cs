using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingularityChat.WebAPI.Utils
{
    public static class Consts
    {
        public static readonly string[] DefaultRoomNames = new string[]
        {
            "polaris", "sirius", "alpha-centauri", "betelgeuse", "rigel", "vega", "pleiades",
            "antares", "canopus", "droak", "zliash", "maix", "cloals", "draakdict", "choamlurs",
            "usevro", "tressab", "kleiccax"
        };

        public static Random Random = new Random();

        public static string GetRandomRoomNames()
        {
            return DefaultRoomNames[Random.Next(0, DefaultRoomNames.Length)];
        }

        public static SecurityKey GetSecurityKey(this string key)
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        }
    }
}
