using Microsoft.Extensions.DependencyInjection;
using SingularityChat.WebAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SingularityChat.WebAPI.Utils
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddJwtService(this IServiceCollection sc, Action<JwtTokenServiceBuilder> config)
        {
            var bd = new JwtTokenServiceBuilder();
            config.Invoke(bd);
            return sc.AddScoped(x => bd.Build());
        }

        public static IServiceCollection AddChatRoomService(this IServiceCollection sc)
        {
            return sc.AddSingleton<ChatRoomServices>();
        }
    }
}
