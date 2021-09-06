﻿using Altinn.ApiClients.Maskinporten.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.ApiClients.Maskinporten.Service
{
    public interface IClientSecret
    {
        Task<ClientSecrets> GetClientSecrets();
    }

    public interface IClientSecret<T> : IClientSecret where T : ICustomClientSecret {}
}