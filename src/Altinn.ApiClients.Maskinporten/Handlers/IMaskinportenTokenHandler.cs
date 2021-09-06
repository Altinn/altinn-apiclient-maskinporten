using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Altinn.ApiClients.Maskinporten.Service;

namespace Altinn.ApiClients.Maskinporten.Handlers
{

    public interface IMaskinportenTokenHandler
    {
    }

    public interface IMaskinportenTokenHandler<T> : IMaskinportenTokenHandler where T : ICustomClientSecret { }
}
