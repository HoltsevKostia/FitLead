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
