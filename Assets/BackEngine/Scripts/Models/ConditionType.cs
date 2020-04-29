namespace BE.Models
{
    /// <summary>
    /// Type of condition
    /// ">" is Greater
    /// ">=" is GreaterEquals
    /// "<" is Lessthan
    /// "<=" is LessthanEquals
    /// "=" is Equals
    /// "!=" is NotEquals
    /// In Array is In
    /// Not In Array is NotIn
    /// </summary>
    public enum ConditionType { Greater, GreaterEquals, Lessthan, LessthanEquals, Equals, NotEquals, In, NotIn,And,Or,Expression }

}