using System.Net;
using Orleans.Runtime;

namespace Orleans.Nats;

static class Extenders
{
    public static string CreateEtag() =>
        Guid.NewGuid().ToString();

    public static Uri ToGatewayUri(this MembershipEntry me) =>
        SiloAddress.New(new IPEndPoint(me.SiloAddress.Endpoint.Address, me.ProxyPort), me.SiloAddress.Generation).ToGatewayUri();

    public static string GetGrainNormalizedName<T>(this GrainId grainId, string stateName) =>
        string.Join('-',
                    typeof(T).FullName!.Replace('.', '-'),
                    grainId.ToString().Replace('/', '-'),
                    stateName.Replace('/', '-').Replace('.', '-').Replace('\\', '-'));
    
    public static string GetReminderNormalizedName(this GrainId grainId, string reminderName) =>
        string.Join('-',
                    grainId.ToString().Replace('/', '-'),
                    reminderName.Replace('/', '-').Replace('.', '-').Replace('\\', '-'));
}