using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ProcNetClient
{
    public class OneUpdate
    {
        public enum ModeUpdate { AddProcess, RemoveProcess };

        public ModeUpdate Mode;
        public Process Proc;

        public OneUpdate(ModeUpdate mode, Process changeProcess)
        {
            this.Mode = mode;
            this.Proc = changeProcess;
        }
    }
}
