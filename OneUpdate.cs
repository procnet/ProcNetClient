using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ProcNetClient
{
    /// <summary>
    /// Обновление информации о процессе
    /// </summary>
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
