using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Domain.Common.Exceptions
{
    public abstract class DomainException : Exception
    {
        public string Code { get; }

        protected DomainException(string code, string message)
            : base(message)
        {
            Code = code;
        }
    }
}
