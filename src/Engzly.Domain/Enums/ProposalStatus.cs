using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engzly.Domain.Enums
{
    public enum ProposalStatus : byte
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Withdrawn = 3
    }
}
