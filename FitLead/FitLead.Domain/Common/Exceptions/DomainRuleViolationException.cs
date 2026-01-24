using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Domain.Common.Exceptions
{
    public sealed class DomainRuleViolationException : DomainException
    {
        public DomainRuleViolationException(string code, string message)
            : base(code, message)
        {
        }
    }
}
