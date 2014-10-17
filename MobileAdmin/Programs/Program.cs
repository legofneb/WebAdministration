using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileAdmin
{

    public class Program
    {
        protected List<ProgramStep> _programSteps;
        protected Logger _log;

        public Program()
        {
            _programSteps = new List<ProgramStep>();
        }

        public virtual void Run()
        {
            foreach (var step in _programSteps)
            {
                step.Execute();
            }
        }

        public void Write(string text)
        {
            _log.Write(text);
        }

        public void Write(string output, string id)
        {
            _log.Write(output, id);
        }
    }
}