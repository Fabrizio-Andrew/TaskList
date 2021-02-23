namespace TaskList.CustomSettings
{
    public class TaskLimit
    {
        /// <summary>
        /// Gets or sets the maximum number of customers.
        /// </summary>
        /// <value>
        /// The maximum number of customers.
        /// </value>
        /// <remarks>Setting a default max customers if non provided in config</remarks>
        public int MaxTaskEntries { get; set; } = 100;
    }
}
