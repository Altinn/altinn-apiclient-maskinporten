﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.ApiClients.Maskinporten.Models
{
    public class TokenRequestException : ApplicationException
    {
        public TokenRequestException(string message)
            : base(message)
        {
        }
    }
}
