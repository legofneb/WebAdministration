using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileAdmin
{
    public abstract class ProgramStep
    {
        protected Logger _log;

        public ProgramStep(Logger log)
        {
            _log = log;
        }

        /// <summary>
        /// Executes this ProgramStep Instance by running a precondition check, then running the programstep, then running a postcondition check.
        /// </summary>
        /// <returns>true if successful, false if a condition check failed or if execution fails.</returns>
        public abstract bool Execute();

        protected abstract bool PreConditionCheck();

        protected abstract void Run();

        protected abstract bool PostConditionCheck();
    }
}