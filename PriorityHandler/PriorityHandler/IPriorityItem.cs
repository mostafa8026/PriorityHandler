namespace PriorityHandler
{
    /// <summary>
    /// This namespace is used with classes used PriorityHandler functions.
    /// </summary>
    public interface IPriorityItem
    {
        /// <summary>
        /// priority number as int, smaller number means items with more priority.
        /// </summary>
        int Priority { get; set; }
    }
}