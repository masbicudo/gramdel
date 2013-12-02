using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gramdel.Core
{
    /// <summary>
    /// Represents the possible states of a production.
    /// </summary>
    public enum ProductionState
    {
        /// <summary>
        /// Indicates that the production method has not yet been started, but the production context exists already.
        /// </summary>
        Created,

        /// <summary>
        /// Indicates that the production method has been started. Product alternatives shall be created or failed in this state.
        /// </summary>
        Started,

        /// <summary>
        /// Indicates that the production method is closed. No more product alternatives will be created.
        /// </summary>
        Closed,
    }
}
