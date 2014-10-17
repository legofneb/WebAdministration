using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileAdmin
{

    public class USMTList
    {
        private static USMTList _instance;
        private static object syncLock = new object();
        private Dictionary<string, Program> _programs;
        private List<string> _runningList;

        protected USMTList()
        {
            _programs = new Dictionary<string, Program>();
            _runningList = new List<string>();
        }

        public static USMTList Instance()
        {
            if (_instance == null)
            {
                lock (syncLock)
                {
                    if (_instance == null)
                    {
                        _instance = new USMTList();
                    }
                }
            }

            return _instance;
        }

        public void AddProgram(string id, Program program)
        {
            if (_programs.ContainsKey(id))
            {
                USMT currentProgram = (USMT)_programs[id];
                currentProgram.Write("USMT already running on this computer.", currentProgram._computer.IPAddress);
            }
            else
            {
                _programs.Add(id, program);
            }
        }

        public void StartProgram(string id)
        {
            if (!_runningList.Contains(id))
            {
                Program currentProgram = _programs[id];
                _runningList.Add(id);
                currentProgram.Run();
            }
            else
            {
                
            }
        }

        public void RemoveProgram(string id)
        {
            _programs.Remove(id);
            _runningList.Remove(id);
        }

        public List<Program> GetPrograms()
        {
            List<Program> programs = new List<Program>();

            foreach (var programDict in _programs)
            {
                programs.Add(programDict.Value);
            }

            return programs;
        }
    }
}