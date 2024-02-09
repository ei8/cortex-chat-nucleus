using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model
{
    public interface IIdentityService
    {
        string UserId { get; set; }
    }
}
